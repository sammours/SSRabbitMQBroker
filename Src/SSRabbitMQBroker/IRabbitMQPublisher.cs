namespace SSRabbitMQBroker;

public interface IRabbitMQPublisher
{
    /// <summary>
    /// Publish a message
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message">The message</param>
    /// <exception cref="ArgumentNullException"></exception>
    void Publish<TMessage>(TMessage message)
        where TMessage : Message;
}
