
using System;
using Dapper;
using Microsoft.ML;
using FraudDetectionEngine.Models;
using Microsoft.Data.SqlClient;
using System.Reflection;
using System.Transactions;

namespace FraudDetectionEngine.Services
{
    public class FraudDecisionResult
    {
        public string? Action { get; set; }

        public float Score { get; set; }

        public bool IsFraud { get; set; }

        public string? Reason { get; set; }
    }

    public class FraudDecisionService
    {
        private readonly ITransformer _model; 
        private readonly MLContext _mlContext = new();
        private readonly string _connectionString; 

        public FraudDecisionService(string connectionString)
        {
            _connectionString = connectionString;
            _model = _mlContext.Model.Load("fraudDetectionModel.zip", out _); // Load the saved model file
        }

        public FraudDecisionResult Decide(TransactionTraningData tx, string transactionId)
        {
            var enriched = UserBehaviorService.Enrich(tx, _connectionString);

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TransactionTraningData, TransactionPrediction>(_model);

            var prediction = predictionEngine.Predict(enriched);

            string action;
            string reason;

            // Rule 1: High fraud probability => block the transaction
            if (prediction.Probability >= 0.9f)
            {
                action = "Block";
                reason = "High risk score";
            }
            // Rule 2: Medium probability => challenge user with OTP or verification
            else if (prediction.Probability >= 0.75f)
            {
                action = "Challenge";
                reason = "Medium risk score";
            }
            // Rule 3: Low probability => allow transaction
            else
            {
                action = "Allow";
                reason = "Low risk score";
            }

            // Log final decision in the fraud database
             LogDecision(enriched, transactionId, prediction, action);

            // Save user behavior history for future learning
             UserBehaviorService.Log(tx, transactionId, _connectionString);

            return new FraudDecisionResult
            {
                Action = action,                   
                Score = prediction.Score,        
                IsFraud = prediction.IsFraud,
                Reason = reason
            };
        }

        private void LogDecision(TransactionTraningData tx, string transactionId, TransactionPrediction prediction, string riskLevel)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Execute(@"
                INSERT INTO FraudTransactionHistory (
                    TransactionId, CardNumber, Amount, CreatedOn, Location, IPAddress, Device, TransactionType,
                    Score, IsFraud, RiskLevel, Source
                ) VALUES (
                    @TransactionId, @CardNumber, @Amount, @CreatedOn, @Location, @IPAddress, @Device, @TransactionType,
                    @Score, @IsFraud, @RiskLevel, @Source)",
                new
                {
                    TransactionId = transactionId,
                    tx.CardNumber,
                    tx.Amount,
                    tx.CreatedOn,
                    tx.IPAddress,
                    tx.Location,
                    tx.Device,
                    tx.TransactionType,
                    Score = prediction.Probability,
                    IsFraud = prediction.IsFraud,
                    RiskLevel = riskLevel,
                    Source = tx.Source
                });
        }
    }
}
