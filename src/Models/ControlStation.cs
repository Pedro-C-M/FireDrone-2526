using System.Collections.Generic;

namespace Models
{
    public class ControlStation
    {
        public int Id { get; set; }

        //--------- Atributos de la entidad ---------
        public ICollection<Dron>? Drones { get; set; }
        public ICollection<BaseStation>? BaseStations { get; set; }
        public float? Lat { get; set; }
        public float? Lon { get; set; }

    }
}
