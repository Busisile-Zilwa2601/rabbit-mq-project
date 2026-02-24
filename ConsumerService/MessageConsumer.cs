using RabbitMqService;
using PublisherService;

namespace ConsumerService
{
    public class MessageConsumer : IConsumer<Message>
    {
        public Task Consume(Message message, CancellationToken ct)
        {
            if ($"Hello my name is, {message.Name}" != message.ToString())
                throw new Exception("Message content is not correct");

            Console.WriteLine($"Hello {message.Name}, I am your father");
            return Task.CompletedTask;
        }
    }
}
