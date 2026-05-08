using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Codes
{
    [JsonProperty("sirena", NullValueHandling = NullValueHandling.Ignore)]
    public object sirena { get; set; }

    [JsonProperty("iata", NullValueHandling = NullValueHandling.Ignore)]
    public object iata { get; set; }

    [JsonProperty("icao", NullValueHandling = NullValueHandling.Ignore)]
    public object icao { get; set; }
}