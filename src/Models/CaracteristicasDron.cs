public class CaracteristicasDron
{
    public int Id { get; set; }
    public string? Modelo { get; set; }
    public ICollection? Sensores { get; set; }
    public int? DronId { get; set; }
    public Dron? Dron { get; set; }
}