
using RabbitMqService;
namespace PublisherService;

public class Message: IStringMessage
{
    public string Name { get; }
    public Message(string name)
    {
        Name = name;
    }
    public override string ToString() => $"Hello my name is, {Name}";
}
