using System.IO;
using System.Runtime.InteropServices;
using ExamGuard.Agent.Models;

namespace ExamGuard.Agent.Services;

public class ExtensionScanner
{
    // Danh sách các ID Extension AI bị cấm (ví dụ: Grammarly, Monica, ChatGPT, QuillBot, ...)
    private readonly HashSet<string> _forbiddenExtensionIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "kbfnbcaeplbcioakkpcpgfkobkghlhen", // Grammarly (Chrome)
        "efkpnbknhmkndpldpndkngedcfoiboej", // Monica (Chrome)
        "fdpohaocaechififmbbbbbknoalclacl", // ChatGPT for Google (Chrome)
        // ... có thể bổ sung thêm
    };

    public List<DetectedApp> Scan()
    {
        var detectedApps = new List<DetectedApp>();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Extension path varies by OS. For now, support Windows.
            return detectedApps;
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        var browserConfigs = new[]
        {
            new { Name = "Google Chrome", ProcessName = "chrome", Path = Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Extensions") },
            new { Name = "Microsoft Edge", ProcessName = "msedge", Path = Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Extensions") }
        };

        foreach (var browser in browserConfigs)
        {
            // Chỉ quét nếu trình duyệt đang chạy
            var processes = System.Diagnostics.Process.GetProcessesByName(browser.ProcessName);
            if (processes.Length == 0) continue;

            if (Directory.Exists(browser.Path))
            {
                try
                {
                    var extensionDirs = Directory.GetDirectories(browser.Path);
                    foreach (var extDir in extensionDirs)
                    {
                        var extId = new DirectoryInfo(extDir).Name;
                        if (_forbiddenExtensionIds.Contains(extId))
                        {
                            detectedApps.Add(new DetectedApp
                            {
                                Name = $"{browser.Name} Extension ({extId})",
                                Description = "Phát hiện Extension AI bị cấm"
                            });
                        }
                    }
                }
                catch
                {
                    // Ignore inaccessible directories
                }
            }
        }

        return detectedApps;
    }
}
