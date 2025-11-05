using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Controllers
{
    [Authorize]
    public class ResidentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResidentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var residents = await _context.Residents.ToListAsync();
            return View(residents);
        }

        public async Task<IActionResult> Details(int id)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null) return NotFound();
            return View(resident);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resident resident)
        {
            if (ModelState.IsValid)
            {
                _context.Add(resident);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(resident);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null) return NotFound();
            return View(resident);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Resident resident)
        {
            if (id != resident.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(resident);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(resident);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null) return NotFound();
            return View(resident);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident != null)
            {
                _context.Residents.Remove(resident);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
