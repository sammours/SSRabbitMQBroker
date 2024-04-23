using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace SSRabbitMQBroker
{
    public static class RetryPolicyFactory
    {
        public static Policy Create<T>(ILogger<T> logger, int retryCount)
            where T : class
                 => Policy
                    .Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .Or<ConnectFailureException>()
                    .WaitAndRetry(
                        retryCount: retryCount,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (ex, span) =>
                        {
                            logger.LogError(ex, "Connect to RabbitMQ client failed after ({TimeOut}) seconds (Exception: {exceptionMessage})", $"{span:n1}", ex.Message);
                        });
    }
}
