using Microsoft.Extensions.Hosting;
using System.Text.Json;
using WebApplication1.Dto.DocumentJobs;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementaions
{
    public class SQSDocumentConsumers : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
   

        public  SQSDocumentConsumers(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
          


        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine(
                "documentProcess started");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var queueService =
                    scope.ServiceProvider
                        .GetRequiredService<IQueueService>();

                var messages =
                    await queueService
                        .RecieveMessageAsync();

                if (messages == null || !messages.Any())
                {
                    await Task.Delay(
                        5000,
                        stoppingToken);

                    continue;
                }

                foreach (var message in messages)
                {
                    var queueMessage =
                        JsonSerializer.Deserialize<DocumentProcessingMessage>(
                            message.Body);

                    if (queueMessage == null)
                    {
                        continue;
                    }

                    var documentProcessingService =
                        scope.ServiceProvider
                            .GetRequiredService<IDocumentProcessingService>();

                    try
                    {
                        var item =
                            await documentProcessingService
                                .GetDocumentProcessingItem(
                                    queueMessage.ItemId);

                        if (item == null)
                        {
                            await queueService
                                .DeleteMessageAsync(
                                    message.ReceiptHandle);

                            continue;
                        }

                        if (item.Status == "Completed")
                        {
                            await queueService
                                .DeleteMessageAsync(
                                    message.ReceiptHandle);

                            continue;
                        }

                        await documentProcessingService
                            .MarkProcessingAsync(
                                queueMessage.ItemId);

                        await documentProcessingService
                            .ProcessSingleParcelDocument(
                                queueMessage.S3Key);

                        await documentProcessingService
                            .MarkCompletedAsync(
                                queueMessage.ItemId);

                        await documentProcessingService
                            .UpdateJobProgressAsync(
                                queueMessage.JobId);

                        await queueService
                            .DeleteMessageAsync(
                                message.ReceiptHandle);
                    }
                    catch (Exception ex)
                    {
                        await documentProcessingService
                            .MarkFailedAsync(
                                queueMessage.ItemId,
                                ex.Message);
                    }
                }

                await Task.Delay(
                    5000,
                    stoppingToken);
            }
        }
    }
}
