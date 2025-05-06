using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using FraudDetectionEngine.Models; // Adjust if your model is elsewhere
using System.Collections.Generic;

public static class PublishFakeTransactions
{
    private static readonly string[] Devices = { "iOS", "Android", "Desktop", "MacOS", "Linux" };
    private static readonly string[] Locations = { "City-10", "City-22", "City-35", "City-48", "City-77" };
    private static readonly string[] Sources = { "Web", "Mobile", "API", "RabbitMQ" };
    private static readonly Random Rand = new();

    private static string GenerateFakeCardNumber()
    {
        string[] prefixes = { "4111", "5500", "6011" };
        return $"{prefixes[Rand.Next(prefixes.Length)]}-{Rand.Next(1000, 9999)}-{Rand.Next(1000, 9999)}-{Rand.Next(1000, 9999)}";
    }

    private static string GenerateRandomIP()
    {
        return $"192.168.{Rand.Next(0, 255)}.{Rand.Next(0, 255)}";
    }

    public static async Task SendAsync(int count)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("fraud_detection_queue", durable: false, exclusive: false, autoDelete: false);

        for (int i = 0; i < count; i++)
        {
            bool isFraud = Rand.NextDouble() > 0.85;

            var transaction = new
            {
                TransactionId = i + 1,
                CardNumber = GenerateFakeCardNumber(),
                Amount = isFraud ? Rand.Next(5000, 10000) : Rand.Next(10, 300),
                Location = Locations[Rand.Next(Locations.Length)],
                IPAddress = GenerateRandomIP(),
                Device = Devices[Rand.Next(Devices.Length)],
                TransactionType = Rand.Next(0, 2),
                Score = Math.Round(Rand.NextDouble(), 4),
                IsFraud = isFraud ? 1 : 0,
                RiskLevel = isFraud ? "High" : (Rand.NextDouble() > 0.5 ? "Medium" : "Low"),
                VerifiedByUser = Rand.Next(0, 2).ToString(),
                Source = Sources[Rand.Next(Sources.Length)],
                CreatedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var json = JsonConvert.SerializeObject(transaction);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync("", "fraud_detection_queue", body);

            Console.WriteLine($"📤 Sent {(isFraud ? "[FRAUD]" : "[NORMAL]")} TX: {transaction.CardNumber} - {transaction.Amount} - {transaction.Device}");
        }

        Console.WriteLine($"✅ Published {count} fake transactions.");
    }
}
