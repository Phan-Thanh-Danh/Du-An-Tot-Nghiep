using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Exceptions;
using Microsoft.Extensions.Options;

namespace Backend.Services.Finance.TuitionPayments;

public class PayOsService : IPayOsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions CompactJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly PayOSOptions _options;

    public PayOsService(HttpClient httpClient, IOptions<PayOSOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<PayOsCreatePaymentLinkResult> CreatePaymentLinkAsync(
        PayOsCreatePaymentLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateSettings();

        var amount = ToVndAmount(request.Amount);
        var returnUrl = _options.ReturnUrl.Trim();
        var cancelUrl = _options.CancelUrl.Trim();
        var description = request.Description.Trim();
        var signature = CreatePaymentRequestSignature(amount, cancelUrl, description, request.OrderCode, returnUrl);

        var payload = new
        {
            orderCode = request.OrderCode,
            amount,
            description,
            items = new[]
            {
                new
                {
                    name = string.IsNullOrWhiteSpace(request.ItemName) ? "Học phí" : request.ItemName,
                    quantity = 1,
                    price = amount
                }
            },
            cancelUrl,
            returnUrl,
            signature
        };

        var requestPayloadJson = JsonSerializer.Serialize(payload, JsonOptions);
        var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/v2/payment-requests";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(requestPayloadJson, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Add("x-client-id", _options.ClientId);
        httpRequest.Headers.Add("x-api-key", _options.ApiKey);

        string responsePayloadJson;
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            responsePayloadJson = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new PayOsProviderException("Không thể kết nối PayOS. Vui lòng thử lại sau.", innerException: exception);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new PayOsProviderException("PayOS từ chối yêu cầu tạo link thanh toán.", responsePayloadJson);
        }

        using var document = ParseJson(responsePayloadJson);
        var root = document.RootElement;
        var code = GetString(root, "code");
        var desc = GetString(root, "desc");

        if (!string.Equals(code, "00", StringComparison.OrdinalIgnoreCase))
        {
            throw new PayOsProviderException(
                string.IsNullOrWhiteSpace(desc) ? "PayOS không tạo được link thanh toán." : desc,
                responsePayloadJson);
        }

        if (!root.TryGetProperty("data", out var data))
        {
            throw new PayOsProviderException("PayOS trả về dữ liệu không hợp lệ.", responsePayloadJson);
        }

        var checkoutUrl = GetString(data, "checkoutUrl");
        if (string.IsNullOrWhiteSpace(checkoutUrl))
        {
            throw new PayOsProviderException("PayOS không trả về checkoutUrl.", responsePayloadJson);
        }

        return new PayOsCreatePaymentLinkResult
        {
            CheckoutUrl = checkoutUrl,
            QrPayload = GetString(data, "qrCode"),
            PaymentLinkId = GetString(data, "paymentLinkId"),
            BankCode = GetString(data, "bin"),
            AccountNumber = GetString(data, "accountNumber"),
            AccountName = GetString(data, "accountName"),
            Amount = GetLong(data, "amount") ?? amount,
            Description = GetString(data, "description"),
            RequestPayloadJson = requestPayloadJson,
            ResponsePayloadJson = responsePayloadJson
        };
    }

    public async Task<PayOsPaymentLinkStatusResult> GetPaymentLinkAsync(
        long orderCode,
        CancellationToken cancellationToken = default)
    {
        ValidateSettings();

        var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/v2/payment-requests/{orderCode.ToString(CultureInfo.InvariantCulture)}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Add("x-client-id", _options.ClientId);
        httpRequest.Headers.Add("x-api-key", _options.ApiKey);

        string responsePayloadJson;
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            responsePayloadJson = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new PayOsProviderException("Không thể kết nối PayOS để kiểm tra trạng thái thanh toán.", innerException: exception);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new PayOsProviderException("PayOS từ chối yêu cầu kiểm tra trạng thái thanh toán.", responsePayloadJson);
        }

        using var document = ParseJson(responsePayloadJson);
        var root = document.RootElement;
        var code = GetString(root, "code");
        var desc = GetString(root, "desc");

        if (!string.Equals(code, "00", StringComparison.OrdinalIgnoreCase))
        {
            throw new PayOsProviderException(
                string.IsNullOrWhiteSpace(desc) ? "PayOS không trả về trạng thái thanh toán hợp lệ." : desc,
                responsePayloadJson);
        }

        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
        {
            throw new PayOsProviderException("PayOS trả về dữ liệu trạng thái không hợp lệ.", responsePayloadJson);
        }

        return new PayOsPaymentLinkStatusResult
        {
            OrderCode = GetLong(data, "orderCode") ?? orderCode,
            Amount = GetDecimal(data, "amount"),
            AmountPaid = GetDecimal(data, "amountPaid"),
            AmountRemaining = GetDecimal(data, "amountRemaining"),
            Status = GetString(data, "status"),
            PaidAt = ExtractPaymentDate(data),
            ResponsePayloadJson = responsePayloadJson
        };
    }

    public bool VerifyWebhookSignature(JsonElement data, string signature)
    {
        ValidateSettings();

        if (string.IsNullOrWhiteSpace(signature) || data.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var dataToSign = ConvertObjectToQueryString(data);
        var computedSignature = CreateHmacSha256(dataToSign, _options.ChecksumKey);
        var expectedBytes = Encoding.ASCII.GetBytes(computedSignature);
        var providedBytes = Encoding.ASCII.GetBytes(signature.Trim().ToLowerInvariant());

        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private string CreatePaymentRequestSignature(
        long amount,
        string cancelUrl,
        string description,
        long orderCode,
        string returnUrl)
    {
        var data = "amount=" + amount.ToString(CultureInfo.InvariantCulture)
            + "&cancelUrl=" + cancelUrl
            + "&description=" + description
            + "&orderCode=" + orderCode.ToString(CultureInfo.InvariantCulture)
            + "&returnUrl=" + returnUrl;

        return CreateHmacSha256(data, _options.ChecksumKey);
    }

    private static string ConvertObjectToQueryString(JsonElement data)
    {
        return string.Join(
            "&",
            data.EnumerateObject()
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => $"{property.Name}={ConvertJsonValueToSignatureValue(property.Value)}"));
    }

    private static string ConvertJsonValueToSignatureValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
            JsonValueKind.Array => JsonSerializer.Serialize(
                value.EnumerateArray().Select(ToSortedSerializableValue).ToList(),
                CompactJsonOptions),
            JsonValueKind.Object => JsonSerializer.Serialize(ToSortedSerializableValue(value), CompactJsonOptions),
            _ => value.GetRawText()
        };
    }

    private static object? ToSortedSerializableValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Object => value.EnumerateObject()
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .ToDictionary(
                    property => property.Name,
                    property => ToSortedSerializableValue(property.Value),
                    StringComparer.Ordinal),
            JsonValueKind.Array => value.EnumerateArray().Select(ToSortedSerializableValue).ToList(),
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number when value.TryGetInt64(out var longValue) => longValue,
            JsonValueKind.Number when value.TryGetDecimal(out var decimalValue) => decimalValue,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => value.GetRawText()
        };
    }

    private static string CreateHmacSha256(string data, string checksumKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(checksumKey);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static JsonDocument ParseJson(string json)
    {
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException exception)
        {
            throw new PayOsProviderException("PayOS trả về JSON không hợp lệ.", json, exception);
        }
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId)
            || string.IsNullOrWhiteSpace(_options.ApiKey)
            || string.IsNullOrWhiteSpace(_options.ChecksumKey)
            || string.IsNullOrWhiteSpace(_options.ReturnUrl)
            || string.IsNullOrWhiteSpace(_options.CancelUrl)
            || string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình PayOS chưa đầy đủ.");
        }
    }

    private static long ToVndAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tiền thanh toán phải lớn hơn 0.");
        }

        if (amount != decimal.Truncate(amount))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tiền thanh toán PayOS phải là số VND nguyên.");
        }

        return decimal.ToInt64(amount);
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ToString()
            : string.Empty;
    }

    private static long? GetLong(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number))
        {
            return number;
        }

        return long.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static decimal GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return 0m;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
        {
            return number;
        }

        return decimal.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0m;
    }

    private static DateTime? ExtractPaymentDate(JsonElement data)
    {
        var paidAt = GetDateTime(data, "paidAt")
            ?? GetDateTime(data, "updatedAt");

        if (data.TryGetProperty("transactions", out var transactions))
        {
            paidAt = ExtractDateFromTransactions(transactions) ?? paidAt;
        }

        return paidAt;
    }

    private static DateTime? ExtractDateFromTransactions(JsonElement transactions)
    {
        if (transactions.ValueKind == JsonValueKind.Array)
        {
            foreach (var transaction in transactions.EnumerateArray())
            {
                var paidAt = GetDateTime(transaction, "transactionDateTime")
                    ?? GetDateTime(transaction, "createdAt")
                    ?? GetDateTime(transaction, "paidAt");

                if (paidAt.HasValue)
                {
                    return paidAt;
                }
            }
        }

        if (transactions.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in transactions.EnumerateObject())
            {
                var paidAt = GetDateTime(property.Value, "transactionDateTime")
                    ?? GetDateTime(property.Value, "createdAt")
                    ?? GetDateTime(property.Value, "paidAt");

                if (paidAt.HasValue)
                {
                    return paidAt;
                }
            }
        }

        return null;
    }

    private static DateTime? GetDateTime(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return DateTime.TryParse(
            value.ToString(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out var parsed)
            ? parsed
            : null;
    }
}
