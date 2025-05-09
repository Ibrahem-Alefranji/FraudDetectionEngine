
using System;
using Dapper;
using Microsoft.ML;
using FraudDetectionEngine.Models;
using Microsoft.Data.SqlClient;

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
            _model = _mlContext.Model.Load("fraudDetectionModel.zip", out _);
        }

        public FraudDecisionResult Decide(TransactionData tx)
        {
            var enriched = UserBehaviorService.Enrich(tx, _connectionString);
            var engine = _mlContext.Model.CreatePredictionEngine<TransactionData, TransactionPrediction>(_model);
            var prediction = engine.Predict(enriched);

            string action;
            string reason;

            if (prediction.Score > 0.90f)
            {
                action = "Block";
                reason = "High risk score";
            }
            else if (prediction.Score > 0.70f)
            {
                action = "Challenge";
                reason = "Medium risk score";
            }
            else
            {
                action = "Allow";
                reason = "Low risk score";
            }


            // Store fraud result
            LogDecision(enriched, tx.TransactionId, prediction, action);

            // ALSO log user behavior
            UserBehaviorService.Log(tx, tx.TransactionId, _connectionString);

            return new FraudDecisionResult
            {
                Action = action,
                Score = prediction.Score,
                IsFraud = prediction.IsFraud,
                Reason = reason
            };
        }

        private void LogDecision(TransactionData tx, Guid transactionId, TransactionPrediction prediction, string riskLevel)
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
                    Score = prediction.Score,
                    IsFraud = prediction.IsFraud,
                    RiskLevel = riskLevel,
                    Source = tx.Source
                });
        }
    }
}
