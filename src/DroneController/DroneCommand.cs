namespace DroneController
{
	public class DroneCommand
	{
		public const string START_FLIGHT_PLAN_CMD = "StartFlightPlan";
		public const string STOP_FLIGHT_PLAN_CMD = "StopFlightPlan";
		public const string STATUS_CMD = "Status";

		public string Command;
		public string Arguments;
	}
}
