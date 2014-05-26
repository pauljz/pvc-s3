using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using PvcCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace PvcPlugins
{
    public class PvcS3 : PvcPlugin
    {

        public static string AccessKey;
        public static string SecretKey;
        public static string BucketName;
        public static RegionEndpoint RegionEndpoint = RegionEndpoint.USEast1;

        private IAmazonS3 s3client;
        private string accessKey;
        private string secretKey;
        private string bucketName;
        private RegionEndpoint regionEndpoint;

        private Dictionary<string, string> keyEtags;
        private Dictionary<string, string> keyMD5Sums;

        public PvcS3(
            string accessKey = null,
            string secretKey = null,
            string bucketName = null,
            Amazon.RegionEndpoint regionEndpoint = null)
        {
            this.accessKey = accessKey != null ? accessKey : PvcS3.AccessKey;
            this.secretKey = secretKey != null ? secretKey : PvcS3.SecretKey;
            this.bucketName = bucketName != null ? bucketName : PvcS3.BucketName;
            this.regionEndpoint = regionEndpoint != null ? regionEndpoint : PvcS3.RegionEndpoint;

            // Set up the API client for S3.
            AWSCredentials creds = new BasicAWSCredentials(this.accessKey, this.secretKey);
            this.s3client = new AmazonS3Client(creds, this.regionEndpoint);

            // Initialize some private stuff that we use to track md5 sums
            this.keyEtags = new Dictionary<string, string>();
            this.keyMD5Sums = new Dictionary<string, string>();
        }

        private string StreamNameToKey(string streamName)
        {
            return streamName.Replace('\\', '/');
        }

        private string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
        }

        private IEnumerable<PvcStream> FilterUploadedFiles(IEnumerable<PvcStream> inputStreams)
        {
            var filteredInputStreams = new List<PvcStream>();

            Console.WriteLine("Checking files in bucket {0}", this.bucketName);
            ListObjectsRequest request = new ListObjectsRequest();
            request.BucketName = this.bucketName;
            var response = this.s3client.ListObjects(request);
            foreach (var o in response.S3Objects)
            {
                keyEtags.Add(o.Key, o.ETag.Trim('"'));
            }

            foreach (var stream in inputStreams)
            {
                using (var md5 = MD5.Create())
                {
                    var md5bytes = md5.ComputeHash(stream);
                    var md5sum = ToHex(md5bytes, false);
                    var md5base64 = Convert.ToBase64String(md5bytes);
                    var key = StreamNameToKey(stream.StreamName);
                    if (!keyEtags.ContainsKey(key) || keyEtags[key] != md5sum)
                    {
                        Console.WriteLine("Including {0} {1}", key, md5sum);
                        filteredInputStreams.Add(stream);
                        keyEtags[key] = md5sum;
                        keyMD5Sums[key] = md5base64;
                    }
                    else
                    {
                        Console.WriteLine("Unchanged {0}", stream.StreamName);
                    }
                }
                stream.ResetStreamPosition();
            }
            return filteredInputStreams;
        }

        public override IEnumerable<PvcStream> Execute(IEnumerable<PvcStream> inputStreams)
        {

            var filteredInputStreams = FilterUploadedFiles(inputStreams);

            var transfer = new TransferUtility(this.s3client);

            foreach (var inputStream in filteredInputStreams)
            {
                if (inputStream.StreamName == null || inputStream.StreamName.Length == 0)
                    continue;

                var uploadReq = new TransferUtilityUploadRequest();
                uploadReq.BucketName = this.bucketName;
                uploadReq.InputStream = inputStream;
                uploadReq.Key = this.StreamNameToKey(inputStream.StreamName);
                uploadReq.Headers.ContentMD5 = this.keyMD5Sums[uploadReq.Key];

                transfer.Upload(uploadReq);
            };
            return inputStreams;
        }
    }
}
