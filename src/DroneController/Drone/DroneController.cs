using Newtonsoft.Json;
using System;
using RabbitMQ.Client;
using Microsoft.Extensions.Hosting;
using ControlBackend;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using System.Text;
using System.Diagnostics;
using System.Threading;


namespace DroneController.Drone
{
    public class DroneController : BackgroundService
    {
        string _droneID;
        string _droneDriver;

        IDroneDriver _drone;

        private readonly IConnection _connection;
        private readonly RabbitMqOptions _options;
        private IChannel _channel = null!;

        public DroneController(string droneID, string droneDriver, IConnection connection, RabbitMqOptions options)
        {
            _droneID = droneID;
            _droneDriver = droneDriver;
            _connection = connection;
            _options = options;

            Log.Debug($"Drone controller {_droneID}-{_droneDriver} starting");


            // Instanciar driver de forma dinámica
            _drone = CreateDroneDriver(_droneDriver);
        }

        // Esperar a recibir mensajes del backend a través de la cola
        // Se procesaran en HandleDroneCommand
        public void Run()
        {
            /*
			 * FALTA POR COMPLETAR
			 * *
			 */
        }

        public async void Stop()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            Log.Debug("Drone controller stopped.");
        }

        // Se instancia el driver de forma dinámica. 
        // Debe haber una clase que implemente la interfaz IDroneDriver y cuyo nombre coincida con el driver
        // en el namespace del controlador
        private IDroneDriver CreateDroneDriver(string DroneDriver)
        {
            Type type = Type.GetType(GetType().Namespace + "." + DroneDriver);
            if (type == null)
            {
                throw new ArgumentException($"Error unable to find drone driver {DroneDriver}");
            }
            IDroneDriver drone = (IDroneDriver)Activator.CreateInstance(type);

            // Sería necesario publicar la información
            drone.SetUpdateCallback(new StatusUpdateCallback(this));

            return drone;
        }

        // Crear una cola para recibir comandos del backend control
        private async Task CreateMessageQueue(string queueName, CancellationToken cancellationToken)
        {
            /*
			 * FALTA POR COMPLETAR
			 * *
			 */
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            string queueName = $"drone.{_droneID}.command";
            _channel = await _connection.CreateChannelAsync();

            // Declare the exchange first (must match ControlBackend's exchange)
            await _channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
        );

            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            // Bind to specific routing key for this drone
            string routingKey = $"drone.{_droneID}.commands";
            await _channel.QueueBindAsync(queue: queueName, exchange: _options.Exchange, routingKey: routingKey);

            Console.WriteLine($"[DroneController] Queue '{queueName}' bound to exchange '{_options.Exchange}' with routing key '{routingKey}'");

            // Set up a consumer to listen for messages on the queue.
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += (model, ea) =>
       {
           var message = Encoding.UTF8.GetString(ea.Body.ToArray());
           Console.WriteLine($"[DroneController] Received message: {message}");

           HandleDroneCommand(message);

           return Task.CompletedTask;
       };

            await _channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _channel.CloseAsync();
            await base.StopAsync(cancellationToken);
        }
        // This method is intentionally left empty because the consumer runs via event handlers.
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;


        // Enviar el estado a través de la cola para recibir al backend control
        internal void SendStatus(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: $"drone.{_droneID}.status",
                body: body
            );

            Log.Debug($"[STATUS] {message}");
        }

        // Gestión de los mensajes de comandos recibidos por el controlador
    // Si se añaden más mensajes se debería gestionar con una tabla
        private void HandleDroneCommand(string commandtext)
        {
       // Decodificar el mensaje
         DroneCommand command = JsonConvert.DeserializeObject<DroneCommand>(commandtext);

            Log.Debug($"Executing drone command {command.Command}");

            if (command.Command == DroneCommand.START_FLIGHT_PLAN_CMD)
  {
        // Decodificar los argumentos
//Waypoint[] waypoints = JsonConvert.DeserializeObject<Waypoint[]>(command.Arguments);
           Waypoint[] waypoints = //De ejemplo rellenado
				{
       new Waypoint { Latitude = 43.36, Longitude = -5.84, Altitude = 50, Speed = 20 },
    new Waypoint { Latitude = 43.361, Longitude = -5.841, Altitude = 55, Speed = 22 },
         new Waypoint { Latitude = 43.362, Longitude = -5.842, Altitude = 60, Speed = 25 },
                    new Waypoint { Latitude = 43.363, Longitude = -5.843, Altitude = 65, Speed = 20 },
     new Waypoint { Latitude = 43.364, Longitude = -5.844, Altitude = 70, Speed = 18 }
                };
                _drone.StartFlightPlan(waypoints);
     }
            else if (command.Command == DroneCommand.STOP_FLIGHT_PLAN_CMD)
            {
    _drone.StopFlightPlan();
      }
            else if (command.Command == DroneCommand.GOTO_MANUAL)
            {
     Console.WriteLine($"[DroneController] GOTO_MANUAL command received");
                
    // Parse the command message to extract lat/lng
           try
  {
         dynamic commandObj = JsonConvert.DeserializeObject<dynamic>(commandtext);
     double lat = commandObj.lat;
               double lng = commandObj.lng;
            
          Console.WriteLine($"[DroneController] Calling GoTo with Lat={lat}, Lng={lng}");
            _drone.GoTo(lat, lng);
        }
  catch (Exception ex)
       {
              Console.WriteLine($"[DroneController] Error parsing goto command: {ex.Message}");
    }
   }
          else if (command.Command == "status") // Get status command
            {
        DroneStatus status = _drone.GetStatus();

          // Codificar el estado como JSON
       var statusStr = JsonConvert.SerializeObject(status);

     SendStatus(statusStr);
            }
        }
    }
}
