namespace DroneController.Drone
{
	public interface IDroneCallback
	{
		void Update(DroneStatus status);
	}
}
