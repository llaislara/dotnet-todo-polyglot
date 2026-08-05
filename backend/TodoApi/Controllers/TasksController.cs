using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[Tags("Tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _repository;
    private readonly IAuditRepository _audit;

    public TasksController(ITaskRepository repository, IAuditRepository audit) 
    {
        _repository = repository;
        _audit = audit;
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status, 
        [FromQuery] DateTime? dueDate, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        bool isAdmin = User.IsInRole("Admin");
        int? filterId = isAdmin ? null : GetUserId();

        var (tasks, totalCount) = await _repository.GetAllAsync(filterId, status, dueDate, page, pageSize);

        return Ok(new 
        {
            data = tasks,
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        bool isAdmin = User.IsInRole("Admin");
        int? filterId = isAdmin ? null : GetUserId();

        var task = await _repository.GetByIdAsync(id, filterId);
        if (task == null) return NotFound(new { message = "Tarefa não encontrada." });

        return Ok(task);
    }

   [HttpPost]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] TaskItem task)
    {
        var validStatuses = new[] { "Pendente", "Em andamento", "Concluído" };
        if (!validStatuses.Contains(task.Status))
            return BadRequest(new { message = "Status inválido. Utilize: Pendente, Em andamento ou Concluído." });

        var validPriorities = new[] { "Baixo", "Médio-Baixo", "Médio", "Alto" };
        if (!string.IsNullOrEmpty(task.Priority) && !validPriorities.Contains(task.Priority))
            return BadRequest(new { message = "Prioridade inválida." });

        task.UserId = GetUserId();
        
        task.IsCompleted = false; 
        
        task.DeletedAt = null;

        var createdTask = await _repository.AddAsync(task);

        await _audit.LogAsync(GetUserId(), "CREATE_TASK", "Task", createdTask.Id, $"Tarefa '{createdTask.Title}' foi criada.");

        return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] TaskItem updatedTask)
    {
        bool isAdmin = User.IsInRole("Admin");
        int? filterId = isAdmin ? null : GetUserId();

        var existingTask = await _repository.GetByIdAsync(id, filterId);
        if (existingTask == null) return NotFound(new { message = "Tarefa não encontrada." });

        var validStatuses = new[] { "Pendente", "Em andamento", "Concluído" };
        if (!validStatuses.Contains(updatedTask.Status))
            return BadRequest(new { message = "Status inválido. Utilize: Pendente, Em andamento ou Concluído." });

        var validPriorities = new[] { "Baixo", "Médio-Baixo", "Médio", "Alto" };
        if (!string.IsNullOrEmpty(updatedTask.Priority) && !validPriorities.Contains(updatedTask.Priority))
            return BadRequest(new { message = "Prioridade inválida." });

        existingTask.Title = updatedTask.Title;
        existingTask.Description = updatedTask.Description;
        existingTask.Status = updatedTask.Status;
        existingTask.Priority = updatedTask.Priority; 
        existingTask.DueDate = updatedTask.DueDate;
        
        existingTask.IsCompleted = updatedTask.Status == "Concluído"; 

        await _repository.UpdateAsync(existingTask);

        await _audit.LogAsync(GetUserId(), "UPDATE_TASK", "Task", existingTask.Id, $"Tarefa '{existingTask.Title}' foi atualizada.");

        return NoContent();
    }
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        bool isAdmin = User.IsInRole("Admin");
        int? filterId = isAdmin ? null : GetUserId();

        var task = await _repository.GetByIdAsync(id, filterId);
        if (task == null) return NotFound(new { message = "Tarefa não encontrada." });

        await _repository.DeleteAsync(task);

        await _audit.LogAsync(GetUserId(), "DELETE_TASK", "Task", task.Id, $"Tarefa '{task.Title}' foi deletada.");

        return NoContent();
    }

    public class ShareRequestDto 
    { 
        public string Email { get; set; } = string.Empty; 
    }

    [HttpPost("{id}/share")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShareTask(int id, [FromBody] ShareRequestDto request)
    {
        try 
        {
            var success = await _repository.ShareTaskAsync(id, GetUserId(), request.Email);
            if (!success) return NotFound(new { message = "Tarefa não encontrada ou você não é o dono dela." });
            
            await _audit.LogAsync(GetUserId(), "SHARE_TASK", "Task", id, $"Tarefa compartilhada com o e-mail: {request.Email}");
            
            return Ok(new { message = "Tarefa compartilhada com sucesso!" });
        } 
        catch (Exception ex) 
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("shared")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSharedTasks()
    {
        var tasks = await _repository.GetSharedTasksAsync(GetUserId());
        return Ok(new { data = tasks });
    }
}