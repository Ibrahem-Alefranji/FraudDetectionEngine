
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

    // This service evaluates a transaction and returns a fraud decision based on a trained ML.NET model.
    public class FraudDecisionService
    {
        private readonly ITransformer _model; // The trained ML model loaded from disk
        private readonly MLContext _mlContext = new(); // ML.NET context for predictions
        private readonly string _connectionString; // Used to query user behavior from the database

        // Constructor: loads the model and sets up the DB connection string
        public FraudDecisionService(string connectionString)
        {
            _connectionString = connectionString;
            _model = _mlContext.Model.Load("fraudDetectionModel.zip", out _); // Load the saved model file
        }

        // Main method: takes a transaction and returns a fraud decision
        public FraudDecisionResult Decide(TransactionTraningData tx, string transactionId)
        {
            // Enrich the transaction with behavior metrics (avg amount, max, etc.) from DB
            var enriched = UserBehaviorService.Enrich(tx, _connectionString);

            // Create a prediction engine to evaluate a single transaction
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TransactionTraningData, TransactionPrediction>(_model);

            // Predict fraud probability and class label
            var prediction = predictionEngine.Predict(enriched);

            // Decision variables
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

            // Optional: Log final decision in the fraud database
            // LogDecision(enriched, transactionId, prediction, action);

            // Optional: Save user behavior history for future learning
            // UserBehaviorService.Log(tx, transactionId, _connectionString);

            // Return structured fraud decision result
            return new FraudDecisionResult
            {
                Action = action,                   // What action to take: Allow, Challenge, Block
                Score = prediction.Score,         // Raw score (can be negative or positive margin)
                IsFraud = prediction.IsFraud,     // Predicted label (true or false)
                Reason = reason                   // Explanation for the action
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
