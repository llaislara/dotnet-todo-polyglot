using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<TaskItem> Tasks, int TotalCount)> GetAllAsync(int? userId, string? status, DateTime? dueDate, int page, int pageSize)
    {
        var query = _context.Tasks.Where(t => t.DeletedAt == null).AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (dueDate.HasValue)
        {
            query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == dueDate.Value.Date);
        }

        var totalCount = await query.CountAsync();
        var tasks = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (tasks, totalCount);
    }

    public async Task<TaskItem?> GetByIdAsync(int id, int? userId)
    {
        var query = _context.Tasks.Where(t => t.Id == id && t.DeletedAt == null);
        
        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }
        
        return await query.FirstOrDefaultAsync();
    }

    public async Task<TaskItem> AddAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        task.DeletedAt = DateTime.UtcNow; 
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ShareTaskAsync(int taskId, int ownerId, string sharedWithEmail)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == ownerId);
        if (task == null) return false;

        var userToShare = await _context.Users.FirstOrDefaultAsync(u => u.Email == sharedWithEmail);
        if (userToShare == null) throw new Exception("Usuário destino não encontrado com este e-mail.");

        var alreadyShared = await _context.TaskShares.AnyAsync(ts => ts.TaskId == taskId && ts.SharedWithUserId == userToShare.Id);
        
        if (!alreadyShared)
        {
            _context.TaskShares.Add(new TaskShare { TaskId = taskId, SharedWithUserId = userToShare.Id });
            await _context.SaveChangesAsync();
        }
        
        return true;
    }

    public async Task<IEnumerable<TaskItem>> GetSharedTasksAsync(int userId)
    {
        var sharedTaskIds = await _context.TaskShares
            .Where(ts => ts.SharedWithUserId == userId)
            .Select(ts => ts.TaskId)
            .ToListAsync();

        return await _context.Tasks
            .Where(t => sharedTaskIds.Contains(t.Id) && t.DeletedAt == null)
            .ToListAsync();
    }
}