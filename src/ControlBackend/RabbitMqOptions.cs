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
        //public string Hostname { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        public string Hostname { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "156.35.163.122";//IP de la maquina virtual
        public string Username { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "admin";
        public string Password { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "admin";
        public string Exchange { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "drone_exchange";
        public string Topic { get; set; } = Environment.GetEnvironmentVariable("RABBITMQ_TOPIC") ?? "drone.#";
    }

    public interface IPublisher
    {
        Task PublishAsync(string topic, byte[] body);
    }
}
