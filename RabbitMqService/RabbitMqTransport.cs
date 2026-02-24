using System.Data.Common;
using System.Threading.Channels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqService
{
    public class RabbitMqTransport : ITransport, IAsyncDisposable
    {
        private readonly ConnectionFactory factory;
        private IConnection? connection;
        protected readonly IChannel? channel;

        public RabbitMqTransport(ConnectionFactory factory)
        { 
            this.factory = factory;
            this.connection = this.factory.CreateConnectionAsync().GetAwaiter().GetResult();
            this.channel  = this.connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        public async Task Publish(string route, ReadOnlyMemory<byte> body)
        {
            await this.channel!.ExchangeDeclareAsync(route, ExchangeType.Fanout, durable: true, autoDelete: false);

            var props = new BasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = DeliveryModes.Persistent;

            await this.channel.BasicPublishAsync(
                exchange: route,
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: props,
                body: body.ToArray());
        }

        private static string SanitizeQueueName(string route)
        {
            if (string.IsNullOrWhiteSpace(route)) return "default-queue";
            // replace characters that are not safe in queue names
            return route.Replace(':', '-').Replace('/', '-').Replace(' ', '-');
        }


        public async Task AddConsumer(string route, Func<ReadOnlyMemory<byte>, Task> handler)
        {
            await this.channel!.ExchangeDeclareAsync(route, ExchangeType.Fanout, durable: true, autoDelete: false);

            // ensure a queue exists and is bound to the exchange for this route
            var queueName = SanitizeQueueName(route);
            await this.channel!.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false,autoDelete: false, arguments: null);
            await this.channel!.QueueBindAsync(queue: queueName,exchange: route,routingKey: string.Empty,arguments: null);

            var consumer = new AsyncEventingBasicConsumer(this.channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    await handler(ea.Body.ToArray());
                    await this.channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    await this.channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await this.channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        }

        public async ValueTask DisposeAsync()
        {
            try { await this.channel?.CloseAsync(); } catch { }
            try { await this.connection?.CloseAsync(); } catch { }
            this.channel?.Dispose();
            this.connection?.Dispose();
        }
    }
}
