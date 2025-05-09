namespace FraudDetectionApi.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }

        public string? OrderId { get; set; }

        public int? SubscribeId { get; set; }

        public float Amount { get; set; }

        public string? Location { get; set; }

        public string? Device { get; set; }

        public string? IPAddress { get; set; }

        public int TransactionType { get; set; }

        public string? Source { get; set; }

        public DateTime? CreatedOn { get; set; }
    }
}
