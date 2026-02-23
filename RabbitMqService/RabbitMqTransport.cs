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

        //protected TopologyOptons config;

        public RabbitMqTransport(ConnectionFactory factory)
        { 
            this.factory = factory;
            this.connection = this.factory.CreateConnectionAsync().GetAwaiter().GetResult();
            this.channel  = this.connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        public async Task Publish(string route, ReadOnlyMemory<byte> body)
        {
            await this.channel!.ExchangeDeclareAsync(route, ExchangeType.Fanout, durable: true, autoDelete: false);

            // ensure a queue exists and is bound to the exchange for this route
            var queueName = SanitizeQueueName(route);
            await this.channel!.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await this.channel!.QueueBindAsync(
                queue: queueName,
                exchange: route,
                routingKey: string.Empty,
                arguments: null);

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
        //protected RabbitMqTransport(ConnectionFactory factory, TopologyOptons config)
        //{

        //    this.config = config;
        //    this.connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        //    this.channel  = this.connection.CreateChannelAsync().GetAwaiter().GetResult();
        //    this.channel.ExchangeDeclareAsync(this.config.ExchangeName, ExchangeType.Fanout, true).GetAwaiter().GetResult();
        //    this.channel.QueueDeclareAsync(this.config.QueueName, true, false, false, null).GetAwaiter().GetResult();
        //    this.channel.QueueBindAsync(this.config.QueueName, this.config.ExchangeName, this.config.RoutingKey, null).GetAwaiter().GetResult();
        //}


        //private async Task EnsureOpen()
        //{ 
        //    this.connection = this.connection ?? await this.factory.CreateConnectionAsync();
        //    this.channel = this.channel ?? await this.connection.CreateChannelAsync();
        //}
        //public void Dispose()
        //{ 
        //    try
        //    {
        //        this.channel?.CloseAsync().GetAwaiter().GetResult();
        //        //Thread.Sleep(500);
        //        //this.channel?.Dispose();
        //        //this.connection?.CloseAsync().GetAwaiter().GetResult();
        //        //Thread.Sleep(500);
        //        //this.connection?.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[RabbitMqServiceBase] Error during dispose: {ex.Message}");
        //    }
        //    try { this.connection?.CloseAsync(); } catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        //}

        //public async Task Publish(TopologyOptons opt, ReadOnlyMemory<byte> body, CancellationToken ct)
        //{
        //    await EnsureOpen();

        //    await this.channel!.ExchangeDeclareAsync(
        //        opt.ExchangeName, 
        //        ExchangeType.Fanout, 
        //        durable:true,
        //        autoDelete: false);

        //    await this.channel!.QueueDeclareAsync(
        //        opt.QueueName, 
        //        durable: true,


        //}

        public async ValueTask DisposeAsync()
        {
            try { await this.channel?.CloseAsync(); } catch { }
            try { await this.connection?.CloseAsync(); } catch { }
            this.channel?.Dispose();
            this.connection?.Dispose();
        }
    }
}
