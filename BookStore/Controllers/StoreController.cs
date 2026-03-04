using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookStore.Controllers
{
    [Authorize(Roles = "Reader,Admin")]
    public class StoreController : Controller
    {
        private readonly BookStoreContext _context;
        private readonly UserManager<DefaultUser> _userManager;

        public StoreController(BookStoreContext context, UserManager<DefaultUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        /*
        public async Task<IActionResult> Index()
        {
            var allowedBooks = await _context.Books
         .Where(b => b.Status == BookStatus.Allowed)
         .ToListAsync();

            return View(allowedBooks);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
                return NotFound();

            return View(book);
        }
        */
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books
                .Where(b => b.Status == BookStatus.Allowed || b.Status == BookStatus.Reported)
                .ToListAsync();

            if (User.IsInRole("Reader"))
            {
                var userId = _userManager.GetUserId(User);

                var user = await _context.Users
                    .Include(u => u.PurchasedBooks)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                ViewBag.PurchasedBookIds = user?.PurchasedBooks
                    .Select(b => b.Id)
                    .ToList() ?? new List<int>();
            }
            else
            {
                ViewBag.PurchasedBookIds = new List<int>();
            }

            return View(books);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            if (User.IsInRole("Reader"))
            {
                var userId = _userManager.GetUserId(User);

                var user = await _context.Users
                    .Include(u => u.PurchasedBooks)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                ViewBag.PurchasedBookIds = user?.PurchasedBooks
                    .Select(b => b.Id)
                    .ToList() ?? new List<int>();
            }

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> ReportBook(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            if (book.Status == BookStatus.Allowed)
            {
                book.Status = BookStatus.Reported;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = id });
        }
    }
}
