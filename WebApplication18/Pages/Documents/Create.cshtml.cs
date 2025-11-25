using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Documents
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CreateModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Document Document { get; set; }

        [BindProperty]
        public IFormFile UploadFile { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (UploadFile != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var fileName = Path.GetFileName(UploadFile.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadFile.CopyToAsync(stream);
                }

                Document.FilePath = $"/uploads/{fileName}";
                Document.FileType = Path.GetExtension(fileName);
                Document.FileSize = UploadFile.Length;
                Document.UploadedAt = DateTime.UtcNow;
                Document.UploadedByUserId = User.Identity.Name ?? "Unknown";

                _context.Documents.Add(Document);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم رفع المستند بنجاح!";
                return RedirectToPage("Index");
            }

            ModelState.AddModelError("", "يرجى اختيار ملف للرفع.");
            return Page();
        }
    }
}
