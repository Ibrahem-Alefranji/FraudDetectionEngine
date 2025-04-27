
using FraudDetectionEngine.Queue;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Data Source=.;Initial Catalog=Bulky;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True";
        RabbitMqConsumer.Start(connectionString);
    }
}
