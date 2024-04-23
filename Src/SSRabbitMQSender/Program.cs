using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SSRabbitMQSender;

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
using var provider = new RabbitMQProvider(new RabbitMQProviderOptions<RabbitMQProvider>
{
    ClientName = "client",
    ConnectionFactory = new ConnectionFactory
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672"),
        DispatchConsumersAsync = true
    },
    Logger = loggerFactory.CreateLogger<RabbitMQProvider>(),
    RetryCount = 5
});

var publisher = new RabbitMQPublisher(
    loggerFactory.CreateLogger<RabbitMQPublisher>(),
    new()
    {
        Provider = provider,
        RetryCount = 5,
        ExpirationTime = TimeSpan.FromHours(2),
    });
publisher.InitializeChannel();

do
{
    for (int i = 0; i < 100; i++)
    {
        if (i % 2 == 0)
        {
            publisher.Publish(new EchoMessage($"Hello, World!"));
        }
        else
        {
            publisher.Publish(new ProgressMessage($"Title {i}", i % 5 == 0));
        }
    }
}
while (Console.ReadLine() != "close");
