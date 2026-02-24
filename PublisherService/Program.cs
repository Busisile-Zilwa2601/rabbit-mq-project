using RabbitMQ.Client;
using RabbitMqService;
using Microsoft.Extensions.DependencyInjection;

namespace PublisherService
{ 
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITransport>(sp =>
            {
                var factory = new ConnectionFactory
                { 
                    HostName = "localhost",
                    Port = 5672,
                    UserName = "guest",
                    Password = "guest"
                };

                return new RabbitMqTransport(factory) { };
            });

            services.AddSingleton<IMessageSerializer, MessageSerializer>();
            services.AddSingleton<ServiceBus>();
            var serviceProvider = services.BuildServiceProvider();
            var serviceBus = serviceProvider.GetRequiredService<ServiceBus>();

            Console.WriteLine("Please enter your name: ");
            var name = Console.ReadLine();
            var message = new Message(name);

            await serviceBus.Publish(message);
            Console.ReadKey();
        }
    }

}
