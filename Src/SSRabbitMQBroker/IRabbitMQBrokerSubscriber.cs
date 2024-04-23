namespace SSRabbitMQBroker;

/// <summary>
/// Create a new subscriber (consumer)
/// </summary>
public interface IRabbitMQBrokerSubscriber
{
    /// <summary>
    /// Subscribe a message type with a handler
    /// </summary>
    void Subscribe();

    /// <summary>
    /// Unsubscribe a message type
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    void Unsubscribe();
}
