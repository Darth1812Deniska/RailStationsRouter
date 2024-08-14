namespace YandexRaspApi.StationsListTypes;

public class Settlement
{
    public string? title { get; set; }
    public Codes? codes { get; set; }
    public List<Station>? stations { get; set; }
}