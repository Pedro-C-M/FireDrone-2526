public class Dron
{
    public int Id { get; set; }
    public int? EstBaseId { get; set; }
    public EstBase? Base { get; set; }
    public int? EstControlId { get; set; }
    public EstControl? Est { get; set; }
    public int? PlanVueloActualId { get; set; }
    public PlanVuelo? Actual { get; set; }
    public string? Estado { get; set; }
    public float? Lat { get; set; }
    public float? Lon { get; set; }
    public int? CaractId { get; set; }
    public int? MuestraId { get; set; }
    public ICollection<Muestra>? Muestras { get; set; }
}