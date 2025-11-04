using System.Collections.Generic;

public class Dron
{
    public int Id { get; set; }
    public int? BaseStationId { get; set; }
    public int? ControlStationId { get; set; }
    public int? FlightPlanId { get; set; }
    public int? SampleId { get; set; }
    
    //--------- Atributos de la entidad ---------
    public BaseStation? Base { get; set; }
    public ControlStation? ControlStation { get; set; }
    public FlightPlan? Actual { get; set; }
    public DronCharacteristics? DronCharacteristics { get; set; }

    public ICollection<Sample>? Muestras { get; set; }
    public string? State { get; set; }
    public float? Lat { get; set; }
    public float? Lon { get; set; }
}