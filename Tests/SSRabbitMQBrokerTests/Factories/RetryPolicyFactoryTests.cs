using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace SSRabbitMQBrokerTests.Factories;

public class RetryPolicyFactoryTests
{
    [Fact]
    public void RetryPolicyFactory_Create_ShouldRetry_Tests()
    {
        // Arrange
        var logger = Substitute.For<ILogger<RetryPolicyFactoryTests>>();

        // Act
        var policy = RetryPolicyFactory.Create(logger, 5);

        // Asserts
        policy.ShouldNotBeNull();
        // TODO: Test replies
    }
}
