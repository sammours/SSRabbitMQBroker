namespace SSRabbitMQReceiver;
public class EchoMessageHandler : IMessageHandler<EchoMessage>
{
    public EchoMessageHandler()
    {
    }

    public async Task ProcessAsync(EchoMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine(message.Message);
        await Task.Yield();
    }
}
