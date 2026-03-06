namespace ControlBackend
{
    /*
     * Clase de configuración para la conexión con RabbitMQ.
     * Ahora los valores se cargarán desde la sección "RabbitMQ" en appsettings.
     */
    public class RabbitMqOptions
    {
        // Estos valores se establecerán desde la configuración (appsettings.json)
        public string Hostname { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        // Valores por defecto si no están presentes en la configuración
        public string Exchange { get; set; } = "drone_exchange";
        public string Topic { get; set; } = "drone.#";

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Hostname))
                throw new InvalidOperationException("RabbitMQ:Hostname configuration is required");
            if (string.IsNullOrWhiteSpace(Username))
                throw new InvalidOperationException("RabbitMQ:Username configuration is required");
            if (string.IsNullOrWhiteSpace(Password))
                throw new InvalidOperationException("RabbitMQ:Password configuration is required");
        }
    }

    public interface IPublisher
    {
        Task PublishAsync(string topic, byte[] body);
    }
}
