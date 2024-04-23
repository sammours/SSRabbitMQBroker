using Shouldly;
using Xunit;

namespace SSRabbitMQBrokerTests.Models;

public class BrokerSubscriberTests
{
    [Fact]
    public void BrokerSubscriber_Add_Tests()
    {
        // Arrange
        var subscriber = new BrokerSubscriber();

        // Act
        subscriber.Add<Stub, StubHandler>();
        subscriber.Add<Stub, StubHandler>();

        // Asserts
        subscriber.GetAll("Stub").Count.ShouldBe(1);
    }

    [Fact]
    public void BrokerSubscriber_Exist_Tests()
    {
        // Arrange
        var subscriber = new BrokerSubscriber();
        subscriber.Add<Stub, StubHandler>();

        // Act

        // Asserts
        subscriber.Exists("Stub").ShouldBeTrue();
        subscriber.Exists("Stub1").ShouldBeFalse();
    }

    [Fact]
    public void BrokerSubscriber_Remove_Tests()
    {
        // Arrange
        var subscriber = new BrokerSubscriber();
        subscriber.Add<Stub, StubHandler>();
        subscriber.Exists("Stub").ShouldBeTrue();

        // Act
        subscriber.Remove<Stub, StubHandler>();

        // Asserts
        subscriber.Exists("Stub").ShouldBeFalse();
    }

    [Fact]
    public void BrokerSubscriber_GetMessageType_Tests()
    {
        // Arrange
        var subscriber = new BrokerSubscriber();
        subscriber.Add<Stub, StubHandler>();

        // Act
        var type = subscriber.GetMessageType("Stub");

        // Asserts
        type.ShouldBe(typeof(Stub));
    }

    [Fact]
    public void BrokerSubscriber_Clear_Tests()
    {
        // Arrange
        var subscriber = new BrokerSubscriber();
        subscriber.Add<Stub, StubHandler>();
        subscriber.GetAll("Stub").Count.ShouldBe(1);

        // Act
        subscriber.Clear();

        // Asserts
        subscriber.GetAll("Stub").Count.ShouldBe(0);
    }
}