using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers.TasksManagement;

[Route("api/tasks-management")]
[ApiController]
[Tags("Tasks Management")]
public class CreateTaskController : ControllerBase
{
    private readonly AppDbContext _context;

    public CreateTaskController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("create-task")]
    [Authorize]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskItem>> Handle([FromBody] TaskItem task)
    {
        // Extrai o ID do usuário logado diretamente das claims do token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado ou ID inválido nas claims." });
        }

        task.Id = 0;
        task.UserId = userId; // Associa a tarefa ao usuário autenticado[cite: 1]
        task.IsCompleted = false;

        var validStatuses = new[] { "Concluído", "Em Andamento", "A Fazer", "Excluído" };
        if (string.IsNullOrWhiteSpace(task.Title))
        {
            return BadRequest(new { message = "O título da tarefa é obrigatório." });
        }

        if (!validStatuses.Contains(task.Status))
        {
            return BadRequest(new { message = "Status inválido." });
        }

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Handle), new { id = task.Id }, task);
    }
}