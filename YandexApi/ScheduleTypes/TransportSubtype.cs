using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class TransportSubtype
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }
}