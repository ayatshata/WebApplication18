using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;
using System.ComponentModel.DataAnnotations;

namespace MughtaribatHouse.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class DocsController : Controller
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

        public async Task<IActionResult> Index(string search = null, string category = null, int? residentId = null)
        {
            var query = _context.Documents.Include(d => d.Resident).Include(d => d.UploadedByUser).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Title.Contains(search) ||
                                       (d.Description != null && d.Description.Contains(search)) ||
                                       (d.Tags != null && d.Tags.Contains(search)));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(d => d.Category == category);

            if (residentId.HasValue)
                query = query.Where(d => d.ResidentId == residentId);

       
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                string userId = GetUserIdSafe();
                query = query.Where(d => d.IsPublic || d.UploadedByUserId == userId);
            }

            var documents = await query.OrderByDescending(d => d.UploadedAt).ToListAsync();
            return View(documents);
        }

 
        [HttpGet("Create")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public IActionResult Create() => View();

        [HttpPost("Create")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> Create(DocumentUploadRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            if (request.File == null || request.File.Length == 0)
            {
                ModelState.AddModelError("", "الرجاء اختيار ملف");
                return View(request);
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".xls", ".xlsx", ".txt" };
            var fileExtension = Path.GetExtension(request.File.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("", "نوع الملف غير مسموح به");
                return View(request);
            }

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.File.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var document = new Document
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                FilePath = $"/uploads/documents/{fileName}",
                FileType = fileExtension,
                FileSize = request.File.Length,
                Category = request.Category ?? "Other",
                ResidentId = request.ResidentId,
                UploadedByUserId = GetUserIdSafe(),
                IsPublic = request.IsPublic,
                UploadedAt = DateTime.UtcNow,
                Status = "Active",
                Tags = request.Tags
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم رفع الملف بنجاح";
            return RedirectToAction("Index");
        }

   
        [HttpGet("Edit/{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> Edit(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            if (!CanEditDocument(document))
            {
                TempData["Error"] = "ليس لديك صلاحية لتعديل هذا الملف";
                return RedirectToAction("Index");
            }

            var model = new UpdateDocumentRequest
            {
                Title = document.Title,
                Description = document.Description,
                Category = document.Category,
                ResidentId = document.ResidentId,
                IsPublic = document.IsPublic,
                Tags = document.Tags
            };

            ViewBag.DocumentId = id;
            return View(model);
        }

        [HttpPost("Edit/{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> Edit(int id, UpdateDocumentRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            if (!CanEditDocument(document))
            {
                TempData["Error"] = "ليس لديك صلاحية لتعديل هذا الملف";
                return RedirectToAction("Index");
            }

            document.Title = request.Title?.Trim() ?? document.Title;
            document.Description = request.Description?.Trim();
            document.Category = request.Category ?? document.Category;
            document.IsPublic = request.IsPublic;
            document.Tags = request.Tags;
            document.UpdatedAt = DateTime.UtcNow;

            if (request.ResidentId.HasValue && request.ResidentId != document.ResidentId)
            {
                var exists = await _context.Residents.AnyAsync(r => r.Id == request.ResidentId);
                if (!exists)
                {
                    ModelState.AddModelError("", "المقيم غير موجود");
                    return View(request);
                }
                document.ResidentId = request.ResidentId;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث الملف بنجاح";
            return RedirectToAction("Index");
        }

        [HttpPost("Delete/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الملف بنجاح";
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            if (!CanAccessDocument(document))
            {
                TempData["Error"] = "ليس لديك صلاحية لتحميل هذا الملف";
                return RedirectToAction("Index");
            }

            var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, GetContentType(document.FileType), $"{document.Title}{document.FileType}");
        }


        private bool CanAccessDocument(Document doc)
        {
            string userId = GetUserIdSafe();
            return doc.IsPublic || doc.UploadedByUserId == userId
                   || User.IsInRole("Admin") || User.IsInRole("Manager");
        }

        private bool CanEditDocument(Document doc)
        {
            string userId = GetUserIdSafe();
            return doc.UploadedByUserId == userId
                   || User.IsInRole("Admin") || User.IsInRole("Manager");
        }

        private string GetUserIdSafe()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "";
        }

        private string GetContentType(string fileType) => fileType.ToLower() switch
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


    public class DocumentUploadRequest
    {
        [Required] public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } = "Other";
        public int? ResidentId { get; set; }
        public bool IsPublic { get; set; } = false;
        public string Tags { get; set; }
        [Required] public IFormFile File { get; set; }
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
}
