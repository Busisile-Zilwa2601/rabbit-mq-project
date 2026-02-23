using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService
{
    public interface IMessageSerializer
    {
        byte[] Serialize<T>(T Message);
    }
}
