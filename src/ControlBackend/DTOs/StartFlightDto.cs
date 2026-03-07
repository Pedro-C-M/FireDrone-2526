using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControlBackend.DTOs
{
    public class StartFlightDto
    {
        public List<WaypointDto>? Waypoints { get; set; }
        public bool? IsPeriodic { get; set; }
    }

    public class WaypointDto
    {
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Altitude must be a positive value")]
        public double? Altitude { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Speed must be a positive value")]
        public double? Speed { get; set; }

        public bool? IsPeriodic { get; set; }
    }
}