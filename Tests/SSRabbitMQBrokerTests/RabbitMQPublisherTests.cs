using Microsoft.Extensions.Logging;
using NSubstitute;
using RabbitMQ.Client;
using Shouldly;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SSRabbitMQBrokerTests;

public class RabbitMQPublisherTests
{
    private readonly IConnectionFactory connectionFactory = Substitute.For<IConnectionFactory>();
    private readonly IConnection connection = Substitute.For<IConnection>();
    private readonly IModel channel = Substitute.For<IModel>();
    private readonly IBasicProperties properties = Substitute.For<IBasicProperties>();
    private readonly RabbitMQPublisher service;
    private readonly RabbitMQBrokerOptions options;

    public RabbitMQPublisherTests()
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

        this.service = new RabbitMQPublisher(
            Substitute.For<ILogger<RabbitMQPublisher>>(),
            this.options);
        this.service.InitializeChannel();
    }

    [Fact]
    public void RabbitMQPublisher_Publish_Tests()
    {
        // Arrange
        var stub = new Stub
        {
            Message = "My Message"
        };
        var serialized = JsonSerializer.Serialize(stub);

        // Act
        this.service.Publish(stub);

        // Asserts
        this.channel.Received(1).BasicPublish(
            exchange: Arg.Any<string>(),
            routingKey: Arg.Any<string>(),
            basicProperties: Arg.Any<IBasicProperties>(),
            body: Arg.Any<ReadOnlyMemory<byte>>());

        this.channel.Received(1).BasicPublish(
            exchange: Arg.Is<string>(i => this.options.ExchangeName == i),
            routingKey: Arg.Is<string>(i => this.options.RoutingKey == i),
            basicProperties: Arg.Is<IBasicProperties>(i => i == this.properties),
            body: Arg.Is<ReadOnlyMemory<byte>>(i => i.Length == Encoding.UTF8.GetBytes(serialized).Length));

        this.properties.ShouldNotBeNull();
        this.properties.DeliveryMode.ShouldBe(new byte[] { 2 }.First());
        this.properties.Persistent.ShouldBeTrue();
        this.properties.Type.ShouldBe(stub.GetType().Name);
        this.properties.MessageId.ShouldBe(stub.Id);
        this.properties.CorrelationId.ShouldBe(stub.CorrelationId);
    }
}