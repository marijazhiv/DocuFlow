namespace DocuFlowAPI.Models
{
    public class UpdateDocumentStatusDto
    {
        public DocumentStatus Status { get; set; }
        public string? CommentContent { get; set; }
    }

}
