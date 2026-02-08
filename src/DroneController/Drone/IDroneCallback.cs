namespace DroneController.Drone
{
	public interface IDroneCallback
	{
		/// Called on each status update
		void Update(DroneStatus status);

		/// Called when an alarm condition is detected (e.g., battery depleted)
		/// <param name="status">The current drone status</param>
		/// <param name="alarmType">The type of alarm triggered</param>
		void OnAlarm(DroneStatus status, AlarmType alarmType);
	}
}
