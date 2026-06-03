using Amazon.S3;
using Amazon.S3.Model;

public class S3Service
{
    private readonly IConfiguration _configuration;

    public S3Service(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<S3Object>> GetParcelDocumentsAsync(
        string parcelId)
    {
        var accessKey =
            _configuration["AWS:AccessKey"];

        var secretKey =
            _configuration["AWS:SecretKey"];

        var bucketName =
            _configuration["AWS:BucketName"];

        var region =
            _configuration["AWS:Region"];

        var client =
            new AmazonS3Client(
                accessKey,
                secretKey,
                Amazon.RegionEndpoint.GetBySystemName(region));
        var normalizedParcelId =parcelId
        .Replace("P-", "parcel-")
        .ToLower();

      

        var request =
            new ListObjectsV2Request
            {
                BucketName = bucketName,


                Prefix = $"parcels/{normalizedParcelId}/"
            };

        var response =
            await client.ListObjectsV2Async(request);

        return response.S3Objects
     .Where(x => !string.IsNullOrWhiteSpace(x.Key)
         && !x.Key.EndsWith("/"))
     .ToList();
    }


    public async Task<Stream>
        GetDocumentStreamAsync(
            string s3Key)
    {
        var accessKey =
            _configuration["AWS:AccessKey"];

        var secretKey =
            _configuration["AWS:SecretKey"];

        var bucketName =
            _configuration["AWS:BucketName"];

        var region =
            _configuration["AWS:Region"];

        var client =
            new AmazonS3Client(
                accessKey,
                secretKey,
                Amazon.RegionEndpoint
                    .GetBySystemName(region));

        var request =
            new GetObjectRequest
            {
                BucketName = bucketName,
                Key = s3Key
            };

        var response =
            await client.GetObjectAsync(request);

        return response.ResponseStream;
    }


}