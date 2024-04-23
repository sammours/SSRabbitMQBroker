namespace SSRabbitMQReceiver;
public class ProgressMessage(string title, bool isProgressed) : Message
{
    public string Title { get; set; } = title;
    public bool IsProgressed { get; set; } = isProgressed;
}
