using Coordinator.Enums;
using Coordinator.Modes;
using Coordinator.Modes.Contexts;
using Coordinator.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Services
{
    public class TransactionService(IHttpClientFactory _httpClientFactory,TwoPhaseCommitContext _context) : ITransactionService
    {

        private HttpClient _orderHttpClient = _httpClientFactory.CreateClient("OrderAPI");
        private HttpClient _stockHttpClient = _httpClientFactory.CreateClient("StockAPI");
        private HttpClient _paymentHttpClient = _httpClientFactory.CreateClient("PaymentAPI");


        
        public async Task<Guid> CreateTransactionAsync()
        {
            //Random Bir transactionId olusturuyoruz
            //Daha sonrasinda tum nodelari teker teker gezip random olusturdugumuz transaction id yi nodelara veriyor ve bu nodelarin ReadyTypeini ve TransactionTypeni pending yapiyor

            Guid transactionId = Guid.NewGuid();
            var nodes = await _context.Nodes.ToListAsync();

            nodes.ForEach(node=>node.NodeStates = new List<NodeState>()
            {
                new (transactionId)
                {
                    IsReady = Enums.ReadyType.Pending,
                    TransactionState = Enums.TransactionState.Pending
                }
            });

            await _context.SaveChangesAsync();
            return transactionId;

        }

        public async Task PrepareServicesAsync(Guid transactionId)
        {
            //Elimizdeki transactionid ile ilgili olan butun satirlari elde edicez ve gerekli nodelarin hepsine istekte bulunup hazir olup olmadiklarini ogrenecegiz.
            var transactionNodes = await _context.NodeStates
                .Include(ns => ns.Node)
                .Where(ns => ns.TransactionId == transactionId)
                .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response =await (transactionNode.Node.Name switch
                    {
                        //Eger Order.API ise benim Order.API servisindeki ready endpointine istekte bulunmam lazim
                        "Order.API" => _orderHttpClient.GetAsync("ready"),
                        "Stock.API" => _stockHttpClient.GetAsync("ready"),
                        "Payment.API" => _paymentHttpClient.GetAsync("ready")
                    });
                    //Resultin icerisinde her bir transactionNode icin readye yapilan istek neticesinde ilgili servislerin hazir olup olmadiklarina result yani sonucu tutuyoruz
                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.IsReady = result ? Enums.ReadyType.Ready : Enums.ReadyType.Unready;
                }
                catch (Exception e)
                {
                    transactionNode.IsReady = Enums.ReadyType.Unready;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckReadyServicesAsync(Guid transactionId)
        {
            //bu kisimda return degeri true olursa 1.asama basarili bir sekilde tamamlanmis olur
            //ilgili transactionId`ye ait nodelar Readytypelarini kontrol ediyoruz hepsi ready ise geriye return ile true degeri donuyor
            return (await _context.NodeStates
                .Where(ns => ns.TransactionId == transactionId)
                .ToListAsync()).TrueForAll(ns => ns.IsReady == Enums.ReadyType.Ready);

        }

        public async Task CommitAsync(Guid transactionId)
        {
            var transactionNodes =await _context.NodeStates
                .Where(ns => ns.TransactionId == transactionId)
                .Include(ns => ns.Node)
                .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response =await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("commit"),
                        "Stock.API" => _stockHttpClient.GetAsync("commit"),
                        "Payment.API" => _paymentHttpClient.GetAsync("commit")
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());

                    transactionNode.TransactionState =
                        result ? Enums.TransactionState.Done : Enums.TransactionState.Abort;

                }
                catch
                {
                    transactionNode.TransactionState = Enums.TransactionState.Abort;
                }
            }

            await _context.SaveChangesAsync();

        }

        public async Task<bool> CheckTransactionStateServicesAsync(Guid transactionId)
        {
            return (await _context.NodeStates
                .Where(ns => ns.TransactionId == transactionId)
                .ToListAsync()).TrueForAll(ns => ns.TransactionState == Enums.TransactionState.Done);
        }

        
        public async Task RollbackAsync(Guid transactionId)
        {
            var transactionNodes =await _context.NodeStates
                .Where(ns => ns.TransactionId == transactionId)
                .Include(ns => ns.Node)
                .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    if (transactionNode.TransactionState ==TransactionState.Done)
                    {
                        _ = await (transactionNode.Node.Name switch
                        {
                            "Order.API" => _orderHttpClient.GetAsync("rollback"),
                            "Stock.API" => _stockHttpClient.GetAsync("rollback"),
                            "Payment.API" => _paymentHttpClient.GetAsync("rollback")
                        });
                    }

                    transactionNode.TransactionState = TransactionState.Abort;

                }
                catch
                {
                    transactionNode.TransactionState = TransactionState.Abort;
                }
            }

            await _context.SaveChangesAsync();

        }
    }
}
