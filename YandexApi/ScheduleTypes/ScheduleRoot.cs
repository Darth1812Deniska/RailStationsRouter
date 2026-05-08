using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class ScheduleRoot
{
    [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
    public object date { get; set; }

    [JsonProperty("station", NullValueHandling = NullValueHandling.Ignore)]
    public Station station { get; set; }

    [JsonProperty("event", NullValueHandling = NullValueHandling.Ignore)]
    public string @event { get; set; }

    [JsonProperty("pagination", NullValueHandling = NullValueHandling.Ignore)]
    public Pagination pagination { get; set; }

    [JsonProperty("schedule", NullValueHandling = NullValueHandling.Ignore)]
    public List<Schedule> schedule { get; set; }

    [JsonProperty("interval_schedule", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> interval_schedule { get; set; }

    [JsonProperty("directions", NullValueHandling = NullValueHandling.Ignore)]
    public List<Direction> directions { get; set; }

    [JsonProperty("schedule_direction", NullValueHandling = NullValueHandling.Ignore)]
    public ScheduleDirection schedule_direction { get; set; }
}