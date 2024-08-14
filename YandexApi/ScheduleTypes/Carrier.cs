using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Carrier
{
    [JsonProperty("code")]
    public int? Code { get; set; }

    [JsonProperty("codes")]
    public Codes Codes { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }
}