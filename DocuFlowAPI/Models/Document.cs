namespace DocuFlowAPI.Models
{
 

    public enum DocumentStatus
    {
        Draft,
        WaitingApproval,
        Approved,
        ReturnedForEdit,
        Archived
    }

    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string UploadedBy { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;

        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

        public int? ApprovedByUserId { get; set; }
        public DateTime? ArchivedAt { get; set; }


        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }

}
