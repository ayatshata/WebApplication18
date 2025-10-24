using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Docs
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Document Document { get; set; }
        public List<Resident> Residents { get; set; } = new List<Resident>();

        [BindProperty]
        public UpdateDocumentRequest UpdateRequest { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Document = await _context.Documents
                .Include(d => d.Resident)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (Document == null)
            {
                return NotFound();
            }

            // Check if user can edit this document
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Document.UploadedByUserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            Residents = await _context.Residents
                .Where(r => r.IsActive)
                .OrderBy(r => r.FullName)
                .ToListAsync();

            // Initialize the update request with current values
            UpdateRequest = new UpdateDocumentRequest
            {
                Title = Document.Title,
                Description = Document.Description,
                Category = Document.Category,
                ResidentId = Document.ResidentId,
                IsPublic = Document.IsPublic,
                Tags = Document.Tags
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Document = await _context.Documents.FindAsync(id);
            if (Document == null)
            {
                return NotFound();
            }

            // Check if user can edit this document
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Document.UploadedByUserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

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
                // Update document properties
                Document.Title = UpdateRequest.Title.Trim();
                Document.Description = UpdateRequest.Description?.Trim();
                Document.Category = UpdateRequest.Category;
                Document.ResidentId = UpdateRequest.ResidentId;
                Document.IsPublic = UpdateRequest.IsPublic;
                Document.Tags = UpdateRequest.Tags;
                Document.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تحديث المستند بنجاح";
                return RedirectToPage("./Details", new { id = Document.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"فشل في تحديث المستند: {ex.Message}");
                Residents = await _context.Residents
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.FullName)
                    .ToListAsync();
                return Page();
            }
        }
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