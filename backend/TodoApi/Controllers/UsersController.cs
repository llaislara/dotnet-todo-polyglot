using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuditRepository _audit;

    public UsersController(AppDbContext context, IAuditRepository audit) 
    { 
        _context = context; 
        _audit = audit;
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMe()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new { id, name, email, role });
    }
    [HttpGet]
    [Authorize] 
    public async Task<IActionResult> GetAllUsers()
    {
        bool isAdmin = User.IsInRole("Admin");

        if (isAdmin)
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Name, u.Email, u.Cpf, u.Role })
                .ToListAsync();

            return Ok(users);
        }
        else
        {
      
            var users = await _context.Users
                .Select(u => new { u.Id, u.Name, u.Email })
                .ToListAsync();

            return Ok(users);
        }
    }

    [HttpPut("{id}/promote")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PromoteToAdmin(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Usuário não encontrado." });

        user.Role = "Admin";
        await _context.SaveChangesAsync();

        await _audit.LogAsync(GetUserId(), "PROMOTE_USER", "User", user.Id, $"Permissão de Admin concedida ao usuário {user.Name}.");

        return Ok(new { message = $"Usuário {user.Name} agora é um Administrador." });
    }

    [HttpPost("admin-create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUserByAdmin([FromBody] UserItem user)
    {
        user.Cpf = user.Cpf.Replace(".", "").Replace("-", "");

        if (string.IsNullOrWhiteSpace(user.Cpf) || user.Cpf.Length < 4)
            return BadRequest(new { message = "O CPF é obrigatório e deve ter no mínimo 4 dígitos." });

        if (await _context.Users.AnyAsync(u => u.Email == user.Email || u.Cpf == user.Cpf))
            return BadRequest(new { message = "E-mail ou CPF já cadastrados." });

        user.Id = 0;
        user.Role = "User";
        user.Password = user.Cpf.Substring(0, 4);
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _audit.LogAsync(GetUserId(), "ADMIN_CREATE_USER", "User", user.Id, $"Administrador criou o usuário {user.Name}.");

        user.Password = string.Empty; 

        return Ok(new { message = "Usuário criado. A senha provisória são os 4 primeiros dígitos do CPF.", user });
    }

    public class ChangePasswordDto 
    { 
        public string OldPassword { get; set; } = string.Empty; 
        public string NewPassword { get; set; } = string.Empty; 
    }

    [HttpPut("me/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null) return NotFound();

        if (user.Password != request.OldPassword)
            return BadRequest(new { message = "A senha atual está incorreta." });

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 4)
            return BadRequest(new { message = "A nova senha deve ter no mínimo 4 caracteres." });

        user.Password = request.NewPassword;
        await _context.SaveChangesAsync();

        await _audit.LogAsync(userId, "CHANGE_PASSWORD", "User", userId, "Usuário alterou a própria senha.");

        return Ok(new { message = "Sua senha foi alterada com sucesso!" });
    }

 [HttpGet("audit-logs")]
    [Authorize] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? date,
        [FromQuery] int? filterUserId, 
        [FromQuery] string? role,
        [FromQuery] string? action)
    {
        var currentUserId = GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var logs = await _audit.GetLogsAsync(currentUserId, isAdmin, date, filterUserId, role, action);

        var formattedLogs = logs.Select(l => new 
        {
            l.Id,
            UserId = l.UserId,
            UserName = l.User?.Name ?? "Sistema / Acesso Externo",
            UserRole = l.User?.Role ?? "N/A",
            l.Action,
            l.EntityType,
            l.EntityId,
            l.Details,
            CreatedAt = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        });

        return Ok(new { total = formattedLogs.Count(), data = formattedLogs });
    }
}