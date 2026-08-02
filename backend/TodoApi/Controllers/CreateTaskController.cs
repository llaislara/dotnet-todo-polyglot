using Microsoft.AspNetCore.Mvc;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/CreateTask")]
    [ApiController]
    [Tags("Tasks Management")]
    public class CreateTaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CreateTaskController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Cria uma nova tarefa no sistema de gerenciamento.
        /// </summary>
        /// <remarks>
        /// - **ID**: Gerado automaticamente pelo banco de dados (não deve ser enviado).
        /// - **Título e Status**: São obrigatórios. Os status aceitos são: "Concluído", "Em Andamento", "A Fazer", "Excluído".
        /// - **IsCompleted**: Nasce automaticamente como `false`.
        /// - **Prioridade**: Opcional. Valores aceitos: "Baixo", "Médio-Baixo", "Médio", "Alto".
        /// - **Campos Opcionais**: Subtítulo, prioridade, data inicial e data final não informados serão salvos como null.
        /// </remarks>
        /// <param name="task">Objeto contendo os dados da tarefa.</param>
        [HttpPost]
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskItem>> Handle([FromBody] TaskItem task)
        {
            // ID gerado automaticamente
            task.Id = 0;

            // Flag de conclusão nasce obrigatoriamente como false na criação
            task.IsCompleted = false;

            // Validação de Status
            var validStatuses = new[] { "Concluído", "Em Andamento", "A Fazer", "Excluído" };
            if (string.IsNullOrWhiteSpace(task.Title))
            {
                return BadRequest(new { message = "O título da tarefa é obrigatório." });
            }

            if (!validStatuses.Contains(task.Status))
            {
                return BadRequest(new { message = "Status inválido. Os status permitidos são: Concluído, Em Andamento, A Fazer, Excluído." });
            }

            // Validação de Prioridade 
            var validPriorities = new[] { "Baixo", "Médio-Baixo", "Médio", "Alto" };
            if (!string.IsNullOrWhiteSpace(task.Priority))
            {
                if (!validPriorities.Contains(task.Priority))
                {
                    return BadRequest(new { message = "Prioridade inválida. As prioridades permitidas são: Baixo, Médio-Baixo, Médio, Alto." });
                }
            }
            else
            {
                task.Priority = null;
            }

            // Tratamento do fuso horário do Brasil para datas
            try
            {
                TimeZoneInfo brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Bahia");
                DateTime brazilNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);
                
                if (task.Status == "Excluído")
                {
                    task.DeletedAt = brazilNow;
                }
            }
            catch
            {
                if (task.Status == "Excluído")
                {
                    task.DeletedAt = DateTime.UtcNow.AddHours(-3);
                }
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Handle), new { id = task.Id }, task);
        }
    }
}