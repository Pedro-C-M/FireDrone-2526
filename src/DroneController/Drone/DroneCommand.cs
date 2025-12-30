using Newtonsoft.Json;

namespace DroneController.Drone
{
	public class DroneCommand
	{
		public const string START_FLIGHT_PLAN_CMD = "start";
		public const string STOP_FLIGHT_PLAN_CMD = "stop";
		public const string GOTO_MANUAL = "goto";

		[JsonProperty("command")]
		public string Command;
		
		[JsonProperty("arguments")]
		public string Arguments;
		
		[JsonProperty("droneId")]
		public int? DroneId;
	}
}
