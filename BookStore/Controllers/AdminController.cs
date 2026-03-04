using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly BookStoreContext _context;

        public AdminController(BookStoreContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Reported()
        {
            var books = await _context.Books
                .Where(b => b.Status == BookStatus.Reported)
                .ToListAsync();

            return View(books);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptReport(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                book.Status = BookStatus.Report_Ack;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Reported");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyReport(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                book.Status = BookStatus.Allowed;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Reported");
        }

        public async Task<IActionResult> Halted()
        {
            var books = await _context.Books
                .Where(b => b.Status == BookStatus.Report_Ack)
                .ToListAsync();

            return View(books);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unban(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                book.Status = BookStatus.Allowed;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Halted");
        }
    }
}
