using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class R2DbSyncTest
    {
        [Test]
        public async Task ListAndMapAllR2Videos()
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

            var listReq = new ListObjectsV2Request { BucketName = bucketName };
            var listRes = await client.ListObjectsV2Async(listReq);

            var videoObjects = listRes.S3Objects
                .Where(o => o.Key.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                            o.Key.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) ||
                            o.Key.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                .ToList();

            TestContext.Progress.WriteLine($"=== TOTAL R2 MEDIA OBJECTS: {videoObjects.Count} ===");
            foreach (var v in videoObjects)
            {
                TestContext.Progress.WriteLine($"KEY: {v.Key} | SIZE: {v.Size}");
            }
        }
    }
}
