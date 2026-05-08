using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
using YandexRaspApi.ScheduleTypes;
using YandexRaspApi.StationsListTypes;

namespace YandexRaspApi
{
    public class YandexApi
    {
        private readonly string _apiToken;
        private const string StationsListAddress = "https://api.rasp.yandex-net.ru/v3.0/stations_list/";
        private const string ScheduleAddress = "https://api.rasp.yandex-net.ru/v3.0/schedule/";
        private Uri StationsListUri => new(StationsListAddress);
        private Uri ScheduleUri => new(ScheduleAddress);

        private string ApiToken => _apiToken;

        public YandexApi(string apiToken)
        {
            _apiToken = apiToken;
        }

        public string GetStationsListJson()
        {
            try
            {
                Dictionary<string, string?> paramsDictionary = new Dictionary<string, string?>()
                {
                    { "apikey", ApiToken },
                    { "format", null },
                    { "lang", null }
                };

                NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(String.Empty);
                foreach (KeyValuePair<string, string?> pair in paramsDictionary)
                {
                    if (!string.IsNullOrEmpty(pair.Value))
                    {
                        queryString.Add(pair.Key, pair.Value);
                    }
                }

                using var client = new HttpClient();
                client.BaseAddress = StationsListUri;
                
                var result = client.GetStringAsync($"{StationsListUri}?{queryString}").Result;

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка станций (JSON): {ex.Message}");
                throw;
            }
        }
        
        public string GetScheduleJson(string stationCode)
        {
            try
            {
                Dictionary<string, string?> paramsDictionary = new Dictionary<string, string?>()
                {
                    { "apikey", ApiToken },
                    { "station", stationCode }
                };

                NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(String.Empty);
                foreach (KeyValuePair<string, string?> pair in paramsDictionary)
                {
                    if (!string.IsNullOrEmpty(pair.Value))
                    {
                        queryString.Add(pair.Key, pair.Value);
                    }
                }

                using var client = new HttpClient();
                client.BaseAddress = ScheduleUri;
                
                var result = client.GetStringAsync($"{ScheduleUri}?{queryString}").Result;

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении расписания (JSON): {ex.Message}");
                throw;
            }
        }

        public StationsListRoot? GetStationsList()
        {
            try
            {
                string jsonResult = GetStationsListJson();
                var root = JsonSerializer.Deserialize<StationsListRoot>(jsonResult);
                return root;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при десериализации списка станций: {ex.Message}");
                throw;
            }
        }
        
        public ScheduleRoot? GetScheduleList(string stationCode)
        {
            try
            {
                string jsonResult = GetScheduleJson(stationCode);
                var root = JsonSerializer.Deserialize<ScheduleRoot?>(jsonResult);
                return root;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при десериализации списка станций: {ex.Message}");
                throw;
            }
        }
    }
}