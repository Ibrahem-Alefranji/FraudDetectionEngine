
using System;
using Microsoft.Data.SqlClient;
using Dapper;

namespace FraudDetectionEngine.Services
{
    public static class OTPService
    {
        public static string Generate(string cardNumber, string transactionId, string connectionString)
        {
            string code = new Random().Next(100000, 999999).ToString();
            DateTime expires = DateTime.Now.AddMinutes(5);

            using var conn = new SqlConnection(connectionString);
            conn.Execute(@"
                INSERT INTO OtpChallenges (CardNumber, TransactionId, Code, ExpiresAt)
                VALUES (@CardNumber, @TransactionId, @Code, @ExpiresAt)",
                new { cardNumber, transactionId, code, ExpiresAt = expires });

            return code;
        }

        public static bool Verify(string cardNumber, string transactionId, string code, string connectionString)
        {
            using var conn = new SqlConnection(connectionString);
            var record = conn.QueryFirstOrDefault(@"
                SELECT * FROM OtpChallenges 
                WHERE CardNumber = @CardNumber AND TransactionId = @TransactionId 
                AND Code = @Code AND ExpiresAt > GETDATE() AND IsVerified = 0",
                new { cardNumber, transactionId, code });

            if (record != null)
            {
                conn.Execute("UPDATE OtpChallenges SET IsVerified = 1 WHERE Id = @Id", new { record.Id });
                conn.Execute("UPDATE FraudTransactionHistory SET VerifiedByUser = 1 WHERE TransactionId = @TransactionId", new { transactionId });
                return true;
            }

            return false;
        }
    }
}
