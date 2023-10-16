namespace RailStationsRouterCommonClasses.GeoRaspClasses;

public class Settlement
{
    public int ID { get; set; }
    public string? title { get; set; }
    public Codes codes { get; set; }
    public List<Station> stations { get; set; }
}