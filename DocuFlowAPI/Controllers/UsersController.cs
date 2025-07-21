using DocuFlowAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DocuFlowAPI.Controllers
{
    [Authorize(Roles = "Administrator")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;

        public UsersController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateUser(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists.");

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
                return BadRequest("Invalid role.");

            using var hmac = new HMACSHA512();
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)),
                PasswordSalt = hmac.Key,
                Role = parsedRole,
                Profession = dto.Profession
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User created.");
        }


        // ChangeUserRole endpoint:

        [HttpPut("{id}/role")]
        public async Task<IActionResult> ChangeUserRole(int id, [FromBody] string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (!Enum.TryParse<UserRole>(role, true, out var parsedRole))
                return BadRequest("Invalid role.");

            user.Role = parsedRole;
            await _context.SaveChangesAsync();

            return Ok("Role updated.");
        }

        //za dbijanje liste rola na frontu

        [HttpGet("roles")]
        public ActionResult<IEnumerable<string>> GetRoles()
        {
            var roles = Enum.GetNames(typeof(UserRole));
            return Ok(roles);
        }


    }

}
