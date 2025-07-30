namespace DocuFlowAPI.Models
{
    public class UpdateDocumentStatusDto
    {
        public DocumentStatus? Status { get; set; } // ← OVO je ključno: enum je sada nullable
        public string? CommentContent { get; set; }
    }
}
