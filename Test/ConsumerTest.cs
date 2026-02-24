using System.Text;
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
}
