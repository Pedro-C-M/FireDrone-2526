using System.Text.Json.Serialization;

namespace ControlBackend.DTOs
{
    public class GoToDto
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("speed")]
        public double Speed { get; set; }
    }
}
