public class EstacionControl
{
    public int Id { get; set; }
    public ICollection<Dron>? Drones { get; set; }
    public ICollection<EstacionBase>? Estaciones { get; set; }
    public float? Lat { get; set; }
    public float? Lon { get; set; }
}