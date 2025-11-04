using System;

public class RoutePoint
{
    public int Id { get; set; }
    public int? RouteId { get; set; }

    //--------- Atributos de la entidad ---------
    public Route? Route { get; set; }
    public float? Long { get; set; }
    public float? Lat { get; set; }
    public float? Height { get; set; }
    public float? Velocity { get; set; }
    public DateTime? Time { get; set; }

}