using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Models;
using BookStore.Data;

namespace BookStore.Controllers
{
    [Authorize(Roles = "Reader")]
    public class ReaderController : Controller
    {
        private readonly UserManager<DefaultUser> _userManager;
        private readonly BookStoreContext _context;

        public ReaderController(
            UserManager<DefaultUser> userManager,
            BookStoreContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> PurchasedBooks()
        {
            var user = await _userManager.Users
                .Include(u => u.PurchasedBooks)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user == null)
                return NotFound();

            return View(user.PurchasedBooks);
        }
    }
}