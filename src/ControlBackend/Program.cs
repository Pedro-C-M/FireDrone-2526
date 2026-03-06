
namespace ControlBackend;

public class Program
{
    protected Program() { }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();

        // Single AddControllers() call with JSON options configured
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

        var rabbitOptions = new RabbitMqOptions();
        builder.Services.AddSingleton(rabbitOptions);
        await builder.Services.AddRabbitMq(rabbitOptions);
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

        await app.RunAsync();
    }
}
