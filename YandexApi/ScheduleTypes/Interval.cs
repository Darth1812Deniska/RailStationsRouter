using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Interval
{
    [JsonProperty("begin_time")]
    public DateTime? BeginTime { get; set; }

    [JsonProperty("density")]
    public string Density { get; set; }

    [JsonProperty("end_time")]
    public DateTime? EndTime { get; set; }
}