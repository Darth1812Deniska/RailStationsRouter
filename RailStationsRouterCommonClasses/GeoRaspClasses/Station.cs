

namespace RailStationsRouterCommonClasses.GeoRaspClasses
{
    public class Station
    {
        public int ID { get; set; }
        public string? direction { get; set; }
        public Codes codes { get; set; }
        public string? station_type { get; set; }
        public string? title { get; set; }
        public double? longitude { get; set; }
        public string? transport_type { get; set; }
        public double? latitude { get; set; }
    }



}