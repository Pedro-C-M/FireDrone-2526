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
        private readonly ILogger<DroneStatusConsumer> _logger;

        public DroneStatusConsumer(IConnection connection, HttpForwarder httpForwarder, RabbitMqOptions options, ILogger<DroneStatusConsumer> logger)
        {
            _connection = connection;
            _httpForwarder = httpForwarder;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const string queueName = "drone.status.receiver";
            const string routingKeyPattern = "drone.*.status";

            try
            {
                var channel = await _connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

                // Recibirá los status de cualquier dron porque usamos drone.*.status
                await channel.QueueBindAsync(queueName, _options.Exchange, routingKey: routingKeyPattern);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var routingKey = ea.RoutingKey; // drone.123.status
                        var droneNumber = routingKey.Split('.')[1]; // "123"
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                        _logger.LogDebug("Status received for drone {DroneNumber}", droneNumber);

                        await _httpForwarder.SendStatusUpstreamAsync(message, droneNumber);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing drone status message");
                    }
                };

                await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
                
                _logger.LogInformation("DroneStatusConsumer started listening on queue {QueueName}", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize DroneStatusConsumer");
                throw;
            }
        }
    }
}
