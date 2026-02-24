using System.Text;
using Moq;
using RabbitMqService;

namespace Test;

[TestClass]
public class PublishTest
{
    [TestMethod]
    public async Task Publish_SendExpectedRouteAndPayload()
    {
        var transportMok = new Mock<ITransport>();
        byte[] cBody = null;
        string cRoute = null;

        transportMok
            .Setup(transport => transport.Publish(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>()))
            .Returns<string, ReadOnlyMemory<byte>>((route, body) =>
            {
                cBody = body.ToArray();
                cRoute = route;
                return Task.CompletedTask;
            });
        
        var serializerMock = new Mock<IMessageSerializer>();

        serializerMock.Setup(s => s.Serialize(It.IsAny<object>()))
            .Returns<object>(o => Encoding.UTF8.GetBytes(o!.ToString()!));

        var sp = new Mock<IServiceProvider>().Object;
        var bus = new ServiceBus(sp, transportMok.Object, serializerMock.Object);
        var message = new PublisherService.Message("Thulani");

        bus.Publish(message);

        Assert.AreEqual(typeof(PublisherService.Message).Namespace + ":" + nameof(PublisherService.Message), cRoute);
        Assert.IsNotNull(cBody);
        StringAssert.Contains(Encoding.UTF8.GetString(cBody), "Thulani");
    }
}
