using Microsoft.Data.Sqlite;

namespace RailStationsRouterCommonClasses.DB.SQLite;

/// <summary>
/// Модуль для сохранения объектов из Яндекс.Расписания в локальную SQLite базу данных
/// </summary>
public class SqliteDataSaver : IDataSaver
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;
    private bool _disposed;

    public SqliteDataSaver(string databasePath)
    {
        ArgumentNullException.ThrowIfNull(databasePath);
        
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    /// <summary>
    /// Открывает соединение с базой данных и создает таблицы при необходимости
    /// </summary>
    public void Initialize()
    {
        try
        {
            Console.WriteLine("Запуск Initialize");
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
            CreateTables();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка инициализации SQLite базы данных: {ex.Message}");
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
            Console.WriteLine("Запуск CreateTables");
            if (_connection == null) 
                throw new InvalidOperationException("Соединение не открыто");

            var commands = new[]
            {
                // Таблица для кодов (yandex_code, esr_code)
                @"CREATE TABLE IF NOT EXISTS codes (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    yandex_code TEXT,
                    esr_code TEXT,
                    UNIQUE(yandex_code, esr_code)
                )",

                // Таблица стран
                @"CREATE TABLE IF NOT EXISTS country (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL,
                    codeid INTEGER UNIQUE,
                    FOREIGN KEY (codeid) REFERENCES codes(id)
                )",

                // Таблица регионов
                @"CREATE TABLE IF NOT EXISTS region (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL,
                    codeid INTEGER UNIQUE,
                    FOREIGN KEY (codeid) REFERENCES codes(id)
                )",

                // Таблица поселений
                @"CREATE TABLE IF NOT EXISTS settlement (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL,
                    codeid INTEGER UNIQUE,
                    FOREIGN KEY (codeid) REFERENCES codes(id)
                )",

                // Таблица станций
                @"CREATE TABLE IF NOT EXISTS station (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    direction TEXT,
                    codeid INTEGER,
                    station_type TEXT,
                    title TEXT,
                    longitude REAL,
                    transport_type TEXT,
                    latitude REAL,
                    FOREIGN KEY (codeid) REFERENCES codes(id)
                )",

                // Связующая таблица страна-регион
                @"CREATE TABLE IF NOT EXISTS country_regions (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    countryid INTEGER NOT NULL,
                    regionid INTEGER NOT NULL,
                    UNIQUE(countryid, regionid),
                    FOREIGN KEY (countryid) REFERENCES country(id),
                    FOREIGN KEY (regionid) REFERENCES region(id)
                )",

                // Связующая таблица регион-поселение
                @"CREATE TABLE IF NOT EXISTS region_settlements (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    regionid INTEGER NOT NULL,
                    settlementid INTEGER NOT NULL,
                    UNIQUE(regionid, settlementid),
                    FOREIGN KEY (regionid) REFERENCES region(id),
                    FOREIGN KEY (settlementid) REFERENCES settlement(id)
                )",

                // Связующая таблица поселение-станция
                @"CREATE TABLE IF NOT EXISTS settlement_stations (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    settlement_id INTEGER NOT NULL,
                    station_id INTEGER NOT NULL,
                    UNIQUE(settlement_id, station_id),
                    FOREIGN KEY (settlement_id) REFERENCES settlement(id),
                    FOREIGN KEY (station_id) REFERENCES station(id)
                )"
            };

            foreach (var commandText in commands)
            {
                using var command = _connection.CreateCommand();
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка создания таблиц SQLite: {ex.Message}");
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
            Console.WriteLine($"Запуск AddCode: yandexCode={yandexCode}, esrCode={esrCode}");
            if (_connection == null) 
                throw new InvalidOperationException("Соединение не открыто");

            return ExecuteCodeOperation(
                @"SELECT id FROM codes 
                  WHERE COALESCE(esr_code, '') = COALESCE(@esr_code, '') 
                  AND COALESCE(yandex_code, '') = COALESCE(@yandex_code, '')",
                @"INSERT INTO codes (yandex_code, esr_code) 
                  SELECT @yandex_code, @esr_code 
                  WHERE NOT EXISTS (
                      SELECT 1 FROM codes 
                      WHERE COALESCE(esr_code, '') = COALESCE(@esr_code, '') 
                      AND COALESCE(yandex_code, '') = COALESCE(@yandex_code, '')
                  )",
                yandexCode,
                esrCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении/получении кода: {ex.Message}");
            throw;
        }
    }

    private long ExecuteCodeOperation(string selectSql, string insertSql, string? yandexCode, string? esrCode)
    {
        try
        {
            Console.WriteLine($"Запуск ExecuteCodeOperation: yandexCode={yandexCode}, esrCode={esrCode}");
            // Пробуем найти существующий код
            using (var selectCommand = _connection!.CreateCommand())
            {
                selectCommand.CommandText = selectSql;
                selectCommand.Parameters.AddWithValue("@esr_code", esrCode ?? string.Empty);
                selectCommand.Parameters.AddWithValue("@yandex_code", yandexCode ?? string.Empty);

                var result = selectCommand.ExecuteScalar();
                if (result != null && long.TryParse(result.ToString(), out var existingId))
                {
                    return existingId;
                }
            }

            // Если не найден, создаем новый
            using (var insertCommand = _connection.CreateCommand())
            {
                insertCommand.CommandText = insertSql;
                insertCommand.Parameters.AddWithValue("@yandex_code", yandexCode ?? string.Empty);
                insertCommand.Parameters.AddWithValue("@esr_code", esrCode ?? string.Empty);
                insertCommand.ExecuteNonQuery();
            }

            // Получаем ID вставленной записи
            using (var lastIdCommand = _connection.CreateCommand())
            {
                lastIdCommand.CommandText = "SELECT last_insert_rowid()";
                var result = lastIdCommand.ExecuteScalar();
                return result != null ? Convert.ToInt64(result) : 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка выполнения операции с кодом: {ex.Message}");
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
            Console.WriteLine($"Запуск AddCountry: codeId={codeId}, title={title}");
            ArgumentNullException.ThrowIfNull(title);
            
            if (_connection == null) 
                throw new InvalidOperationException("Соединение не открыто");

            return ExecuteAddOrUpdateEntity(
                "country",
                codeId,
                title,
                "SELECT id FROM country WHERE codeid = @codeid",
                "UPDATE country SET title = @title WHERE id = @id",
                "INSERT INTO country (title, codeid) VALUES (@title, @codeid)");
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
            Console.WriteLine($"Запуск AddRegion: codeId={codeId}, title={title}");
            ArgumentNullException.ThrowIfNull(title);
            
            if (_connection == null) 
                throw new InvalidOperationException("Соединение не открыто");

            return ExecuteAddOrUpdateEntity(
                "region",
                codeId,
                title,
                "SELECT id FROM region WHERE codeid = @codeid",
                "UPDATE region SET title = @title WHERE id = @id",
                "INSERT INTO region (title, codeid) VALUES (@title, @codeid)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении/обновлении региона: {ex.Message}");
            throw;
        }
    }

    private long ExecuteAddOrUpdateEntity(string tableName, long codeId, string title, 
        string selectSql, string updateSql, string insertSql)
    {
        try
        {
            Console.WriteLine($"Запуск ExecuteAddOrUpdateEntity: tableName={tableName}, codeId={codeId}, title={title}");
            // Проверяем существование
            using (var selectCommand = _connection!.CreateCommand())
            {
                selectCommand.CommandText = selectSql;
                selectCommand.Parameters.AddWithValue("@codeid", codeId);

                var result = selectCommand.ExecuteScalar();
                if (result != null && long.TryParse(result.ToString(), out var existingId))
                {
                    // Обновляем запись
                    using (var updateCommand = _connection.CreateCommand())
                    {
                        updateCommand.CommandText = updateSql;
                        updateCommand.Parameters.AddWithValue("@title", title);
                        updateCommand.Parameters.AddWithValue("@id", existingId);
                        updateCommand.ExecuteNonQuery();
                    }
                    return existingId;
                }
            }

            // Создаем новую запись
            using (var insertCommand = _connection.CreateCommand())
            {
                insertCommand.CommandText = insertSql;
                insertCommand.Parameters.AddWithValue("@title", title);
                insertCommand.Parameters.AddWithValue("@codeid", codeId);
                insertCommand.ExecuteNonQuery();
            }

            // Возвращаем ID
            using (var lastIdCommand = _connection.CreateCommand())
            {
                lastIdCommand.CommandText = "SELECT last_insert_rowid()";
                var result = lastIdCommand.ExecuteScalar();
                return result != null ? Convert.ToInt64(result) : 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка выполнения операции сущности {tableName}: {ex.Message}");
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
            Console.WriteLine($"Запуск AddRegionToCountry: countryId={countryId}, regionId={regionId}");
            if (_connection == null) 
                throw new InvalidOperationException("Соединение не открыто");

            ExecuteRelationshipOperation(
                "country_regions",
                "regionid",
                "countryid",
                countryId,
                regionId);
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
            Console.WriteLine($"Запуск AddSettlement: codeId={codeId}, title={title}");
            ArgumentNullException.ThrowIfNull(title);
            
            if (_connection == null) 
                throw new InvalidOperationException("Соединение не открыто");

            return ExecuteAddOrUpdateEntity(
                "settlement",
                codeId,
                title,
                "SELECT id FROM settlement WHERE codeid = @codeid",
                "UPDATE settlement SET title = @title WHERE id = @id",
                "INSERT INTO settlement (title, codeid) VALUES (@title, @codeid)");
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
            Console.WriteLine($"Запуск AddSettlementToRegion: regionId={regionId}, settlementId={settlementId}");
            if (_connection == null) 
                throw new InvalidOperationException("Соединение не открыто");

            ExecuteRelationshipOperation(
                "region_settlements",
                "settlementid",
                "regionid",
                regionId,
                settlementId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении связи поселения с регионом: {ex.Message}");
            throw;
        }
    }

    private void ExecuteRelationshipOperation(string tableName, string deleteKeyColumn, 
        string insertKeyColumn, long keyValue1, long keyValue2)
    {
        try
        {
            Console.WriteLine($"Запуск ExecuteRelationshipOperation: tableName={tableName}, deleteKeyColumn={deleteKeyColumn}, insertKeyColumn={insertKeyColumn}, keyValue1={keyValue1}, keyValue2={keyValue2}");
            
            using var command = _connection!.CreateCommand();
            command.CommandText = $@"
                DELETE FROM {tableName} WHERE {deleteKeyColumn} = @{deleteKeyColumn};
                INSERT INTO {tableName} ({insertKeyColumn}, {deleteKeyColumn}) VALUES (@keyValue1, @keyValue2);";
            
            command.Parameters.AddWithValue($"@{deleteKeyColumn}", keyValue2);
            command.Parameters.AddWithValue("@keyValue1", keyValue1);
            command.Parameters.AddWithValue("@keyValue2", keyValue2);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка выполнения операции связи {tableName}: {ex.Message}");
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
            Console.WriteLine($"Запуск AddStation: codeId={codeId}, direction={direction}, stationType={stationType}, title={title}, longitude={longitude}, transportType={transportType}, latitude={latitude}");
            if (_connection == null) 
                throw new InvalidOperationException("Соединение не открыто");

            // Проверяем существование по codeid
            using (var selectCommand = _connection.CreateCommand())
            {
                selectCommand.CommandText = "SELECT id FROM station WHERE codeid = @codeid";
                selectCommand.Parameters.AddWithValue("@codeid", codeId);

                var result = selectCommand.ExecuteScalar();
                if (result != null && long.TryParse(result.ToString(), out var existingId))
                {
                    UpdateStation(existingId, direction, stationType, title, longitude, transportType, latitude);
                    return existingId;
                }
            }

            InsertStation(codeId, direction, stationType, title, longitude, transportType, latitude);

            // Возвращаем ID
            using (var lastIdCommand = _connection.CreateCommand())
            {
                lastIdCommand.CommandText = "SELECT last_insert_rowid()";
                var result = lastIdCommand.ExecuteScalar();
                return result != null ? Convert.ToInt64(result) : 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении/обновлении станции: {ex.Message}");
            throw;
        }
    }

    private void UpdateStation(long id, string? direction, string? stationType, string? title,
        double? longitude, string? transportType, double? latitude)
    {
        try
        {
            Console.WriteLine($"Запуск UpdateStation: id={id}, direction={direction}, stationType={stationType}, title={title}, longitude={longitude}, transportType={transportType}, latitude={latitude}");
            using var command = _connection!.CreateCommand();
            command.CommandText = @"
                UPDATE station 
                SET direction = @direction,
                    station_type = @station_type,
                    title = @title,
                    longitude = @longitude,
                    transport_type = @transport_type,
                    latitude = @latitude
                WHERE id = @id";

            command.Parameters.AddWithValue("@direction", (object?)direction ?? DBNull.Value);
            command.Parameters.AddWithValue("@station_type", (object?)stationType ?? DBNull.Value);
            command.Parameters.AddWithValue("@title", (object?)title ?? DBNull.Value);
            command.Parameters.AddWithValue("@longitude", (object?)longitude ?? DBNull.Value);
            command.Parameters.AddWithValue("@transport_type", (object?)transportType ?? DBNull.Value);
            command.Parameters.AddWithValue("@latitude", (object?)latitude ?? DBNull.Value);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении станции: {ex.Message}");
            throw;
        }
    }

    private void InsertStation(long codeId, string? direction, string? stationType, string? title,
        double? longitude, string? transportType, double? latitude)
    {
        try
        {
            Console.WriteLine($"Запуск InsertStation: codeId={codeId}, direction={direction}, stationType={stationType}, title={title}, longitude={longitude}, transportType={transportType}, latitude={latitude}");
            using var command = _connection!.CreateCommand();
            command.CommandText = @"
                INSERT INTO station (direction, codeid, station_type, title, longitude, transport_type, latitude) 
                VALUES (@direction, @codeid, @station_type, @title, @longitude, @transport_type, @latitude)";

            command.Parameters.AddWithValue("@direction", (object?)direction ?? DBNull.Value);
            command.Parameters.AddWithValue("@codeid", codeId);
            command.Parameters.AddWithValue("@station_type", (object?)stationType ?? DBNull.Value);
            command.Parameters.AddWithValue("@title", (object?)title ?? DBNull.Value);
            command.Parameters.AddWithValue("@longitude", (object?)longitude ?? DBNull.Value);
            command.Parameters.AddWithValue("@transport_type", (object?)transportType ?? DBNull.Value);
            command.Parameters.AddWithValue("@latitude", (object?)latitude ?? DBNull.Value);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при вставке станции: {ex.Message}");
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
            Console.WriteLine($"Запуск AddStationToSettlement: settlementId={settlementId}, stationId={stationId}");
            if (_connection == null) 
                throw new InvalidOperationException("Соединение не открыто");

            ExecuteRelationshipOperation(
                "settlement_stations",
                "station_id",
                "settlement_id",
                settlementId,
                stationId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении связи станции с поселением: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Закрывает соединение с базой данных
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (!_disposed)
            {
                _connection?.Close();
                _connection?.Dispose();
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
