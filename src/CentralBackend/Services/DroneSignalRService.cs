using CentralBackend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Models;

namespace CentralBackend.Services
{
    /// Servicio para enviar actualizaciones de posición de drones a través de SignalR
    public class DroneSignalRService
    {
        private readonly IHubContext<DroneHub> _hubContext;

        public DroneSignalRService(IHubContext<DroneHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// Envía la actualización de posición de un dron a todos los clientes conectados
        public async Task BroadcastDroneUpdate(Dron drone)
        {
            try
            {
                Console.WriteLine($"[DroneSignalRService] Broadcasting update for Drone {drone.Id}: Lat={drone.Lat}, Lon={drone.Lon}");

                // Enviar a todos los clientes
                await _hubContext.Clients.All.SendAsync("ReceiveDroneUpdate", new
                {
                    id = drone.Id,
                    lat = drone.Lat,
                    lon = drone.Lon,
                    altitude = drone.Altitude,
                    speed = drone.Speed,
                    battery = drone.Battery,
                    state = drone.State,
                    flightPlanId = drone.FlightPlanId,
                    timestamp = DateTime.UtcNow
                });

                // También enviar al grupo específico del dron (si hay clientes suscritos)
                await _hubContext.Clients.Group($"drone_{drone.Id}")
               .SendAsync("ReceiveSpecificDroneUpdate", new
               {
                   id = drone.Id,
                   lat = drone.Lat,
                   lon = drone.Lon,
                   altitude = drone.Altitude,
                   speed = drone.Speed,
                   battery = drone.Battery,
                   state = drone.State,
                   flightPlanId = drone.FlightPlanId,
                   timestamp = DateTime.UtcNow
               });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DroneSignalRService] Error broadcasting: {ex.Message}");
            }
        }

        /// Envía la lista completa de drones a todos los clientes
        public async Task BroadcastAllDrones(IEnumerable<Dron> drones)
        {
            try
            {
                Console.WriteLine($"[DroneSignalRService] Broadcasting {drones.Count()} drones");

                var droneData = drones.Select(d => new
                {
                    id = d.Id,
                    lat = d.Lat,
                    lon = d.Lon,
                    altitude = d.Altitude,
                    speed = d.Speed,
                    battery = d.Battery,
                    state = d.State,
                    flightPlanId = d.FlightPlanId
                });

                await _hubContext.Clients.All.SendAsync("ReceiveAllDrones", droneData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DroneSignalRService] Error broadcasting all drones: {ex.Message}");
            }
        }

        /// Notifica que un dron ha sido asignado a un plan de vuelo
        public async Task NotifyDroneAssignment(int droneId, int flightPlanId)
        {
            try
            {
                Console.WriteLine($"[DroneSignalRService] Notifying drone {droneId} assigned to flight plan {flightPlanId}");

                await _hubContext.Clients.All.SendAsync("DroneAssigned", new
                {
                    droneId,
                    flightPlanId,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DroneSignalRService] Error notifying assignment: {ex.Message}");
            }
        }

        /// Notifica cambio de estado de un dron
        public async Task NotifyDroneStateChange(int droneId, DroneState newState)
        {
            try
            {
                Console.WriteLine($"[DroneSignalRService] Notifying drone {droneId} state changed to {newState}");

                await _hubContext.Clients.All.SendAsync("DroneStateChanged", new
                {
                    droneId,
                    state = newState,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DroneSignalRService] Error notifying state change: {ex.Message}");
            }
        }
    }
}
