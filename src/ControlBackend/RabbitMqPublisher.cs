using RabbitMQ.Client;

namespace ControlBackend
{
    public class RabbitMqPublisher: IPublisher
    {
        private readonly IConnection _connection;
        private readonly RabbitMqOptions _options;

        public RabbitMqPublisher(IConnection connection, RabbitMqOptions options)
        {
            _connection = connection;
            _options = options;
        }

        public async Task PublishAsync(string topic, byte[] body)
        {
            // Cada publicación crea un canal temporal
            using var channel = await _connection.CreateChannelAsync();
            await channel.BasicPublishAsync(exchange: _options.Exchange, routingKey: topic, body: body);
        }
    }
}
