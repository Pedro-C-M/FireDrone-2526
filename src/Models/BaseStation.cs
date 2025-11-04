using System.Collections.Generic;

public class BaseStation //Este fichero mirarlo tambien(pongo esto para que salga en la PR)
{
	public int Id { get; set; }
    //--------- Atributos de la entidad ---------
    public ICollection<Dron>? Drones { get; set; }
}