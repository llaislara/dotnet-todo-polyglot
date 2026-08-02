namespace TodoApi.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pendente"; // "Pendente", "Em andamento", "Concluído"
        public DateTime DueDate { get; set; }
    }
}