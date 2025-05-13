using Dapper;
using FraudDetectionWeb.Models;
using Microsoft.Data.SqlClient;

namespace FraudDetectionWeb.Services
{
    public class SubscriptionService
    {
        private readonly IConfiguration _configuration;
        private readonly string? _connectionString;

        public SubscriptionService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public string Create(Subscription subscription)
        {
            using var conn = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Subscription (
                    FullName, BusinessName, Url, Email, Phone, Country, Region, City, Address, ClientId, ClientSecret,
                    ExpirationDate, Active, Deleted, CreatedOn, CreatedBy
                ) VALUES (
                    @FullName, @BusinessName, @Url, @Email, @Phone, @Country, @Region, @City, @Address, @ClientId, @ClientSecret,
                    @ExpirationDate, @Active, @Deleted, @CreatedOn, @CreatedBy
                )";

            subscription.Deleted = false;
            subscription.CreatedOn = DateTime.Now;
            conn.Execute(sql, new
            {
                subscription.FullName,
                subscription.BusinessName,
                subscription.Url,
                subscription.Phone,
                subscription.Email,
                subscription.Country,
                subscription.Region,
                subscription.City,
                subscription.Address,
                subscription.ClientId,
                subscription.ClientSecret,
                subscription.ExpirationDate,
                subscription.Active,
                subscription.Deleted,
                subscription.CreatedOn,
                subscription.CreatedBy
            });

            return "The Subscription Created succesfully";
        }     
        
        public string Update(Subscription subscription)
        {
            using var conn = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Subscription SET
                    FullName = @FullName, BusinessName = @BusinessName, Url = @Url, Email = @Email, Phone = @Phone,
                    Country = @Country, Region = @Region, City = @City, Address = @Address, ClientId = @ClientId,
                    ClientSecret = @ClientSecret, Active = @Active, Deleted = @Deleted
                WHERE 
                    Id = @Id";
            conn.Execute(sql, new
            {
                subscription.Id,
                subscription.FullName,
                subscription.BusinessName,
                subscription.Url,
                subscription.Phone,
                subscription.Email,
                subscription.Country,
                subscription.Region,
                subscription.City,
                subscription.Address,
                subscription.ClientId,
                subscription.ClientSecret,
                subscription.Active,
                subscription.Deleted,
            });

            return "The Subscription Uodated succesfully";
        }

        public string Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Subscription SET Deleted = @Deleted
                WHERE 
                    Id = @Id";
            conn.Execute(sql, new
            {
                Id = id,
                Deleted = true,
            });

            return "The Subscription Deleted succesfully";
        }


        public IEnumerable<Subscription> GetAll(string query, string country, string region)
        {
            using var conn = new SqlConnection(_connectionString);

            string sql = @"SELECT * FROM Subscription WHERE (FullName LIKE @query or BusinessName LIKE @query)
                    AND Deleted = @Deleted";

            if (!string.IsNullOrEmpty(country))
            {
                sql += $" AND Country = '{country}'";
			} 
            if (!string.IsNullOrEmpty(region))
            {
                sql += $" AND Region = '{region}'";
			}
            var record = conn.Query<Subscription>(sql, new
            {
				query = $"%{query}%",
				Deleted = false
            });

            return record;
        }  
        
        public Subscription? GetSingle(int id)
        {
            using var conn = new SqlConnection(_connectionString);

            string sql = @"SELECT * FROM Subscription WHERE Deleted = @Deleted and Id = @Id";
            var record = conn.QueryFirstOrDefault<Subscription>(sql, new
            {
                Id = id,
                Deleted = false
            });

            return record;
        }
    }
}
