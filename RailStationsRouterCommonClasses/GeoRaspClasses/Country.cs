namespace RailStationsRouterCommonClasses.GeoRaspClasses;

public class Country
{
    public int ID { get; set; }
    public List<Region> regions { get; set; }
    public Codes codes { get; set; }
    public string? title { get; set; }
}