namespace ControlBackend
{
    /*
     * Clase de configuración para la conexión con RabbitMQ.
     * Los valores se cargarán desde variables de entorno primero (para producción),
     * y luego desde la sección "RabbitMQ" en appsettings (para desarrollo/CI).
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

        /// <summary>
        /// Obtiene las credenciales desde variables de entorno si están disponibles.
        /// Esto permite sobreescribir los valores de appsettings.json en producción.
        /// </summary>
        public void LoadFromEnvironment()
        {
            var envHostname = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            var envUsername = Environment.GetEnvironmentVariable("RABBITMQ_USER");
            var envPassword = GetPasswordFromEnvironment();

            if (!string.IsNullOrWhiteSpace(envHostname))
                Hostname = envHostname;
            if (!string.IsNullOrWhiteSpace(envUsername))
                Username = envUsername;
            if (!string.IsNullOrWhiteSpace(envPassword))
                Password = envPassword;
        }

        /// <summary>
        /// Obtiene la contraseña desde variables de entorno de forma segura.
        /// Compliant con recomendaciones de SonarQube para credenciales.
        /// </summary>
        private static string? GetPasswordFromEnvironment()
        {
            return Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
        }

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
