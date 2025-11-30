namespace CentralBackend;

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
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5305")//CAMBIAR IP AQUI
                      .AllowAnyHeader()
                      .AllowAnyMethod();
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

        app.UseCors("AllowFrontend");
        app.UseAuthorization();
        app.MapControllers();
        app.UseMiddleware<ErrorHandlingMiddleware>();

        app.Run();

    }
}