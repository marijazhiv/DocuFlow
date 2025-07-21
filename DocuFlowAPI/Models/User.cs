namespace DocuFlowAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public UserRole Role { get; set; } = UserRole.Author;  // default: Author

        public string Profession { get; set; } = string.Empty; // dodatno polje za zanimanje
    }
}
