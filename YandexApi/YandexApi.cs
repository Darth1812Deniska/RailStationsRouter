using System.Linq;

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

        public string GetStationsList()
        {
            string result = string.Empty;
            
            Dictionary<string, string?> paramsDictionary = new Dictionary<string, string?>()
            {
                { "apikey", ApiToken },
                { "format", null },
                { "lang", null }
            };
            List<string> preparedParams = paramsDictionary.Where(dict => dict.Value != null)
                .Select(dict => $"{dict.Key}={dict.Value}").ToList();
            string paramsString = $"?{string.Join("&", preparedParams)}";
            string queryWithParams = $"{StationsListAddress}{paramsString}";
            using (var client = new HttpClient())
            {
                result = client.GetStringAsync(queryWithParams).Result;
            }

            return result;
        }
    }
}