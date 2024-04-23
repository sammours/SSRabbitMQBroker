using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace SSRabbitMQBroker;

public class RabbitMQPublisher : RabbitMQBroker, IRabbitMQPublisher, IDisposable
{
    public RabbitMQPublisher(ILogger<RabbitMQBroker> logger, RabbitMQBrokerOptions options)
        : base(logger, options)
    {
    }

    /// <summary>
    /// Publish a message
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message">The message</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Publish<TMessage>(TMessage message)
        where TMessage : Message
    {
        if (message is null)
        {
            throw new ArgumentNullException("The published message is null");
        }

        using (this.Logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = message.CorrelationId
        }))
        {
            if (string.IsNullOrEmpty(message.Id))
            {
                message.Id = Guid.NewGuid().ToString();
            }

            var policy = RetryPolicyFactory.Create(this.Logger, this.Options.RetryCount);

            var serialized = JsonSerializer.Serialize(message);
            this.Logger.LogInformation("Publish message: {messageName} (id={id}, CorrelationId: {correlationId}, Discriminator: {discriminator})", nameof(message), message.Id, message.CorrelationId, message.Discriminator);

            if (this.Options.ShouldRetry)
            {
                policy.Execute(() =>
                {
                    this.Channel.BasicPublish(
                        exchange: this.Options.ExchangeName,
                        routingKey: this.Options.RoutingKey,
                        basicProperties: this.CreateBasicProperties(message),
                        body: Encoding.UTF8.GetBytes(serialized));
                });
            }
            else
            {
                this.Channel.BasicPublish(
                        exchange: this.Options.ExchangeName,
                        routingKey: this.Options.RoutingKey,
                        basicProperties: this.CreateBasicProperties(message),
                        body: Encoding.UTF8.GetBytes(serialized));
            }
        }
    }
}