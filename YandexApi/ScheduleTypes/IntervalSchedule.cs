using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class IntervalSchedule
{
    [JsonProperty("days")]
    public string Days { get; set; }

    [JsonProperty("except_days")]
    public object ExceptDays { get; set; }

    [JsonProperty("is_fuzzy")]
    public bool? IsFuzzy { get; set; }

    [JsonProperty("platform")]
    public string Platform { get; set; }

    [JsonProperty("stops")]
    public string Stops { get; set; }

    [JsonProperty("terminal")]
    public object Terminal { get; set; }

    [JsonProperty("thread")]
    public Thread Thread { get; set; }
}