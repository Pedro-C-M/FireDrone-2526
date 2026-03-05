using Microsoft.Extensions.Configuration;
using System.Text;

namespace ControlBackend
{
    public class HttpForwarder
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HttpForwarder> _logger;

        public HttpForwarder(HttpClient httpClient, IConfiguration configuration, ILogger<HttpForwarder> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendStatusUpstreamAsync(string statusJson, string dronId)
        {
            var content = new StringContent(statusJson, Encoding.UTF8, "application/json");

            // Get CentralBackend URL from configuration
            var centralBackendBaseUrl = _configuration["CentralBackend:BaseUrl"];
            if (string.IsNullOrEmpty(centralBackendBaseUrl))
            {
                _logger.LogError("CentralBackend:BaseUrl configuration is missing");
                return;
            }
            
            var centralBackendUrl = $"{centralBackendBaseUrl}/api/Drone/{dronId}/status";

            _logger.LogInformation("Sending drone {DroneId} status to {Url}", dronId, centralBackendUrl);

            try
            {
                var response = await _httpClient.PostAsync(centralBackendUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error sending HTTP for drone {DroneId}: {StatusCode}", dronId, response.StatusCode);
                }
                else
                {
                    _logger.LogInformation("Drone {DroneId} status sent successfully", dronId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send status for drone {DroneId} to {Url}", dronId, centralBackendUrl);
            }
        }
    }
}
