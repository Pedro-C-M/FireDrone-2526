namespace CentralBackend;

using CentralBackend.Hubs;
using CentralBackend.Middleware;
using CentralBackend.Services;
using Microsoft.Data.Sqlite;
using Models;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<FireDrone>();

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddControllers();

        builder.Services.AddHttpClient();

        builder.Services.AddScoped<FlightPlanService>();
        builder.Services.AddScoped<DroneService>();
 
        // Agregar servicio de SignalR para drones (Singleton para mantener estado de conexiones)
        builder.Services.AddSingleton<DroneSignalRService>();

        // Agregar SignalR
        builder.Services.AddSignalR();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5305")//CAMBIAR IP AQUI
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // IMPORTANTE: Necesario para SignalR WebSocket
            });
        });
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Initialize database with seed data on startup (only if database doesn't exist or is empty)
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<FireDrone>();

                // Check if database needs initialization
                var databaseExists = db.Database.CanConnect();
                var hasDrones = databaseExists && db.Drones.Any();

                if (!hasDrones)
                {
                    Console.WriteLine("Database is empty or doesn't exist. Initializing with seed data...");
                    InstanciateBD.FormaBaseDeBD();
                    Console.WriteLine("Database initialization completed.");
                }
                else
                {
                    Console.WriteLine($"Database already initialized with {db.Drones.Count()} drones.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not initialize database: {ex.Message}");
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // ?? ErrorHandlingMiddleware AL PRINCIPIO pero ignora /droneHub
        app.UseMiddleware<ErrorHandlingMiddleware>();
 
        // CORS
        app.UseCors("AllowFrontend");
        app.UseAuthorization();
  
        // Mapear el Hub de SignalR
        app.MapHub<DroneHub>("/droneHub");
     
        // Controllers
        app.MapControllers();

        Console.WriteLine("[Program] SignalR Hub configured at /droneHub");
        Console.WriteLine("[Program] Real-time drone updates enabled");

        app.Run();
    }
}