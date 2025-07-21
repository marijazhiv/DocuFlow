namespace DocuFlowAPI.Models
{
    public class DocumentUploadDTO
    {
        public string Project { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile File { get; set; } = null!;
    }
}
