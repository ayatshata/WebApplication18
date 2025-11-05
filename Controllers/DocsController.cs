using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;
using MughtaribatHouse.Services;
using System.ComponentModel.DataAnnotations;

namespace MughtaribatHouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DocsController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocsController> _logger;

        public DocsController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<DocsController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // CREATE - Upload new document
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> CreateDocument([FromForm] DocumentUploadRequest request)
        {
            try
            {
                // Validation
                if (request.File == null || request.File.Length == 0)
                    return Error("الرجاء اختيار ملف");

                if (string.IsNullOrEmpty(request.Title))
                    return Error("عنوان المستند مطلوب");

                // Validate file size (10MB max)
                if (request.File.Length > 10 * 1024 * 1024)
                    return Error("حجم الملف يجب ألا يتجاوز 10 ميجابايت");

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".xls", ".xlsx", ".txt" };
                var fileExtension = Path.GetExtension(request.File.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                    return Error("نوع الملف غير مسموح به. المسموح: PDF, Word, Excel, Images, Text");

                // Create uploads directory if not exists
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.File.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                // Create document entity
                var document = new Document
                {
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim(),
                    FilePath = $"/uploads/documents/{fileName}",
                    FileType = fileExtension,
                    FileSize = request.File.Length,
                    Category = request.Category ?? "Other",
                    ResidentId = request.ResidentId,
                    UploadedByUserId = UserId,
                    IsPublic = request.IsPublic,
                    UploadedAt = DateTime.UtcNow,
                    Status = "Active",
                    Tags = request.Tags
                };

                // Save to database
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // Load related data for response
                await _context.Entry(document)
                    .Reference(d => d.Resident)
                    .LoadAsync();

                await _context.Entry(document)
                    .Reference(d => d.UploadedByUser)
                    .LoadAsync();

                _logger.LogInformation($"Document uploaded: {document.Title} by user {UserId}");

                return Success(document, "تم رفع الملف بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return Error($"فشل في رفع الملف: {ex.Message}");
            }
        }

        // READ - Get all documents with filtering and pagination
        [HttpGet]
        public async Task<IActionResult> GetDocuments(
            [FromQuery] string category = null,
            [FromQuery] int? residentId = null,
            [FromQuery] string search = null,
            [FromQuery] bool? isPublic = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Documents
                    .Include(d => d.Resident)
                    .Include(d => d.UploadedByUser)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(category))
                    query = query.Where(d => d.Category == category);

                if (residentId.HasValue)
                    query = query.Where(d => d.ResidentId == residentId);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(d => d.Title.Contains(search) ||
                                           (d.Description != null && d.Description.Contains(search)) ||
                                           (d.Tags != null && d.Tags.Contains(search)));

                if (isPublic.HasValue)
                    query = query.Where(d => d.IsPublic == isPublic.Value);

                // Apply security - users can only see public documents or their own
                if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
                {
                    query = query.Where(d => d.IsPublic || d.UploadedByUserId == UserId);
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var documents = await query
                    .OrderByDescending(d => d.UploadedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new
                {
                    Documents = documents,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents");
                return Error($"فشل في جلب المستندات: {ex.Message}");
            }
        }

        // READ - Get single document by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Resident)
                    .Include(d => d.UploadedByUser)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                    return NotFound("الملف غير موجود");

                // Check access permissions
                if (!CanAccessDocument(document))
                    return Unauthorized("ليس لديك صلاحية للوصول إلى هذا الملف");

                return Success(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting document {id}");
                return Error($"فشل في جلب الملف: {ex.Message}");
            }
        }

        // READ - Download document file
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                    return NotFound("الملف غير موجود");

                // Check access permissions
                if (!CanAccessDocument(document))
                    return Unauthorized("ليس لديك صلاحية لتحميل هذا الملف");

                var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                    return NotFound("الملف غير موجود على الخادم");

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(document.FileType);

                return File(fileBytes, contentType, $"{document.Title}{document.FileType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading document {id}");
                return Error($"فشل في تحميل الملف: {ex.Message}");
            }
        }

        // UPDATE - Update document metadata
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] UpdateDocumentRequest request)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                    return NotFound("الملف غير موجود");

                // Check if user can edit this document
                if (!CanEditDocument(document))
                    return Unauthorized("ليس لديك صلاحية لتعديل هذا الملف");

                // Update document properties
                document.Title = request.Title?.Trim() ?? document.Title;
                document.Description = request.Description?.Trim();
                document.Category = request.Category ?? document.Category;
                document.IsPublic = request.IsPublic;
                document.Tags = request.Tags;
                document.UpdatedAt = DateTime.UtcNow;

                // Validate resident exists if provided
                if (request.ResidentId.HasValue && request.ResidentId != document.ResidentId)
                {
                    var residentExists = await _context.Residents.AnyAsync(r => r.Id == request.ResidentId);
                    if (!residentExists)
                        return Error("المقيم غير موجود");

                    document.ResidentId = request.ResidentId;
                }

                await _context.SaveChangesAsync();

                // Load related data for response
                await _context.Entry(document)
                    .Reference(d => d.Resident)
                    .LoadAsync();

                await _context.Entry(document)
                    .Reference(d => d.UploadedByUser)
                    .LoadAsync();

                _logger.LogInformation($"Document updated: {document.Title} by user {UserId}");

                return Success(document, "تم تحديث الملف بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating document {id}");
                return Error($"فشل في تحديث الملف: {ex.Message}");
            }
        }

        // UPDATE - Replace document file
        [HttpPut("{id}/file")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> ReplaceDocumentFile(int id, [FromForm] ReplaceFileRequest request)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                    return NotFound("الملف غير موجود");

                // Check if user can edit this document
                if (!CanEditDocument(document))
                    return Unauthorized("ليس لديك صلاحية لتعديل هذا الملف");

                if (request.File == null || request.File.Length == 0)
                    return Error("الرجاء اختيار ملف");

                // Validate file size (10MB max)
                if (request.File.Length > 10 * 1024 * 1024)
                    return Error("حجم الملف يجب ألا يتجاوز 10 ميجابايت");

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".xls", ".xlsx", ".txt" };
                var fileExtension = Path.GetExtension(request.File.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                    return Error("نوع الملف غير مسموح به. المسموح: PDF, Word, Excel, Images, Text");

                // Delete old file
                var oldFilePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                // Create uploads directory if not exists
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.File.FileName)}";
                var newFilePath = Path.Combine(uploadsPath, fileName);

                // Save new file to disk
                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                // Update document entity
                document.FilePath = $"/uploads/documents/{fileName}";
                document.FileType = fileExtension;
                document.FileSize = request.File.Length;
                document.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Document file replaced: {document.Title} by user {UserId}");

                return Success(document, "تم استبدال الملف بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error replacing document file {id}");
                return Error($"فشل في استبدال الملف: {ex.Message}");
            }
        }

        // DELETE - Delete document
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                    return NotFound("الملف غير موجود");

                // Delete physical file
                var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                // Delete from database
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Document deleted: {document.Title} by user {UserId}");

                return Success(null, "تم حذف الملف بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting document {id}");
                return Error($"فشل في حذف الملف: {ex.Message}");
            }
        }

        // DELETE - Soft delete (archive) document
        [HttpDelete("{id}/archive")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> ArchiveDocument(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                    return NotFound("الملف غير موجود");

                // Check if user can edit this document
                if (!CanEditDocument(document))
                    return Unauthorized("ليس لديك صلاحية لأرشفة هذا الملف");

                document.Status = "Archived";
                document.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Document archived: {document.Title} by user {UserId}");

                return Success(document, "تم أرشفة الملف بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error archiving document {id}");
                return Error($"فشل في أرشفة الملف: {ex.Message}");
            }
        }

        // BULK OPERATIONS

        // Bulk delete documents
        [HttpPost("bulk-delete")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> BulkDeleteDocuments([FromBody] BulkDocumentRequest request)
        {
            try
            {
                if (request.DocumentIds == null || !request.DocumentIds.Any())
                    return Error("لم يتم تحديد أي ملفات للحذف");

                var documents = await _context.Documents
                    .Where(d => request.DocumentIds.Contains(d.Id))
                    .ToListAsync();

                foreach (var document in documents)
                {
                    // Delete physical file
                    var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.Documents.RemoveRange(documents);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Bulk deleted {documents.Count} documents by user {UserId}");

                return Success(null, $"تم حذف {documents.Count} ملف بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk delete documents");
                return Error($"فشل في حذف الملفات: {ex.Message}");
            }
        }

        // Bulk update documents
        [HttpPost("bulk-update")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> BulkUpdateDocuments([FromBody] BulkUpdateDocumentRequest request)
        {
            try
            {
                if (request.DocumentIds == null || !request.DocumentIds.Any())
                    return Error("لم يتم تحديد أي ملفات للتحديث");

                var documents = await _context.Documents
                    .Where(d => request.DocumentIds.Contains(d.Id))
                    .ToListAsync();

                foreach (var document in documents)
                {
                    if (!string.IsNullOrEmpty(request.Category))
                        document.Category = request.Category;

                    if (request.IsPublic.HasValue)
                        document.IsPublic = request.IsPublic.Value;

                    if (!string.IsNullOrEmpty(request.Tags))
                        document.Tags = request.Tags;

                    document.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Bulk updated {documents.Count} documents by user {UserId}");

                return Success(null, $"تم تحديث {documents.Count} ملف بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk update documents");
                return Error($"فشل في تحديث الملفات: {ex.Message}");
            }
        }

        // UTILITY METHODS

        // Helper method to check document access
        private bool CanAccessDocument(Document document)
        {
            return document.IsPublic ||
                   document.UploadedByUserId == UserId ||
                   User.IsInRole("Admin") ||
                   User.IsInRole("Manager");
        }

        // Helper method to check edit permissions
        private bool CanEditDocument(Document document)
        {
            return document.UploadedByUserId == UserId ||
                   User.IsInRole("Admin") ||
                   User.IsInRole("Manager");
        }

        // Helper method to get content type
        private string GetContentType(string fileType)
        {
            return fileType.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        // ADDITIONAL ENDPOINTS

        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            var categories = new[]
            {
                new { Value = "Contract", Name = "عقد" },
                new { Value = "Report", Name = "تقرير" },
                new { Value = "Invoice", Name = "فاتورة" },
                new { Value = "Identity", Name = "هوية" },
                new { Value = "License", Name = "رخصة" },
                new { Value = "Certificate", Name = "شهادة" },
                new { Value = "Photo", Name = "صورة" },
                new { Value = "Other", Name = "أخرى" }
            };

            return Success(categories);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDocumentStats()
        {
            try
            {
                var totalDocuments = await _context.Documents.CountAsync();
                var totalSize = await _context.Documents.SumAsync(d => d.FileSize);
                var byCategory = await _context.Documents
                    .GroupBy(d => d.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .ToListAsync();

                var stats = new
                {
                    TotalDocuments = totalDocuments,
                    TotalSize = totalSize,
                    TotalSizeDisplay = FormatFileSize(totalSize),
                    ByCategory = byCategory,
                    RecentUploads = await _context.Documents
                        .OrderByDescending(d => d.UploadedAt)
                        .Take(5)
                        .Select(d => new { d.Id, d.Title, d.UploadedAt, d.FileSize })
                        .ToListAsync()
                };

                return Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document stats");
                return Error($"فشل في جلب إحصائيات المستندات: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchDocuments([FromQuery] string q, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(q) || q.Length < 2)
                    return Error("يرجى إدخال مصطلح بحث مكون من حرفين على الأقل");

                var query = _context.Documents
                    .Include(d => d.Resident)
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.Title.Contains(q) ||
                               (d.Description != null && d.Description.Contains(q)) ||
                               (d.Tags != null && d.Tags.Contains(q)));

                // Apply security
                if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
                {
                    query = query.Where(d => d.IsPublic || d.UploadedByUserId == UserId);
                }

                var results = await query
                    .OrderByDescending(d => d.UploadedAt)
                    .Take(limit)
                    .ToListAsync();

                return Success(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents");
                return Error($"فشل في البحث: {ex.Message}");
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    // REQUEST MODELS

    public class DocumentUploadRequest
    {
        [Required(ErrorMessage = "عنوان المستند مطلوب")]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Category { get; set; } = "Other";

        public int? ResidentId { get; set; }

        public bool IsPublic { get; set; } = false;

        public string Tags { get; set; }

        [Required(ErrorMessage = "الملف مطلوب")]
        public IFormFile File { get; set; }
    }

    public class UpdateDocumentRequest
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public int? ResidentId { get; set; }

        public bool IsPublic { get; set; }

        public string Tags { get; set; }
    }

    public class ReplaceFileRequest
    {
        [Required(ErrorMessage = "الملف مطلوب")]
        public IFormFile File { get; set; }
    }

    public class BulkDocumentRequest
    {
        [Required(ErrorMessage = "معرفات الملفات مطلوبة")]
        public List<int> DocumentIds { get; set; }
    }

    public class BulkUpdateDocumentRequest
    {
        [Required(ErrorMessage = "معرفات الملفات مطلوبة")]
        public List<int> DocumentIds { get; set; }

        public string Category { get; set; }

        public bool? IsPublic { get; set; }

        public string Tags { get; set; }
    }
}