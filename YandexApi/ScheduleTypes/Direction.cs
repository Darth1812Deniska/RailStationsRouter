using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Direction
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }
}