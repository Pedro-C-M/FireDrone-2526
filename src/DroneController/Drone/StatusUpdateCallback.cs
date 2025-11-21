using Newtonsoft.Json;

namespace DroneController.Drone
{
    internal class StatusUpdateCallback : IDroneCallback
    {
        private readonly DroneController _controller;

        public StatusUpdateCallback(DroneController controller)
        {
            _controller = controller;
        }

        public void Update(DroneStatus status)
        {
            // Convertir a JSON
            string statusJson = JsonConvert.SerializeObject(status);

            // Enviar por RabbitMQ
            _controller.SendStatus(statusJson);
        }
    }
}
