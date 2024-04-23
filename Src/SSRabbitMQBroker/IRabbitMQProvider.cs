using RabbitMQ.Client;

namespace SSRabbitMQBroker;
public interface IRabbitMQProvider : IDisposable
{
    bool IsOpen { get; }
    bool IsClosed { get; }
    bool TryConnect();
    IModel CreateModel();
}
