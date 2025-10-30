public class Incidencia
{
    public int Id { get; set; }
    public string? Msg { get; set; }
    public string? Tipo { get; set; }
    public DateTime? Time { get; set; }
    public int? PlanVueloActualId { get; set; }
    public PlanVuelo? Actual { get; set; }
}