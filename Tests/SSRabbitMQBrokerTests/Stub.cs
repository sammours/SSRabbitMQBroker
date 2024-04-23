namespace SSRabbitMQBrokerTests;
public class Stub : Message
{
    public string Message { get; set; }
}

public class StubHandler : IMessageHandler<Stub>
{
    public async Task ProcessAsync(Stub message, CancellationToken cancellationToken)
    {
        await Task.Yield();
    }
}
