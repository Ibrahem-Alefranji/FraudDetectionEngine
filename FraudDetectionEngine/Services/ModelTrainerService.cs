using Dapper;
using FraudDetectionEngine.Models;
using Microsoft.Data.SqlClient;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace FraudDetectionEngine.Services
{
    public class ModelTrainerService
    {
        /// <summary>
        /// Loads training data from the database by joining fraud transaction history with user behavior.
        /// Extracts aggregated user behavior features like average, max, and count of recent transactions.
        /// </summary>
        public static List<TransactionTraningData> LoadDataFromDatabase(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            // SQL query to join fraud transactions with user behavior log
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
            GROUP BY 
                f.Amount, f.CreatedOn, f.IPAddress, f.Device, 
                f.Location, f.TransactionType, f.IsFraud";

            // Query the database and map result to List<TransactionTraningData>
            var result = connection.Query<TransactionTraningData>(sql, commandTimeout: 1000).ToList();
            return result;
        }

        /// <summary>
        /// Trains a binary classification model using LightGbm.
        /// Input: training data loaded from database
        /// Output: ML.NET model saved to disk and printed evaluation metrics
        /// </summary>
        public static void TrainModel(List<TransactionTraningData> trainingData, string modelPath)
        {
            var mlContext = new MLContext();

            // Load data into IDataView
            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // Split the data into training (80%) and testing (20%) sets
            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Pipeline Step 1: Ensure numeric types for some fields
            var pipeline = mlContext.Transforms.Conversion.ConvertType(new[]
                {
                new InputOutputColumnPair("TransactionType"),
                new InputOutputColumnPair("UserTransactionCount7Days")
            }, outputKind: DataKind.Single)

                // Pipeline Step 2: One-hot encode categorical fields
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(new[]
                {
                new InputOutputColumnPair("Device"),
                new InputOutputColumnPair("Location"),
                new InputOutputColumnPair("IPAddress")
                }))

                // Pipeline Step 3: Combine all feature columns into a single "Features" vector
                .Append(mlContext.Transforms.Concatenate("Features",
                    "Amount", "TransactionType", "UserAvgAmount30Days",
                    "UserMaxAmount30Days", "UserTransactionCount7Days",
                    "Device", "Location", "IPAddress"))

                // Pipeline Step 4: Normalize feature values (MinMax normalization)
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))

                // Pipeline Step 5: Add LightGbm binary classification trainer
                .Append(mlContext.BinaryClassification.Trainers.LightGbm(
                    labelColumnName: "IsFraud",
                    featureColumnName: "Features"));

            // Train the model using the training set
            var model = pipeline.Fit(split.TrainSet);

            // Evaluate the model using the test set
            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "IsFraud");

            // Output evaluation results to console
            Console.WriteLine($"✅ Model trained successfully!");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"AUC: {metrics.AreaUnderRocCurve:P2}"); // Area Under ROC Curve
            Console.WriteLine($"F1 Score: {metrics.F1Score:P2}");

            // Save the trained model to a file
            mlContext.Model.Save(model, dataView.Schema, modelPath);
            Console.WriteLine($"📦 Model saved to: {modelPath}");
        }
    }

}
