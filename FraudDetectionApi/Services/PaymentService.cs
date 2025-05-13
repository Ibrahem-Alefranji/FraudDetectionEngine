
using System;
using Microsoft.Data.SqlClient;
using Dapper;
using FraudDetectionApi.Models;
using FraudDetectionApi.Interface;
using FraudDetectionApi.DTOs;

namespace FraudDetectionApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;

        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Subscription? VerifySubscribe(string clientId, string clientSecret)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var record = conn.QueryFirstOrDefault<Subscription>(@"
                SELECT * FROM Subscription 
                WHERE ClientId = @ClientId AND ClientSecret = @clientSecret 
                AND Active = 1 AND ExpirationDate > GETDATE() AND Deleted = 0"
            , new { clientId, clientSecret });

            if (record != null)
            { 
                return record;
            }

            return null;
        }

        public PaymentTransaction Add(PaymentRequest paymentRequest)
        {
            paymentRequest.CreatedOn = DateTime.Now;
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Execute(@"
                INSERT INTO PaymentTransaction (OrderId, SubscribeId, Amount, Location, Device, IPAddress, TransactionType, Source, CreatedOn)
                VALUES (@OrderId, @SubscribeId, @Amount, @Location, @Device, @IPAddress, @TransactionType, @Source, @CreatedOn)",
                new { 
                    paymentRequest.OrderId,
                    paymentRequest.SubscribeId,
                    paymentRequest.Amount,
                    paymentRequest.Location,
                    paymentRequest.Device,
                    paymentRequest.IPAddress,
                    paymentRequest.TransactionType,
                    paymentRequest.Source,
                    paymentRequest.CreatedOn
                });

            return paymentRequest;
        }
    }
}
