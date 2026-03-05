using RabbitMQ.Client;

namespace ControlBackend
{
    public static class RabbitMqExtensions
    {
        public static async Task<IServiceCollection> AddRabbitMq(this IServiceCollection services, RabbitMqOptions options)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.Hostname,
                UserName = options.Username,
                Password = options.Password,
            };

            // Una sola conexión compartida por toda la app
            var connection = await factory.CreateConnectionAsync();
            services.AddSingleton<IConnection>(connection);

            // Crear un canal temporal para declarar el exchange
            using var channel = await connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(exchange: options.Exchange, type: ExchangeType.Topic, durable: true, autoDelete: false);

            return services;
        }
    }
}

