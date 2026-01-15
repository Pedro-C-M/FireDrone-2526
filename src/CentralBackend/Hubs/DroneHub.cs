using Microsoft.AspNetCore.SignalR;

namespace CentralBackend.Hubs
{
    /// SignalR Hub para comunicación en tiempo real de posiciones de drones
    public class DroneHub : Hub
    {
        public DroneHub()
        {
            Console.WriteLine("[DroneHub] Hub instance created");
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[DroneHub] Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"[DroneHub] Client disconnected: {Context.ConnectionId}");
            if (exception != null)
            {
                Console.WriteLine($"[DroneHub] Disconnect reason: {exception.Message}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// Método que el frontend puede llamar para suscribirse a actualizaciones de un dron específico
        public async Task SubscribeToDrone(int droneId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"drone_{droneId}");
            Console.WriteLine($"[DroneHub] Client {Context.ConnectionId} subscribed to drone {droneId}");
        }

        /// Método que el frontend puede llamar para desuscribirse de actualizaciones de un dron
        public async Task UnsubscribeFromDrone(int droneId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"drone_{droneId}");
            Console.WriteLine($"[DroneHub] Client {Context.ConnectionId} unsubscribed from drone {droneId}");
        }

        /// Método que permite al cliente solicitar el estado actual de todos los drones
        public async Task RequestAllDroneStatus()
        {
            Console.WriteLine($"[DroneHub] Client {Context.ConnectionId} requested all drone status");
        }
    }
}
