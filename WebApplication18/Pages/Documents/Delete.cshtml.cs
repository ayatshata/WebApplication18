using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Documents
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DeleteModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Document Document { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Document = await _context.Documents.FindAsync(id);
            if (Document == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var docInDb = await _context.Documents.FindAsync(Document.Id);
            if (docInDb == null)
                return NotFound();

         
            if (!string.IsNullOrEmpty(docInDb.FilePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, docInDb.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Documents.Remove(docInDb);
            await _context.SaveChangesAsync();

            TempData["Success"] = "?? ??? ??????? ?????!";
            return RedirectToPage("Index");
        }
    }
}
