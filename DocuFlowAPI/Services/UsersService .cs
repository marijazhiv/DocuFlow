using DocuFlowAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace DocuFlowAPI.Services
{
    public class UsersService : IUsersService
    {
        private readonly DataContext _context;

        public UsersService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
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
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Profession = u.Profession,
                    Role = u.Role.ToString()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Where(u => u.Username == username)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Profession = u.Profession,
                    Role = u.Role.ToString()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(RegisterDto dto)
        {

            //validacija za isti username
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return (false, "Username already exists.");

            //parsira string dto.Role u enum UserRole, ignorišući velika/mala slova
            if (!Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
                return (false, "Invalid role.");

            //Kreira novi HMACSHA512 objekat koji se koristi za heširanje lozinke
            using var hmac = new HMACSHA512();
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)), //// heš lozinke
                PasswordSalt = hmac.Key,
                Role = parsedRole,
                Profession = dto.Profession,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "User created.");
        }

        public async Task<(bool Success, string Message)> ChangeUserRoleAsync(int id, string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return (false, "User not found.");

            if (!Enum.TryParse<UserRole>(role, true, out var parsedRole))
                return (false, "Invalid role.");

            user.Role = parsedRole;
            await _context.SaveChangesAsync();

            return (true, "Role updated.");
        }

        public Task<List<string>> GetRolesAsync()
        {
            var roles = Enum.GetNames(typeof(UserRole)).ToList();
            return Task.FromResult(roles);
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return (false, "User not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return (true, "User deleted successfully.");
        }
    }
}
