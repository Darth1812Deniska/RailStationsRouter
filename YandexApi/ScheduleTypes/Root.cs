using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Root
{
    [JsonProperty("date")]
    public string Date { get; set; }

    [JsonProperty("directions")]
    public List<Direction> Directions { get; set; }

    [JsonProperty("interval_schedule")]
    public List<IntervalSchedule> IntervalSchedule { get; set; }

    [JsonProperty("pagination")]
    public Pagination Pagination { get; set; }

    [JsonProperty("schedule")]
    public List<Schedule> Schedule { get; set; }

    [JsonProperty("schedule_direction")]
    public ScheduleDirection ScheduleDirection { get; set; }

    [JsonProperty("station")]
    public Station Station { get; set; }
}