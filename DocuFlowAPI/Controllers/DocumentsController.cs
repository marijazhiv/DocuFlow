namespace DocuFlowAPI.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using DocuFlowAPI.Models;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentsController(DataContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload([FromForm] DocumentUploadDTO dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("File is required");

            var uploadsFolder = Path.Combine(_env.ContentRootPath, "UploadedDocs");
            Directory.CreateDirectory(uploadsFolder);

            var originalFileName = Path.GetFileNameWithoutExtension(dto.File.FileName);
            var extension = Path.GetExtension(dto.File.FileName);
            var documentType = extension.TrimStart('.').ToLower(); // npr. "pdf"

            var existingVersions = _context.Documents
                .Where(d => d.Project == dto.Project && d.FileName == dto.File.FileName)
                .Count();

            var version = existingVersions + 1;
            var savedFileName = $"{originalFileName}_v{version}{extension}";
            var savedPath = Path.Combine(uploadsFolder, savedFileName);

            using (var stream = new FileStream(savedPath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var userName = User.Identity!.Name!;

            var document = new Document
            {
                FileName = dto.File.FileName,
                FilePath = savedPath,
                Project = dto.Project,
                Description = dto.Description,
                Version = version,
                UploadedBy = userName,
                UploadedAt = DateTime.UtcNow,
                DocumentType = documentType,
                Status = "Draft", // inicijalni status
                CommentIds = new List<int>(),
                ApprovedByUserId = null
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Document uploaded successfully",
                version = version,
                documentId = document.Id
            });
        }


        // Pregled svih dokumenata (opciono filtriranje po projektu)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllDocuments([FromQuery] string? project)
        {
            var query = _context.Documents.AsQueryable();

            if (!string.IsNullOrEmpty(project))
            {
                query = query.Where(d => d.Project.ToLower() == project.ToLower());
            }

            var documents = await query
                .OrderByDescending(d => d.UploadedAt)
                .Select(d => new
                {
                    d.Id,
                    d.FileName,
                    d.Project,
                    d.Description,
                    d.Version,
                    d.UploadedBy,
                    d.UploadedAt
                })
                .ToListAsync();

            return Ok(documents);
        }

        // Pretraga dokumenata po nazivu ili opisu
        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchDocuments([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required");
            }

            var lowerQuery = query.ToLower();

            var results = await _context.Documents
                .Where(d =>
                    d.FileName.ToLower().Contains(lowerQuery) ||
                    (d.Description != null && d.Description.ToLower().Contains(lowerQuery)))
                .OrderByDescending(d => d.UploadedAt)
                .Select(d => new
                {
                    d.Id,
                    d.FileName,
                    d.Project,
                    d.Description,
                    d.Version,
                    d.UploadedBy,
                    d.UploadedAt
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}
