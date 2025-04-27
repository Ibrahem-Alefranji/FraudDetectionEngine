
using Microsoft.ML.Data;

namespace FraudDetectionEngine.Models
{
    public class TransactionData
    {
        public int UserId { get; set; }
        public float Amount { get; set; }
        public float Time { get; set; }
        public int Location { get; set; }
        public int Device { get; set; }
        public int TransactionType { get; set; }

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
