using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

// ScheduleRoot myDeserializedClass = JsonConvert.DeserializeObject<ScheduleRoot>(myJsonResponse);

public class Schedule
{
    [JsonProperty("thread", NullValueHandling = NullValueHandling.Ignore)]
    public Thread thread { get; set; }

    [JsonProperty("is_fuzzy", NullValueHandling = NullValueHandling.Ignore)]
    public bool? is_fuzzy { get; set; }

    [JsonProperty("platform", NullValueHandling = NullValueHandling.Ignore)]
    public string platform { get; set; }

    [JsonProperty("terminal", NullValueHandling = NullValueHandling.Ignore)]
    public object terminal { get; set; }

    [JsonProperty("days", NullValueHandling = NullValueHandling.Ignore)]
    public string days { get; set; }

    [JsonProperty("except_days", NullValueHandling = NullValueHandling.Ignore)]
    public object except_days { get; set; }

    [JsonProperty("stops", NullValueHandling = NullValueHandling.Ignore)]
    public string stops { get; set; }

    [JsonProperty("departure", NullValueHandling = NullValueHandling.Ignore)]
    public string departure { get; set; }

    [JsonProperty("arrival", NullValueHandling = NullValueHandling.Ignore)]
    public string arrival { get; set; }
}