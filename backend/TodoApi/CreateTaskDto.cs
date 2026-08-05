namespace TodoApi.DTOs
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty; 
        public string? Subtitle { get; set; }             
        public string? Priority { get; set; }             
        public DateTime? StartDate { get; set; }          
        public string? StartTime { get; set; }            
        public DateTime? EndDate { get; set; }            
        public string? EndTime { get; set; }              
        public string Status { get; set; } = "A Fazer";   
    }
}