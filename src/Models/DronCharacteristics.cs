using System.Collections;

public class DronCharacteristics
{
    public int Id { get; set; }
    public int? DronId { get; set; }

    //--------- Atributos de la entidad ---------
    public string? Model { get; set; }
    public ICollection<Sensor>? Sensors { get; set; }
    public Dron? Dron { get; set; }
}