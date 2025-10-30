public class PlanVuelo
{
    public int Id { get; set; }
    public int RutaId { get; set; }
    public Ruta? Ruta { get; set; }
    public int DronId { get; set; }
    public Dron? Dron { get; set; }
    public int? EstControlId { get; set; }
    public EstControl? Ctrl { get; set; }
    public int? PuntoInicioId { get; set; }
    public PuntoRuta? Inicio { get; set; }
}