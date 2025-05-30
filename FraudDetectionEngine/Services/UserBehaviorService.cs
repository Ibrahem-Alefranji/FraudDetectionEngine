
using Microsoft.Data.SqlClient;
using Dapper;
using FraudDetectionEngine.Models;

namespace FraudDetectionEngine.Services
{
    public static class UserBehaviorService
    {
        public static TransactionTraningData Enrich(TransactionTraningData tx, string connectionString)
        {
            using var conn = new SqlConnection(connectionString);

            var stats = conn.QueryFirstOrDefault(@"
                SELECT 
                    AVG(TransactionAmount) AS AvgAmount,
                    MAX(TransactionAmount) AS MaxAmount,
                    COUNT(*) AS CountLast7Days
                FROM UserBehaviorLog
                WHERE CardNumber = @CardNumber AND CreatedOn >= DATEADD(DAY, -30, GETDATE())",
                new { tx.CardNumber });

            tx.UserAvgAmount30Days = (float)(stats?.AvgAmount ?? 0);
            tx.UserMaxAmount30Days = (float)(stats?.MaxAmount ?? 0);
            tx.UserTransactionCount7Days = stats?.CountLast7Days ?? 0;

            return tx;
        }

        public static void Log(TransactionTraningData tx, string transactionId, string connectionString)
        {
            using var conn = new SqlConnection(connectionString);

            string sql = @"
                INSERT INTO UserBehaviorLog (
                    CardNumber, TransactionAmount, CreatedOn, IPAddress, Device, TransactionType
                ) VALUES (
                    @CardNumber, @Amount, @CreatedOn, @IPAddress, @Device, @TransactionType
                )";

            conn.Execute(sql, new
            {
                tx.CardNumber,
                TransactionId = transactionId,
                tx.Amount,
                tx.CreatedOn,
                tx.IPAddress,
                tx.Device,
                tx.TransactionType
            });
        }
    }
}
