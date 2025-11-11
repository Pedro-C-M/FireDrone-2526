using System;

namespace Models
{
    public class Sample
    {
        public int Id { get; set; }
        public int? DronId { get; set; }


        //--------- Atributos de la entidad ---------
        public Dron? Dron { get; set; }
        public string? File { get; set; }
        public float? Lat { get; set; }
        public float? Lon { get; set; }
        public DateTime? Time { get; set; }
    }
}
