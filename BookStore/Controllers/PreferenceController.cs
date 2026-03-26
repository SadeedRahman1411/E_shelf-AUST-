using Microsoft.AspNetCore.Mvc;
using BookStore.Data;
using BookStore.Models;
using System.Linq;
using System.Security.Claims;

namespace BookStore.Controllers
{
    public class PreferenceController : Controller
    {
        private readonly BookStoreContext _context;

        public PreferenceController(BookStoreContext context)
        {
            _context = context;
        }

        // SHOW PAGE
        public IActionResult SetPreference()
        {
            return View();
        }

        // SAVE DATA
        [HttpPost]
        public IActionResult SetPreference(string genre)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existing = _context.UserPreferences
                .FirstOrDefault(u => u.UserId == userId);

            if (existing != null)
            {
                existing.FavoriteGenre = genre;
            }
            else
            {
                var pref = new UserPreference
                {
                    UserId = userId,
                    FavoriteGenre = genre
                };

                _context.UserPreferences.Add(pref);
            }

            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
    }
}