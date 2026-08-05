namespace TodoApi.Models;

public class TaskShare
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int SharedWithUserId { get; set; }
}