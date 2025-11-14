
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

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapControllers();

        app.MapGet("/weatherforecast", (HttpContext httpContext) =>
        {
            var forecast =  Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = summaries[Random.Shared.Next(summaries.Length)]
                })
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast")
        .WithOpenApi();

        app.Run();
    }
}
