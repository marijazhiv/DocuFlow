using DocuFlowAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocuFlowAPI.Services
{
    public interface IUsersService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<(bool Success, string Message)> CreateUserAsync(RegisterDto dto);
        Task<(bool Success, string Message)> ChangeUserRoleAsync(int id, string role);
        Task<List<string>> GetRolesAsync();
        Task<(bool Success, string Message)> DeleteUserAsync(int id);
    }
}
