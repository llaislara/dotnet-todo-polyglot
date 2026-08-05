using TodoApi.Models;

namespace TodoApi.Repositories;

public interface IAuditRepository
{
    Task LogAsync(int? userId, string action, string entityType, int? entityId, string details);
    Task<IEnumerable<AuditLog>> GetLogsAsync(int currentUserId, bool isAdmin, DateTime? date, int? filterUserId, string? role, string? action);
}