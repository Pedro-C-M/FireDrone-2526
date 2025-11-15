using RabbitMQ.Client;

namespace ControlBackend
{
    public static class RabbitMqExtensions
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, RabbitMqOptions options)
        {
            // Register options
            services.AddSingleton(options);

            // Register connection factory as singleton
            services.AddSingleton<IConnection>(sp =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = options.Hostname,
                    UserName = options.Username,
                    Password = options.Password,
                };

                try
                {
                    Console.WriteLine($"[RabbitMQ] Attempting to connect to {options.Hostname}...");

                    // Create connection - using GetAwaiter().GetResult() is acceptable in factory pattern
                    var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();

                    Console.WriteLine($"[RabbitMQ] Successfully connected to {options.Hostname}");

                    // Declare exchange
                    var channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
                    try
                    {
                        channel.ExchangeDeclareAsync(
                            exchange: options.Exchange,
                            type: ExchangeType.Topic,
                            durable: true,
                            autoDelete: false
                        ).GetAwaiter().GetResult();

                        Console.WriteLine($"[RabbitMQ] Exchange '{options.Exchange}' declared successfully");
                    }
                    finally
                    {
                        channel.CloseAsync().GetAwaiter().GetResult();
                        channel.Dispose();
                    }

                    return connection;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMQ] ERROR: Failed to connect to RabbitMQ at {options.Hostname}");
                    Console.WriteLine($"[RabbitMQ] Error details: {ex.Message}");
                    Console.WriteLine($"[RabbitMQ] Make sure RabbitMQ is running. You can start it with:");
                    Console.WriteLine($"[RabbitMQ]   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management");
                    throw;
                }
            });

            return services;
        }
    }
}

