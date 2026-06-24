using Microsoft.Extensions.Options;

namespace Backend.Services.Storage;

public class LocalApplicationEvidenceObjectStore : IApplicationEvidenceObjectStore
{
    private readonly string _root;

    public LocalApplicationEvidenceObjectStore(
        IOptions<ApplicationEvidenceStorageOptions> options,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment() &&
            !string.Equals(environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Local application evidence storage is only allowed in Development or Testing.");
        }

        _root = ValidateRoot(options.Value.LocalRoot);
        Directory.CreateDirectory(_root);
    }

    public async Task StoreAsync(
        string storageKey,
        Stream content,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        if (File.Exists(path))
        {
            throw new ApplicationEvidenceStorageException("Object already exists.");
        }

        var tempPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var output = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await content.CopyToAsync(output, cancellationToken);
            }

            File.Move(tempPath, path, overwrite: false);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            SafeDelete(tempPath);
            throw new ApplicationEvidenceStorageException("Cannot store application evidence object.", exception);
        }
        catch
        {
            SafeDelete(tempPath);
            throw;
        }
    }

    public Task<ApplicationEvidenceObjectReadResult> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageKey);
        if (!File.Exists(path))
        {
            throw new ApplicationEvidenceObjectNotFoundException("Application evidence object not found.");
        }

        try
        {
            var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            return Task.FromResult(new ApplicationEvidenceObjectReadResult
            {
                Content = stream,
                ContentLength = stream.Length
            });
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new ApplicationEvidenceStorageException("Cannot read application evidence object.", exception);
        }
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageKey);
        try
        {
            if (!File.Exists(path))
            {
                throw new ApplicationEvidenceObjectNotFoundException("Application evidence object not found.");
            }

            File.Delete(path);
            return Task.CompletedTask;
        }
        catch (ApplicationEvidenceObjectNotFoundException)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new ApplicationEvidenceStorageException("Cannot delete application evidence object.", exception);
        }
    }

    private string ResolvePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) ||
            storageKey.Contains('\\') ||
            storageKey.Split('/').Any(part => part is "" or "." or ".."))
        {
            throw new ApplicationEvidenceStorageException("Invalid application evidence storage key.");
        }

        var normalized = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_root, normalized));
        if (!fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApplicationEvidenceStorageException("Invalid application evidence storage key.");
        }

        return fullPath;
    }

    private static string ValidateRoot(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("Application evidence local storage root is required.");
        }

        var fullRoot = Path.GetFullPath(root.Trim());
        var pathRoot = Path.GetPathRoot(fullRoot);
        var currentDirectory = Path.GetFullPath(Directory.GetCurrentDirectory());
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (string.Equals(fullRoot.TrimEnd(Path.DirectorySeparatorChar), pathRoot?.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fullRoot.TrimEnd(Path.DirectorySeparatorChar), currentDirectory.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fullRoot.TrimEnd(Path.DirectorySeparatorChar), Path.Combine(currentDirectory, "Backend").TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fullRoot.TrimEnd(Path.DirectorySeparatorChar), Path.Combine(currentDirectory, "Backend", "wwwroot").TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fullRoot.TrimEnd(Path.DirectorySeparatorChar), home.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            !Path.GetFileName(fullRoot).Contains("LMS_DT3_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Application evidence local storage root must be a dedicated LMS_DT3_ test/development directory.");
        }

        return fullRoot.TrimEnd(Path.DirectorySeparatorChar);
    }

    private static void SafeDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // best-effort cleanup only
        }
    }
}
