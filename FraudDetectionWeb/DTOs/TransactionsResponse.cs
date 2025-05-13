using FraudDetectionWeb.Models;

namespace FraudDetectionWeb.DTOs
{
	public class TransactionsResponse : PaymentTransaction
	{
        public string BusinessName { get; set; }

        public string TransactionTypeText { get; set; }
    }
}
