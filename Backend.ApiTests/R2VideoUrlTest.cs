using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class R2VideoUrlTest
    {
        [Test]
        public async Task TestGeneratePresignedUrlAndAccess()
        {
            var endpoint = "https://87934b0fb36afe0a6b19db75efc7fe24.r2.cloudflarestorage.com";
            var accessKey = "872e796be9c27223e4d2b7fe48afd75e";
            var secretKey = "46a0c09da41ff2f0a7cc7aacad3bb8ed6c418eb4530617862c860d248bf2e28b";
            var bucketName = "aet-lms-media";

            var config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                AuthenticationRegion = "auto"
            };

            using var client = new AmazonS3Client(accessKey, secretKey, config);

            var sampleKey = "videos/2026/08/15/CNTT/SQL/BÀI 1/Giới thiệu tổng quan về SQL và hệ quản trị SQL Server.mp4";

            // 1. Check Presigned URL
            var presignedRequest = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = sampleKey,
                Expires = DateTime.UtcNow.AddHours(2)
            };

            var presignedUrl = client.GetPreSignedURL(presignedRequest);
            TestContext.Progress.WriteLine($"[PRESIGNED_URL]: {presignedUrl}");

            using var httpClient = new HttpClient();
            var headRes = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, presignedUrl));
            TestContext.Progress.WriteLine($"Head Response Status: {headRes.StatusCode}");
            TestContext.Progress.WriteLine($"Content Length: {headRes.Content.Headers.ContentLength} bytes");
            TestContext.Progress.WriteLine($"Content Type: {headRes.Content.Headers.ContentType}");

            // 2. Direct S3 GetObject
            var getObj = await client.GetObjectAsync(bucketName, sampleKey);
            TestContext.Progress.WriteLine($"GetObject Success: Size = {getObj.ContentLength}, ContentType = {getObj.Headers.ContentType}");
        }
    }
}
