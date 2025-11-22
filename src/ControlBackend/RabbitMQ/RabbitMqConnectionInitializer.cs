using RabbitMQ.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace ControlBackend.RabbitMQ
{
    public class RabbitMqConnectionInitializer : IHostedService
    {
        private readonly RabbitMqOptions _options;
        private readonly IServiceProvider _services;

        public RabbitMqConnectionInitializer(RabbitMqOptions options, IServiceProvider services)
        {
            _options = options;
            _services = services;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
            };

            var connection = await factory.CreateConnectionAsync();

            // Registrar IConnection dinámicamente
            var services = _services as IServiceCollection;
            if (services != null)
            {
                services.AddSingleton<IConnection>(connection);
            }

            // Alternativa más práctica: si ya tienes acceso al ServiceProvider,
            // podrías almacenar connection en un singleton propio y que Publisher/Consumer lo tomen de ahí
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
