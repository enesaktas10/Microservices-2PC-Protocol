namespace Coordinator.Services.Abstraction
{
    public interface ITransactionService
    {
        //Burada bir transaction olustuyoruz
        Task<Guid> CreateTransactionAsync();


        //1.Asamada servislerimizin hazir olup olmadigini dogrulayabilmemiz gerekmektedir.
        //kullanicidan gelen talep uzerine servislerimizi hazir olup olmadigini buradan anlayacagiz.
        Task PrepareServicesAsync(Guid transactionId);

        Task<bool> CheckReadyServicesAsync(Guid transactionId);


        //2.Asamin ilk adimi
        //Hepsi ready ise commit talimatini verir
        Task CommitAsync(Guid transactionId);

        //Burada tum servisler islemlerini basarili bir sekilde tamamladilar mi eger tamamlamadiysa Rollback fonksiyonu cagirmaliyiz
        Task<bool> CheckTransactionStateServicesAsync(Guid transactionId);
        Task RollbackAsync(Guid transactionId);
    }
}
