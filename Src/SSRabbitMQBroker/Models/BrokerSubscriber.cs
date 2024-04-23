namespace SSRabbitMQBroker;

public class BrokerSubscriber
{
    private Dictionary<string, List<Type>> Subscribers { get; set; } = [];
    private Dictionary<string, Type> Messages { get; set; } = [];

    public void Add<TMessage, THandler>()
        where TMessage : Message
        where THandler : IMessageHandler<TMessage>
    {
        var messageName = typeof(TMessage).Name;
        if (this.Subscribers.ContainsKey(messageName))
        {
            if (this.Subscribers[messageName].Contains(typeof(THandler)))
            {
                return;
            }

            this.Subscribers[messageName].Add(typeof(THandler));
            return;
        }

        this.Subscribers.Add(messageName, [typeof(THandler)]);
        this.Messages.Add(messageName, typeof(TMessage));
    }

    public Type? GetMessageType(string message) => this.Messages.ContainsKey(message) ? this.Messages[message] : default;

    public List<Type> GetAll(string message) =>
        this.Subscribers.ContainsKey(message) ? this.Subscribers[message] : [];

    public bool Exists(string messageName)
    {
        return this.Messages.ContainsKey(messageName);
    }

    public void Remove<TMessage, THandler>()
        where TMessage : Message
        where THandler : IMessageHandler<TMessage>
    {
        var messageName = typeof(TMessage).Name;
        if (this.Subscribers.ContainsKey(messageName))
        {
            this.Subscribers[messageName].Remove(typeof(THandler));
            if (!this.Subscribers[messageName].Any())
            {
                this.Subscribers.Remove(messageName);
                this.Messages.Remove(messageName);
            }
        }
    }

    public void Clear() => this.Subscribers.Clear();
}
