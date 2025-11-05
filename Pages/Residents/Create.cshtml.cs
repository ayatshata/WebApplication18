using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MughtaribatHouse.Data; 
using MughtaribatHouse.Models; 

namespace MughtaribatHouse.Pages.Residents
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public Resident Resident { get; set; }

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Residents.Add(Resident);
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
