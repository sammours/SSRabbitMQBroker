using Microsoft.Extensions.Logging;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;
using Xunit;

namespace SSRabbitMQBrokerTests;

public class RabbitMQSubscriberTests
{
    private readonly IConnectionFactory connectionFactory = Substitute.For<IConnectionFactory>();
    private readonly IConnection connection = Substitute.For<IConnection>();
    private readonly IModel channel = Substitute.For<IModel>();
    private readonly IBasicProperties properties = Substitute.For<IBasicProperties>();
    private readonly RabbitMQSubscriber<Stub, StubHandler> service;
    private readonly RabbitMQBrokerOptions options;

    public RabbitMQSubscriberTests()
    {
        connection.IsOpen.Returns(true);
        this.connectionFactory.CreateConnection(Arg.Any<string>())
            .Returns(this.connection);

        this.channel.CreateBasicProperties().Returns(this.properties);
        this.connection.CreateModel()
            .Returns(this.channel);

        this.options = new RabbitMQBrokerOptions
        {
            Provider = new RabbitMQProvider(new RabbitMQProviderOptions<RabbitMQProvider>
            {
                Logger = Substitute.For<ILogger<RabbitMQProvider>>(),
                ConnectionFactory = this.connectionFactory,
                RetryCount = 0,
            })
        };

        this.service = new RabbitMQSubscriber<Stub, StubHandler>(
            Substitute.For<ILogger<RabbitMQSubscriber<Stub, StubHandler>>>(),
            this.options);
        this.service.InitializeChannel();
    }

    [Fact]
    public void RabbitMQSubscriber_Subscribe_Tests()
    {
        // Arrange

        // Act
        this.service.Subscribe();

        // Asserts

        // Fix unit testing
        //this.channel.Received(1).BasicConsume(
        //    queue: Arg.Any<string>(),
        //    autoAck: Arg.Any<bool>(),
        //    consumerTag: Arg.Any<string>(),
        //    consumer: Arg.Any<IBasicConsumer>());

        //this.channel.Received(1).BasicConsume(
        //    queue: Arg.Is<string>(i => i == this.options.QueueName),
        //    autoAck: Arg.Is<bool>(i => !i),
        //    consumer: Arg.Any<AsyncEventingBasicConsumer>());

        this.options.BrokerSubscriber.Exists("Stub").ShouldBeTrue();
    }

    [Fact]
    public void RabbitMQSubscriber_Unsubscribe_Tests()
    {
        // Arrange
        this.service.Subscribe();

        // Act
        this.service.Unsubscribe();

        // Asserts
        this.channel.Received(1).QueueUnbind(
            queue: Arg.Any<string>(),
            exchange: Arg.Any<string>(),
            routingKey: Arg.Any<string>(),
            arguments: Arg.Any<Dictionary<string, object>>());

        this.channel.Received(1).QueueUnbind(
            queue: Arg.Is<string>(i => i == this.options.QueueName),
            exchange: Arg.Is<string>(i => i == this.options.ExchangeName),
            routingKey: Arg.Is<string>(i => i == this.options.RoutingKey),
            arguments: Arg.Any<Dictionary<string, object>>());

        this.options.BrokerSubscriber.Exists("Stub").ShouldBeFalse();
    }
}