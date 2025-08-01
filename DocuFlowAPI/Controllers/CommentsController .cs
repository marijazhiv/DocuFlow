using DocuFlowAPI.Models;
using DocuFlowAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DocuFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("document/{documentId}")]
        //[Authorize]
        public async Task<IActionResult> GetCommentsForDocument(int documentId)
        {
            var comments = await _commentService.GetCommentsForDocumentAsync(documentId);
            if (comments == null || comments.Count == 0)
                return NotFound($"No comments found for document with id {documentId}");

            return Ok(comments);
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto dto)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized();

            var result = await _commentService.AddCommentAsync(dto, userName);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message, commentId = result.CommentId });
        }
    }

    // DTO ostaje isti
    public class AddCommentDto
    {
        public int DocumentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
