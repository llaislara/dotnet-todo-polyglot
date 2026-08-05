using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly AppDbContext _context;

    public AuditRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int? userId, string action, string entityType, int? entityId, string details)
    {
        var brazilTime = DateTime.UtcNow.AddHours(-3);

        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAt = brazilTime
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

public async Task<IEnumerable<AuditLog>> GetLogsAsync(int currentUserId, bool isAdmin, DateTime? date, int? filterUserId, string? role, string? action)
    {
        var query = _context.AuditLogs.Include(a => a.User).AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(a => 
                a.UserId == currentUserId ||
                (a.EntityType == "User" && a.EntityId == currentUserId) ||(ex: um admin promovendo ele)
                (a.EntityType == "Task" && _context.Tasks.Any(t => t.Id == a.EntityId && t.UserId == currentUserId)) || 
                (a.EntityType == "Task" && _context.TaskShares.Any(ts => ts.TaskId == a.EntityId && ts.SharedWithUserId == currentUserId))
            );
        }

        if (date.HasValue)
            query = query.Where(a => a.CreatedAt.Date == date.Value.Date);

        if (filterUserId.HasValue)
            query = query.Where(a => a.UserId == filterUserId.Value);

        if (!string.IsNullOrEmpty(role))
            query = query.Where(a => a.User != null && a.User.Role == role);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    
}