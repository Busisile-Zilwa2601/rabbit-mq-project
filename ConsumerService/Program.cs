
using Microsoft.Extensions.DependencyInjection;
using PublisherService;
using RabbitMQ.Client;
using RabbitMqService;

namespace ConsumerService
{ 
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITransport>(sp =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                    Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var p) ? p : 5672,
                    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
                    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest",
                };
                return new RabbitMqTransport(factory) { };
            });

            services.AddSingleton<IMessageSerializer, MessageSerializer>();
            
            services.AddTransient<MessageConsumer>();
            services.AddSingleton<ServiceBus>();
            var serviceProvider = services.BuildServiceProvider();
            var serviceBus = serviceProvider.GetRequiredService<ServiceBus>();

            await serviceBus.AddConsumer<Message, MessageConsumer>();
            Console.WriteLine("Welcome to ConsumerService");
            Console.ReadLine();
        }
    }

}