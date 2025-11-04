using System.Collections;
using System.Collections.Generic;

public class Perimeter
{
    public int Id { get; set; }

    //--------- Atributos de la entidad ---------
    public ICollection<Coordinate>? Coords { get; set; } 
    public ICollection<Route>? Routes { get; set; }
}

public class Coordinate//Preguntar si se pueden guardar clases
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}