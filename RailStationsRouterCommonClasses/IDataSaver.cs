namespace RailStationsRouterCommonClasses;

/// <summary>
/// Интерфейс для модулей сохранения данных о станциях
/// </summary>
public interface IDataSaver : IDisposable
{
    /// <summary>
    /// Добавляет или получает код (yandex_code, esr_code)
    /// </summary>
    long AddCode(string? yandexCode, string? esrCode);

    /// <summary>
    /// Добавляет или обновляет страну
    /// </summary>
    long AddCountry(long codeId, string title);

    /// <summary>
    /// Добавляет или обновляет регион
    /// </summary>
    long AddRegion(long codeId, string title);

    /// <summary>
    /// Добавляет связь региона со страной
    /// </summary>
    void AddRegionToCountry(long countryId, long regionId);

    /// <summary>
    /// Добавляет или обновляет поселение
    /// </summary>
    long AddSettlement(long codeId, string title);

    /// <summary>
    /// Добавляет связь поселения с регионом
    /// </summary>
    void AddSettlementToRegion(long regionId, long settlementId);

    /// <summary>
    /// Добавляет или обновляет станцию
    /// </summary>
    long AddStation(
        long codeId,
        string? direction,
        string? stationType,
        string? title,
        double? longitude,
        string? transportType,
        double? latitude);

    /// <summary>
    /// Добавляет связь станции с поселением
    /// </summary>
    void AddStationToSettlement(long settlementId, long stationId);
}
