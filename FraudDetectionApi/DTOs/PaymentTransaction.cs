using FraudDetectionApi.Models;

namespace FraudDetectionApi.DTOs
{
    public class PaymentRequest : PaymentTransaction
    {
        public string? ClientId { get; set; }

        public string? ClientSecret { get; set; }

        public string? CardNumber { get; set; }
    }
}
