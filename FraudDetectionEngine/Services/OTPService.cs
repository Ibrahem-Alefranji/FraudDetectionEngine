
using System;
using Microsoft.Data.SqlClient;
using Dapper;

namespace FraudDetectionEngine.Services
{
    public static class OTPService
    {
        public static void Generate(int userId, Guid transactionId, string connectionString)
        {
            string code = new Random().Next(100000, 999999).ToString();
            DateTime expires = DateTime.Now.AddMinutes(5);

            using var conn = new SqlConnection(connectionString);
            conn.Execute(@"
                INSERT INTO OtpChallenges (UserId, TransactionId, Code, ExpiresAt)
                VALUES (@UserId, @TransactionId, @Code, @ExpiresAt)",
                new { userId, transactionId, code, expires });

            Console.WriteLine($"[OTP] Sent code {code} to user {userId}");
        }

        public static bool Verify(int userId, Guid transactionId, string code, string connectionString)
        {
            using var conn = new SqlConnection(connectionString);
            var record = conn.QueryFirstOrDefault(@"
                SELECT * FROM OtpChallenges 
                WHERE UserId = @UserId AND TransactionId = @TransactionId 
                AND Code = @Code AND ExpiresAt > GETDATE() AND IsVerified = 0",
                new { userId, transactionId, code });

            if (record != null)
            {
                conn.Execute("UPDATE OtpChallenges SET IsVerified = 1 WHERE Id = @Id", new { record.Id });
                conn.Execute("UPDATE FraudTransactionSample SET VerifiedByUser = 1 WHERE TransactionId = @TransactionId", new { transactionId });
                return true;
            }

            return false;
        }
    }
}
