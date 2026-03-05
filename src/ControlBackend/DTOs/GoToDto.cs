using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ControlBackend.DTOs
{
    public class GoToDto
    {
        [JsonPropertyName("latitude")]
        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }

        [JsonPropertyName("speed")]
        [Range(0, double.MaxValue, ErrorMessage = "Speed must be a positive value")]
        public double Speed { get; set; }
    }
}
