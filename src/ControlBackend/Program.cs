
namespace ControlBackend;

public class Program

/**
 * PARA CUANDO SE HAGAN LOS ENDPOINTS Y LA API COMO SE USA RABBITMQ
 * 
 * [ApiController]
    [Route("[controller]")]
    public class DroneController : ControllerBase
    {
    private readonly IPublisher _publisher;

    public DroneController(IPublisher publisher)
    {
      _publisher = publisher;
    }

    [HttpGet("sendtest")]
    public async Task<IActionResult> SendTestMessage()
    {
      var message = "Hola desde backend: " + DateTime.Now;
      var body = Encoding.UTF8.GetBytes(message);

      await _publisher.PublishAsync("drone.test", body);

      return Ok("Mensaje enviado!");
    }
    }
    Así, cualquier endpoint de los controlles puede enviar mensajes a los drones sin necesidad de crear conexión o canal cada vez.
 * 
 */
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();

        //Esto inyecta en el programa el publisher de RabbitMQ 
        var rabbitOptions = new RabbitMqOptions();
        builder.Services.AddSingleton(rabbitOptions);
        builder.Services.AddRabbitMq(rabbitOptions);
        builder.Services.AddSingleton<IPublisher, RabbitMqPublisher>();

        builder.Services.AddSingleton<HttpForwarder>();
        builder.Services.AddHostedService<DroneStatusConsumer>();
        builder.Services.AddHttpClient<HttpForwarder>(); // HttpClient Registration


        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
