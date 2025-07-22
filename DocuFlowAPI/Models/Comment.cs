namespace DocuFlowAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Document Document { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        //ne znam??
    }
}
