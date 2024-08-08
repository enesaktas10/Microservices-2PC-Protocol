using Coordinator.Enums;

namespace Coordinator.Modes
{
    public record NodeState(Guid TransactionId)
    {
        public Guid Id { get; set; }

        //1.Aşamanın durumunu ifade ediyor
        public ReadyType IsReady { get; set; }

        //2.Aşamının neticesinde işlemin başarıyla tamamlanıp tamamlanmadığını ifade diyor.
        public TransactionState TransactionState { get; set; }


        public Node Node { get; set; }
    }
}
