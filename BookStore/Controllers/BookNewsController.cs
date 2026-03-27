using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.Controllers
{
    public class BookNewsController : Controller
    {
        private readonly BookStoreContext _context;

        public BookNewsController(BookStoreContext context)
        {
            _context = context;
        }

        // 📢 SHOW ALL NEWS (FOR EVERYONE)
        [AllowAnonymous]
        public IActionResult Index()
        {
            var newsList = _context.BookNews
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            // 🔥 LIKE COUNT
            ViewBag.LikeCounts = _context.Reactions
                .Where(r => r.IsLike)
                .GroupBy(r => r.BookNewsId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 🔥 DISLIKE COUNT
            ViewBag.DislikeCounts = _context.Reactions
                .Where(r => !r.IsLike)
                .GroupBy(r => r.BookNewsId)
                .ToDictionary(g => g.Key, g => g.Count());

            return View(newsList);
        }

        // 📄 SHOW CREATE FORM (ONLY PUBLISHER)
        [Authorize(Roles = "Publisher")]
        public IActionResult Create()
        {
            return View();
        }

        // 💾 SAVE NEWS (ONLY PUBLISHER)
        [HttpPost]
        [Authorize(Roles = "Publisher")]
        public async Task<IActionResult> Create(BookNews news, IFormFile imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 📸 Image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine("wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                news.ImageUrl = "/images/" + fileName;
            }

            news.PublisherId = userId;
            news.CreatedAt = DateTime.Now;

            _context.BookNews.Add(news);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // 👍👎 REACTION SYSTEM
        [HttpPost]
        [Authorize]
        public IActionResult React(int newsId, bool isLike)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existing = _context.Reactions
                .FirstOrDefault(r => r.BookNewsId == newsId && r.UserId == userId);

            if (existing != null)
            {
                // 🔁 Update reaction
                existing.IsLike = isLike;
            }
            else
            {
                // ➕ Add new reaction
                var reaction = new Reaction
                {
                    BookNewsId = newsId,
                    UserId = userId,
                    IsLike = isLike
                };

                _context.Reactions.Add(reaction);
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}