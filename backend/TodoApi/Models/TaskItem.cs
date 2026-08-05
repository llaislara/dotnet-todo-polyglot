namespace TodoApi.Models;

public class TaskItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Priority { get; set; } 
    public string Status { get; set; } = "Pendente"; 
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}