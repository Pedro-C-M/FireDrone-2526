public class Sensor
{
    public int Id { get; set; }
    public string? Modelo { get; set; }
    public int? DronId { get; set; }
    public Dron? Dron { get; set; }
}