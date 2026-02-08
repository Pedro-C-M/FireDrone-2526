using System;

namespace DroneController.Drone
{
    public class ConsoleDroneUpdate : IDroneCallback
    {
        public ConsoleDroneUpdate()
        {
            System.IO.File.Delete(@"gps_coordinates.csv");
        }

        public void Update(DroneStatus status)
        {
            // Almacenar las coordenadas para visualizarlas en modo depuración
            // https://www.gpsvisualizer.com
            string coordinate = string.Format(System.Globalization.CultureInfo.InvariantCulture, $"{status.Latitude},{status.Longitude}" + Environment.NewLine);
            System.IO.File.AppendAllText(@"gps_coordinates.csv", coordinate);

            // Se impreme, pero se podría usar para publicar
            Log.Debug($"Drone Update: lat: {status.Latitude} long: {status.Longitude}, alt: {status.Altitude}, spd: {status.Speed}");
        }

        public void OnAlarm(DroneStatus status, AlarmType alarmType)
        {
            string alarmMessage = alarmType switch
            {
                AlarmType.BatteryDepleted => "CRITICAL: Battery depleted!",
                AlarmType.LowBattery => "WARNING: Low battery!",
                _ => "Unknown alarm"
            };

            Log.Debug($"[ALARM] {alarmMessage} - Battery: {status.Battery}, Position: ({status.Latitude}, {status.Longitude})");
        }
    }
}
