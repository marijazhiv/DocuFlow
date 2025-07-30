using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocuFlowAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DocuFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly DataContext _context;

        public CommentsController(DataContext context)
        {
            _context = context;
        }

        // GET api/comments/document/{documentId}
        [HttpGet("document/{documentId}")]
        //[Authorize]
        public async Task<IActionResult> GetCommentsForDocument(int documentId)
        {
            var comments = await _context.Comments
                .Where(c => c.DocumentId == documentId)
                .Include(c => c.User)  // Učitaj podatke o korisniku koji je napisao komentar
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    User = new
                    {
                        c.User.Id,
                        c.User.Username,
                        c.User.FirstName ,
                        c.User.LastName // ili koje god polje korisnik ima
                    }
                })
                .ToListAsync();

            if (comments == null || comments.Count == 0)
            {
                return NotFound($"No comments found for document with id {documentId}");
            }

            return Ok(comments);
        }

        // Opciono: Dodavanje novog komentara na dokument
        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto dto)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
                return Unauthorized();

            var document = await _context.Documents.FindAsync(dto.DocumentId);
            if (document == null)
                return NotFound("Document not found");

            var comment = new Comment
            {
                DocumentId = dto.DocumentId,
                UserId = user.Id,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment added successfully", commentId = comment.Id });
        }
    }

    // DTO za dodavanje komentara
    public class AddCommentDto
    {
        public int DocumentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
