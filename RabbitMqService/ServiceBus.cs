using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService
{
    public sealed class ServiceBus
    {
        private readonly ITransport _transport;
        public ServiceBus(ITransport transport)
        { 
            _transport = transport;
        }

        public Task Publish<T>(T message) where T : class
        { 
            var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(message);
            Type myType = typeof(T);
            var route = $"{myType.Namespace}:{myType.Name}";
            return _transport.Publish(route, json);
        }
    }

}
