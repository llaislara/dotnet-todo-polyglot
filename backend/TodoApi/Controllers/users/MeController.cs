using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TodoApi.Controllers.Users;

[Route("api/users")]
[ApiController]
[Tags("Users")]
public class MeController : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Handle()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int id))
        {
            return Unauthorized(new { message = "Usuário não autenticado ou ID inválido." });
        }

        string? token = null;
        if (Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var authHeaderStr = authHeader.ToString();
            if (authHeaderStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeaderStr.Substring("Bearer ".Length).Trim();
            }
            else
            {
                token = authHeaderStr.Trim();
            }
        }

        return Ok(new
        {
            id = id,
            name = name,
            email = email,
            token = token
        });
    }
}