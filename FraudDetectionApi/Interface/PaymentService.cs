using FraudDetectionApi.DTOs;
using FraudDetectionApi.Models;

namespace FraudDetectionApi.Interface
{
    public interface IPaymentService
    {
        Subscription? VerifySubscribe(string clientId, string clientSecret);

        PaymentTransaction Add(PaymentRequest paymentRequest);
    }
}
