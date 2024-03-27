using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using ImageResize.Services;
using ImageResize.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ImageResize;

public class Function
{
    private readonly IImageManipulationService _imageManipulationService = new ImageManipulationService();
    IAmazonS3 S3Client { get; set; }

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        S3Client = new AmazonS3Client();
    }

    /// <summary>
    /// Constructs an instance with a preconfigured S3 client. This can be used for testing outside of the Lambda environment.
    /// </summary>
    /// <param name="s3Client">The service client to access Amazon S3.</param>
    public Function(IAmazonS3 s3Client)
    {
        S3Client = s3Client;
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(S3Event evnt)
    {
        var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();

        int fails = 0;

        foreach (var record in eventRecords)
        {
            var s3Event = record.S3;

            if (s3Event is null)
                continue;

            LambdaLogger.Log($"Event: {s3Event}");

            try
            {
                string filePath = s3Event.Object.Key;

                LambdaLogger.Log($"Getting file: {filePath}");

                if (!FileExtensionsHelper.IsAccepted(filePath))
                {
                    LambdaLogger.Log($"File extension isn't supported - fileName: {filePath}");
                    continue;
                }

                using var bucketItem = await S3Client.GetObjectAsync(s3Event.Bucket.Name, filePath);

                if (bucketItem is null)
                    continue;

                using Stream response = bucketItem.ResponseStream;

                using var thumbnailStream = await _imageManipulationService.GetThumbnailFromImageAsync(response);
                
                var request = new PutObjectRequest()
                {
                    Key = _imageManipulationService.GetThumbnailPath(filePath),
                    BucketName = bucketItem.BucketName,
                    InputStream = thumbnailStream
                };

                LambdaLogger.Log($"Generated thumbnail key: {request.Key}");

                await S3Client.PutObjectAsync(request);
            }
            catch (Exception e)
            {
                fails++;

                LambdaLogger.Log($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                LambdaLogger.Log(e.Message);
                LambdaLogger.Log(e.StackTrace);
            }
        }

        LambdaLogger.Log($"Total: {eventRecords.Count} | Processed: {eventRecords.Count - fails} | Fails: {fails}");
    }
}