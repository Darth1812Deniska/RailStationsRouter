using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

public class Schedule
{
    [JsonProperty("arrival")]
    public DateTime? Arrival { get; set; }

    [JsonProperty("days")]
    public string Days { get; set; }

    [JsonProperty("departure")]
    public DateTime? Departure { get; set; }

    [JsonProperty("except_days")]
    public string ExceptDays { get; set; }

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