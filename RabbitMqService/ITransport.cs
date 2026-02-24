using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService
{
    public interface ITransport
    {
        Task Publish(string route, ReadOnlyMemory<byte> body);
        Task AddConsumer(string route, Func<ReadOnlyMemory<byte>, Task> handler);
    }
}
