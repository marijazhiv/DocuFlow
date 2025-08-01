using DocuFlowAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocuFlowAPI.Services
{
    public class DocumentService: IDocumentService
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentService(DataContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        //upload novog dokumenta
        public async Task<(bool Success, string Message, int Version, int DocumentId)> UploadDocumentAsync(DocumentUploadDTO dto, string userName)
        {

            // provera da li je fajl prosledjen i da li nije prazan
            if (dto.File == null || dto.File.Length == 0)
                return (false, "File is required", 0, 0);

            // formira putanju do foldera za upload na osnovu root direktorijuma aplikacije
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "UploadedDocs");

            // kreira folder ako ne postoji
            Directory.CreateDirectory(uploadsFolder);

            //ime fajla i extenzija
            var originalFileName = Path.GetFileNameWithoutExtension(dto.File.FileName);
            var extension = Path.GetExtension(dto.File.FileName);
            var documentType = extension.TrimStart('.').ToLower();

            //pronalazi sve dokumente u bazi koji imaju isti projekat i ime fajla
            var matchingDocs = await _context.Documents
                .Where(d => d.Project == dto.Project && d.FileName == dto.File.FileName)
                .ToListAsync();

            int version = 1;

            //ako vec postoji dokument sa istim imenom i projektom uvecaj verziju
            if (matchingDocs.Any())
            {
                version = matchingDocs.Max(d => d.Version) + 1;
            }

            //kombinuje folder i ime fajla u punu putanju za cuvanje
            var savedFileName = $"{originalFileName}_v{version}{extension}";
            var savedPath = Path.Combine(uploadsFolder, savedFileName);

            // snima fajl na disk na datu putanju
            using (var stream = new FileStream(savedPath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            // kreira novi objekat dokumenta za bazu podataka

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

            //cuvanje u bazi
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return (true, "Document uploaded successfully", version, document.Id);
        }

        public async Task<List<object>> GetAllDocumentsAsync(string? project)
        {

            //pravim osnovni upit nad tabelom Documents koristeci IQueryable
            var query = _context.Documents.AsQueryable();

            //filtriramo dokumente po projektu (case-insensitive)
            if (!string.IsNullOrEmpty(project))
            {
                query = query.Where(d => d.Project.ToLower() == project.ToLower());
            }

            //sortiram po datumu i selektujem polja za prikaz
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

            return documents.Cast<object>().ToList();
        }

        //search po filename i descriptionu
        //sort ugradjen i u ovaj search
        public async Task<List<object>> SearchDocumentsAsync(string query, string sortBy, string order)
        {
            // query string u mala slova - case-insensitive pretraga
            var lowerQuery = query.ToLower();

            // Pretraga po FileName, Description i Project
            var searchResults = _context.Documents
                .Where(d =>
                    d.FileName.ToLower().Contains(lowerQuery) ||
                    (d.Description != null && d.Description.ToLower().Contains(lowerQuery)) ||
                    (d.Project != null && d.Project.ToLower().Contains(lowerQuery)));

            // Sortiranje po sortBy i order
            searchResults = sortBy.ToLower() switch
            {
                "name" => order.ToLower() == "asc"
                    ? searchResults.OrderBy(d => d.FileName)
                    : searchResults.OrderByDescending(d => d.FileName),
                _ => order.ToLower() == "asc"
                    ? searchResults.OrderBy(d => d.UploadedAt)
                    : searchResults.OrderByDescending(d => d.UploadedAt)
            };

            // Projekcija - biramo samo potrebna polja
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

            return results.Cast<object>().ToList();
        }

        //pretraga po tipu, sttausu, datumu
        //ukljucen i sort
        public async Task<List<object>> SearchAdvancedAsync(string? documentType, string? status, DateTime? fromDate, DateTime? toDate, string sortBy, string order)
        {
            var query = _context.Documents.AsQueryable();

            if (!string.IsNullOrEmpty(documentType))
            {
                query = query.Where(d => d.DocumentType.ToLower() == documentType.ToLower());
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<DocumentStatus>(status, true, out var statusEnum))   //string-enum (moramo da parsiramo)
                {
                    query = query.Where(d => d.Status == statusEnum);
                }
                else
                {
                    throw new ArgumentException("Invalid status value");
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

            return results.Cast<object>().ToList();
        }

        //sort sbih dokumenata; po imenu; datumu; opadajući i rastući

        public async Task<List<object>> SortDocumentsAsync(string sortBy, string order)
        {
            var query = _context.Documents.AsQueryable();

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

            return results.Cast<object>().ToList();
        }

        public async Task<(bool Success, string Message, Stream? Stream)> StreamDocumentAsync(int id)
        {

            //nalazimo doc po idiju
            var document = await _context.Documents.FindAsync(id);

            //proveravamo da li postoji na toj putanji
            if (document == null || !File.Exists(document.FilePath))
                return (false, "Document not found", null);

            //otvaramo ga kao tok za citanje
            var stream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
            return (true, "Document stream ready", stream);
        }


        //workflow- izmena statusa i komentarisanje dokumenta
        public async Task<(bool Success, string Message, Document? UpdatedDocument)> UpdateStatusAsync(int id, UpdateDocumentStatusDto dto, User currentUser)
        {
            var document = await _context.Documents.Include(d => d.Comments).FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
                return (false, "Document not found", null);

            if (currentUser.Role == UserRole.Author)
                return (false, "Authors cannot change document status.", null);

            if (dto.Status != null)
            {
                if (currentUser.Role == UserRole.Reviewer && dto.Status != DocumentStatus.WaitingApproval && dto.Status != DocumentStatus.ReturnedForEdit)
                    return (false, "Reviewer can only set status to WaitingApproval or ReturnedForEdit.", null);

                if (currentUser.Role == UserRole.Approver && dto.Status != DocumentStatus.Approved && dto.Status != DocumentStatus.ReturnedForEdit)
                    return (false, "Approver can only set status to Approved or ReturnedForEdit.", null);

                document.Status = dto.Status.Value;

                if (dto.Status == DocumentStatus.Approved && currentUser.Role == UserRole.Approver)
                {
                    document.ApprovedByUserId = currentUser.Id;
                }
                else if (dto.Status != DocumentStatus.Approved)
                {
                    document.ApprovedByUserId = null;
                }
            }

            string commentText;
            if (!string.IsNullOrWhiteSpace(dto.CommentContent))
            {
                commentText = dto.CommentContent.Trim();
            }
            else if (dto.Status != null)
            {
                commentText = $"Document reviewed by {currentUser.Username}, profession: {currentUser.Profession}";
            }
            else
            {
                return (false, "You must provide a comment or a status update.", null);
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

            return (true, dto.Status != null ? "Document status updated and comment added." : "Comment added.", document);
        }

        //menja sttaus na arhiviran ako vec nije i upisuje vreme arhiviranja
        public async Task<(bool Success, string Message)> ArchiveDocumentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return (false, "Document not found");

            if (document.Status == DocumentStatus.Archived)
                return (false, "Document is already archived.");

            document.Status = DocumentStatus.Archived;
            document.ArchivedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (true, "Document successfully archived.");
        }

        //brise arhivirane dokumente starije od 3 dana
        public async Task<(bool Success, string Message, int DeletedCount)> DeleteArchivedDocumentsAsync()
        {
            //// izracunava se datum koji je 3 dana pre sadasnjeg UTC vremena
            var threeDaysAgo = DateTime.UtcNow.AddDays(-3);

            //dohvata sve takve iz baze
            var documentsToDelete = await _context.Documents
                .Where(d => d.Status == DocumentStatus.Archived && d.ArchivedAt <= threeDaysAgo)
                .ToListAsync();

            foreach (var doc in documentsToDelete)
            {
                //proverava se da li fajl fizicki postoji na disku
                if (File.Exists(doc.FilePath))
                {
                    File.Delete(doc.FilePath);  //brise i sa diska i iz baze
                }

                _context.Documents.Remove(doc);
            }

            await _context.SaveChangesAsync();

            return (true, $"{documentsToDelete.Count} archived document(s) permanently deleted.", documentsToDelete.Count);
        }
    }
}
