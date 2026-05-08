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
    /// Открывает соединение с базой данных и создает таблицы при необходимости
    /// </summary>
    public void Initialize()
    {
        try
        {
            Logger.Log("Запуск Initialize");
            CreateSchemas();
            CreateTables();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка инициализации PostgreSQL базы данных: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Создает необходимые схемы в базе данных
    /// </summary>
    private void CreateSchemas()
    {
        try
        {
            Logger.Log("Запуск CreateSchemas");
            using var command = _dataSource.CreateCommand("""
                DO $$ BEGIN
                    CREATE SCHEMA IF NOT EXISTS stations_list;
                EXCEPTION
                    WHEN duplicate_schema THEN NULL;
                END $$;
                
                DO $$ BEGIN
                    CREATE SCHEMA IF NOT EXISTS schedule;
                EXCEPTION
                    WHEN duplicate_schema THEN NULL;
                END $$;
                """);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка создания схем PostgreSQL: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Создает все необходимые таблицы в базе данных
    /// </summary>
    private void CreateTables()
    {
        try
        {
            Logger.Log("Запуск CreateTables");
            
            var commands = new[]
            {
                // === Таблицы в схеме public (для StationsListTypes) ===
                
                // Таблица для кодов (yandex_code, esr_code)
                @"CREATE TABLE IF NOT EXISTS public.codes (
                    id BIGSERIAL PRIMARY KEY,
                    yandex_code TEXT,
                    esr_code TEXT,
                    UNIQUE(yandex_code, esr_code)
                )",

                // Таблица стран
                @"CREATE TABLE IF NOT EXISTS public.country (
                    id BIGSERIAL PRIMARY KEY,
                    title TEXT NOT NULL,
                    codeid BIGINT UNIQUE REFERENCES public.codes(id)
                )",

                // Таблица регионов
                @"CREATE TABLE IF NOT EXISTS public.region (
                    id BIGSERIAL PRIMARY KEY,
                    title TEXT NOT NULL,
                    codeid BIGINT UNIQUE REFERENCES public.codes(id)
                )",

                // Таблица поселений
                @"CREATE TABLE IF NOT EXISTS public.settlement (
                    id BIGSERIAL PRIMARY KEY,
                    title TEXT NOT NULL,
                    codeid BIGINT UNIQUE REFERENCES public.codes(id)
                )",

                // Таблица станций
                @"CREATE TABLE IF NOT EXISTS public.station (
                    id BIGSERIAL PRIMARY KEY,
                    direction TEXT,
                    codeid BIGINT REFERENCES public.codes(id),
                    station_type TEXT,
                    title TEXT,
                    longitude DOUBLE PRECISION,
                    transport_type TEXT,
                    latitude DOUBLE PRECISION
                )",

                // Связующая таблица страна-регион
                @"CREATE TABLE IF NOT EXISTS public.country_regions (
                    id BIGSERIAL PRIMARY KEY,
                    countryid BIGINT NOT NULL REFERENCES public.country(id),
                    regionid BIGINT NOT NULL REFERENCES public.region(id),
                    UNIQUE(countryid, regionid)
                )",

                // Связующая таблица регион-поселение
                @"CREATE TABLE IF NOT EXISTS public.region_settlements (
                    id BIGSERIAL PRIMARY KEY,
                    regionid BIGINT NOT NULL REFERENCES public.region(id),
                    settlementid BIGINT NOT NULL REFERENCES public.settlement(id),
                    UNIQUE(regionid, settlementid)
                )",

                // Связующая таблица поселение-станция
                @"CREATE TABLE IF NOT EXISTS public.settlement_stations (
                    id BIGSERIAL PRIMARY KEY,
                    settlement_id BIGINT NOT NULL REFERENCES public.settlement(id),
                    station_id BIGINT NOT NULL REFERENCES public.station(id),
                    UNIQUE(settlement_id, station_id)
                )",

                // === Таблицы в схеме schedule (для ScheduleTypes) ===
                
                // Таблица перевозчиков (Carrier)
                @"CREATE TABLE IF NOT EXISTS schedule.carrier (
                    id BIGSERIAL PRIMARY KEY,
                    code INTEGER,
                    title TEXT,
                    codes_json JSONB
                )",

                // Таблица направлений (Direction)
                @"CREATE TABLE IF NOT EXISTS schedule.direction (
                    id BIGSERIAL PRIMARY KEY,
                    code TEXT,
                    title TEXT
                )",

                // Таблица транспортных подтипов (TransportSubtype)
                @"CREATE TABLE IF NOT EXISTS schedule.transport_subtype (
                    id BIGSERIAL PRIMARY KEY,
                    title TEXT,
                    code TEXT,
                    color TEXT
                )",

                // Таблица потоков/рейсов (Thread)
                @"CREATE TABLE IF NOT EXISTS schedule.thread (
                    id BIGSERIAL PRIMARY KEY,
                    number TEXT,
                    title TEXT,
                    short_title TEXT,
                    express_type TEXT,
                    transport_type TEXT,
                    carrier_id BIGINT REFERENCES schedule.carrier(id),
                    uid TEXT,
                    vehicle JSONB,
                    transport_subtype_id BIGINT REFERENCES schedule.transport_subtype(id)
                )",

                // Таблица расписаний (Schedule)
                @"CREATE TABLE IF NOT EXISTS schedule.schedule (
                    id BIGSERIAL PRIMARY KEY,
                    thread_id BIGINT REFERENCES schedule.thread(id),
                    is_fuzzy BOOLEAN,
                    platform TEXT,
                    terminal JSONB,
                    days TEXT,
                    except_days JSONB,
                    stops TEXT,
                    departure TEXT,
                    arrival TEXT
                )",

                // Таблица интервального расписания (IntervalSchedule)
                @"CREATE TABLE IF NOT EXISTS schedule.interval_schedule (
                    id BIGSERIAL PRIMARY KEY,
                    except_days JSONB,
                    thread_id BIGINT REFERENCES schedule.thread(id),
                    is_fuzzy BOOLEAN,
                    days TEXT,
                    stops TEXT,
                    terminal JSONB,
                    platform TEXT
                )",

                // Таблица интервалов (Interval)
                @"CREATE TABLE IF NOT EXISTS schedule.interval (
                    id BIGSERIAL PRIMARY KEY,
                    density TEXT,
                    end_time TIMESTAMP,
                    begin_time TIMESTAMP,
                    interval_schedule_id BIGINT REFERENCES schedule.interval_schedule(id)
                )",

                // Таблица пагинации (Pagination)
                @"CREATE TABLE IF NOT EXISTS schedule.pagination (
                    id BIGSERIAL PRIMARY KEY,
                    total INTEGER,
                    limit INTEGER,
                    offset INTEGER
                )",

                // Таблица кодов для ScheduleTypes
                @"CREATE TABLE IF NOT EXISTS schedule.codes (
                    id BIGSERIAL PRIMARY KEY,
                    sirena JSONB,
                    iata JSONB,
                    icao JSONB
                )",

                // Таблица станций для ScheduleTypes
                @"CREATE TABLE IF NOT EXISTS schedule.station (
                    id BIGSERIAL PRIMARY KEY,
                    type TEXT,
                    title TEXT,
                    short_title TEXT,
                    popular_title TEXT,
                    code TEXT,
                    station_type TEXT,
                    station_type_name TEXT,
                    transport_type TEXT
                )",

                // Таблица корневого объекта расписания (ScheduleRoot)
                @"CREATE TABLE IF NOT EXISTS schedule.schedule_root (
                    id BIGSERIAL PRIMARY KEY,
                    date JSONB,
                    station_id BIGINT REFERENCES schedule.station(id),
                    event TEXT,
                    pagination_id BIGINT REFERENCES schedule.pagination(id),
                    schedule_direction_code TEXT,
                    schedule_direction_title TEXT
                )",

                // Связь расписания с направлениями
                @"CREATE TABLE IF NOT EXISTS schedule.schedule_directions (
                    id BIGSERIAL PRIMARY KEY,
                    schedule_root_id BIGINT REFERENCES schedule.schedule_root(id),
                    direction_id BIGINT REFERENCES schedule.direction(id)
                )"
            };

            foreach (var commandText in commands)
            {
                using var command = _dataSource.CreateCommand(commandText);
                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка создания таблиц PostgreSQL: {ex.Message}");
            throw;
        }
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
        var rawResult = command.ExecuteScalar();
        return rawResult != null ? (long)rawResult : 0;
    }

    private static void ExecuteNonQueryCommand(NpgsqlCommand command)
    {
        command.ExecuteNonQuery();
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
