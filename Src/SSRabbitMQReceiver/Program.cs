using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Critical));
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

var echoSubscriber = new RabbitMQSubscriber<EchoMessage, EchoMessageHandler>(
    loggerFactory.CreateLogger<RabbitMQSubscriber<EchoMessage, EchoMessageHandler>>(),
    new()
    {
        RetryCount = 0,
        Provider = provider,
        ExpirationTime = TimeSpan.FromHours(2),
    });
echoSubscriber.InitializeChannel();
echoSubscriber.Subscribe();

var progressSubscriber = new RabbitMQSubscriber<ProgressMessage, ProgressMessageHandler>(
    loggerFactory.CreateLogger<RabbitMQSubscriber<ProgressMessage, ProgressMessageHandler>>(),
    new()
    {
        RetryCount = 0,
        Provider = provider,
        ExpirationTime = TimeSpan.FromHours(2),
    });
progressSubscriber.InitializeChannel();
progressSubscriber.Subscribe();

await Task.Run(() =>
{
    var interval = TimeSpan.FromSeconds(5);
    Thread.Sleep(interval);
    echoSubscriber.Unsubscribe();
    Console.WriteLine("Echo Unsubscribed");
});

await Task.Run(() =>
{
    var interval = TimeSpan.FromSeconds(5);
    Thread.Sleep(interval);
    progressSubscriber.Unsubscribe();
    Console.WriteLine("Progress Unsubscribed");
});

Console.Read();
