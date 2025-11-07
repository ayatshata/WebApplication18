using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Documents
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EditModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Document Document { get; set; }

        [BindProperty]
        public IFormFile? UploadFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Document = await _context.Documents.FindAsync(id);
            if (Document == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var docInDb = await _context.Documents.FindAsync(Document.Id);
            if (docInDb == null)
                return NotFound();

            // تحديث الحقول
            docInDb.Title = Document.Title;
            docInDb.Description = Document.Description;
            docInDb.Category = Document.Category;
            docInDb.IsPublic = Document.IsPublic;
            docInDb.UpdatedAt = DateTime.UtcNow;

            // استبدال الملف إذا تم رفع واحد جديد
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

                docInDb.FilePath = $"/uploads/{fileName}";
                docInDb.FileType = Path.GetExtension(fileName);
                docInDb.FileSize = UploadFile.Length;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تعديل المستند بنجاح!";
            return RedirectToPage("Index");
        }
    }
}
