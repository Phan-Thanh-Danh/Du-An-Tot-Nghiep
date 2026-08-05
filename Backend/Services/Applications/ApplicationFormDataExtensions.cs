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
                case string str:
                    if (int.TryParse(str, out var parsedInt))
                    {
                        value = parsedInt;
                        return true;
                    }
                    value = 0;
                    return false;
                case System.Text.Json.JsonElement element:
                    if (element.ValueKind == System.Text.Json.JsonValueKind.Number && element.TryGetInt32(out var elementInt))
                    {
                        value = elementInt;
                        return true;
                    }
                    if (element.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(element.GetString(), out var parsedElementStr))
                    {
                        value = parsedElementStr;
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
        if (values.TryGetValue(key, out var raw))
        {
            if (raw is string text)
            {
                value = text;
                return true;
            }
            if (raw is System.Text.Json.JsonElement element && element.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var str = element.GetString();
                if (str != null)
                {
                    value = str;
                    return true;
                }
            }
        }

        value = string.Empty;
        return false;
    }
}
