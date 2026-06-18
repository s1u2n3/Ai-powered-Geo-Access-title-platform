using Amazon.SQS;
using Amazon.SQS.Model;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Text.Json;
using WebApplication1.Dto.DocumentJobs;
using WebApplication1.Services.Interfaces;

public class QueueService : IQueueService
{
    private readonly IConfiguration _configuration;

    public QueueService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendMessageAsync(
        DocumentProcessingMessage message)
    {
        var accessKey =
            _configuration["AWS:AccessKey"];

        var secretKey =
            _configuration["AWS:SecretKey"];

        var region =
            _configuration["AWS:Region"];

        var queueUrl =
            _configuration["AWS:SQS:QueueUrl"];

        var client =
            new AmazonSQSClient(
                accessKey,
                secretKey,
                Amazon.RegionEndpoint
                    .GetBySystemName(region));

        await client.SendMessageAsync(
            new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody =
                    JsonSerializer.Serialize(message)
            });
    }
    public async Task<List<Message>> RecieveMessageAsync()
    {
        var accessKey =
        _configuration["AWS:AccessKey"];

        var secretKey =
            _configuration["AWS:SecretKey"];

        var region =
            _configuration["AWS:Region"];

        var queueUrl =
            _configuration["AWS:SQS:QueueUrl"];

        var client =
            new AmazonSQSClient(
                accessKey,
                secretKey,
                Amazon.RegionEndpoint
                    .GetBySystemName(region));
        var response = await client.ReceiveMessageAsync(
               new ReceiveMessageRequest
               {
                   QueueUrl = queueUrl,

                   MaxNumberOfMessages = 10,

                   WaitTimeSeconds = 20
               });

      
        return response.Messages ?? new List<Message>(); ;
    }
    public async Task DeleteMessageAsync(
    string receiptHandle)
    {
        var accessKey =
            _configuration["AWS:AccessKey"];

        var secretKey =
            _configuration["AWS:SecretKey"];

        var region =
            _configuration["AWS:Region"];

        var queueUrl =
            _configuration["AWS:SQS:QueueUrl"];

        var client =
            new AmazonSQSClient(
                accessKey,
                secretKey,
                Amazon.RegionEndpoint
                    .GetBySystemName(region));

        await client.DeleteMessageAsync(
            queueUrl,
            receiptHandle);
    }
}