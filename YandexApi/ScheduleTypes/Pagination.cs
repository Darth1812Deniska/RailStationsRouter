using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Pagination
{
    [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
    public int? total { get; set; }

    [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
    public int? limit { get; set; }

    [JsonProperty("offset", NullValueHandling = NullValueHandling.Ignore)]
    public int? offset { get; set; }
}