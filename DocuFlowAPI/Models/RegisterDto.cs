namespace DocuFlowAPI.Models
{
    public class RegisterDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = "User"; // Optional for Admin
    }

}
