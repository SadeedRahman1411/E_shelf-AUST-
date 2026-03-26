using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

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

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5;

            var query = _context.Books
                .Where(b => b.Status == BookStatus.Allowed || b.Status == BookStatus.Reported);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b => b.Title.Contains(searchString));
            }

            int totalBooks = await query.CountAsync();

            var books = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (User.IsInRole("Reader"))
            {
                var userId = _userManager.GetUserId(User);

                var user = await _context.Users
                    .Include(u => u.PurchasedBooks)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                var purchasedIds = user?.PurchasedBooks
                    .Select(b => b.Id)
                    .ToList() ?? new List<int>();

                ViewBag.PurchasedBookIds = purchasedIds;

                // 🔥 GET USER GENRES
                var userGenres = await _context.UserPreferences
                    .Where(u => u.UserId == userId)
                    .Select(u => u.Genre)
                    .ToListAsync();

                if (userGenres.Any())
                {
                    // ✅ NORMALIZE OUTSIDE QUERY (IMPORTANT FIX)
                    var normalizedGenres = userGenres
                        .Select(g => g.Trim().ToLower())
                        .ToList();

                    var allBooks = await _context.Books
                        .Where(b => (b.Status == BookStatus.Allowed || b.Status == BookStatus.Reported))
                        .ToListAsync();

                    // ✅ FILTER IN MEMORY (100% RELIABLE)
                    var recommendedBooks = allBooks
                        .Where(b =>
                            normalizedGenres.Contains(b.Genre.Trim().ToLower()) &&
                            !purchasedIds.Contains(b.Id))
                        .ToList();

                    ViewBag.RecommendedBooks = recommendedBooks;
                }
                else
                {
                    ViewBag.RecommendedBooks = new List<Book>();
                }
            }
            else
            {
                ViewBag.PurchasedBookIds = new List<int>();
                ViewBag.RecommendedBooks = new List<Book>();
            }

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBooks / pageSize);

            return View(books);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            var purchasedIds = new List<int>();

            if (User.IsInRole("Reader"))
            {
                var userId = _userManager.GetUserId(User);

                var user = await _context.Users
                    .Include(u => u.PurchasedBooks)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                purchasedIds = user?.PurchasedBooks
                    .Select(b => b.Id)
                    .ToList() ?? new List<int>();
            }

            ViewBag.PurchasedBookIds = purchasedIds;

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Reviews = reviews;

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);

                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.BookId == id && r.UserId == userId);

                ViewBag.UserReview = existingReview;
            }

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> ReportBook(int id, string reportMessage)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            if (book.Status == BookStatus.Allowed)
            {
                book.Status = BookStatus.Reported;
                book.ReportMessage = reportMessage;
                book.ReportedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = id });
        }
    }
}