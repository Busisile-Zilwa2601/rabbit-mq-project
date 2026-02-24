using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService
{
    public interface IConsumer<T>
    {
        Task Consume(T message, CancellationToken ct);
    }
}
