using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Payments
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Payment Payment { get; set; } = new Payment();

        public SelectList Residents { get; set; }

        public void OnGet()
        {
            Residents = new SelectList(_context.Residents, "Id", "FullName");
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Residents = new SelectList(_context.Residents, "Id", "FullName");
                return Page();
            }

            _context.Payments.Add(Payment);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "?? ??? ?????? ????? ?";
            return RedirectToPage("Index");
        }
    }
}
