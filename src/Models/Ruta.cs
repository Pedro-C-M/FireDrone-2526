public class Ruta
{
    public int Id { get; set; }
    public float? Coords { get; set; } //Esto no se commo ponerlo
    public int? PerimetroId { get; set; }
    public Perimetro? Perimetro { get; set; } //Esto tampoco se como ponerlo
    public int? PlanId { get; set; }
}