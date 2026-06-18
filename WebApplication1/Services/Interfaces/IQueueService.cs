using Amazon.SQS.Model;
using WebApplication1.Dto.DocumentJobs;

namespace WebApplication1.Services.Interfaces
{
    public interface IQueueService
    {
        Task SendMessageAsync(DocumentProcessingMessage message);

        Task<List<Message>> RecieveMessageAsync();

        Task DeleteMessageAsync(string reciepthandle);
    }
}
