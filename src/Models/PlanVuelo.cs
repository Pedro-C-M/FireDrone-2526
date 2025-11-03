using System.Collections;

public class PlanVuelo
{
    public int Id { get; set; }
    public int RutaId { get; set; }
    public Ruta? Ruta { get; set; }
    public int DronId { get; set; }
    public Dron? Dron { get; set; }
    public int? EstControlId { get; set; }
    public EstacionControl? Ctrl { get; set; }
    public int? PuntoInicioId { get; set; }
    public PuntoRuta? Inicio { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; } //Puede ser null si aún no ha terminado

    public EstadoPlanVuelo Estado { get; set; }

    public List<CambioModo> HistorialCambiosModo { get; set; } = new List<CambioModo>();

    public ICollection? PuntoRuta { get; set; }

}

public enum EstadoPlanVuelo
{
    EnCurso,
    Completado,
    Cancelado
}

public enum ModoVuelo
{
    Auto,
    Manual
}

public class CambioModo
{
    public DateTime Momento { get; set; } //Instante de tiempo en el que se realiza el cambio de modo
    public ModoVuelo Modo { get; set; } //Modo de vuelo al que se cambia
}