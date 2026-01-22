using Microsoft.EntityFrameworkCore;
using Models;

/**INICIAR BD
 * 
dotnet restore
dotnet build
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
 * 
 * Solo necesario una vez:
dotnet tool install --global dotnet-ef

Para poder copiarlo más rápido:
*
dotnet restore
dotnet build
dotnet ef migrations add InitialCreate
dotnet ef database update
*
 */
namespace CentralBackend;
public class FireDrone : DbContext
{
    public DbSet<BaseStation> BaseStations { get; set; }
    public DbSet<ControlStation> ControlStations { get; set; }
    public DbSet<Dron> Drones { get; set; }
    public DbSet<DronCharacteristics> DronCharacteristics { get; set; }
    public DbSet<FlightPlan> FlightPlans { get; set; }
    public DbSet<Incidence> Incidences { get; set; }
    public DbSet<Perimeter> Perimeters { get; set; }
    public DbSet<Models.Route> Routes { get; set; }
    public DbSet<RoutePoint> RoutePoints { get; set; }
    public DbSet<Sample> Samples { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<ChangeMode> ChangeModes { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder options)
    => options.UseSqlite("Data Source=../../FireDrone-2526-3.db");//Ruta relativa normal, si se va a ejecutar desde tests, poner vuestra ruta fija, siempre dejar esta de nuevo
    //=> options.UseSqlite("Data Source=C:\\Users\\Pedro\\source\\repos\\FireDrone-2526-3\\FireDrone-2526-3.db");//Ruta fija Pedro

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the relationship between Dron and FlightPlan
        modelBuilder.Entity<Dron>()
            .HasOne(d => d.Actual)
            .WithOne(fp => fp.Dron)
            .HasForeignKey<FlightPlan>(fp => fp.DronId);

        // Configure ChangeMode as owned entity type (part of FlightPlan)
        modelBuilder.Entity<ChangeMode>()
            .HasOne<FlightPlan>()
            .WithMany(fp => fp.ModeChangeHistoric)
            .HasForeignKey("FlightPlanId");

        modelBuilder.Entity<Coordinate>()
            .HasOne(c => c.Perimeter)
            .WithMany(p => p.Coords)
            .HasForeignKey(c => c.PerimeterId);
    }
}
