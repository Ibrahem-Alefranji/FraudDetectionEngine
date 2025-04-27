using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using FraudDetectionEngine.Models;
using FraudDetectionEngine.Services;
using Microsoft.Extensions.Configuration;

public class RabbitMqListenerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _connectionString;
    private IConnection _connection;
    private IChannel _channel;
    private readonly ConnectionFactory _factory;

    public RabbitMqListenerService(IServiceProvider serviceProvider, IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _connectionString = config.GetConnectionString("DefaultConnection");

        _factory = new ConnectionFactory()
        {
            HostName = "localhost"  // No DispatchConsumersAsync here
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(queue: "fraud_detection_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
      
        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new AsyncEventingBasicConsumer(_channel); // async consumer

        consumer.ReceivedAsync += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var transaction = JsonConvert.DeserializeObject<TransactionData>(json);

            var fraudService = new FraudDecisionService(_connectionString);
            var result = fraudService.Decide(transaction);

            Console.WriteLine($"[FraudDetection] {transaction.UserId} - Action: {result.Action} (Score: {result.Score})");

            if (result.Action == "Challenge")
            {
                OTPService.Generate(transaction.UserId, Guid.NewGuid(), _connectionString);
            }

            //await Task.Yield(); // let the thread yield
            return Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(queue: "fraud_detection_queue", autoAck: true, consumer: consumer);

        Console.WriteLine("✅ RabbitMQ listener started.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();

        await base.StopAsync(cancellationToken);
    }
}
