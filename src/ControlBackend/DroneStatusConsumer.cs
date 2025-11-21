using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ControlBackend
{
    public class DroneStatusConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly HttpForwarder _httpForwarder;
        private readonly RabbitMqOptions _options;
        private IChannel _channel = null!;

        public DroneStatusConsumer(IConnection connection, HttpForwarder httpForwarder, RabbitMqOptions options)
        {
            _connection = connection;
            _httpForwarder = httpForwarder;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string queueName = $"drone.status.receiver";

            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            // Recibirá los status de cualquier dron porque usamos drone.*.status
            await _channel.QueueBindAsync(queueName, _options.Exchange, routingKey: "drone.*.status");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine($"[BACKEND] Status recibido: {message}");

                await _httpForwarder.SendStatusUpstreamAsync(message);
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
        }
    }
}
