using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookStore.Controllers
{
    [Authorize(Roles = "Reader,Admin")]
    public class StoreController : Controller
    {
        private readonly BookStoreContext _context;

        public StoreController(BookStoreContext context)
        {
            _context=context;
        }
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
    }
}
