using Dapper;
using FraudDetectionEngine.Models;
using Microsoft.Data.SqlClient;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace FraudDetectionEngine.Services
{
    public class ModelTrainerService
    {
        public static List<TransactionTraningData> LoadDataFromDatabase(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            string sql = @"
                    SELECT  
                        f.Amount, f.CreatedOn, f.IPAddress, f.Device, f.Location, f.TransactionType, 
                        AVG(b.TransactionAmount) AS UserAvgAmount30Days,
                        MAX(b.TransactionAmount) AS UserMaxAmount30Days,
                        COUNT(b.Id) AS UserTransactionCount7Days,
                        f.IsFraud
                    FROM FraudTransactionHistory f
                    JOIN UserBehaviorLog b ON f.CardNumber = b.CardNumber
                    WHERE f.VerifiedByUser IS NOT NULL
                    AND b.CreatedOn >= DATEADD(DAY, -30, GETDATE())
                    GROUP BY f.Amount, f.CreatedOn, f.IPAddress, f.Device, f.Location, f.TransactionType, f.IsFraud";

            var result = connection.Query<TransactionTraningData>(sql, commandTimeout: 1000).ToList();
            return result;
        }

        public static void TrainModel(List<TransactionTraningData> trainingData, string modelPath)
        {
            var mlContext = new MLContext();

            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);
            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = mlContext.Transforms.Conversion.ConvertType(new[]
                    {
                    new InputOutputColumnPair("TransactionType"),
                    new InputOutputColumnPair("UserTransactionCount7Days")
                    }, outputKind: DataKind.Single)

                .Append(mlContext.Transforms.Categorical.OneHotEncoding(new[]
                {
                    new InputOutputColumnPair("Device"),
                    new InputOutputColumnPair("Location"),
                    new InputOutputColumnPair("IPAddress")
                }))

                .Append(mlContext.Transforms.Concatenate("Features",
                    "Amount", "TransactionType", "UserAvgAmount30Days",
                    "UserMaxAmount30Days", "UserTransactionCount7Days",
                    "Device", "Location", "IPAddress"))

                .Append(mlContext.Transforms.NormalizeMinMax("Features"))

                .Append(mlContext.BinaryClassification.Trainers.LightGbm(
                    labelColumnName: "IsFraud",
                    featureColumnName: "Features"));

            var model = pipeline.Fit(split.TrainSet);

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
