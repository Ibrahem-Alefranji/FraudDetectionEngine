using FraudDetectionEngine.Models;
using FraudDetectionEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace FraudDetectionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FraudDetectionController : ControllerBase
    {
        [HttpGet("train-model")]
        public ActionResult TrainModelAsync()
        {
            var getData = ModelTrainerService.LoadDataFromDatabase("Data Source=.;Initial Catalog=Bulky;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True");
           
            ModelTrainerService.TrainModel(getData, "fraudDetectionModel.zip");


            return Ok(new { message = "train model was updated." });
        }   
        
        [HttpGet("publish-fake-transaction")]
        public async Task<ActionResult> PublishFakeTransactionsAsync()
        {
           await FakeTransactionPublisher.PublishFakeTransactions(100);


            return Ok(new { message = "publish fake transaction was sent." });
        }

        [HttpPost("predict")]
        public async Task<IActionResult> SendTestTransaction([FromBody] TransactionData transaction)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "fraud_detection_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var json = JsonConvert.SerializeObject(transaction);
            var body = Encoding.UTF8.GetBytes(json);

            // ✅ Only one publish, correct
            await channel.BasicPublishAsync(exchange: "", routingKey: "fraud_detection_queue", body: body);

            Console.WriteLine($"✅ Transaction sent: {transaction.UserId}");

            return Ok(new { message = "Transaction sent to RabbitMQ queue." });
        }
    }
}
