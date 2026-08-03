using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers.Users;

[Route("api/users")]
[ApiController]
[Tags("Users")]
public class RegisterUserController : ControllerBase
{
    private readonly AppDbContext _context;

    public RegisterUserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserItem>> Handle([FromBody] UserItem user)
    {
        if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
        {
            return BadRequest(new { message = "Name, Email e Password são obrigatórios." });
        }

        var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
        if (emailExists)
        {
            return BadRequest(new { message = "O e-mail informado já está em uso." });
        }

        user.Id = 0;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Created($"/api/users/{user.Id}", user);
    }
}