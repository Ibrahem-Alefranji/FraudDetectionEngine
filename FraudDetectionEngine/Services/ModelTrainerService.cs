using Dapper;
using FraudDetectionEngine.Models;
using Microsoft.Data.SqlClient;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace FraudDetectionEngine.Services
{
    public class ModelTrainerService
    {
        public static List<TransactionData> LoadDataFromDatabase(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            string sql = @"
        SELECT 
            f.Amount, f.Time, f.Location, f.Device, f.TransactionType, 
            AVG(b.TransactionAmount) AS UserAvgAmount30Days,
            MAX(b.TransactionAmount) AS UserMaxAmount30Days,
            COUNT(b.Id) AS UserTransactionCount7Days,
            f.IsFraud
        FROM FraudTransactionSample f
        JOIN UserBehaviorLog b ON f.UserId = b.UserId
        WHERE f.VerifiedByUser IS NOT NULL
        AND b.Timestamp >= DATEADD(DAY, -30, GETDATE())
        GROUP BY f.Amount, f.Time, f.Location, f.Device, f.TransactionType, f.IsFraud";

            var result = connection.Query<TransactionData>(sql).ToList();
            return result;
        }

        public static void TrainModel(List<TransactionData> trainingData, string modelPath)
        {
            var mlContext = new MLContext();

            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // Call the AutoFeatureEngineering
            string[] featureColumns;
            var pipeline = AutoFeatureEngineering.BuildPipeline(mlContext, dataView, "IsFraud", out featureColumns);

            // Continue like normal
            var model = pipeline.Append(mlContext.BinaryClassification.Trainers.LightGbm(labelColumnName: "IsFraud"))
                                .Fit(dataView);

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "IsFraud");

            Console.WriteLine($"✅ Model trained successfully!");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"AUC: {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"F1 Score: {metrics.F1Score:P2}");

            mlContext.Model.Save(model, dataView.Schema, modelPath);
            Console.WriteLine($"📦 Model saved to: {modelPath}");
        }
    }
}
