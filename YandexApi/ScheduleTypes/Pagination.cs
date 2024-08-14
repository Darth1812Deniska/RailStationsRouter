using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Pagination
{
    [JsonProperty("limit")]
    public int? Limit { get; set; }

    [JsonProperty("offset")]
    public int? Offset { get; set; }

    [JsonProperty("total")]
    public int? Total { get; set; }
}