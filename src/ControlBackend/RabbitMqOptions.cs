namespace ControlBackend
{
    /*
     * Clase de configuración para la conexión con RabbitMQ.
     * Permite definir los parámetros del broker (host, credenciales, exchange y topic)
     * mediante variables de entorno, usando valores por defecto si no están definidas.
     * 
     * Esto facilita desplegar el sistema en distintos entornos (local, servidor, nube)
     * sin necesidad de modificar el código fuente usando variables de entorno.
     *
     * http://156.35.163.122:15672/ - RabbitMQ Management UI de la MV contraseña y usuario son admin admin
     */
    public class RabbitMqOptions
    {
        public string Hostname { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_HOST") 
            ?? throw new InvalidOperationException("RABBITMQ_HOST environment variable is required");
        public string Username { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_USER") 
            ?? throw new InvalidOperationException("RABBITMQ_USER environment variable is required");
        public string Password { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") 
            ?? throw new InvalidOperationException("RABBITMQ_PASSWORD environment variable is required");
        public string Exchange { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "drone_exchange";
        public string Topic { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_TOPIC") ?? "drone.#";
    }

    public interface IPublisher
    {
        Task PublishAsync(string topic, byte[] body);
    }
}
