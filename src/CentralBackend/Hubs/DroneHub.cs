using Microsoft.AspNetCore.SignalR;

namespace CentralBackend.Hubs
{
    /// <summary>
    /// SignalR Hub para comunicación en tiempo real de posiciones de drones
    /// </summary>
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

        /// <summary>
        /// Método que el frontend puede llamar para suscribirse a actualizaciones de un dron específico
    /// </summary>
        public async Task SubscribeToDrone(int droneId)
        {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"drone_{droneId}");
       Console.WriteLine($"[DroneHub] Client {Context.ConnectionId} subscribed to drone {droneId}");
        }

        /// <summary>
        /// Método que el frontend puede llamar para desuscribirse de actualizaciones de un dron
  /// </summary>
        public async Task UnsubscribeFromDrone(int droneId)
        {
          await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"drone_{droneId}");
    Console.WriteLine($"[DroneHub] Client {Context.ConnectionId} unsubscribed from drone {droneId}");
        }

        /// <summary>
      /// Método que permite al cliente solicitar el estado actual de todos los drones
        /// </summary>
    public async Task RequestAllDroneStatus()
        {
    Console.WriteLine($"[DroneHub] Client {Context.ConnectionId} requested all drone status");
            // El servicio responderá con los datos actuales
        }
    }
}
