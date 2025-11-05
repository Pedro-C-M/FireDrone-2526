using Microsoft.EntityFrameworkCore;

public class FireDrone : DbContext
{
  public DbSet<BaseStation> BaseStations { get; set; }
    public DbSet<ControlStation> ControlStations { get; set; }
    public DbSet<Dron> Drones { get; set; }
    public DbSet<DronCharacteristics> DronCharacteristics { get; set; }
    public DbSet<FlightPlan> FlightPlans { get; set; }
    public DbSet<Incidence> Incidences { get; set; }
    public DbSet<Perimeter> Perimeters { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<RoutePoint> RoutePoints { get; set; }
    public DbSet<Sample> Samples { get; set; }
    public DbSet<Sensor> Sensors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
     => options.UseSqlite("Data Source=../FireDrone.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the relationship between Dron and FlightPlan
        modelBuilder.Entity<Dron>()
            .HasOne(d => d.Actual)
            .WithOne(fp => fp.Dron)
            .HasForeignKey<FlightPlan>(fp => fp.DronId);

        // Configure ChangeMode as owned entity type (part of FlightPlan)
        modelBuilder.Entity<FlightPlan>()
            .OwnsMany(fp => fp.ModeChangeHistoric);

        // Configure Coordinate as owned entity type (part of Perimeter)
        modelBuilder.Entity<Perimeter>()
            .OwnsMany(p => p.Coords);
    }
}
