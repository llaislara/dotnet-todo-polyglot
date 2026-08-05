using TodoApi.Models;

namespace TodoApi.Repositories;

public interface ITaskRepository
{
    Task<(IEnumerable<TaskItem> Tasks, int TotalCount)> GetAllAsync(int? userId, string? status, DateTime? dueDate, int page, int pageSize);
    Task<TaskItem?> GetByIdAsync(int id, int? userId);
    Task<TaskItem> AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
    Task<bool> ShareTaskAsync(int taskId, int ownerId, string sharedWithEmail);
    Task<IEnumerable<TaskItem>> GetSharedTasksAsync(int userId);
}