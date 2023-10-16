namespace RailStationsRouterCommonClasses.GeoRaspClasses;

public class Region
{
    public int ID { get; set; }
    public List<Settlement> settlements { get; set; }
    public Codes codes { get; set; }
    public string? title { get; set; }
}