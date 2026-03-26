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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var preferences = _context.UserPreferences
                .Where(u => u.UserId == userId)
                .ToList();

            return View(preferences);
        }

        // SAVE
        [HttpPost]
        public IActionResult SetPreference(string genre)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var exists = _context.UserPreferences
                .Any(u => u.UserId == userId && u.Genre == genre);

            if (!exists)
            {
                _context.UserPreferences.Add(new UserPreference
                {
                    UserId = userId,
                    Genre = genre
                });

                _context.SaveChanges();
            }

            return RedirectToAction("SetPreference");
        }

        // 🔥 REMOVE (MAIN FIX)
        [HttpPost]
        public IActionResult RemovePreference(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pref = _context.UserPreferences
                .FirstOrDefault(p => p.Id == id && p.UserId == userId);

            if (pref != null)
            {
                _context.UserPreferences.Remove(pref);
                _context.SaveChanges();
            }

            return RedirectToAction("SetPreference");
        }
    }
}