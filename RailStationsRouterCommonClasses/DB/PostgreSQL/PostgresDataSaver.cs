using Npgsql;

namespace RailStationsRouterCommonClasses.DB.PostgreSQL;

/// <summary>
/// Модуль для сохранения объектов из Яндекс.Расписания в базу данных PostgreSQL
/// </summary>
public class PostgresDataSaver : IDataSaver
{
    private readonly NpgsqlDataSource _dataSource;
    private bool _disposed;

    public PostgresDataSaver(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        _dataSource = dataSourceBuilder.Build();
    }

    /// <summary>
    /// Добавляет или получает код (yandex_code, esr_code)
    /// </summary>
    public long AddCode(string? yandexCode, string? esrCode)
    {
        try
        {
            Logger.Log($"Запуск AddCode: yandexCode={yandexCode}, esrCode={esrCode}");
            using var command = _dataSource.CreateCommand("SELECT public.rsr_f_add_code(:p_yandex_code, :p_esr_code);");
            command.Parameters.AddWithValue("p_yandex_code", yandexCode ?? string.Empty);
            command.Parameters.AddWithValue("p_esr_code", esrCode ?? string.Empty);
            
            return ExecuteScalarCommand(command);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении/получении кода: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Добавляет или обновляет страну
    /// </summary>
    public long AddCountry(long codeId, string title)
    {
        try
        {
            Logger.Log($"Запуск AddCountry: codeId={codeId}, title={title}");
            ArgumentNullException.ThrowIfNull(title);
            
            using var command = _dataSource.CreateCommand("SELECT public.rsr_f_add_country(:p_codeid, :p_title);");
            command.Parameters.AddWithValue("p_codeid", codeId);
            command.Parameters.AddWithValue("p_title", title);
            
            return ExecuteScalarCommand(command);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении/обновлении страны: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Добавляет или обновляет регион
    /// </summary>
    public long AddRegion(long codeId, string title)
    {
        try
        {
            Logger.Log($"Запуск AddRegion: codeId={codeId}, title={title}");
            ArgumentNullException.ThrowIfNull(title);
            
            using var command = _dataSource.CreateCommand("SELECT public.rsr_f_add_region(:p_code_id, :p_title);");
            command.Parameters.AddWithValue("p_code_id", codeId);
            command.Parameters.AddWithValue("p_title", title);
            
            return ExecuteScalarCommand(command);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении/обновлении региона: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Добавляет связь региона со страной
    /// </summary>
    public void AddRegionToCountry(long countryId, long regionId)
    {
        try
        {
            Logger.Log($"Запуск AddRegionToCountry: countryId={countryId}, regionId={regionId}");
            using var command = _dataSource.CreateCommand("SELECT public.rsr_f_add_region_to_country(:p_country_id, :p_region_id);");
            command.Parameters.AddWithValue("p_country_id", countryId);
            command.Parameters.AddWithValue("p_region_id", regionId);
            ExecuteNonQueryCommand(command);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении связи региона со страной: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Добавляет или обновляет поселение
    /// </summary>
    public long AddSettlement(long codeId, string title)
    {
        try
        {
            Logger.Log($"Запуск AddSettlement: codeId={codeId}, title={title}");
            ArgumentNullException.ThrowIfNull(title);
            
            using var command = _dataSource.CreateCommand("SELECT public.rsr_f_add_settlement(:p_code_id, :p_title);");
            command.Parameters.AddWithValue("p_code_id", codeId);
            command.Parameters.AddWithValue("p_title", title);
            
            return ExecuteScalarCommand(command);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении/обновлении поселения: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Добавляет связь поселения с регионом
    /// </summary>
    public void AddSettlementToRegion(long regionId, long settlementId)
    {
        try
        {
            Logger.Log($"Запуск AddSettlementToRegion: regionId={regionId}, settlementId={settlementId}");
            using var command = _dataSource.CreateCommand("SELECT public.rsr_f_add_settlement_to_region(:p_region_id, :p_settlement_id);");
            command.Parameters.AddWithValue("p_region_id", regionId);
            command.Parameters.AddWithValue("p_settlement_id", settlementId);
            ExecuteNonQueryCommand(command);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении связи поселения с регионом: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Добавляет или обновляет станцию
    /// </summary>
    public long AddStation(
        long codeId,
        string? direction,
        string? stationType,
        string? title,
        double? longitude,
        string? transportType,
        double? latitude)
    {
        try
        {
            Logger.Log($"Запуск AddStation: codeId={codeId}, direction={direction}, stationType={stationType}, title={title}, longitude={longitude}, transportType={transportType}, latitude={latitude}");
            using var command = _dataSource.CreateCommand(
                "SELECT public.rsr_f_add_station(" +
                ":p_codeid, " +
                ":p_direction, " +
                ":p_station_type, " +
                ":p_title, " +
                ":p_longitude, " +
                ":p_transport_type, " +
                ":p_latitude);");
            
            command.Parameters.AddWithValue("p_codeid", codeId);
            command.Parameters.AddWithValue("p_direction", string.IsNullOrEmpty(direction) ? DBNull.Value : direction);
            command.Parameters.AddWithValue("p_station_type", string.IsNullOrEmpty(stationType) ? DBNull.Value : stationType);
            command.Parameters.AddWithValue("p_title", string.IsNullOrEmpty(title) ? DBNull.Value : title);
            command.Parameters.AddWithValue("p_longitude", longitude == null ? DBNull.Value : longitude);
            command.Parameters.AddWithValue("p_transport_type", string.IsNullOrEmpty(transportType) ? DBNull.Value : transportType);
            command.Parameters.AddWithValue("p_latitude", latitude == null ? DBNull.Value : latitude);
            
            return ExecuteScalarCommand(command);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении/обновлении станции: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Добавляет связь станции с поселением
    /// </summary>
    public void AddStationToSettlement(long settlementId, long stationId)
    {
        try
        {
            Logger.Log($"Запуск AddStationToSettlement: settlementId={settlementId}, stationId={stationId}");
            using var command = _dataSource.CreateCommand("SELECT public.rsr_f_add_station_to_settlement(:p_settlement_id, :p_station_id);");
            command.Parameters.AddWithValue("p_settlement_id", settlementId);
            command.Parameters.AddWithValue("p_station_id", stationId);
            ExecuteNonQueryCommand(command);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении связи станции с поселением: {ex.Message}");
            throw;
        }
    }

    private static long ExecuteScalarCommand(NpgsqlCommand command)
    {
        var rawResult = command.ExecuteScalarAsync().Result;
        return rawResult != null ? (long)rawResult : 0;
    }

    private static void ExecuteNonQueryCommand(NpgsqlCommand command)
    {
        command.ExecuteScalarAsync();
    }

    public void Dispose()
    {
        try
        {
            if (!_disposed)
            {
                Logger.Log("Запуск Dispose");
                _dataSource.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при закрытии соединения с базой данных: {ex.Message}");
            throw;
        }
    }
}
