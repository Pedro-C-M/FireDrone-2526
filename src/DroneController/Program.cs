using ControlBackend;
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
    class Program
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
            string DroneID = args[0];

            // Ejemplo: DroneSimulator
            string DroneDriver = args[1];

            var options = new RabbitMqOptions();

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
                                    new Drone.DroneController(DroneID, DroneDriver, connection, options));
                            }).Build();
            await host.RunAsync();
        }
    }
}
