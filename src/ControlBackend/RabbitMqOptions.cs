namespace ControlBackend
{
    /*
     * Clase de configuración para la conexión con RabbitMQ.
     * Permite definir los parámetros del broker (host, credenciales, exchange y topic)
     * mediante variables de entorno, usando valores por defecto si no están definidas.
     * 
     * Esto facilita desplegar el sistema en distintos entornos (local, servidor, nube)
     * sin necesidad de modificar el código fuente usando variables de entorno.
     */
    public class RabbitMqOptions
    {
        public string Hostname { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        public string Username { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
        public string Password { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
        public string Exchange { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "drone_exchange";
        public string Topic { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_TOPIC") ?? "drone.#";
    }

    public interface IPublisher
    {
        Task PublishAsync(string topic, byte[] body);
    }
}
