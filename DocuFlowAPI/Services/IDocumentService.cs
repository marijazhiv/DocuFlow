using DocuFlowAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DocuFlowAPI.Services
{
    public interface IDocumentService
    {
        Task<(bool Success, string Message, int Version, int DocumentId)> UploadDocumentAsync(DocumentUploadDTO dto, string userName);

        Task<List<object>> GetAllDocumentsAsync(string? project);

        Task<List<object>> SearchDocumentsAsync(string query, string sortBy, string order);

        Task<List<object>> SearchAdvancedAsync(string? documentType, string? status, DateTime? fromDate, DateTime? toDate, string sortBy, string order);

        Task<List<object>> SortDocumentsAsync(string sortBy, string order);

        Task<(bool Success, string Message, Stream? Stream)> StreamDocumentAsync(int id);

        Task<(bool Success, string Message, Document? UpdatedDocument)> UpdateStatusAsync(int id, UpdateDocumentStatusDto dto, User currentUser);

        Task<(bool Success, string Message)> ArchiveDocumentAsync(int id);

        Task<(bool Success, string Message, int DeletedCount)> DeleteArchivedDocumentsAsync();
    }
}
