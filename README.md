# SSRabbitMQBroker

Welcome to SSRabbitMQBroker, a .NET8-based messaging broker that simplifies the process of subscribing and unsubscribing to queues, as well as sending and receiving messages through an object-oriented approach. This project is designed to provide developers with the tools necessary to implement a robust RabbitMQ broker pattern in their applications with ease.

## Features

- **Easy-to-use API**: SSRabbitMQBroker provides a straightforward and intuitive API for managing message queues.
- **OOP Design**: The broker is built with object-oriented principles in mind, ensuring that the code is modular, reusable, and easy to maintain.
- **Examples Included**: Comes with SSRabbitMQReceiver and SSRabbitMQSender examples to get you started quickly.

## Getting Started

To begin using SSRabbitMQBroker, include the SSRabbitMQBroker project in your solution. Ensure you have the .NET8 version installed on your system.

### Prerequisites

- .NET 8.0 or higher
- RabbitMQ server

### Installation

1. Clone the repository:
   ```sh
   git clone https://github.com/sammours/SSRabbitMQBroker.git
   ```
2. Include the SSRabbitMQBroker project in your solution.
3. Restore the NuGet packages:
   ```sh
   dotnet restore
   ```

## Usage

To use SSRabbitMQBroker in your project, you need to create instances of the broker, receiver, and sender classes.

### Setting up the Broker

```csharp
using var provider = new RabbitMQProvider(new RabbitMQProviderOptions<RabbitMQProvider>
{
    ClientName = "[client_name]",
    ConnectionFactory = new ConnectionFactory
    {
        Uri = new Uri("[connection_string]"),
        DispatchConsumersAsync = true
    },
    Logger = loggerFactory.CreateLogger<RabbitMQProvider>(),
    RetryCount = 5
});
```

### Subscribing to a Queue

This example create a subscriber for the message EchoMessage
```csharp
var echoSubscriber = new RabbitMQSubscriber<EchoMessage, EchoMessageHandler>(
    loggerFactory.CreateLogger<RabbitMQSubscriber<EchoMessage, EchoMessageHandler>>(),
    new()
    {
        RetryCount = 0,
        Provider = provider,
        ExpirationTime = TimeSpan.FromHours(2),
    });

// initialize the subscriber 
echoSubscriber.InitializeChannel();

// Subscribe. Here the subscriber is active and ready to receive messages
echoSubscriber.Subscribe();
```

### Unsubscribing from a Queue

```csharp
echoSubscriber.Unsubscribe();
```

### Sending a Message

To send a message you need first to create a publisher
```csharp
var publisher = new RabbitMQPublisher(
    loggerFactory.CreateLogger<RabbitMQPublisher>(),
    new()
    {
        Provider = provider,
        RetryCount = 5,
        ExpirationTime = TimeSpan.FromHours(2),
    });
publisher.InitializeChannel();
```
The publisher can send all type of messages that inherit the abstract class Message.
```csharp
publisher.Publish(new EchoMessage($"Hello, World!"));
publisher.Publish(new EchoMessage($"Another Hello, World!"));
```

### Receiving a Message

Receiving the messages happend automatically when the subscriber is on.


## Examples

The SSRabbitMQReceiver and SSRabbitMQSender projects provide clear examples of how to implement a receiver and a sender using SSRabbitMQBroker.

## Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

## License

Distributed under the MIT License. See `LICENSE` for more information.

## Contact

Samy Sammour - info@samysammour.com

Project Link: [https://github.com/sammours/SSRabbitMQBroker](https://github.com/sammours/SSRabbitMQBroker)

---

This README provides all the necessary information to get started with SSRabbitMQBroker. It includes instructions on setting up the environment, installing the broker, and examples of how to use it. The README also encourages community contributions and provides contact information for further support. <|\im_end|>
