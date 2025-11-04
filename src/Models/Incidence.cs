using System;

public class Incidence
{
    public int Id { get; set; }
    public int? FlightPlanId { get; set; }

    //--------- Atributos de la entidad ---------
    public string? Msg { get; set; }
    public string? Type { get; set; }
    public DateTime? Time { get; set; }
    public FlightPlan? Actual { get; set; }
}