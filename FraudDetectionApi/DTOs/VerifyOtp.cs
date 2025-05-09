namespace FraudDetectionApi.DTOs
{
    public class VerifyOtp
    {
        public string CardNumber { get; set; }

        public Guid TransactionId { get; set; }

        public string Code { get; set; }
    }
}
