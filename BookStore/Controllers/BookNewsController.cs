using BookStore.Data;
using BookStore.Models;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookStore.Controllers
{
    public class BookNewsController : Controller
    {
        private readonly BookStoreContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public BookNewsController(BookStoreContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
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

            // 💬 COMMENTS (🔥 THIS IS WHAT YOU ASKED)
            ViewBag.Comments = _context.Comments
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            // 👍 Users who liked
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

        // CREATE PAGE
        [Authorize(Roles = "Publisher")]
        public IActionResult Create()
        {
            return View();
        }

        // SAVE NEWS
        [HttpPost]
        [Authorize(Roles = "Publisher")]
        public async Task<IActionResult> Create(BookNews news, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (imageFile != null && imageFile.Length > 0)
            {
                string extension = Path.GetExtension(imageFile.FileName);
                string fileName = $"{Guid.NewGuid()}{extension}";

                using var stream = imageFile.OpenReadStream();
                news.ImageUrl = await _cloudinaryService.UploadFileAsync(stream, fileName);  
            }

            news.PublisherId = userId;
            news.CreatedAt = DateTime.Now;

            _context.BookNews.Add(news);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        //  REACTION SYSTEM (AJAX)
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

        // 💬 NORMAL COMMENT (fallback)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int newsId, string content, int? parentCommentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Index");

            var comment = new Comment
            {
                BookNewsId = newsId,
                UserId = userId!,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ⚡ AJAX COMMENT (MAIN SYSTEM)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddCommentAjax(int newsId, string content, int? parentCommentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(content))
                return Json(new { success = false });

            var comment = new Comment
            {
                BookNewsId = newsId,
                UserId = userId!,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return Json(new
            {
                success = true,
                username = user?.UserName?.Split('@')[0] ?? "User",
                content = comment.Content,
                commentId = comment.Id,
                parentId = comment.ParentCommentId
            });
        }

        // ❌ DELETE COMMENT
        [HttpPost]
        public IActionResult DeleteComment(int id)
        {
            var comment = _context.Comments.Find(id);

            if (comment != null)
            {
                _context.Comments.Remove(comment);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }
    }
}