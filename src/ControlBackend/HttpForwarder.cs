using Microsoft.Extensions.Configuration;
using System.Text;

namespace ControlBackend
{
    public class HttpForwarder
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HttpForwarder(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendStatusUpstreamAsync(string statusJson, string dronId)
        {
            var content = new StringContent(statusJson, Encoding.UTF8, "application/json");

            // Get CentralBackend URL from configuration
            var centralBackendBaseUrl = _configuration["CentralBackend:BaseUrl"] ?? "http://localhost:5306";
            var centralBackendUrl = $"{centralBackendBaseUrl}/api/Drone/{dronId}/status";

            Console.WriteLine($"[HttpForwarder] Sending drone {dronId} status to {centralBackendUrl}");

            var response = await _httpClient.PostAsync(centralBackendUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[HttpForwarder] Error sending HTTP: {response.StatusCode}");
            }
            else
            {
                Console.WriteLine($"[HttpForwarder] {DateTime.Now:dd-MM-yyyy HH:mm:ss} - Drone {dronId} status sent successfully");
            }
        }
    }
}
