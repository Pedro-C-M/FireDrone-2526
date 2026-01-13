namespace DroneController.Drone
{
	public interface IDroneDriver
    {
        void StartFlightPlan(Waypoint[] plan, bool isPeriodic = false);
        void StopFlightPlan();
        void GoTo(double latitude, double longitude);

		public DroneStatus GetStatus();
		void SetUpdateCallback(IDroneCallback callback);
    }
}
