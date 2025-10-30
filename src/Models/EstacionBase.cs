public class EstacionBase //Este fichero mirarlo tambien(pongo esto para que salga en la PR)
{
	public int Id { get; set; }
	public ICollection<Dron>? Drones { get; set; }
}