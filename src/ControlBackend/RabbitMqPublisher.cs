using RabbitMQ.Client;

namespace ControlBackend
{
    public class RabbitMqPublisher: IPublisher
    {
        private readonly IConnection _connection;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(IConnection connection, RabbitMqOptions options, ILogger<RabbitMqPublisher> logger)
        {
            _connection = connection;
            _options = options;
            _logger = logger;
        }

        public async Task PublishAsync(string topic, byte[] body)
        {
            try
            {
                // Cada publicación crea un canal temporal
                using var channel = await _connection.CreateChannelAsync();
                await channel.BasicPublishAsync(exchange: _options.Exchange, routingKey: topic, body: body);
                
                _logger.LogDebug("Message published to exchange {Exchange} with routing key {RoutingKey}", _options.Exchange, topic);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to publish message to exchange {_options.Exchange} with routing key {topic}", ex);
            }
        }
    }
}
