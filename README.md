pvc-s3
========

Amazon S3 Plugin for PVC

Example Usage
-------------

    pvc.Source("*.png")
	   .Pipe(new PvcS3(
	    accessKey: "abcd123456",
        secretKey: "abcdefg\12345\abcdefgh",
        bucketName: "pvcs3test"
	   ))