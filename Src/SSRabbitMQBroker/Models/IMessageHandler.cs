namespace SSRabbitMQBroker
{
    public interface IMessageHandler<T>
        where T : Message
    {
        Task ProcessAsync(T message, CancellationToken cancellationToken);
    }
}
