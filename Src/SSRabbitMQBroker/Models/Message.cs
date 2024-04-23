using Newtonsoft.Json;

namespace SSRabbitMQBroker
{
    public abstract class Message
    {
        [JsonProperty(PropertyName = "id")]
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty(PropertyName = "correlationId")]
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty(PropertyName = "discriminator")]
        public string Discriminator => this.GetType().FullName ?? string.Empty;
    }
}
