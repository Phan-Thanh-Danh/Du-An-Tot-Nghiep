using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using ExamGuard.Agent.Models;
using ExamGuard.Agent.Services;

var processScanner = new ProcessScanner();
var extensionScanner = new ExtensionScanner();
var backendReporter = new BackendReporter();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Loopback, 17892, listenOptions =>
    {
        try
        {
            var cert = CreateSelfSignedCert();
            listenOptions.UseHttps(cert);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[agent] Warn: Could not bind HTTPS with self-signed cert ({ex.Message}), falling back to default HTTPS.");
            listenOptions.UseHttps();
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ViteCors", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("ViteCors");

static X509Certificate2 CreateSelfSignedCert()
{
    using var rsa = RSA.Create(2048);
    var request = new CertificateRequest("CN=127.0.0.1", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    
    var sanBuilder = new SubjectAlternativeNameBuilder();
    sanBuilder.AddIpAddress(IPAddress.Loopback);
    sanBuilder.AddDnsName("localhost");
    request.CertificateExtensions.Add(sanBuilder.Build());

    var cert = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(10));
    var pfxBytes = cert.Export(X509ContentType.Pkcs12, "examguard");
    return new X509Certificate2(pfxBytes, "examguard", X509KeyStorageFlags.Exportable);
}

app.MapGet("/health", () => Results.Json(new
{
    running = true,
    name = "ExamGuard.Agent",
    version = "1.0.0"
}));

app.MapPost("/check", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        var request = JsonSerializer.Deserialize<CheckRequest>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (request is null || string.IsNullOrWhiteSpace(request.SessionId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "SessionId is required." });
            return;
        }

        Console.WriteLine($"[check] received sessionId={request.SessionId} apiBaseUrl={request.ApiBaseUrl}");

        var result = processScanner.Scan();
        var extResult = extensionScanner.Scan();
        
        result.DetectedApps.AddRange(extResult);
        
        if (result.DetectedApps.Count > 0)
        {
            result.Safe = false;
            result.Status = "Unsafe";
            result.RiskScore = 90;
            Console.WriteLine($"[check] detected {result.DetectedApps.Count} remote-control/AI process(es)");
            foreach (var app in result.DetectedApps)
            {
                Console.WriteLine($"  - {app.Name}");
            }
        }
        else
        {
            Console.WriteLine("[check] no suspicious remote-control process found");
        }

        result.Message = result.Safe
            ? "Không phát hiện ứng dụng điều khiển từ xa."
            : "Phát hiện ứng dụng điều khiển từ xa.";

        var reportSucceeded = false;
        if (!string.IsNullOrWhiteSpace(request.ApiBaseUrl))
        {
            reportSucceeded = await backendReporter.ReportAsync(request.ApiBaseUrl, request.SessionId, result);
        }

        var response = new
        {
            success = true,
            sessionId = request.SessionId,
            safe = result.Safe,
            status = result.Status,
            riskScore = result.RiskScore,
            message = result.Message,
            detectedApps = result.DetectedApps,
            backendReported = reportSucceeded
        };

        await context.Response.WriteAsJsonAsync(response);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[check] error: {ex.Message}");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { success = false, error = ex.Message });
    }
});

Console.WriteLine("[agent] starting ExamGuard.Agent on https://127.0.0.1:17892");
app.Run();
