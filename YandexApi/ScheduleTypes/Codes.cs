using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Codes
{
    [JsonProperty("iata")]
    public object Iata { get; set; }

    [JsonProperty("icao")]
    public object Icao { get; set; }

    [JsonProperty("sirena")]
    public object Sirena { get; set; }
}