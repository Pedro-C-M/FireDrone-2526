using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;

public class Perimeter
{
    public int Id { get; set; }

    //--------- Atributos de la entidad ---------
    public ICollection<Coordinate>? Coords { get; set; } 
    public ICollection<Route>? Routes { get; set; }
}

public class Coordinate
{
    public int Id { get; set; } // clave primaria obligatoria
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // FK opcional a Perimeter
    public int? PerimeterId { get; set; }
    public Perimeter? Perimeter { get; set; }
}