using System;

namespace DroneController
{
	public class ConsoleDroneUpdate: IDroneCallback
	{
		public ConsoleDroneUpdate()
		{
			System.IO.File.Delete(@"gps_coordinates.csv");
		}

		public void Update(DroneStatus status)
		{
			// Almacenar las coordenadas para visualizarlas en modo depuración
			// https://www.gpsvisualizer.com
			String coordinate = String.Format(System.Globalization.CultureInfo.InvariantCulture, $"{status.Latitude},{status.Longitude}" + Environment.NewLine);
			System.IO.File.AppendAllText(@"gps_coordinates.csv", coordinate);

			// Se impreme, pero se podría usar para publicar
			Log.Debug($"Drone Update: lat: {status.Latitude} long: {status.Longitude}, alt: {status.Altitude}, spd: {status.Speed}");
		}
	}
}
