
using RabbitMqService;

namespace PublisherService
{
    public class Message : IStringMessage
    {
        public string Name { get; }
        public Message(string name)
        { 
            Name = name;
        }
    }
}
