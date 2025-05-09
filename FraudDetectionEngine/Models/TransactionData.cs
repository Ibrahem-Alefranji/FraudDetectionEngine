
using Microsoft.ML.Data;

namespace FraudDetectionEngine.Models
{
    public class TransactionData
    {
        public Guid TransactionId { get; set; }

        public string CardNumber { get; set; }
        public float Amount { get; set; }
        public string Location { get; set; }
        public string Device { get; set; }
        public string IPAddress { get; set; }
        public int TransactionType { get; set; }
        public string Source { get; set; }
        public DateTime CreatedOn { get; set; }

        public float UserAvgAmount30Days { get; set; }
        public float UserMaxAmount30Days { get; set; }
        public float UserTransactionCount7Days { get; set; }

        public bool IsFraud { get; set; }
    }

    public class TransactionPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool IsFraud { get; set; }
        public float Score { get; set; }
    }
}
