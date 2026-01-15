namespace DroneController.Drone
{
	public enum DroneState
	{
		Stopped,
		Flying,
		Landed
	}
	public class DroneStatus
	{
		public double Latitude;
		public double Longitude;
		public double Altitude;
		public double Speed; // km/h
		public double Battery;
		public DroneState State;
	}
}
