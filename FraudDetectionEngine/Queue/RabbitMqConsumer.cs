
using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using FraudDetectionEngine.Models;
using FraudDetectionEngine.Services;

namespace FraudDetectionEngine.Queue
{
    public class RabbitMqConsumer
    {
        public static async void Start(string connectionString)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "fraud_detection_queue", durable: false, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            var fraudService = new FraudDecisionService(connectionString);

            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var transaction = JsonConvert.DeserializeObject<TransactionData>(json);

                var result = fraudService.Decide(transaction);

                Console.WriteLine($"[FraudDecision] Action: {result.Action} | Score: {result.Score} | Reason: {result.Reason}");

                if (result.Action == "Challenge")
                    OTPService.Generate(transaction.CardNumber, Guid.NewGuid(), connectionString);

                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: "fraud_detection_queue", autoAck: true, consumer: consumer);
            Console.WriteLine("Listening for transactions...");
            Console.ReadLine();
        }
    }
}
