using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace SSRabbitMQBroker;
public class RabbitMQProviderOptions<T>
    where T : IRabbitMQProvider
{
    public ILogger<T> Logger { get; set; }
    public IConnectionFactory ConnectionFactory { get; set; }
    public bool ShouldRetry => this.RetryCount > 0;
    public int RetryCount { get; set; } = 5;
    public string ClientName { get; set; } = null;
}
