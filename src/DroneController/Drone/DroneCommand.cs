namespace DroneController.Drone
{
	public class DroneCommand
	{
		public const string START_FLIGHT_PLAN_CMD = "start";
		public const string STOP_FLIGHT_PLAN_CMD = "stop";
		public const string GOTO_MANUAL = "goto";

		public string Command;
		public string Arguments;
	}
}
