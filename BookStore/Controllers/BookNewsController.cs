using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            // 👉 After posting → go to News Feed
            return RedirectToAction("Index");
        }
    }
}