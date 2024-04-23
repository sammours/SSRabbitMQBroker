namespace SSRabbitMQReceiver;
public class ProgressMessageHandler : IMessageHandler<ProgressMessage>
{
    public ProgressMessageHandler()
    {
    }

    public async Task ProcessAsync(ProgressMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Title: {message.Title} IsProgressed: {message.IsProgressed}");
        await Task.Yield();
    }
}
