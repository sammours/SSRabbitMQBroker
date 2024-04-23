namespace SSRabbitMQBroker;

public class RabbitMQBrokerOptions
{
    public IRabbitMQProvider Provider { get; set; }
    public string QueueName { get; set; } = $"RabbitMQBroker_Queue";
    public bool QueueDurable { get; set; } = false;
    public bool QueueExclusive { get; set; } = false;
    public bool QueueAutoDelete { get; set; } = false;
    public BrokerSubscriber BrokerSubscriber { get; set; } = new BrokerSubscriber();
    public string ExchangeName { get; set; } = $"RabbitMQBroker_Exchange";
    public string RoutingKey { get; set; } = $"RabbitMQBroker_RoutingKey";
    public bool ExchangeDurable { get; set; } = false;
    public bool ExchangeAutoDelete { get; set; } = false;
    public bool ShouldRetry => this.RetryCount > 0;
    public int RetryCount { get; set; } = 5;
    public TimeSpan? ExpirationTime { get; set; }
}
