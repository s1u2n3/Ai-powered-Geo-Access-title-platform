using WebApplication1.entites.Audits;

namespace WebApplication1.Services.Interfaces
{
    public interface IAuditLogsService
    {
       
    Task LogAsync(
        string action,
        string entityType,
        string? entityId,
        string description,
        string? createdBy);
    
}
}
