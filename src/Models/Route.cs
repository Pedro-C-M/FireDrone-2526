using System.Collections;
using System.Collections.Generic;

public class Route
{
    public int Id { get; set; }
    public int? PerimeterId { get; set; }
   
    //--------- Atributos de la entidad ---------
    public ICollection<RoutePoint>? Coords { get; set; } //Esto no se commo ponerlo

    public Perimeter? Perimeter { get; set; } //Esto tampoco se como ponerlo
    public ICollection<FlightPlan>? Plans { get; set; }
    public RouteType Type { get; set; }
}

public enum RouteType 
{
    Simple,
    Periodic
}