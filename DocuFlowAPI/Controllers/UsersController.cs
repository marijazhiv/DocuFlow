using DocuFlowAPI.Models;
using DocuFlowAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocuFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;

        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _usersService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> CreateUser(RegisterDto dto)
        {
            var (success, message) = await _usersService.CreateUserAsync(dto);
            if (!success) return BadRequest(new { error = message });

            return Ok(new { message });
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeUserRole(int id, [FromBody] string role)
        {
            var (success, message) = await _usersService.ChangeUserRoleAsync(id, role);
            if (!success) return BadRequest(message);

            return Ok(message);
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<IEnumerable<string>>> GetRoles()
        {
            var roles = await _usersService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _usersService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { error = "User not found." });

            return Ok(user);
        }

        [HttpGet("username/{username}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
        {
            var user = await _usersService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound(new { error = "User not found." });

            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var (success, message) = await _usersService.DeleteUserAsync(id);
            if (!success) return NotFound(new { error = message });

            return Ok(new { message });
        }
    }
}
