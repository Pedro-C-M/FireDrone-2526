namespace DroneController.Drone
{
	public interface IDroneDriver
    {
        void StartFlightPlan(Waypoint[] plan);
        void StopFlightPlan();

		public DroneStatus GetStatus();
		void SetUpdateCallback(IDroneCallback callback);
    }
}
