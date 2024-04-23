using Microsoft.Extensions.Logging;
using NSubstitute;
using RabbitMQ.Client;
using Shouldly;
using Xunit;

namespace SSRabbitMQBrokerTests;

public class RabbitMQProviderTests
{
    private readonly IConnectionFactory connectionFactory;
    private readonly IConnection connection;
    private readonly RabbitMQProvider service;

    public RabbitMQProviderTests()
    {
        this.connection = Substitute.For<IConnection>();

        connection.IsOpen.Returns(true);
        this.connectionFactory = Substitute.For<IConnectionFactory>();
        this.connectionFactory.CreateConnection(Arg.Any<string>())
            .Returns(this.connection);

        this.connection.CreateModel()
            .Returns(Substitute.For<IModel>());

        this.service = new RabbitMQProvider(new RabbitMQProviderOptions<RabbitMQProvider>
        {
            Logger = Substitute.For<ILogger<RabbitMQProvider>>(),
            ConnectionFactory = this.connectionFactory,
        });
    }

    [Fact]
    public void RabbitMQProvider_TryConnect_Tests()
    {
        // Arrange

        // Act
        this.service.TryConnect();

        // Asserts
        this.service.IsOpen.ShouldBe(true);
    }

    [Fact]
    public void RabbitMQProvider_CreateModel_Tests()
    {
        // Arrange

        // Act
        this.service.TryConnect();
        var model = this.service.CreateModel();

        // Asserts
        model.ShouldNotBeNull();
    }
}