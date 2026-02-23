using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMqService
{
    public sealed class ServiceBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITransport _transport;
        private readonly IMessageSerializer _serializer;
        public ServiceBus(IServiceProvider serviceProvider, ITransport transport, IMessageSerializer serializer)
        { 
            _serviceProvider = serviceProvider;
            _transport = transport;
            _serializer = serializer;
        }

        public Task Publish<T>(T message) where T : class
        { 
            var json = _serializer.Serialize(message);
            Type myType = typeof(T);
            var route = $"{myType.Namespace}:{myType.Name}";
            return _transport.Publish(route, json);
        }
    }

}
