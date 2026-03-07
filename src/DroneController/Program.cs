using ControlBackend;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DroneController
{
    /* 
	 * El controlador del dron recibe dos parámetros a través de la línea de comandos cuando se ejecuta:
	 *	- Identificador del dron (necesario para crear la cola que lo comunica con el backend)
	 *	- Driver que se usa para controlar el dron
	 */
    static class Program
    {
        // Para gestionar la terminación
        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        // Terminación del controlador: aprovechar esta función para liberar recursos
        private static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            Log.Debug("Shutting down DroneController");
            _closing.Set();
        }

        static async Task Main(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Invalid number of arguments");
            // Ejemplo: 124af46
            string droneId = args[0];

            // Ejemplo: DroneSimulator
            string droneDriver = args[1];

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var options = new RabbitMqOptions();
            config.GetSection("RabbitMq").Bind(options);
            
            // Load credentials from environment variables if available (production override)
            options.LoadFromEnvironment();

            // Crear la conexión
            var factory = new RabbitMQ.Client.ConnectionFactory
            {
                HostName = options.Hostname,
                UserName = options.Username,
                Password = options.Password
            };
            var connection = await factory.CreateConnectionAsync();

            var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(options);           // Configuración
                services.AddSingleton(connection);        // Conexión singleton
                services.AddHostedService(provider =>     // Registrar dron como BackgroundService
                    new Drone.DroneController(droneId, droneDriver, connection, options));
            }).Build();
            await host.RunAsync();
        }
    }
}
