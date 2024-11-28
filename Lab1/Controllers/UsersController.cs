using Lab1.Data;
using Lab1.DTOs;
using Lab1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new User
        {
            Name = userDto.Name,
            Email = userDto.Email
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
    {
        if (id != userDto.Id)
            return BadRequest("User ID in the URL does not match the ID in the request body.");

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.Name = userDto.Name;
        user.Email = userDto.Email;

        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
