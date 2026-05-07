namespace RailStationsRouterCommonClasses;

/// <summary>
/// Статический класс для логгирования операций
/// </summary>
public static class Logger
{
    /// <summary>
    /// Флаг включения/отключения логгирования. При false выводятся только ошибки.
    /// </summary>
    public static bool IsLogging { get; set; } = true;

    /// <summary>
    /// Выводит сообщение в консоль если включено логгирование
    /// </summary>
    public static void Log(string message)
    {
        if (IsLogging)
        {
            Console.WriteLine(message);
        }
    }
}
