using DocuFlowAPI.Models;
using DocuFlowAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DocuFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly DataContext _context;

        // Kknstruktor prima IDocumentService, ne konkretnu klasu DocumentService
        public DocumentsController(IDocumentService documentService, DataContext context)
        {
            _documentService = documentService;
            _context = context;
        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload([FromForm] DocumentUploadDTO dto)
        {
            var userName = User.Identity?.Name ?? "";
            var result = await _documentService.UploadDocumentAsync(dto, userName);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new
            {
                message = result.Message,
                version = result.Version,
                documentId = result.DocumentId
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllDocuments([FromQuery] string? project)
        {
            var documents = await _documentService.GetAllDocumentsAsync(project);
            return Ok(documents);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchDocuments(
            [FromQuery] string query,
            [FromQuery] string sortBy = "date",
            [FromQuery] string order = "desc")
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required");

            var results = await _documentService.SearchDocumentsAsync(query, sortBy, order);
            return Ok(results);
        }

        [HttpGet("search/advanced")]
        [Authorize]
        public async Task<IActionResult> SearchAdvanced(
            [FromQuery] string? documentType,
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string sortBy = "date",
            [FromQuery] string order = "desc")
        {
            try
            {
                var results = await _documentService.SearchAdvancedAsync(documentType, status, fromDate, toDate, sortBy, order);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("sort")]
        [Authorize]
        public async Task<IActionResult> SortDocuments(
            [FromQuery] string sortBy = "date",
            [FromQuery] string order = "desc")
        {
            var results = await _documentService.SortDocumentsAsync(sortBy, order);
            return Ok(results);
        }

        [HttpGet("{id}/stream")]
        [Authorize]
        public async Task<IActionResult> StreamDocument(int id)
        {
            var (success, message, stream) = await _documentService.StreamDocumentAsync(id);
            if (!success)
                return NotFound(message);

            return File(stream!, "application/pdf");
        }

        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateDocumentStatusDto dto)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized();

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (currentUser == null)
                return Unauthorized();

            var (success, message, updatedDocument) = await _documentService.UpdateStatusAsync(id, dto, currentUser);

            if (!success)
            {
                if (message.Contains("forbid", StringComparison.OrdinalIgnoreCase))
                    return Forbid(message);
                return BadRequest(message);
            }

            return Ok(new { message, status = updatedDocument!.Status.ToString() });
        }

        [HttpPost("{id}/archive")]
        //[Authorize]
        public async Task<IActionResult> ArchiveDocument(int id)
        {
            var (success, message) = await _documentService.ArchiveDocumentAsync(id);
            if (!success)
                return BadRequest(message);

            return Ok(new { message });
        }

        [HttpDelete("cleanup-archived")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteArchivedDocuments()
        {
            var (success, message, count) = await _documentService.DeleteArchivedDocumentsAsync();
            if (!success)
                return BadRequest(message);

            return Ok(new { message });
        }
    }
}
