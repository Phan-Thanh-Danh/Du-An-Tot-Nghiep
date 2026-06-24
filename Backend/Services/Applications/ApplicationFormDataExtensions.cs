namespace Backend.Services.Applications;

internal static class ApplicationFormDataExtensions
{
    public static bool TryGetInt(this IReadOnlyDictionary<string, object?> values, string key, out int value)
    {
        if (values.TryGetValue(key, out var raw))
        {
            switch (raw)
            {
                case int integer:
                    value = integer;
                    return true;
                case decimal number:
                    if (decimal.Truncate(number) == number &&
                        number >= int.MinValue &&
                        number <= int.MaxValue)
                    {
                        value = (int)number;
                        return true;
                    }

                    value = 0;
                    return false;
            }
        }

        value = 0;
        return false;
    }

    public static bool TryGetDecimal(this IReadOnlyDictionary<string, object?> values, string key, out decimal value)
    {
        if (values.TryGetValue(key, out var raw) && raw is decimal number)
        {
            value = number;
            return true;
        }

        value = 0;
        return false;
    }

    public static bool TryGetString(this IReadOnlyDictionary<string, object?> values, string key, out string value)
    {
        if (values.TryGetValue(key, out var raw) && raw is string text)
        {
            value = text;
            return true;
        }

        value = string.Empty;
        return false;
    }
}
