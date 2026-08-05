using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IAuditRepository _audit;

    public AuthController(AppDbContext context, IConfiguration configuration, IAuditRepository audit)
    {
        _context = context;
        _configuration = configuration;
        _audit = audit;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Password == loginDto.Password);

        if (user == null)
            return Unauthorized(new { message = "E-mail ou senha inválidos." });

        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            return StatusCode(500, new { message = "Erro interno: Chave de segurança não configurada." });

        var key = Encoding.UTF8.GetBytes(jwtKey);
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        await _audit.LogAsync(user.Id, "USER_LOGIN", "User", user.Id, $"Usuário {user.Email} realizou login.");

        return Ok(new { token = tokenHandler.WriteToken(token), expires_in = "8 horas" });
    }

    [HttpPost("logout")]
    [Authorize] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        
        if (jti != null)
        {
            _context.BlacklistedTokens.Add(new BlacklistedToken 
            { 
                Token = jti, 
                ExpiryDate = DateTime.UtcNow.AddHours(8) 
            });
            await _context.SaveChangesAsync();
            
            await _audit.LogAsync(GetUserId(), "USER_LOGOUT", "User", GetUserId(), "Usuário realizou logout e invalidou o token.");
        }

        return Ok(new { message = "Logout realizado com sucesso. A sessão foi encerrada de forma segura." });
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserItem>> Register([FromBody] UserItem user)
    {
        if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
            return BadRequest(new { message = "Nome, Email e Senha são obrigatórios." });

        user.Cpf = user.Cpf.Replace(".", "").Replace("-", "");
        if (string.IsNullOrWhiteSpace(user.Cpf))
            return BadRequest(new { message = "O CPF é obrigatório." });

        if (await _context.Users.AnyAsync(u => u.Email == user.Email || u.Cpf == user.Cpf))
            return BadRequest(new { message = "O e-mail ou CPF informado já está em uso." });

        user.Id = 0; 
        user.Role = "User"; 
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _audit.LogAsync(null, "REGISTER_USER", "User", user.Id, $"Usuário externo {user.Name} se cadastrou na plataforma.");

        user.Password = string.Empty; 
        
        return Created($"/api/users/{user.Id}", user);
    }
}