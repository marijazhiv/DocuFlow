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
            var documentType = extension.TrimStart('.').ToLower();

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
                Status = "In Preparation", // ili "Draft"
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
                    d.UploadedAt,
                    d.DocumentType,
                    d.Status
                })
                .ToListAsync();

            return Ok(documents);
        }

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
                    d.UploadedAt,
                    d.DocumentType,
                    d.Status
                })
                .ToListAsync();

            return Ok(results);
        }

        // 🔍 Napredna pretraga
        [HttpGet("search/advanced")]
        [Authorize]
        public async Task<IActionResult> SearchAdvanced(
            [FromQuery] string? documentType,
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var query = _context.Documents.AsQueryable();

            if (!string.IsNullOrEmpty(documentType))
            {
                query = query.Where(d => d.DocumentType.ToLower() == documentType.ToLower());
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(d => d.Status.ToLower() == status.ToLower());
            }

            if (fromDate.HasValue)
            {
                query = query.Where(d => d.UploadedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.UploadedAt <= toDate.Value);
            }

            var results = await query
                .OrderByDescending(d => d.UploadedAt)
                .Select(d => new
                {
                    d.Id,
                    d.FileName,
                    d.Project,
                    d.Description,
                    d.Version,
                    d.UploadedBy,
                    d.UploadedAt,
                    d.DocumentType,
                    d.Status
                })
                .ToListAsync();

            return Ok(results);
        }

        // 🔽 Sortiranje po datumu ili nazivu
        [HttpGet("sort")]
        [Authorize]
        public async Task<IActionResult> SortDocuments(
            [FromQuery] string sortBy = "date",  // "date" ili "name"
            [FromQuery] string order = "desc"    // "asc" ili "desc"
        )
        {
            var query = _context.Documents.AsQueryable();

            switch (sortBy.ToLower())
            {
                case "name":
                    query = order.ToLower() == "asc"
                        ? query.OrderBy(d => d.FileName)
                        : query.OrderByDescending(d => d.FileName);
                    break;

                case "date":
                default:
                    query = order.ToLower() == "asc"
                        ? query.OrderBy(d => d.UploadedAt)
                        : query.OrderByDescending(d => d.UploadedAt);
                    break;
            }

            var results = await query
                .Select(d => new
                {
                    d.Id,
                    d.FileName,
                    d.Project,
                    d.Description,
                    d.Version,
                    d.UploadedBy,
                    d.UploadedAt,
                    d.DocumentType,
                    d.Status
                })
                .ToListAsync();

            return Ok(results);
        }

    }
}

