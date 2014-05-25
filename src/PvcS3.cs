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

namespace PvcPlugins
{
    public class PvcS3 : PvcPlugin
    {
        
        public static string AccessKey;
        public static string SecretKey;
        public static string BucketName;
        public static RegionEndpoint RegionEndpoint = RegionEndpoint.USEast1;

        private IAmazonS3 client;
        private string accessKey;
        private string secretKey;
        private string bucketName;
        private RegionEndpoint regionEndpoint;

        public PvcS3(
            string accessKey = null,
            string secretKey = null,
            string bucketName = null,
            Amazon.RegionEndpoint regionEndpoint = null)
        {
            this.regionEndpoint = regionEndpoint != null ? regionEndpoint : PvcS3.RegionEndpoint;
            this.accessKey = accessKey != null ? accessKey : PvcS3.AccessKey;
            this.secretKey = secretKey != null ? secretKey : PvcS3.SecretKey;
            this.bucketName = bucketName != null ? bucketName : PvcS3.BucketName;

            Console.WriteLine(this.accessKey);
            Console.WriteLine(this.secretKey);

            AWSCredentials creds = new BasicAWSCredentials(this.accessKey, this.secretKey);
            this.client = new AmazonS3Client(creds, this.regionEndpoint);
        }

        public override IEnumerable<PvcStream> Execute(IEnumerable<PvcStream> inputStreams)
        {
            var transfer = new TransferUtility(this.client);
            foreach(var inputStream in inputStreams)
            {
                transfer.Upload(inputStream, this.bucketName, inputStream.StreamName);
            };

            return inputStreams;
        }
    }
}
