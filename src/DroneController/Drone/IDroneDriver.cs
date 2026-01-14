namespace DroneController.Drone
{
	public interface IDroneDriver
    {
        void StartFlightPlan(Waypoint[] plan);
        void StopFlightPlan();
        void GoTo(double latitude, double longitude);

		public DroneStatus GetStatus();
		void SetUpdateCallback(IDroneCallback callback);
    }
}
