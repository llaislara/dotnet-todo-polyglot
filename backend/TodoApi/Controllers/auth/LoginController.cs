using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi.Data;

namespace TodoApi.Controllers.Auth;

[Route("api/auth")]
[ApiController]
[Tags("Authentication")]
public class LoginController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public LoginController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromBody] LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Password == loginDto.Password);

        if (user == null)
        {
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = _configuration["Jwt:Key"] ?? "MinhaChaveSuperSecretaComMaisDe32CaracteresParaGarantirSegurancaTotal123!";
        var key = Encoding.UTF8.GetBytes(jwtKey);

        // Utilizando padrões oficiais de Claims do JWT (sub, email, name)
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name)
            }),
            Expires = DateTime.UtcNow.AddYears(100),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { token = tokenString, expires_in = "100 anos" });
    }
}