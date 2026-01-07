using System.Collections.Generic;

namespace ControlBackend.DTOs
{
    public class StartFlightDto
    {
        public List<WaypointDto>? Waypoints { get; set; }
    }

  public class WaypointDto
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
 public double? Altitude { get; set; }
        public double? Speed { get; set; }
    }
}
