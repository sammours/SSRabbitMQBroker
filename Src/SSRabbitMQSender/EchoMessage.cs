namespace SSRabbitMQSender;
public class EchoMessage(string message) : Message
{
    public string Message { get; set; } = message;
    public override string Id { get; set; } = Guid.NewGuid().ToString();
}
