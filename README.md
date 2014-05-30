pvc-s3
========

Amazon S3 Plugin for PVC

Example Usage
-------------

```
pvc.Source("*.png")
   .Pipe(new PvcS3(
        accessKey: "abcd123456",
        secretKey: "abcdefg\12345\abcdefgh",
        bucketName: "pvcs3test"
))
```

Gzipped Files
-------------

If the S3 plugin sees files that have been tagged with "gzip" (e.g. from the [pvc-gzip](https://github.com/pauljz/pvc-gzip) plugin), it will attach a Content-Encoding: gzip header automatically. Make sure to use `new PvcGzip(addExtension: false)`
