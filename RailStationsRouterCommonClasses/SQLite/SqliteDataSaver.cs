using Microsoft.Data.Sqlite;

namespace RailStationsRouterCommonClasses.SQLite;

/// <summary>
/// Модуль для сохранения объектов из Яндекс.Расписания в локальную SQLite базу данных
/// </summary>
public class SqliteDataSaver : IDataSaver
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;

    public SqliteDataSaver(string databasePath)
    {
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
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();
        CreateTables();
    }

    /// <summary>
    /// Создает все необходимые таблицы в базе данных
    /// </summary>
    private void CreateTables()
    {
        if (_connection == null) throw new InvalidOperationException("Соединение не открыто");

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

    /// <summary>
    /// Добавляет или получает код (yandex_code, esr_code)
    /// </summary>
    public long AddCode(string? yandexCode, string? esrCode)
    {
        if (_connection == null) throw new InvalidOperationException("Соединение не открыто");

        // Пробуем найти существующий код
        using (var selectCommand = _connection.CreateCommand())
        {
            selectCommand.CommandText = @"SELECT id FROM codes 
                WHERE COALESCE(esr_code, '') = COALESCE(@esr_code, '') 
                AND COALESCE(yandex_code, '') = COALESCE(@yandex_code, '')";
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
            insertCommand.CommandText = @"INSERT INTO codes (yandex_code, esr_code) 
                SELECT @yandex_code, @esr_code 
                WHERE NOT EXISTS (
                    SELECT 1 FROM codes 
                    WHERE COALESCE(esr_code, '') = COALESCE(@esr_code, '') 
                    AND COALESCE(yandex_code, '') = COALESCE(@yandex_code, '')
                )";
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

    /// <summary>
    /// Добавляет или обновляет страну
    /// </summary>
    public long AddCountry(long codeId, string title)
    {
        if (_connection == null) throw new InvalidOperationException("Соединение не открыто");

        // Проверяем существование
        using (var selectCommand = _connection.CreateCommand())
        {
            selectCommand.CommandText = "SELECT id FROM country WHERE codeid = @codeid";
            selectCommand.Parameters.AddWithValue("@codeid", codeId);

            var result = selectCommand.ExecuteScalar();
            if (result != null && long.TryParse(result.ToString(), out var existingId))
            {
                // Обновляем запись
                using (var updateCommand = _connection.CreateCommand())
                {
                    updateCommand.CommandText = "UPDATE country SET title = @title WHERE id = @id";
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
            insertCommand.CommandText = "INSERT INTO country (title, codeid) VALUES (@title, @codeid)";
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

    /// <summary>
    /// Добавляет или обновляет регион
    /// </summary>
    public long AddRegion(long codeId, string title)
    {
        if (_connection == null) throw new InvalidOperationException("Соединение не открыто");

        // Проверяем существование
        using (var selectCommand = _connection.CreateCommand())
        {
            selectCommand.CommandText = "SELECT id FROM region WHERE codeid = @codeid";
            selectCommand.Parameters.AddWithValue("@codeid", codeId);

            var result = selectCommand.ExecuteScalar();
            if (result != null && long.TryParse(result.ToString(), out var existingId))
            {
                // Обновляем запись
                using (var updateCommand = _connection.CreateCommand())
                {
                    updateCommand.CommandText = "UPDATE region SET title = @title WHERE id = @id";
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
            insertCommand.CommandText = "INSERT INTO region (title, codeid) VALUES (@title, @codeid)";
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

    /// <summary>
    /// Добавляет связь региона со страной
    /// </summary>
    public void AddRegionToCountry(long countryId, long regionId)
    {
        if (_connection == null) throw new InvalidOperationException("Соединение не открыто");

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = @"
                DELETE FROM country_regions WHERE regionid = @regionid;
                INSERT INTO country_regions (countryid, regionid) VALUES (@countryid, @regionid);";
            command.Parameters.AddWithValue("@countryid", countryId);
            command.Parameters.AddWithValue("@regionid", regionId);
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Добавляет или обновляет поселение
    /// </summary>
    public long AddSettlement(long codeId, string title)
    {
        if (_connection == null) throw new InvalidOperationException("Соединение не открыто");

        // Проверяем существование
        using (var selectCommand = _connection.CreateCommand())
        {
            selectCommand.CommandText = "SELECT id FROM settlement WHERE codeid = @codeid";
            selectCommand.Parameters.AddWithValue("@codeid", codeId);

            var result = selectCommand.ExecuteScalar();
            if (result != null && long.TryParse(result.ToString(), out var existingId))
            {
                // Обновляем запись
                using (var updateCommand = _connection.CreateCommand())
                {
                    updateCommand.CommandText = "UPDATE settlement SET title = @title WHERE id = @id";
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
            insertCommand.CommandText = "INSERT INTO settlement (title, codeid) VALUES (@title, @codeid)";
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

    /// <summary>
    /// Добавляет связь поселения с регионом
    /// </summary>
    public void AddSettlementToRegion(long regionId, long settlementId)
    {
        if (_connection == null) throw new InvalidOperationException("Соединение не открыто");

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = @"
                DELETE FROM region_settlements WHERE settlementid = @settlementid;
                INSERT INTO region_settlements (regionid, settlementid) VALUES (@regionid, @settlementid);";
            command.Parameters.AddWithValue("@regionid", regionId);
            command.Parameters.AddWithValue("@settlementid", settlementId);
            command.ExecuteNonQuery();
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
        if (_connection == null) throw new InvalidOperationException("Соединение не открыто");

        // Проверяем существование по codeid
        using (var selectCommand = _connection.CreateCommand())
        {
            selectCommand.CommandText = "SELECT id FROM station WHERE codeid = @codeid";
            selectCommand.Parameters.AddWithValue("@codeid", codeId);

            var result = selectCommand.ExecuteScalar();
            if (result != null && long.TryParse(result.ToString(), out var existingId))
            {
                // Обновляем запись
                using (var updateCommand = _connection.CreateCommand())
                {
                    updateCommand.CommandText = @"
                        UPDATE station 
                        SET direction = @direction,
                            station_type = @station_type,
                            title = @title,
                            longitude = @longitude,
                            transport_type = @transport_type,
                            latitude = @latitude
                        WHERE id = @id";

                    updateCommand.Parameters.AddWithValue("@direction", (object?)direction ?? DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@station_type", (object?)stationType ?? DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@title", (object?)title ?? DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@longitude", (object?)longitude ?? DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@transport_type", (object?)transportType ?? DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@latitude", (object?)latitude ?? DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@id", existingId);
                    updateCommand.ExecuteNonQuery();
                }
                return existingId;
            }
        }

        // Создаем новую запись
        using (var insertCommand = _connection.CreateCommand())
        {
            insertCommand.CommandText = @"
                INSERT INTO station (direction, codeid, station_type, title, longitude, transport_type, latitude) 
                VALUES (@direction, @codeid, @station_type, @title, @longitude, @transport_type, @latitude)";

            insertCommand.Parameters.AddWithValue("@direction", (object?)direction ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("@codeid", codeId);
            insertCommand.Parameters.AddWithValue("@station_type", (object?)stationType ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("@title", (object?)title ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("@longitude", (object?)longitude ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("@transport_type", (object?)transportType ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("@latitude", (object?)latitude ?? DBNull.Value);
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

    /// <summary>
    /// Добавляет связь станции с поселением
    /// </summary>
    public void AddStationToSettlement(long settlementId, long stationId)
    {
        if (_connection == null) throw new InvalidOperationException("Соединение не открыто");

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = @"
                DELETE FROM settlement_stations WHERE station_id = @station_id;
                INSERT INTO settlement_stations (settlement_id, station_id) VALUES (@settlement_id, @station_id);";
            command.Parameters.AddWithValue("@settlement_id", settlementId);
            command.Parameters.AddWithValue("@station_id", stationId);
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Закрывает соединение с базой данных
    /// </summary>
    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
