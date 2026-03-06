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
            _controller.SendStatusAsync(statusJson).GetAwaiter().GetResult();
        }

        public void OnAlarm(DroneStatus status, AlarmType alarmType)
        {
            // Log the alarm
            string alarmMessage = alarmType switch
            {
                AlarmType.BatteryDepleted => "CRITICAL: Battery depleted!",
                AlarmType.LowBattery => "WARNING: Low battery!",
                _ => "Unknown alarm"
            };

            Log.Debug($"[ALARM] {alarmMessage} - Battery: {status.Battery}");

            // Create alarm notification message
            var alarmData = new
            {
                Type = "Alarm",
                AlarmType = alarmType.ToString(),
                Message = alarmMessage,
                Status = status,
                Timestamp = System.DateTime.UtcNow
            };

            string alarmJson = JsonConvert.SerializeObject(alarmData);

            // Send alarm via RabbitMQ (same channel as status, but could use dedicated alarm queue)
            _controller.SendStatusAsync(alarmJson).GetAwaiter().GetResult();
        }
    }
}
