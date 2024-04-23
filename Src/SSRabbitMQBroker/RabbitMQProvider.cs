using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace SSRabbitMQBroker;
public class RabbitMQProvider : IRabbitMQProvider
{
    private readonly RabbitMQProviderOptions<RabbitMQProvider> options;
    private bool disposed;
    private readonly object safeLock = new();
    private IConnection? connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQProvider"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="connectionFactory"></param>
    /// <param name="retry"></param>
    /// <param name="clientName"></param>
    public RabbitMQProvider(RabbitMQProviderOptions<RabbitMQProvider> options)
    {
        this.options = options;
    }

    public bool IsOpen
    {
        get => this.connection?.IsOpen == true && !this.disposed;
    }

    public bool IsClosed
    {
        get => !this.IsOpen;
    }

    /// <summary>
    /// Try to connection
    /// </summary>
    /// <returns>True when connected. Otherwise; false</returns>
    public bool TryConnect()
    {
        this.options.Logger.LogInformation("Try connect RabbitMQ client");

        // Only one can create connection at a time
        lock (this.safeLock)
        {
            // Connection retry policy
            var policy = RetryPolicyFactory.Create(this.options.Logger, this.options.RetryCount);

            try
            {
                if (this.options.ShouldRetry)
                {
                    // try connect
                    policy.Execute(() => this.connection = this.options.ConnectionFactory.CreateConnection(this.options.ClientName));
                }
                else 
                {
                    this.connection = this.options.ConnectionFactory.CreateConnection(this.options.ClientName);
                }
            }
            catch (BrokerUnreachableException ex)
            {
                this.options.Logger.LogError("Connect RabbitMQ client failed. (Exception: {exceptionMessage})", ex.Message);
            }

            // TODO: Should Add Connection events ???
            // ConnectionShutdown 
            //CallbackException
            //ConnectionBlocked
            
            if (this.IsOpen)
            {
                this.options.Logger.LogInformation("RabbitMQ connected successfully");
                return true;
            }

            this.options.Logger.LogInformation("Failed to connect to RabbitMQ");
            return false;
        }
    }

    /// <summary>
    /// Create RabbitMQModel and try to connect if Connection is closed
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>IModel when successful.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public IModel CreateModel()
    {
        if (!this.IsClosed)
        {
            this.options.Logger.LogError("RabbitMQ Connection is closed. Try connecting");
            this.TryConnect();
        }

        if (this.connection is null)
        {
            throw new ArgumentNullException("RabbitMQ Connection could not be established.");
        }

        return this.connection.CreateModel();
    }

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        try
        {
            this.connection?.Close();
            this.connection?.Dispose();
        }
        catch (IOException ex)
        {
            this.options.Logger.LogCritical("Couldn't dispose connection. (Exception: {exceptionMessage})", ex.Message);
        }
    }
}
