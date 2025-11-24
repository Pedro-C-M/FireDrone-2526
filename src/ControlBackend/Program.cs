
namespace ControlBackend;

public class Program
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
