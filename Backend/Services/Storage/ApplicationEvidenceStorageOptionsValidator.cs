using Microsoft.Extensions.Options;

namespace Backend.Services.Storage;

public class ApplicationEvidenceStorageOptionsValidator : IValidateOptions<ApplicationEvidenceStorageOptions>
{
    private readonly IWebHostEnvironment _environment;
    private readonly R2StorageSettings _r2Settings;

    public ApplicationEvidenceStorageOptionsValidator(
        IWebHostEnvironment environment,
        R2StorageSettings r2Settings)
    {
        _environment = environment;
        _r2Settings = r2Settings;
    }

    public ValidateOptionsResult Validate(string? name, ApplicationEvidenceStorageOptions options)
    {
        var provider = options.Provider?.Trim();
        if (string.IsNullOrWhiteSpace(provider))
        {
            return ValidateOptionsResult.Fail("Application evidence storage provider is required.");
        }

        if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            if (_environment.IsProduction())
            {
                return ValidateOptionsResult.Fail("Local application evidence storage is not allowed in Production.");
            }

            if (_environment.IsEnvironment("Testing"))
            {
                return ValidateLocalRoot(options.LocalRoot, requireTestPrefix: true);
            }

            if (_environment.IsDevelopment())
            {
                return ValidateLocalRoot(options.LocalRoot, requireTestPrefix: false);
            }

            return ValidateOptionsResult.Fail("Local application evidence storage is only allowed in Development or Testing.");
        }

        if (provider.Equals("R2", StringComparison.OrdinalIgnoreCase))
        {
            return ValidateR2Settings();
        }

        return ValidateOptionsResult.Fail("Application evidence storage provider must be R2 or Local.");
    }

    private ValidateOptionsResult ValidateR2Settings()
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(_r2Settings.Endpoint))
        {
            missing.Add("R2Storage:Endpoint");
        }

        if (string.IsNullOrWhiteSpace(_r2Settings.AccessKeyId))
        {
            missing.Add("R2Storage:AccessKeyId");
        }

        if (string.IsNullOrWhiteSpace(_r2Settings.SecretAccessKey))
        {
            missing.Add("R2Storage:SecretAccessKey");
        }

        if (string.IsNullOrWhiteSpace(_r2Settings.BucketName))
        {
            missing.Add("R2Storage:BucketName");
        }

        return missing.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("Application evidence R2 storage configuration is incomplete.");
    }

    private ValidateOptionsResult ValidateLocalRoot(string? root, bool requireTestPrefix)
    {
        if (!TryValidateLocalRoot(
                root,
                _environment.ContentRootPath,
                _environment.WebRootPath,
                requireTestPrefix,
                out var error))
        {
            return ValidateOptionsResult.Fail(error);
        }

        return ValidateOptionsResult.Success;
    }

    public static bool TryValidateLocalRoot(string? root, out string error)
    {
        return TryValidateLocalRoot(
            root,
            Directory.GetCurrentDirectory(),
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
            requireTestPrefix: true,
            out error);
    }

    public static bool TryValidateLocalRoot(
        string? root,
        string? contentRootPath,
        string? webRootPath,
        bool requireTestPrefix,
        out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(root))
        {
            error = "Application evidence local storage root is required.";
            return false;
        }

        string fullRoot;
        try
        {
            fullRoot = NormalizePath(root.Trim());
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            error = "Application evidence local storage root is invalid.";
            return false;
        }

        if (requireTestPrefix &&
            !Path.GetFileName(fullRoot).Contains("LMS_TEST_", StringComparison.OrdinalIgnoreCase))
        {
            error = "Application evidence local storage root must be a dedicated LMS_TEST_ directory in Testing.";
            return false;
        }

        if (IsFileSystemRoot(fullRoot))
        {
            error = "Application evidence local storage root must not be a filesystem root.";
            return false;
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(home) && IsSamePath(fullRoot, home))
        {
            error = "Application evidence local storage root must not be the user home directory.";
            return false;
        }

        var blockedRoots = GetBlockedRoots(contentRootPath, webRootPath);
        foreach (var blockedRoot in blockedRoots)
        {
            if (IsSameOrChildPath(fullRoot, blockedRoot))
            {
                error = "Application evidence local storage root must be outside application content and public source directories.";
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<string> GetBlockedRoots(string? contentRootPath, string? webRootPath)
    {
        var candidates = new List<string?>();
        candidates.Add(contentRootPath);
        candidates.Add(webRootPath);

        var currentDirectory = Directory.GetCurrentDirectory();
        candidates.Add(currentDirectory);
        candidates.Add(Path.Combine(currentDirectory, "Backend"));
        candidates.Add(Path.Combine(currentDirectory, "Backend", "wwwroot"));

        var repoRoot = FindRepositoryRoot(contentRootPath) ?? FindRepositoryRoot(currentDirectory);
        if (!string.IsNullOrWhiteSpace(repoRoot))
        {
            candidates.Add(repoRoot);
            candidates.Add(Path.Combine(repoRoot, "Backend"));
            candidates.Add(Path.Combine(repoRoot, "Backend", "wwwroot"));
        }

        if (!string.IsNullOrWhiteSpace(contentRootPath) &&
            string.Equals(Path.GetFileName(NormalizePath(contentRootPath)), "Backend", StringComparison.OrdinalIgnoreCase))
        {
            candidates.Add(contentRootPath);
            candidates.Add(Path.Combine(NormalizePath(contentRootPath), "wwwroot"));
            var parent = Directory.GetParent(NormalizePath(contentRootPath))?.FullName;
            candidates.Add(parent);
        }

        return candidates
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => NormalizePath(path!))
            .Distinct(GetPathComparer());
    }

    private static string? FindRepositoryRoot(string? startPath)
    {
        if (string.IsNullOrWhiteSpace(startPath))
        {
            return null;
        }

        try
        {
            var directory = new DirectoryInfo(NormalizePath(startPath));
            while (directory is not null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }

        return null;
    }

    private static bool IsFileSystemRoot(string path)
    {
        var root = Path.GetPathRoot(path);
        return !string.IsNullOrWhiteSpace(root) && IsSamePath(path, root);
    }

    private static bool IsSamePath(string path, string parent)
    {
        return string.Equals(NormalizePath(path), NormalizePath(parent), GetPathComparison());
    }

    private static bool IsSameOrChildPath(string path, string parent)
    {
        var normalizedPath = NormalizePath(path);
        var normalizedParent = NormalizePath(parent);
        if (string.Equals(normalizedPath, normalizedParent, GetPathComparison()))
        {
            return true;
        }

        var parentWithSeparator = normalizedParent.EndsWith(Path.DirectorySeparatorChar)
            ? normalizedParent
            : normalizedParent + Path.DirectorySeparatorChar;

        return normalizedPath.StartsWith(parentWithSeparator, GetPathComparison());
    }

    private static string NormalizePath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath);
        var trimmed = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.IsNullOrEmpty(trimmed) && !string.IsNullOrEmpty(root)
            ? root
            : trimmed;
    }

    private static StringComparison GetPathComparison()
    {
        return OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
    }

    private static StringComparer GetPathComparer()
    {
        return OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
    }
}
