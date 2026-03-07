using Newtonsoft.Json;

namespace DroneController.Drone
{
    public class DroneCommand
    {
        public const string START_FLIGHT_PLAN_CMD = "start";
        public const string STOP_FLIGHT_PLAN_CMD = "stop";
        public const string GOTO_MANUAL = "goto";

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("arguments")]
        public string Arguments { get; set; }

        [JsonProperty("droneId")]
        public int? DroneId { get; set; }
    }
}
