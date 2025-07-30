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
                Status = DocumentStatus.Draft,
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
        public async Task<IActionResult> SearchDocuments(
    [FromQuery] string query,
    [FromQuery] string sortBy = "date",  // ← dodato
    [FromQuery] string order = "desc")   // ← dodato
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required");

            var lowerQuery = query.ToLower();

            var searchResults = _context.Documents
                .Where(d =>
                    d.FileName.ToLower().Contains(lowerQuery) ||
                    (d.Description != null && d.Description.ToLower().Contains(lowerQuery)));

            // SORTIRANJE
            searchResults = sortBy.ToLower() switch
            {
                "name" => order.ToLower() == "asc"
                    ? searchResults.OrderBy(d => d.FileName)
                    : searchResults.OrderByDescending(d => d.FileName),
                _ => order.ToLower() == "asc"
                    ? searchResults.OrderBy(d => d.UploadedAt)
                    : searchResults.OrderByDescending(d => d.UploadedAt)
            };

            var results = await searchResults
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


        [HttpGet("search/advanced")]
        [Authorize]
        public async Task<IActionResult> SearchAdvanced(
    [FromQuery] string? documentType,
    [FromQuery] string? status,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate,
    [FromQuery] string sortBy = "date",     // ← dodato
    [FromQuery] string order = "desc")      // ← dodato
        {
            var query = _context.Documents.AsQueryable();

            if (!string.IsNullOrEmpty(documentType))
            {
                query = query.Where(d => d.DocumentType.ToLower() == documentType.ToLower());
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<DocumentStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(d => d.Status == statusEnum);
                }
                else
                {
                    return BadRequest("Invalid status value");
                }
            }

            if (fromDate.HasValue)
            {
                query = query.Where(d => d.UploadedAt >= fromDate.Value.ToUniversalTime());
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.UploadedAt <= toDate.Value.ToUniversalTime());
            }

            // SORTIRANJE
            query = sortBy.ToLower() switch
            {
                "name" => order.ToLower() == "asc"
                    ? query.OrderBy(d => d.FileName)
                    : query.OrderByDescending(d => d.FileName),
                _ => order.ToLower() == "asc"
                    ? query.OrderBy(d => d.UploadedAt)
                    : query.OrderByDescending(d => d.UploadedAt)
            };

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

        [HttpGet("{id}/stream")]
        [Authorize]
        public async Task<IActionResult> StreamDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null || !System.IO.File.Exists(document.FilePath))
                return NotFound("Document not found");

            var stream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
            return File(stream, "application/pdf");
        }



        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateDocumentStatusDto dto)
        {
            var document = await _context.Documents.Include(d => d.Comments).FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
                return NotFound();

            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized();

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (currentUser == null)
                return Unauthorized();

            // Provera role i pravila statusa
            if (currentUser.Role == UserRole.Author)
                return Forbid("Authors cannot change document status.");

            if (currentUser.Role == UserRole.Reviewer)
            {
                if (dto.Status != DocumentStatus.WaitingApproval)
                    return BadRequest("Reviewer can only set status to WaitingApproval.");
            }

            if (currentUser.Role == UserRole.Approver)
            {
                if (dto.Status != DocumentStatus.Approved && dto.Status != DocumentStatus.ReturnedForEdit)
                    return BadRequest("Approver can only set status to Approved or ReturnedForEdit.");
            }

            document.Status = dto.Status;

            if (dto.Status == DocumentStatus.Approved && currentUser.Role == UserRole.Approver)
            {
                document.ApprovedByUserId = currentUser.Id;
            }
            else if (dto.Status != DocumentStatus.Approved)
            {
                document.ApprovedByUserId = null;
            }

            // Komentar
            string commentText;
            if (string.IsNullOrWhiteSpace(dto.CommentContent))
            {
                commentText = $"Document reviewed by {currentUser.Username}, profession: {currentUser.Profession}";
            }
            else
            {
                commentText = dto.CommentContent.Trim();
            }

            var comment = new Comment
            {
                DocumentId = document.Id,
                UserId = currentUser.Id,
                Content = commentText,
                CreatedAt = DateTime.UtcNow
            };
            document.Comments.Add(comment);
            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();



            return Ok(new { message = "Document status updated", status = document.Status.ToString() });
        }



    }
}

