using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RabbitMqService;


namespace Test;

[TestClass]
public class ConsumerTest
{
    private class TestConsumer : IConsumer<PublisherService.Message>
    {
        private readonly Action _onConsume;
        public TestConsumer(Action onConsume) => _onConsume = onConsume;
        public Task Consume(PublisherService.Message message, CancellationToken ct)
        {
            _onConsume();
            return Task.CompletedTask;
        }
    }

    private class MockConsumerWrapper : IConsumer<PublisherService.Message>
    {
        private readonly IConsumer<PublisherService.Message> _inner;
        public MockConsumerWrapper(IConsumer<PublisherService.Message> inner) => _inner = inner;
        public Task Consume(PublisherService.Message message, CancellationToken ct) => _inner.Consume(message, ct);
    }

    [TestMethod]
    public async Task Consumer_InvokesRegisteredConsumer()
    {
        var transportMock = new Mock<ITransport>();
        Func<ReadOnlyMemory<byte>, Task> handler = null;
        transportMock
            .Setup(transport => transport.AddConsumer(It.IsAny<string>(), It.IsAny<Func<ReadOnlyMemory<byte>, Task>>()))
            .Returns<string, Func<ReadOnlyMemory<byte>, Task>>((r, h) =>
            {
                handler = h;
                return Task.CompletedTask;
            });

        var serializerMock = new Mock<IMessageSerializer>();

        serializerMock.Setup(s => s.Serialize(It.IsAny<object>()))
            .Returns<object>(o => Encoding.UTF8.GetBytes(o!.ToString()!));

        var consumed = false;

        var services = new ServiceCollection();
        services.AddSingleton<TestConsumer>( sp => new TestConsumer(() => consumed = true));
        var provider = services.BuildServiceProvider();

        var bus = new ServiceBus(provider, transportMock.Object, serializerMock.Object);

        await bus.AddConsumer<PublisherService.Message, TestConsumer>();

        var payload = Encoding.UTF8.GetBytes("Elizabeth");
        await handler!(new ReadOnlyMemory<byte>(payload));

        Assert.IsTrue(consumed, "Registered consumer should have been invoked");
    }

    [TestMethod]
    public async Task Consumer_DeserializesJsonPayload_AndInvokesConsumerWithCorrectName()
    {
        // Arrange
        var transportMock = new Mock<ITransport>();
        Func<ReadOnlyMemory<byte>, Task>? handler = null;
        transportMock
            .Setup(t => t.AddConsumer(It.IsAny<string>(), It.IsAny<Func<ReadOnlyMemory<byte>, Task>>()))
            .Returns<string, Func<ReadOnlyMemory<byte>, Task>>((r, h) =>
            {
                handler = h;
                return Task.CompletedTask;
            });

        var serializerMock = new Mock<IMessageSerializer>();
        serializerMock.Setup(s => s.Serialize(It.IsAny<object>()))
            .Returns<object>(o => JsonSerializer.SerializeToUtf8Bytes(o));

        PublisherService.Message? received = null;

        // consumer that records the received message
        var consumer = new Mock<IConsumer<PublisherService.Message>>();
        consumer
            .Setup(c => c.Consume(It.IsAny<PublisherService.Message>(), It.IsAny<CancellationToken>()))
            .Returns<PublisherService.Message, CancellationToken>((m, ct) =>
            {
                received = m;
                return Task.CompletedTask;
            });

        var services = new ServiceCollection();
        services.AddSingleton<MockConsumerWrapper>(mcw => new MockConsumerWrapper(consumer.Object));
        //services.AddSingleton(consumer.Object);
        var provider = services.BuildServiceProvider();

        var bus = new ServiceBus(provider, transportMock.Object, serializerMock.Object);

        // Act
        await bus.AddConsumer<PublisherService.Message, MockConsumerWrapper>();

        var payloadObj = new PublisherService.Message("S Biko");
        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payloadObj);
        await handler!(new ReadOnlyMemory<byte>(payloadBytes));

        // Assert
        Assert.IsNotNull(received);
        Assert.AreEqual("S Biko", received!.Name);
    }
}
