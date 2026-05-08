using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Interval
{
    [JsonProperty("density", NullValueHandling = NullValueHandling.Ignore)]
    public string density { get; set; }

    [JsonProperty("end_time", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? end_time { get; set; }

    [JsonProperty("begin_time", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? begin_time { get; set; }
}