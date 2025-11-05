using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Docs
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public DocumentUploadRequest DocumentUpload { get; set; } = new DocumentUploadRequest();

        public List<Resident> Residents { get; set; } = new List<Resident>();

        public async Task OnGetAsync()
        {
            Residents = await _context.Residents
                .Where(r => r.IsActive)
                .OrderBy(r => r.FullName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Residents = await _context.Residents
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.FullName)
                    .ToListAsync();
                return Page();
            }

            try
            {
                // Validate file
                if (DocumentUpload.File == null || DocumentUpload.File.Length == 0)
                {
                    ModelState.AddModelError("DocumentUpload.File", "الرجاء اختيار ملف");
                    return Page();
                }

                // Validate file size (10MB max)
                if (DocumentUpload.File.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("DocumentUpload.File", "حجم الملف يجب ألا يتجاوز 10 ميجابايت");
                    return Page();
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".xls", ".xlsx", ".txt" };
                var fileExtension = Path.GetExtension(DocumentUpload.File.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("DocumentUpload.File", "نوع الملف غير مسموح به");
                    return Page();
                }

                // Create uploads directory
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(DocumentUpload.File.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await DocumentUpload.File.CopyToAsync(stream);
                }

                // Create document
                var document = new Document
                {
                    Title = DocumentUpload.Title.Trim(),
                    Description = DocumentUpload.Description?.Trim(),
                    FilePath = $"/uploads/documents/{fileName}",
                    FileType = fileExtension,
                    FileSize = DocumentUpload.File.Length,
                    Category = DocumentUpload.Category ?? "Other",
                    ResidentId = DocumentUpload.ResidentId,
                    UploadedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    IsPublic = DocumentUpload.IsPublic,
                    Tags = DocumentUpload.Tags,
                    UploadedAt = DateTime.UtcNow,
                    Status = "Active"
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم رفع المستند بنجاح";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"فشل في رفع المستند: {ex.Message}");
                Residents = await _context.Residents
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.FullName)
                    .ToListAsync();
                return Page();
            }
        }
    }

    public class DocumentUploadRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } = "Other";
        public int? ResidentId { get; set; }
        public bool IsPublic { get; set; } = false;
        public string Tags { get; set; }
        public IFormFile File { get; set; }
    }
}