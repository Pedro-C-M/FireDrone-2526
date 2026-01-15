namespace CentralBackend;

using CentralBackend.Hubs;
using CentralBackend.Middleware;
using CentralBackend.Services;
using Microsoft.Data.Sqlite;
using Models;
using StackExchange.Redis;
using System.Text.Json.Serialization;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<FireDrone>();

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        builder.Services.AddHttpClient();

        builder.Services.AddScoped<FlightPlanService>();
        builder.Services.AddScoped<DroneService>();
        builder.Services.AddScoped<RouteService>();

        // Agregar servicio de SignalR para drones (Singleton para mantener estado de conexiones)
        builder.Services.AddSingleton<DroneSignalRService>();

        // Agregar SignalR
        builder.Services.AddSignalR();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
{
      policy.WithOrigins("http://localhost:5305")
   .AllowAnyHeader()
           .AllowAnyMethod()
     .AllowCredentials(); // IMPORTANTE: Necesario para SignalR WebSocket
        });
        });
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Redis Connection Configuration
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
     {
            var configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
            var options = ConfigurationOptions.Parse(configuration);
            options.AbortOnConnectFail = false; // Don't crash if Redis is unavailable
            options.ConnectTimeout = 5000;
            options.SyncTimeout = 5000;
            
            try
            {
                var redis = ConnectionMultiplexer.Connect(options);
                Console.WriteLine($"[Redis] Connected successfully to {configuration}");
                return redis;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis] WARNING: Could not connect to Redis at {configuration}: {ex.Message}");
                Console.WriteLine("[Redis] Application will continue without caching");
                throw;
            }
        });

        // Register Redis Cache Service
        builder.Services.AddSingleton<RedisCacheService>();

        var app = builder.Build();

        // Test Redis connection on startup - FIX: Don't use scope for Singleton
        try
        {
            var cache = app.Services.GetRequiredService<RedisCacheService>();
            var status = cache.GetConnectionStatus();
            Console.WriteLine($"[Redis] Cache service status: {status}");
            
            // Try a simple ping operation to verify Redis is actually working
            var testKey = "startup:test";
            var testValue = DateTime.UtcNow.ToString("O");
            await cache.SetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
            var retrieved = await cache.GetAsync<string>(testKey);
            
            if (retrieved == testValue)
            {
                Console.WriteLine("[Redis] ? Cache is WORKING - successfully tested SET/GET operations");
                await cache.RemoveAsync(testKey);
            }
            else
            {
                Console.WriteLine("[Redis] ?? Cache test failed - could not retrieve test value");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis] WARNING: Cache service not available: {ex.Message}");
        }

        //Descomentar para generar una vez luego volveer a comentar
        //InstanciateBD.FormaBaseDeBD();

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
        Console.WriteLine("[Program] Redis caching enabled for improved performance");

        app.Run();
    }
}