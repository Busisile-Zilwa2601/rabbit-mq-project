using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMqService
{
    public class MessageSerializer : IMessageSerializer
    {
        public byte[] Serialize<T>(T message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message is IStringMessage raw)
            {
                return Encoding.UTF8.GetBytes(raw.Name);
            }
            if (message is string str)
            {
                return Encoding.UTF8.GetBytes(str);
            }
            return JsonSerializer.SerializeToUtf8Bytes(message);
        }
    }
}
