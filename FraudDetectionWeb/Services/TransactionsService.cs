using Dapper;
using FraudDetectionWeb.DTOs;
using FraudDetectionWeb.Models;
using Microsoft.Data.SqlClient;

namespace FraudDetectionWeb.Services
{
    public class TransactionsService
	{
        private readonly IConfiguration _configuration;
        private readonly string? _connectionString;

        public TransactionsService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<TransactionsResponse> GetAll()
        {
            using var conn = new SqlConnection(_connectionString);

            string sql = @"SELECT pay.*, sub.BusinessName, CASE WHEN pay.TransactionType = 1 THEN '' ELSE '' END as TransactionTypeText
                           FROM PaymentTransaction pay, Subscription sub 
                           WHERE pay.SubscribeId = sub.Id";
            var record = conn.Query<TransactionsResponse>(sql);

            return record;
        }  
        
        public PaymentTransaction? GetSingle(int id)
        {
            using var conn = new SqlConnection(_connectionString);

            string sql = @"SELECT * FROM PaymentTransaction WHERE Id = @Id";
            var record = conn.QueryFirstOrDefault<PaymentTransaction>(sql, new
            {
                Id = id
            });

            return record;
        }


    }
}
