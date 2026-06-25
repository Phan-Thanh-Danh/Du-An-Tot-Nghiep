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

            return ValidateLocalRoot(options.LocalRoot);
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

    private static ValidateOptionsResult ValidateLocalRoot(string? root)
    {
        if (!TryValidateLocalRoot(root, out var error))
        {
            return ValidateOptionsResult.Fail(error);
        }

        return ValidateOptionsResult.Success;
    }

    public static bool TryValidateLocalRoot(string? root, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(root))
        {
            error = "Application evidence local storage root is required.";
            return false;
        }

        var fullRoot = Path.GetFullPath(root.Trim());
        var pathRoot = Path.GetPathRoot(fullRoot);
        var currentDirectory = Path.GetFullPath(Directory.GetCurrentDirectory());
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var trimmedRoot = fullRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (string.Equals(trimmedRoot, pathRoot?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmedRoot, currentDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmedRoot, Path.Combine(currentDirectory, "Backend").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmedRoot, Path.Combine(currentDirectory, "Backend", "wwwroot").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmedRoot, home.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            !Path.GetFileName(fullRoot).Contains("LMS_TEST_", StringComparison.OrdinalIgnoreCase))
        {
            error = "Application evidence local storage root must be a dedicated LMS_TEST_ directory.";
            return false;
        }

        return true;
    }
}
