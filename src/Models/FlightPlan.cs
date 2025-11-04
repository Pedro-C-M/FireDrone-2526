using System;
using System.Collections;
using System.Collections.Generic;

public class FlightPlan
{
    public int Id { get; set; }
    public int RutaId { get; set; }
    public int DronId { get; set; }
    public int? EstControlId { get; set; }
    public int? StartingPointId { get; set; }
    public int? EndingPointId { get; set; }
    //--------- Atributos de la entidad ---------
    public Route? Ruta { get; set; }
    public Dron? Dron { get; set; }
    public ControlStation? Ctrl { get; set; }
    public RoutePoint? StartPoint { get; set; }
    public RoutePoint? EndPoint { get; set; }

    public DateTime StartingTime { get; set; }
    public DateTime? EndingTime { get; set; } //Puede ser null si aún no ha terminado
    public FlightStatus State { get; set; }

    public List<ChangeMode> ModeChangeHistoric { get; set; } = new List<ChangeMode>();
    public ICollection<RoutePoint>? RoutePoints { get; set; }//Puntos pasados en el plan de vuelo
}

public enum FlightStatus
{
    OnCourse,
    Completed,
    Cancelled
}

public enum FlightMode
{
    Auto,
    Manual
}

public class ChangeMode
{
    public DateTime Moment { get; set; } //Instante de tiempo en el que se realiza el cambio de modo
    public FlightMode Mode { get; set; } //Modo de vuelo al que se cambia
}