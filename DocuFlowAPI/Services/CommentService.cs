using DocuFlowAPI.Controllers;
using DocuFlowAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocuFlowAPI.Services
{
    public class CommentService : ICommentService
    {
        private readonly DataContext _context;

        public CommentService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetCommentsForDocumentAsync(int documentId)
        {
            var comments = await _context.Comments
                .Where(c => c.DocumentId == documentId)
                .Include(c => c.User)
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
                        c.User.FirstName,
                        c.User.LastName
                    }
                })
                .ToListAsync();

            return comments.Cast<object>().ToList();
        }

        public async Task<(bool Success, string Message, int CommentId)> AddCommentAsync(AddCommentDto dto, string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
                return (false, "Unauthorized", 0);

            var document = await _context.Documents.FindAsync(dto.DocumentId);
            if (document == null)
                return (false, "Document not found", 0);

            var comment = new Comment
            {
                DocumentId = dto.DocumentId,
                UserId = user.Id,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return (true, "Comment added successfully", comment.Id);
        }
    }
}
