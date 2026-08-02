namespace TodoApi.DTOs
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty; // Obrigatório
        public string? Subtitle { get; set; }             // Opcional (vira null se não enviado)
        public string? Priority { get; set; }             // Opcional (vira null se não enviado)
        public DateTime? StartDate { get; set; }          // Opcional (data inicial ou dia único)
        public string? StartTime { get; set; }            // Opcional (formato "HH:mm", ex: "14:30")
        public DateTime? EndDate { get; set; }            // Opcional (data final do período)
        public string? EndTime { get; set; }              // Opcional (formato "HH:mm")
        public string Status { get; set; } = "A Fazer";   // Obrigatório
    }
}