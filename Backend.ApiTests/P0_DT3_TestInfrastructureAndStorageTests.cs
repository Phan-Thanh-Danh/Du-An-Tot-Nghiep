using Backend.Services.Applications;
using Backend.Services.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_DT3_TestInfrastructureAndStorageTests
{
    [Test]
    public void SharedDbGuard_MissingConnectionString_ShouldFail()
    {
        using var env = new EnvScope(("LMS_TEST_CONNECTION_STRING", null));

        Assert.Throws<AssertionException>(() => Harness.ConnectionString());
    }

    [Test]
    public void SharedDbGuard_InvalidConnectionString_ShouldFail()
    {
        using var env = new EnvScope(("LMS_TEST_CONNECTION_STRING", "not a connection string"));

        Assert.Throws<AssertionException>(() => Harness.ConnectionString());
    }

    [TestCase("LMS")]
    [TestCase("LMS_DT3_OldPrefix")]
    public void SharedDbGuard_UnsafeDatabaseName_ShouldFail(string databaseName)
    {
        var connection = BuildConnectionString(databaseName);
        using var env = new EnvScope(("LMS_TEST_CONNECTION_STRING", connection));

        Assert.Throws<AssertionException>(() => Harness.ConnectionString());
    }

    [Test]
    public void SharedDbGuard_BackendDatabaseMismatch_ShouldFail()
    {
        using var env = new EnvScope(
            ("LMS_TEST_CONNECTION_STRING", BuildConnectionString("LMS_TEST_A")),
            ("ConnectionStrings__DefaultConnection", BuildConnectionString("LMS_TEST_B")));

        Assert.Throws<AssertionException>(() => Harness.ValidateDb());
    }

    [Test]
    public void SharedDbGuard_BackendServerMismatch_ShouldFail()
    {
        using var env = new EnvScope(
            ("LMS_TEST_CONNECTION_STRING", BuildConnectionString("LMS_TEST_A", "DELL\\SQLEXPRESS02")),
            ("ConnectionStrings__DefaultConnection", BuildConnectionString("LMS_TEST_A", "localhost\\SQLEXPRESS02")));

        Assert.Throws<AssertionException>(() => Harness.ValidateDb());
    }

    [Test]
    public void SharedDbGuard_ValidLmsTestDatabase_ShouldPass()
    {
        using var env = new EnvScope(
            ("LMS_TEST_CONNECTION_STRING", BuildConnectionString("LMS_TEST_FullSuite_DT3")),
            ("ConnectionStrings__DefaultConnection", BuildConnectionString("LMS_TEST_FullSuite_DT3")));

        Assert.That(Harness.ConnectionString(), Does.Contain("LMS_TEST_FullSuite_DT3"));
        Assert.DoesNotThrow(() => Harness.ValidateDb());
    }

    [Test]
    public void SharedPassword_Missing_ShouldFail()
    {
        using var env = new EnvScope(("LMS_TEST_PASSWORD", null));

        Assert.Throws<AssertionException>(() => Harness.Password());
    }

    [Test]
    public void SharedStorageRoot_UnsafeRoot_ShouldFail()
    {
        using var env = new EnvScope(("LMS_TEST_STORAGE_ROOT", Path.GetPathRoot(Environment.CurrentDirectory)));

        Assert.Throws<AssertionException>(() => Harness.StorageRoot());
    }

    [Test]
    public void OptionsValidator_ProductionLocalProvider_ShouldFail()
    {
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Production"),
            ValidR2Settings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = SafeStorageRoot()
        });

        Assert.That(result.Failed, Is.True);
    }

    [TestCase("")]
    [TestCase(null)]
    public void OptionsValidator_R2MissingBucketOrCredentials_ShouldFail(string? missingValue)
    {
        var settings = ValidR2Settings();
        settings.BucketName = missingValue ?? string.Empty;
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Production"),
            settings);

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions { Provider = "R2" });

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void OptionsValidator_ValidTestingLocalProvider_ShouldPass()
    {
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Testing"),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = SafeStorageRoot()
        });

        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void OptionsValidator_DevelopmentLocalWithoutTestPrefix_ShouldPass()
    {
        using var paths = TestPathScope.Create();
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Development", paths.ContentRoot, paths.WebRoot),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = paths.SafeDevelopmentRoot
        });

        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void OptionsValidator_TestingLocalWithoutTestPrefix_ShouldFail()
    {
        using var paths = TestPathScope.Create();
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Testing", paths.ContentRoot, paths.WebRoot),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = paths.SafeDevelopmentRoot
        });

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void OptionsValidator_TestingLocalWithTestPrefix_ShouldPass()
    {
        using var paths = TestPathScope.Create();
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Testing", paths.ContentRoot, paths.WebRoot),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = paths.SafeTestingRoot
        });

        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void OptionsValidator_LocalInUnknownEnvironment_ShouldFail()
    {
        using var paths = TestPathScope.Create();
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Staging", paths.ContentRoot, paths.WebRoot),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = paths.SafeTestingRoot
        });

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void OptionsValidator_WebRootDescendant_ShouldFail()
    {
        using var paths = TestPathScope.Create();
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Testing", paths.ContentRoot, paths.WebRoot),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = Path.Combine(paths.WebRoot, "LMS_TEST_Evidence")
        });

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void OptionsValidator_ContentRootDescendant_ShouldFail()
    {
        using var paths = TestPathScope.Create();
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Testing", paths.ContentRoot, paths.WebRoot),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = Path.Combine(paths.ContentRoot, "private", "LMS_TEST_Evidence")
        });

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void OptionsValidator_PathPrefixSibling_ShouldNotBeTreatedAsChild()
    {
        using var paths = TestPathScope.Create();
        var siblingRoot = paths.ContentRoot + "-sibling";
        var localRoot = Path.Combine(siblingRoot, "LMS_TEST_Evidence");
        Directory.CreateDirectory(localRoot);
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Testing", paths.ContentRoot, paths.WebRoot),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = localRoot
        });

        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void OptionsValidator_FileSystemRoot_ShouldFail()
    {
        using var paths = TestPathScope.Create();
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Development", paths.ContentRoot, paths.WebRoot),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = Path.GetPathRoot(paths.ContentRoot) ?? paths.ContentRoot
        });

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void OptionsValidator_ExactHome_ShouldFail()
    {
        using var paths = TestPathScope.Create();
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Development", paths.ContentRoot, paths.WebRoot),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        });

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void OptionsValidator_InvalidPath_ShouldReturnValidationFailure()
    {
        using var paths = TestPathScope.Create();
        var validator = new ApplicationEvidenceStorageOptionsValidator(
            new TestEnvironment("Development", paths.ContentRoot, paths.WebRoot),
            new R2StorageSettings());

        var result = validator.Validate(null, new ApplicationEvidenceStorageOptions
        {
            Provider = "Local",
            LocalRoot = "invalid\0path"
        });

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public async Task LocalStore_ContentShorterThanDeclared_ShouldFailWithoutFinalFile()
    {
        var root = SafeStorageRoot();
        var store = new LocalApplicationEvidenceObjectStore(
            Options.Create(new ApplicationEvidenceStorageOptions { Provider = "Local", LocalRoot = root }),
            new TestEnvironment("Testing"));
        var key = $"applications/evidence/1/1/{Guid.NewGuid():N}.pdf";

        Assert.ThrowsAsync<ApplicationEvidenceStorageException>(async () =>
            await store.StoreAsync(key, new MemoryStream([1, 2, 3]), "application/pdf", 4));
        Assert.That(File.Exists(Path.Combine(root, key.Replace('/', Path.DirectorySeparatorChar))), Is.False);
    }

    [Test]
    public async Task LocalStore_ContentLongerThanDeclared_ShouldFailWithoutFinalFile()
    {
        var root = SafeStorageRoot();
        var store = new LocalApplicationEvidenceObjectStore(
            Options.Create(new ApplicationEvidenceStorageOptions { Provider = "Local", LocalRoot = root }),
            new TestEnvironment("Testing"));
        var key = $"applications/evidence/1/1/{Guid.NewGuid():N}.pdf";

        Assert.ThrowsAsync<ApplicationEvidenceStorageException>(async () =>
            await store.StoreAsync(key, new MemoryStream([1, 2, 3]), "application/pdf", 2));
        Assert.That(File.Exists(Path.Combine(root, key.Replace('/', Path.DirectorySeparatorChar))), Is.False);
    }

    [Test]
    public async Task LocalStore_ExactLength_ShouldStore()
    {
        var root = SafeStorageRoot();
        var store = new LocalApplicationEvidenceObjectStore(
            Options.Create(new ApplicationEvidenceStorageOptions { Provider = "Local", LocalRoot = root }),
            new TestEnvironment("Testing"));
        var key = $"applications/evidence/1/1/{Guid.NewGuid():N}.pdf";

        await store.StoreAsync(key, new MemoryStream([1, 2, 3]), "application/pdf", 3);

        Assert.That(File.Exists(Path.Combine(root, key.Replace('/', Path.DirectorySeparatorChar))), Is.True);
    }

    [Test]
    public void Inspect_TotalDeclaredSizeOver25Mb_ShouldFailBeforeOpeningStreams()
    {
        var files = Enumerable.Range(1, 3)
            .Select(i => new ThrowingOpenFormFile($"file-{i}.pdf", 9L * 1024 * 1024))
            .Cast<IFormFile>()
            .ToList();
        var inspector = new ApplicationEvidenceFileInspector();

        var exception = Assert.ThrowsAsync<Backend.Exceptions.ApiException>(async () =>
            await inspector.InspectAsync(files));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(files.Cast<ThrowingOpenFormFile>().All(x => x.OpenReadStreamCalls == 0), Is.True);
        });
    }

    private static string BuildConnectionString(string databaseName, string server = @"DELL\SQLEXPRESS02")
    {
        return $"Server={server};Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
    }

    private static R2StorageSettings ValidR2Settings()
    {
        return new R2StorageSettings
        {
            Endpoint = "https://example.invalid",
            AccessKeyId = "access",
            SecretAccessKey = "secret",
            BucketName = "bucket"
        };
    }

    private static string SafeStorageRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"LMS_TEST_Storage_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private sealed class Harness : ApiTestBase
    {
        public static string ConnectionString() => GetSharedTestConnectionString();
        public static string Password() => GetSharedTestPassword();
        public static string StorageRoot() => GetSharedTestStorageRoot();
        public static void ValidateDb() => ValidateSharedBackendDatabase();
    }

    private sealed class EnvScope : IDisposable
    {
        private readonly Dictionary<string, string?> _oldValues = new(StringComparer.OrdinalIgnoreCase);

        public EnvScope(params (string Key, string? Value)[] values)
        {
            foreach (var (key, value) in values)
            {
                _oldValues[key] = Environment.GetEnvironmentVariable(key);
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        public void Dispose()
        {
            foreach (var (key, value) in _oldValues)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public TestEnvironment(
            string environmentName,
            string? contentRootPath = null,
            string? webRootPath = null)
        {
            EnvironmentName = environmentName;
            ContentRootPath = contentRootPath ?? Environment.CurrentDirectory;
            WebRootPath = webRootPath ?? Path.Combine(ContentRootPath, "wwwroot");
            ContentRootFileProvider = new NullFileProvider();
            WebRootFileProvider = new NullFileProvider();
        }

        public string ApplicationName { get; set; } = "Backend.ApiTests";
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; }
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
    }

    private sealed class TestPathScope : IDisposable
    {
        private TestPathScope(string root)
        {
            Root = root;
            ContentRoot = Path.Combine(root, "RepoRoot");
            WebRoot = Path.Combine(ContentRoot, "Backend", "wwwroot");
            SafeDevelopmentRoot = Path.Combine(root, "EvidenceDev");
            SafeTestingRoot = Path.Combine(root, "LMS_TEST_Evidence");

            Directory.CreateDirectory(ContentRoot);
            Directory.CreateDirectory(WebRoot);
            Directory.CreateDirectory(SafeDevelopmentRoot);
            Directory.CreateDirectory(SafeTestingRoot);
        }

        public string Root { get; }
        public string ContentRoot { get; }
        public string WebRoot { get; }
        public string SafeDevelopmentRoot { get; }
        public string SafeTestingRoot { get; }

        public static TestPathScope Create()
        {
            return new TestPathScope(Path.Combine(Path.GetTempPath(), $"LMS_TEST_PathPolicy_{Guid.NewGuid():N}"));
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Root))
                {
                    Directory.Delete(Root, recursive: true);
                }
            }
            catch
            {
                // Test cleanup only.
            }
        }
    }

    private sealed class ThrowingOpenFormFile : IFormFile
    {
        public ThrowingOpenFormFile(string fileName, long length)
        {
            FileName = fileName;
            Length = length;
        }

        public int OpenReadStreamCalls { get; private set; }
        public string ContentType => "application/pdf";
        public string ContentDisposition => string.Empty;
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public long Length { get; }
        public string Name => "files";
        public string FileName { get; }

        public void CopyTo(Stream target)
        {
            throw new InvalidOperationException("Stream should not be opened.");
        }

        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Stream should not be opened.");
        }

        public Stream OpenReadStream()
        {
            OpenReadStreamCalls++;
            throw new InvalidOperationException("Stream should not be opened.");
        }
    }
}
