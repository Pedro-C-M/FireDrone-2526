using Microsoft.Extensions.Configuration;
using System.Text;

namespace ControlBackend
{
    public class HttpForwarder
    {
        private readonly HttpClient _httpClient;

        public HttpForwarder(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendStatusUpstreamAsync(string statusJson, string dronId)
        {
            var content = new StringContent(statusJson, Encoding.UTF8, "application/json");
            //Console.WriteLine(content);
            var centralBackendUrl = $"http://localhost:5306/api/Drone/{dronId}/status";//CAMBIAR CENTRALIZADO
            var response = await _httpClient.PostAsync(centralBackendUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error enviando HTTP: {response.StatusCode}");
            }
            else
            {
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + " - Envío de estatus de dron desde Controller Backend");
            }
        }
    }
}
