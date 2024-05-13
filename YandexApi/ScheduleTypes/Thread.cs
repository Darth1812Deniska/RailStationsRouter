using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Thread
{
    [JsonProperty("carrier")]
    public object Carrier { get; set; }

    [JsonProperty("express_type")]
    public object ExpressType { get; set; }

    [JsonProperty("interval")]
    public Interval Interval { get; set; }

    [JsonProperty("number")]
    public string Number { get; set; }

    [JsonProperty("short_title")]
    public string ShortTitle { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("transport_subtype")]
    public TransportSubtype TransportSubtype { get; set; }

    [JsonProperty("transport_type")]
    public string TransportType { get; set; }

    [JsonProperty("uid")]
    public string Uid { get; set; }

    [JsonProperty("vehicle")]
    public object Vehicle { get; set; }
}