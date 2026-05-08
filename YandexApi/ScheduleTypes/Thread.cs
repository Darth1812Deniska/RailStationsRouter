using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Thread
{
    [JsonProperty("number", NullValueHandling = NullValueHandling.Ignore)]
    public string number { get; set; }

    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string title { get; set; }

    [JsonProperty("short_title", NullValueHandling = NullValueHandling.Ignore)]
    public string short_title { get; set; }

    [JsonProperty("express_type", NullValueHandling = NullValueHandling.Ignore)]
    public object express_type { get; set; }

    [JsonProperty("transport_type", NullValueHandling = NullValueHandling.Ignore)]
    public string transport_type { get; set; }

    [JsonProperty("carrier", NullValueHandling = NullValueHandling.Ignore)]
    public Carrier carrier { get; set; }

    [JsonProperty("uid", NullValueHandling = NullValueHandling.Ignore)]
    public string uid { get; set; }

    [JsonProperty("vehicle", NullValueHandling = NullValueHandling.Ignore)]
    public object vehicle { get; set; }

    [JsonProperty("transport_subtype", NullValueHandling = NullValueHandling.Ignore)]
    public TransportSubtype transport_subtype { get; set; }
}