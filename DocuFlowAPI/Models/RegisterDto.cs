using System.ComponentModel.DataAnnotations;

namespace DocuFlowAPI.Models
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Username must be a valid email address.")]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = null!;

        public string Role { get; set; } = "Author";

        public string Profession { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }
}
