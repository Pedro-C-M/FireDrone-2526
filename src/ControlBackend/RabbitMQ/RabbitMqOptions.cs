namespace ControlBackend.RabbitMQ
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
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5305;
        public string Exchange { get; set; } = "drone_exchange";
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Topic { get; set; } = "drone.#";

    }

    public interface IPublisher
    {
        Task PublishAsync(string topic, byte[] body);
    }
}
