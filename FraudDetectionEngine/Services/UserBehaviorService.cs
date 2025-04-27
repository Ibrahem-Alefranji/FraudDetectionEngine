
using Microsoft.Data.SqlClient;
using Dapper;
using FraudDetectionEngine.Models;

namespace FraudDetectionEngine.Services
{
    public static class UserBehaviorService
    {
        public static TransactionData Enrich(TransactionData tx, string connectionString)
        {
            using var conn = new SqlConnection(connectionString);

            var stats = conn.QueryFirstOrDefault(@"
                SELECT 
                    AVG(TransactionAmount) AS AvgAmount,
                    MAX(TransactionAmount) AS MaxAmount,
                    COUNT(*) AS CountLast7Days
                FROM UserBehaviorLog
                WHERE UserId = @UserId AND Timestamp >= DATEADD(DAY, -30, GETDATE())",
                new { tx.UserId });

            //tx.UserAvgAmount30Days = 150;// stats?.AvgAmount ?? 0;
            //tx.UserMaxAmount30Days = 300;// stats?.MaxAmount ?? 0;
            //tx.UserTransactionCount7Days = 2;// stats?.CountLast7Days ?? 0;

            return tx;
        }

        public static void Log(TransactionData tx, Guid transactionId, string connectionString)
        {
            using var conn = new SqlConnection(connectionString);

            string sql = @"
                INSERT INTO UserBehaviorLog (
                    UserId, TransactionId, Amount, Time, Location, Device, TransactionType
                ) VALUES (
                    @UserId, @TransactionId, @Amount, @Time, @Location, @Device, @TransactionType
                )";

            conn.Execute(sql, new
            {
                tx.UserId,
                TransactionId = transactionId,
                tx.Amount,
                tx.Time,
                tx.Location,
                tx.Device,
                tx.TransactionType
            });
        }
    }
}
