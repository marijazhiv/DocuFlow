namespace DocuFlowAPI.Models
{
    public class RegisterDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = "Author"; // string jer se šalje sa frontenda
        public string Profession { get; set; } = string.Empty;
    }


}
