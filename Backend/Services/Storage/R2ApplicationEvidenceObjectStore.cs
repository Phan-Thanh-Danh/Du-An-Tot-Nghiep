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
            request.Headers.ContentLength = contentLength;

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
                Content = new OwnedS3ResponseStream(response),
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

    private sealed class OwnedS3ResponseStream : Stream
    {
        private readonly GetObjectResponse _response;
        private readonly Stream _inner;

        public OwnedS3ResponseStream(GetObjectResponse response)
        {
            _response = response;
            _inner = response.ResponseStream;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inner.Read(buffer, offset, count);
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            return await _inner.ReadAsync(buffer, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _response.Dispose();
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            _response.Dispose();
            await base.DisposeAsync();
        }
    }
}
