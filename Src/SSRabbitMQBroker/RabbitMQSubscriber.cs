using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace SSRabbitMQBroker;

public class RabbitMQSubscriber<TMessage, THandler> : RabbitMQBroker, IRabbitMQBrokerSubscriber, IDisposable
    where TMessage : Message
    where THandler : IMessageHandler<TMessage>
{
    public RabbitMQSubscriber(ILogger<RabbitMQSubscriber<TMessage, THandler>> logger, RabbitMQBrokerOptions options)
        : base(logger, options)
    {
    }

    /// <summary>
    /// Subscribe a message type with a handler
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    public void Subscribe()
    {
        var messageName = typeof(TMessage).Name;

        if (!this.Options.BrokerSubscriber.Exists(messageName))
        {
            var handlerName = typeof(THandler).Name;
            this.Logger.LogInformation("Start adding Subscriber: (Message: {message}, Handler: {handler}, Queue: {queue})", messageName, handlerName, this.Options.QueueName);

            this.AddBasicConsumer(messageName);
            this.Options.BrokerSubscriber.Add<TMessage, THandler>();
        }
    }

    /// <summary>
    /// Unsubscribe a message type
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    public void Unsubscribe()
    {
        var messageName = typeof(TMessage).Name;
        if (this.Options.BrokerSubscriber.Exists(messageName))
        {
            this.Logger.LogInformation("Unsubscribe: (Message: {message}, Queue: {queue})", messageName, this.Options.QueueName);

            this.Channel.BasicCancel(messageName);
            if (this.Channel.ConsumerCount(this.Options.QueueName) == 0 )
            {
                this.Channel.QueueUnbind(
                       queue: this.Options.QueueName,
                       exchange: this.Options.ExchangeName,
                       routingKey: this.Options.RoutingKey,
                       arguments: null);
            }

            this.Options.BrokerSubscriber.Remove<TMessage, THandler>();
            this.Logger.LogInformation("Consumer unsubscribed successfully");
        }
    }

    private void AddBasicConsumer(string messageName)
    {
        if (this.Channel is null)
        {
            this.Logger.LogError("Couldn't find the channel");
            return;
        }

        this.Logger.LogInformation("Start RabbitMQ Consumer (exchange={exchange}, queue={queue})", this.Options.ExchangeName, this.Options.QueueName);

        var consumer = new AsyncEventingBasicConsumer(this.Channel);
        consumer.Received += async (sender, args) =>
        {
            var cancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (await this.ProcessMessage(args, cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    this.Channel.BasicAck(args.DeliveryTag, multiple: false);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError("Cannot find RabbitMQ Message Handler (name={messageName}, id={id}) (Exception: {exception})", messageName, args.BasicProperties.MessageId, ex.Message);
                cancellationTokenSource.Cancel();
            }
        };

        this.Channel.BasicConsume(
            queue: this.Options.QueueName,
            autoAck: false,
            consumerTag: messageName,
            consumer: consumer);

        this.Logger.LogInformation("Start RabbitMQ Consumer added successfully");
    }

    private async Task<bool> ProcessMessage(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        var messageName = args.BasicProperties.Type;
        if (!this.Options.BrokerSubscriber.Exists(messageName))
        {
            return false;
        }
        this.Logger.LogInformation("Check if Message Subscriber is registered");
        var messageType = this.Options.BrokerSubscriber.GetMessageType(messageName);
        if (messageType is null)
        {
            this.Logger.LogInformation("Message Subscriber (Message: {message}) is not registered ", messageType);
            return false;
        }

        this.Logger.LogInformation("Message Subscriber is registered. Getting all handlers");
        using (this.Logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = args.BasicProperties.CorrelationId,
        }))
        {
            foreach (var handlerType in this.Options.BrokerSubscriber.GetAll(messageName))
            {
                if (handlerType is null)
                {
                    continue;
                }

                this.Logger.LogInformation("Message Handler (Handler: {handler}) was found", handlerType);

                if (JsonSerializer.Deserialize(args.Body.ToArray(), messageType) is not Message message)
                {
                    return false;
                }
                var handler = Activator.CreateInstance(handlerType);
                if (handler is null)
                {
                    continue;
                }

                var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);
                var method = concreteType.GetMethod("ProcessAsync");
                if (method is null)
                {
                    continue;
                }

                this.Logger.LogInformation("Process Method was found in the Handler. Invoking");
                await ((Task)method.Invoke(handler, new object[] { message, cancellationToken })).ConfigureAwait(false);
                this.Logger.LogInformation("Process Method was successfully processed");
            }
        }

        this.Logger.LogInformation("All Handlers were successfully processed");

        return true;
    }
}
