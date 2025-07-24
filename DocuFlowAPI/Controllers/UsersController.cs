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

        /*[HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }*/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Profession = u.Profession,
                    Role = u.Role.ToString()
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateUser(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest(new { error = "Username already exists." });

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
                return BadRequest(new { error = "Invalid role." });

            using var hmac = new HMACSHA512();
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)),
                PasswordSalt = hmac.Key,
                Role = parsedRole,
                Profession = dto.Profession,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created." });
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

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { error = "User not found." });

            return Ok(user);
        }


    }

}
