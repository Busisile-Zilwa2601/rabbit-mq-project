using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMqService
{
    public sealed class ServiceBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITransport _transport;
        private readonly IMessageSerializer _serializer;

        private readonly ConcurrentDictionary<string, List<Type>> _consumerMap = new();
        private readonly ConcurrentDictionary<string, object?> _routeInitGuards = new();
        public ServiceBus(IServiceProvider serviceProvider, ITransport transport, IMessageSerializer serializer)
        {
            _serviceProvider = serviceProvider;
            _transport = transport;
            _serializer = serializer;
        }

        public Task Publish<T>(T message) where T : class
        {
            var json = _serializer.Serialize(message);
            Type myType = typeof(T);
            var route = $"{myType.Namespace}:{myType.Name}";
            return _transport.Publish(route, json);
        }

        public async Task AddConsumer<TMessage, TConsumer>() where TMessage : class where TConsumer : class, IConsumer<TMessage>
        {
            Type myType = typeof(TMessage);
            var route = $"{myType.Namespace}:{myType.Name}";
            _consumerMap.AddOrUpdate(route,
                _ => new List<Type> { typeof(TConsumer) },
                (_, list) =>
                {
                    lock (list)
                    {
                        if (!list.Contains(typeof(TConsumer)))
                            list.Add(typeof(TConsumer));
                    }
                    return list;
                });

            if (_routeInitGuards.TryAdd(route, null))
            {
                await _transport.AddConsumer(route, async body => {
                    if (!_consumerMap.TryGetValue(route, out var types) || types.Count == 0) return;
                    List<Type> snapshot;
                    lock (types) snapshot = types.ToList();

                    var payload = Encoding.UTF8.GetString(body.Span);
                    TMessage mesg;
                    try
                    {
                        // Try JSON first (matches publisher when JSON is used)
                        if (!string.IsNullOrWhiteSpace(payload) && (payload.StartsWith('{') || payload.StartsWith('[')))
                        {
                            mesg = System.Text.Json.JsonSerializer.Deserialize<TMessage>(payload)!;
                        }
                        else
                        {
                            // fallback: if TMessage has a constructor that accepts a single string, use it
                            var ctor = typeof(TMessage).GetConstructor(new[] { typeof(string) });
                            if (ctor != null)
                            {
                                mesg = (TMessage)ctor.Invoke(new object[] { payload })!;
                            }
                            else
                            {
                                mesg = System.Text.Json.JsonSerializer.Deserialize<TMessage>(payload)!;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to deserialize message for route {route}: {ex.Message}");
                        throw;
                    }
                    using var scope = _serviceProvider.CreateScope();

                    foreach (var consumeType in snapshot)
                    { 
                        var consumer = (IConsumer<TMessage>)scope.ServiceProvider.GetRequiredService(consumeType);
                        await consumer.Consume(mesg, CancellationToken.None);
                    }
                });
            }

        }
    }

}
