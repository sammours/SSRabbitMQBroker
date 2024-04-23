using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace SSRabbitMQBroker;

public abstract class RabbitMQBroker(ILogger<RabbitMQBroker> logger, RabbitMQBrokerOptions options) : IDisposable
{
    protected ILogger<RabbitMQBroker> Logger { get; set; } = logger;

    protected RabbitMQBrokerOptions Options { get; set; } = options;

    protected IModel Channel { get; set; }

    public void Dispose()
    {
        this.Options?.BrokerSubscriber?.Clear();
    }

    public void InitializeChannel()
    {
        if (this.Options.Provider.IsClosed)
        {
            this.Options.Provider.TryConnect();
        }

        var queueName = this.Options.QueueName;
        var exchangeName = this.Options.ExchangeName;
        var routingKey = this.Options.RoutingKey;
        this.Logger.LogInformation("Start creating RabbitMQ Channel (exchange={exchange}, queue={queue})", exchangeName, queueName);

        try
        {
            this.Channel = this.Options.Provider.CreateModel();
            this.Logger.LogInformation("Start declaring Exchange (exchange={exchange}, durable={durable}, autoDelete={autoDelete})", exchangeName, this.Options.ExchangeDurable, this.Options.ExchangeAutoDelete);
            this.Channel.ExchangeDeclare(exchange: exchangeName, type: "direct", durable: this.Options.ExchangeDurable, autoDelete: this.Options.ExchangeAutoDelete, arguments: null);

            this.Logger.LogInformation("Exchange declared successfully");

            this.Logger.LogInformation("Start declaring Queue (queue={queue}, durable={durable}, exclusive={exclusive}, autoDelete={autoDelete})", queueName, this.Options.QueueDurable, this.Options.QueueExclusive, this.Options.QueueAutoDelete);
            this.Channel.QueueDeclare(
                queue: queueName,
                durable: this.Options.QueueDurable,
                exclusive: this.Options.QueueExclusive,
                autoDelete: this.Options.QueueAutoDelete,
                arguments: null);

            this.Channel.QueueBind(queueName, exchangeName, routingKey, null);

            this.Logger.LogInformation("RabbitMQ Channel created successfully (exchange={exchange}, queue={queue})", exchangeName, queueName);
        }
        catch (Exception ex)
        {
            this.Logger.LogError("RabbitMQ channel cannot be created (exchange={exchange}, queue={queue}) (Exception: {exception})", exchangeName, queueName, ex.Message);
            throw;
        }
    }

    protected IBasicProperties CreateBasicProperties<TMessage>(TMessage message)
        where TMessage : Message
    {
        var properties = this.Channel.CreateBasicProperties();
        properties.DeliveryMode = 2; // persistent
        properties.Persistent = true;
        properties.Type = message.GetType().Name;
        properties.MessageId = message.Id;
        properties.CorrelationId = message.CorrelationId;
        if (this.Options.ExpirationTime.HasValue)
        {
            properties.Expiration = this.Options.ExpirationTime.Value.TotalMilliseconds.ToString();
        }

        return properties;
    }
}
