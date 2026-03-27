using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    public class BookNewsController : Controller
    {
        private readonly BookStoreContext _context;

        public BookNewsController(BookStoreContext context)
        {
            _context = context;
        }

        // 📢 SHOW ALL NEWS
        [AllowAnonymous]
        public IActionResult Index()
        {
            var newsList = _context.BookNews
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            // 👍 Like count
            ViewBag.LikeCounts = _context.Reactions
                .Where(r => r.IsLike)
                .GroupBy(r => r.BookNewsId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 👎 Dislike count
            ViewBag.DislikeCounts = _context.Reactions
                .Where(r => !r.IsLike)
                .GroupBy(r => r.BookNewsId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 👍 Users who liked (SAFE NULL HANDLING)
            ViewBag.LikedUsers = _context.Reactions
                .Include(r => r.User)
                .Where(r => r.IsLike)
                .AsEnumerable()
                .Select(r => new
                {
                    r.BookNewsId,
                    UserName = r.User != null && r.User.UserName != null
                        ? r.User.UserName.Split('@')[0]
                        : "User"
                })
                .GroupBy(x => x.BookNewsId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.UserName).ToList()
                );

            // 👎 Users who disliked
            ViewBag.DislikedUsers = _context.Reactions
                .Include(r => r.User)
                .Where(r => !r.IsLike)
                .AsEnumerable()
                .Select(r => new
                {
                    r.BookNewsId,
                    UserName = r.User != null && r.User.UserName != null
                        ? r.User.UserName.Split('@')[0]
                        : "User"
                })
                .GroupBy(x => x.BookNewsId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.UserName).ToList()
                );

            return View(newsList);
        }

        // 📄 CREATE PAGE
        [Authorize(Roles = "Publisher")]
        public IActionResult Create()
        {
            return View();
        }

        // 💾 SAVE NEWS
        [HttpPost]
        [Authorize(Roles = "Publisher")]
        public async Task<IActionResult> Create(BookNews news, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine("wwwroot/images", fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                news.ImageUrl = "/images/" + fileName;
            }

            news.PublisherId = userId;
            news.CreatedAt = DateTime.Now;

            _context.BookNews.Add(news);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // 👍👎 REACTION SYSTEM (AJAX)
        [HttpPost]
        [Authorize]
        public IActionResult React(int newsId, bool isLike)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existing = _context.Reactions
                .FirstOrDefault(r => r.BookNewsId == newsId && r.UserId == userId);

            if (existing != null)
            {
                if (existing.IsLike == isLike)
                {
                    _context.Reactions.Remove(existing);
                }
                else
                {
                    existing.IsLike = isLike;
                }
            }
            else
            {
                _context.Reactions.Add(new Reaction
                {
                    BookNewsId = newsId,
                    UserId = userId!,
                    IsLike = isLike
                });
            }

            _context.SaveChanges();

            var likeCount = _context.Reactions.Count(r => r.BookNewsId == newsId && r.IsLike);
            var dislikeCount = _context.Reactions.Count(r => r.BookNewsId == newsId && !r.IsLike);

            return Json(new { likeCount, dislikeCount });
        }
    }
}