using DocuFlowAPI.Controllers;
using DocuFlowAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocuFlowAPI.Services
{
    public interface ICommentService
    {
        Task<List<object>> GetCommentsForDocumentAsync(int documentId);
        Task<(bool Success, string Message, int CommentId)> AddCommentAsync(AddCommentDto dto, string userName);
    }
}
