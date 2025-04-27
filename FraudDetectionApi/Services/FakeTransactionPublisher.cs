using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using FraudDetectionEngine.Models; // Adjust namespace if needed

public class FakeTransactionPublisher
{
    public static async Task PublishFakeTransactions(int numberOfTransactions)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: "fraud_detection_queue", durable: false, exclusive: false, autoDelete: false);

        var rand = new Random();

        for (int i = 0; i < numberOfTransactions; i++)
        {
            var isFraudulent = rand.NextDouble() < 0.15; // ~15% are fraud-like

            var transaction = new TransactionData
            {
                UserId = rand.Next(1, 200),
                Amount = isFraudulent
                    ? (float)rand.Next(5000, 15000) // Fraud = high amount
                    : (float)rand.Next(10, 300),
                Time = (float)rand.Next(0, 86400),
                Location = isFraudulent
                    ? rand.Next(70, 100) // Fraud = new location
                    : rand.Next(1, 30),
                Device = isFraudulent
                    ? rand.Next(10, 20) // Fraud = new device
                    : rand.Next(1, 5),
                TransactionType = rand.Next(0, 2),
                UserAvgAmount30Days = (float)rand.Next(50, 200),
                UserMaxAmount30Days = (float)rand.Next(200, 500),
                UserTransactionCount7Days = rand.Next(5, 20)
            };

            var json = JsonConvert.SerializeObject(transaction);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(exchange: "", routingKey: "fraud_detection_queue", body: body);

            Console.WriteLine($"📤 Published {(isFraudulent ? "[FRAUD]" : "[NORMAL]")} Transaction: User {transaction.UserId} - Amount: {transaction.Amount}");
        }

        Console.WriteLine($"✅ Finished sending {numberOfTransactions} fake transactions.");
    }
}
