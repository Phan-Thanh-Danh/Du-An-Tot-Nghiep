using Amazon.S3;
using Amazon.S3.Model;

namespace Backend.Services.Storage;

public class R2ApplicationEvidenceObjectStore : IApplicationEvidenceObjectStore
{
    private readonly IAmazonS3 _s3Client;
    private readonly R2StorageSettings _settings;

    public R2ApplicationEvidenceObjectStore(R2StorageSettings settings)
    {
        _settings = settings;
        var s3Config = new AmazonS3Config
        {
            ServiceURL = settings.Endpoint,
            ForcePathStyle = true,
            Timeout = TimeSpan.FromMinutes(2)
        };

        _s3Client = new AmazonS3Client(settings.AccessKeyId, settings.SecretAccessKey, s3Config);
    }

    public async Task StoreAsync(
        string storageKey,
        Stream content,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = storageKey,
                InputStream = content,
                ContentType = contentType,
                DisablePayloadSigning = true
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);
            if ((int)response.HttpStatusCode < 200 || (int)response.HttpStatusCode >= 300)
            {
                throw new ApplicationEvidenceStorageException("Application evidence object storage failed.");
            }
        }
        catch (ApplicationEvidenceStorageException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ApplicationEvidenceStorageException("Application evidence object storage failed.", exception);
        }
    }

    public async Task<ApplicationEvidenceObjectReadResult> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(_settings.BucketName, storageKey, cancellationToken);
            return new ApplicationEvidenceObjectReadResult
            {
                Content = response.ResponseStream,
                ContentLength = response.ContentLength,
                ContentType = response.Headers.ContentType
            };
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new ApplicationEvidenceObjectNotFoundException("Application evidence object not found.", exception);
        }
        catch (Exception exception)
        {
            throw new ApplicationEvidenceStorageException("Cannot read application evidence object.", exception);
        }
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(_settings.BucketName, storageKey, cancellationToken);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new ApplicationEvidenceObjectNotFoundException("Application evidence object not found.", exception);
        }
        catch (Exception exception)
        {
            throw new ApplicationEvidenceStorageException("Cannot delete application evidence object.", exception);
        }
    }
}
