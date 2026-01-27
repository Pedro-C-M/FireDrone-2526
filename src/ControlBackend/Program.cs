
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

	builder.Services.AddControllers().AddJsonOptions(options =>
	{
    		options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
	});


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