using System.Text.Json;
using RailStationsRouterCommonClasses.DB;
using YandexRaspApi.StationsListTypes;

namespace RailStationsRouterCommonClasses
{
    public class RailStationDownloader
    {
        private readonly IDataSaver _dataSaver;

        public RailStationDownloader(IDataSaver dataSaver)
        {
            _dataSaver = dataSaver;
        }

        /// <summary>
        /// Загружает и сохраняет данные о станциях из Яндекс в базу данных
        /// </summary>
        public void DownloadAndSaveStations(Root root)
        {
            if (root?.countries == null)
                return;

            List<Country> countries = root.countries;

            foreach (Country country in countries)
            {
                SaveCountry(country);
            }
        }

        /// <summary>
        /// Сохраняет страну и все её вложенные данные (регионы, поселения, станции)
        /// </summary>
        private void SaveCountry(Country country)
        {
            var countryCode = country.codes;
            if (countryCode == null)
                return;

            var countryCodeId = _dataSaver.AddCode(countryCode.yandex_code, countryCode.esr_code);
            var countryId = _dataSaver.AddCountry(countryCodeId, country.title ?? string.Empty);

            if (country.regions != null)
            {
                foreach (Region region in country.regions)
                {
                    SaveRegion(region, countryId, country.title);
                }
            }
        }

        /// <summary>
        /// Сохраняет регион и все его вложенные данные (поселения, станции)
        /// </summary>
        private void SaveRegion(Region region, long countryId, string? countryTitle)
        {
            var regionCode = region.codes;
            if (regionCode == null)
                return;

            var regionCodeId = _dataSaver.AddCode(regionCode.yandex_code, regionCode.esr_code);
            var regionId = _dataSaver.AddRegion(regionCodeId, region.title ?? string.Empty);
            _dataSaver.AddRegionToCountry(countryId, regionId);

            if (region.settlements != null)
            {
                foreach (Settlement settlement in region.settlements)
                {
                    SaveSettlement(settlement, regionId, countryTitle, region.title);
                }
            }
        }

        /// <summary>
        /// Сохраняет поселение и все его станции
        /// </summary>
        private void SaveSettlement(Settlement settlement, long regionId, string? countryTitle, string? regionTitle)
        {
            var settlementCode = settlement.codes;
            if (settlementCode == null)
                return;

            var settlementCodeId = _dataSaver.AddCode(settlementCode.yandex_code, settlementCode.esr_code);
            var settlementId = _dataSaver.AddSettlement(settlementCodeId, settlement.title ?? string.Empty);
            _dataSaver.AddSettlementToRegion(regionId, settlementId);

            foreach (Station station in settlement.stations)
            {
                SaveStation(station, settlementId, countryTitle, regionTitle, settlement.title);
            }
        }

        /// <summary>
        /// Сохраняет станцию
        /// </summary>
        private void SaveStation(Station station, long settlementId, string? countryTitle, string? regionTitle, string? settlementTitle)
        {
            Codes? codes = station.codes;
            if (codes == null)
                return;

            double? convLongitude = null;
            if (station.longitude is JsonElement { ValueKind: JsonValueKind.Number } jsLongitude)
            {
                convLongitude = jsLongitude.GetDouble();
            }

            double? convLatitude = null;
            if (station.latitude is JsonElement { ValueKind: JsonValueKind.Number } jsLatitude)
            {
                convLatitude = jsLatitude.GetDouble();
            }

            long codeId = _dataSaver.AddCode(codes.yandex_code, codes.esr_code);
            long stationId = _dataSaver.AddStation(codeId,
                station.direction,
                station.station_type,
                station.title,
                convLongitude,
                station.transport_type,
                convLatitude);
            _dataSaver.AddStationToSettlement(settlementId, stationId);

            string fullStationText = $"Страна:{countryTitle}, " +
                                     $"Регион: {regionTitle}, " +
                                     $"Поселение: {settlementTitle}, " +
                                     $"Станция: {station.title}";
            Console.WriteLine(fullStationText);
        }
    }
}
