using WebApplication1.entites.Audits;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementaions
{
    public class AuditLogsService:IAuditLogsService
    {
        private readonly AppDbContext _context;
        public AuditLogsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
              string action,
              string entityType,
              string? entityId,
              string description,
              string? createdBy)
        {
            var log = new AuditLogs
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);

            await _context.SaveChangesAsync();
        }
    }
}
