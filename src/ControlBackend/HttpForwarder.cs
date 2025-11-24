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

        public async Task SendStatusUpstreamAsync(string statusJson)
        {
            var content = new StringContent(statusJson, Encoding.UTF8, "application/json");
            Console.WriteLine(content);
            /**
             * 
            var response = await _httpClient.PostAsync("https://api.central.com/drone/status", content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error enviando HTTP: {response.StatusCode}");
            }
            else
            {
                Console.WriteLine($"Estado enviado correctamente a HTTP");
            }
             */
        }
    }
}
