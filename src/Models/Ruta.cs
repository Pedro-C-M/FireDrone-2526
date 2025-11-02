public class Ruta
{
    public int Id { get; set; }
    public ICollection? Coords { get; set; } //Esto no se commo ponerlo
    public int? PerimetroId { get; set; }
    public Perimetro? Perimetro { get; set; } //Esto tampoco se como ponerlo
    public ICollection<PlanVuelo>? Planes { get; set; }
    public TipoRuta Tipo { get; set;  }
}

public enum TipoRuta 
{
    Simple,
    Periódica
}