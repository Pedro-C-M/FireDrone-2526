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
        string _droneID;
        string _droneDriver;

        IDroneDriver _drone;

        private readonly IConnection _connection;
        private readonly RabbitMqOptions _options;
        private IChannel _channel = null!;

        // Whitelist of allowed driver names to prevent reflection injection
        private static readonly System.Collections.Generic.HashSet<string> AllowedDrivers =
            new System.Collections.Generic.HashSet<string> { "DroneSimulator", "MavLinkSEUDriver" };

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
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        internal void SendStatus(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: $"drone.{_droneID}.status",
                body: body
            ).GetAwaiter().GetResult();

            Log.Debug($"[STATUS] {message}");
        }

        // Gestión de los mensajes de comandos recibidos por el controlador
        // Si se añaden más mensajes se debería gestionar con una tabla
        private void HandleDroneCommand(string commandtext)
        {
            // Decodificar el mensaje
            DroneCommand command = JsonConvert.DeserializeObject<DroneCommand>(commandtext);

            if (command == null)
            {
                Console.WriteLine("[DroneController] Received null or invalid command, ignoring.");
                return;
            }

            Log.Debug($"Executing drone command {command.Command}");

            if (command.Command == DroneCommand.START_FLIGHT_PLAN_CMD)
            {
                // Decodificar los argumentos desde el mensaje JSON completo
                try
                {
                    dynamic commandObj = JsonConvert.DeserializeObject<dynamic>(commandtext);
                    bool isPeriodic = false;
                    if (commandObj.isPeriodic != null)
                    {
                        isPeriodic = (bool)commandObj.isPeriodic;
                        Console.WriteLine($"[DroneController] Route is periodic: {isPeriodic}");
                    }
                    // Check if waypoints are provided in the message
                    if (commandObj.waypoints != null && commandObj.waypoints.Count > 0)
                    {
                        Console.WriteLine($"[DroneController] Received {commandObj.waypoints.Count} waypoints from message");
                        // Convert dynamic waypoints to Waypoint array
                        List<Waypoint> waypointList = new List<Waypoint>();
                        foreach (var wp in commandObj.waypoints)
                        {
                            double? lat = wp.Latitude ?? wp.latitude;
                            double? lon = wp.Longitude ?? wp.longitude;
                            double? alt = wp.Altitude ?? wp.altitude;
                            double? spd = wp.Speed ?? wp.speed;
                            Console.WriteLine($"[DroneController] Waypoint: lat={lat}, lon={lon}, alt={alt}, speed={spd}");
                            var waypoint = new Waypoint
                            {
                                Latitude = lat ?? 0,
                                Longitude = lon ?? 0,
                                Altitude = alt ?? 50,
                                Speed = spd ?? 20
                            };
                            waypointList.Add(waypoint);
                        }
                        Waypoint[] waypoints = waypointList.ToArray();

                        // Log first and last waypoint for verification
                        if (waypoints.Length > 0)
                        {
                            Console.WriteLine($"[DroneController] First waypoint: Lat={waypoints[0].Latitude}, Lon={waypoints[0].Longitude}");
                            if (waypoints.Length > 1)
                            {
                                Console.WriteLine($"[DroneController] Last waypoint: Lat={waypoints[waypoints.Length - 1].Latitude}, Lon={waypoints[waypoints.Length - 1].Longitude}");
                            }
                        }
                        Console.WriteLine($"[DroneController] Starting flight plan with {waypoints.Length} waypoints, isPeriodic={isPeriodic}");
                        _drone.StartFlightPlan(waypoints, isPeriodic);
                    }
                    else
                    {
                        Console.WriteLine($"[DroneController] No waypoints provided in message, using default route");
                        // Fallback to hardcoded waypoints if none provided
                        Waypoint[] waypoints = new[]
                               {
                                    new Waypoint { Latitude = 43.36, Longitude = -5.84, Altitude = 50, Speed = 20 },
                                    new Waypoint { Latitude = 43.361, Longitude = -5.841, Altitude = 55, Speed = 22 },
                                    new Waypoint { Latitude = 43.362, Longitude = -5.842, Altitude = 60, Speed = 25 },
                                    new Waypoint { Latitude = 43.363, Longitude = -5.843, Altitude = 65, Speed = 20 },
                                    new Waypoint { Latitude = 43.364, Longitude = -5.844, Altitude = 70, Speed = 18 }
                                };
                        _drone.StartFlightPlan(waypoints, isPeriodic);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DroneController] Error parsing waypoints: {ex.Message}");
                    Console.WriteLine($"[DroneController] Stack trace: {ex.StackTrace}");
                }
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

                    double speed = 20; //Velocidad por defecto
                    if (commandObj.speed != null)
                    {
                        speed = (double)commandObj.speed; //Velocidad que pasa el usuario
                    }

                    Console.WriteLine($"[DroneController] Calling GoTo with Lat={lat}, Lng={lng}, Speed={speed}");
                    _drone.GoTo(lat, lng, speed);
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