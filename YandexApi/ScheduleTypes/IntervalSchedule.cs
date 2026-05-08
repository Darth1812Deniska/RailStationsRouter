using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class IntervalSchedule
{
    [JsonProperty("except_days", NullValueHandling = NullValueHandling.Ignore)]
    public object except_days { get; set; }

    [JsonProperty("thread", NullValueHandling = NullValueHandling.Ignore)]
    public Thread thread { get; set; }

    [JsonProperty("is_fuzzy", NullValueHandling = NullValueHandling.Ignore)]
    public bool? is_fuzzy { get; set; }

    [JsonProperty("days", NullValueHandling = NullValueHandling.Ignore)]
    public string days { get; set; }

    [JsonProperty("stops", NullValueHandling = NullValueHandling.Ignore)]
    public string stops { get; set; }

    [JsonProperty("terminal", NullValueHandling = NullValueHandling.Ignore)]
    public object terminal { get; set; }

    [JsonProperty("platform", NullValueHandling = NullValueHandling.Ignore)]
    public string platform { get; set; }
}