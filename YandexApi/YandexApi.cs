using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
using YandexRaspApi.StationsListTypes;

namespace YandexRaspApi
{
    public class YandexApi
    {
        private readonly string _apiToken;
        private const string StationsListAddress = "https://api.rasp.yandex.net/v3.0/stations_list/";
        
        private Uri StationsListUri => new Uri(StationsListAddress);

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
    }
}