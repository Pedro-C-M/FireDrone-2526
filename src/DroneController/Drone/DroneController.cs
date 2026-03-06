using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        readonly string _droneID;

        readonly IDroneDriver _drone;

        private readonly IConnection _connection;
        private readonly RabbitMqOptions _options;
        private IChannel _channel = null!;

        // Whitelist of allowed driver names to prevent reflection injection
        private static readonly System.Collections.Generic.HashSet<string> AllowedDrivers =
            new System.Collections.Generic.HashSet<string> { "DroneSimulator", "MavLinkSEUDriver" };

        public DroneController(string droneID, string droneDriver, IConnection connection, RabbitMqOptions options)
        {
            _droneID = droneID;
            _connection = connection;
            _options = options;

            Log.Debug($"Drone controller {_droneID}-{droneDriver} starting");

            // Instanciar driver de forma dinámica
            _drone = CreateDroneDriver(droneDriver);
        }

        // Se instancia el driver de forma dinámica.
        private IDroneDriver CreateDroneDriver(string droneDriver)
        {
            // Security: validate against whitelist before using reflection (prevents injection)
            if (!AllowedDrivers.Contains(droneDriver))
            {
                throw new ArgumentException($"Unknown drone driver: {droneDriver}");
            }

            Type type = Type.GetType(GetType().Namespace + "." + droneDriver)
                ?? throw new ArgumentException($"Error unable to find drone driver {droneDriver}");

            IDroneDriver drone = (IDroneDriver)(Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Failed to create instance of {droneDriver}"));

            // Sería necesario publicar la información
            drone.SetUpdateCallback(new StatusUpdateCallback(this));

            return drone;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            string queueName = $"drone.{_droneID}.command";
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // Declare the exchange first (must match ControlBackend's exchange)
            await _channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
        );

            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);

            // Bind to specific routing key for this drone
            string routingKey = $"drone.{_droneID}.commands";
            await _channel.QueueBindAsync(queue: queueName, exchange: _options.Exchange, routingKey: routingKey, cancellationToken: cancellationToken);

            Console.WriteLine($"[DroneController] Queue '{queueName}' bound to exchange '{_options.Exchange}' with routing key '{routingKey}'");

            // Set up a consumer to listen for messages on the queue.
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine($"[DroneController] Received message: {message}");

                await HandleDroneCommand(message);
            };

            await _channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _channel.CloseAsync();
            await base.StopAsync(cancellationToken);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        internal async Task SendStatusAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: $"drone.{_droneID}.status",
                body: body
            );

            Log.Debug($"[STATUS] {message}");
        }

        // Gestión de los mensajes de comandos recibidos por el controlador
        private async Task HandleDroneCommand(string commandtext)
        {
            DroneCommand command = JsonConvert.DeserializeObject<DroneCommand>(commandtext);
            if (command == null)
            {
                Console.WriteLine("[DroneController] Received null or invalid command, ignoring.");
                return;
            }

            Log.Debug($"Executing drone command {command.Command}");

            if (command.Command == DroneCommand.START_FLIGHT_PLAN_CMD)
                HandleStartFlightPlan(commandtext);
            else if (command.Command == DroneCommand.STOP_FLIGHT_PLAN_CMD)
                _drone.StopFlightPlan();
            else if (command.Command == DroneCommand.GOTO_MANUAL)
                HandleGotoManual(commandtext);
            else if (command.Command == "status")
                await HandleStatusCommand();
        }

        private void HandleStartFlightPlan(string commandtext)
        {
            try
            {
                dynamic commandObj = JsonConvert.DeserializeObject<dynamic>(commandtext);
                bool isPeriodic = ParseIsPeriodic(commandObj);
                Waypoint[] waypoints = ParseWaypoints(commandObj);
                Console.WriteLine($"[DroneController] Starting flight plan with {waypoints.Length} waypoints, isPeriodic={isPeriodic}");
                _drone.StartFlightPlan(waypoints, isPeriodic);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DroneController] Error parsing waypoints: {ex.Message}");
                Console.WriteLine($"[DroneController] Stack trace: {ex.StackTrace}");
            }
        }

        private void HandleGotoManual(string commandtext)
        {
            Console.WriteLine("[DroneController] GOTO_MANUAL command received");
            try
            {
                dynamic commandObj = JsonConvert.DeserializeObject<dynamic>(commandtext);
                double lat = commandObj.lat;
                double lng = commandObj.lng;
                double speed = commandObj.speed != null ? (double)commandObj.speed : 20;
                Console.WriteLine($"[DroneController] Calling GoTo with Lat={lat}, Lng={lng}, Speed={speed}");
                _drone.GoTo(lat, lng, speed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DroneController] Error parsing goto command: {ex.Message}");
            }
        }

        private async Task HandleStatusCommand()
        {
            DroneStatus status = _drone.GetStatus();
            await SendStatusAsync(JsonConvert.SerializeObject(status));
        }

        private static bool ParseIsPeriodic(dynamic commandObj)
        {
            if (commandObj.isPeriodic == null) return false;
            bool isPeriodic = (bool)commandObj.isPeriodic;
            Console.WriteLine($"[DroneController] Route is periodic: {isPeriodic}");
            return isPeriodic;
        }

        private static Waypoint[] ParseWaypoints(dynamic commandObj)
        {
            if (commandObj.waypoints == null || commandObj.waypoints.Count == 0)
            {
                Console.WriteLine("[DroneController] No waypoints provided in message, using default route");
                return DefaultWaypoints();
            }

            Console.WriteLine($"[DroneController] Received {commandObj.waypoints.Count} waypoints from message");
            var waypointList = new List<Waypoint>();
            foreach (var wp in commandObj.waypoints)
            {
                double? lat = wp.Latitude ?? wp.latitude;
                double? lon = wp.Longitude ?? wp.longitude;
                double? alt = wp.Altitude ?? wp.altitude;
                double? spd = wp.Speed ?? wp.speed;
                Console.WriteLine($"[DroneController] Waypoint: lat={lat}, lon={lon}, alt={alt}, speed={spd}");
                waypointList.Add(new Waypoint { Latitude = lat ?? 0, Longitude = lon ?? 0, Altitude = alt ?? 50, Speed = spd ?? 20 });
            }

            Waypoint[] waypoints = waypointList.ToArray();
            LogWaypointBounds(waypoints);
            return waypoints;
        }

        private static void LogWaypointBounds(Waypoint[] waypoints)
        {
            if (waypoints.Length == 0) return;
            Console.WriteLine($"[DroneController] First waypoint: Lat={waypoints[0].Latitude}, Lon={waypoints[0].Longitude}");
            if (waypoints.Length > 1)
                Console.WriteLine($"[DroneController] Last waypoint: Lat={waypoints[waypoints.Length - 1].Latitude}, Lon={waypoints[waypoints.Length - 1].Longitude}");
        }

        private static Waypoint[] DefaultWaypoints() => new[]
        {
            new Waypoint { Latitude = 43.36,  Longitude = -5.84,  Altitude = 50, Speed = 20 },
            new Waypoint { Latitude = 43.361, Longitude = -5.841, Altitude = 55, Speed = 22 },
            new Waypoint { Latitude = 43.362, Longitude = -5.842, Altitude = 60, Speed = 25 },
            new Waypoint { Latitude = 43.363, Longitude = -5.843, Altitude = 65, Speed = 20 },
            new Waypoint { Latitude = 43.364, Longitude = -5.844, Altitude = 70, Speed = 18 }
        };
    }
}