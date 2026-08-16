using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class R2InspectorTest
    {
        [Test]
        public async Task ListAllR2Objects()
        {
            var endpoint = "https://87934b0fb36afe0a6b19db75efc7fe24.r2.cloudflarestorage.com";
            var accessKey = "872e796be9c27223e4d2b7fe48afd75e";
            var secretKey = "46a0c09da41ff2f0a7cc7aacad3bb8ed6c418eb4530617862c860d248bf2e28b";
            var bucketName = "aet-lms-media";

            TestContext.Progress.WriteLine($"Connecting to Cloudflare R2 bucket: {bucketName}...");
            var config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                AuthenticationRegion = "auto"
            };

            using var client = new AmazonS3Client(accessKey, secretKey, config);

            var request = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            var response = await client.ListObjectsV2Async(request);
            TestContext.Progress.WriteLine($"Total Objects in R2 Bucket: {response.S3Objects.Count}");
            foreach (var obj in response.S3Objects)
            {
                TestContext.Progress.WriteLine($"[R2_OBJECT] Key: {obj.Key} | Size: {obj.Size} | LastModified: {obj.LastModified}");
            }
        }
    }
}
