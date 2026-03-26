using BookStore.Data;
using BookStore.Models;
using Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Reader,Admin")]
public class ReviewController : Controller
{
    private readonly BookStoreContext _context;
    private readonly UserManager<DefaultUser> _userManager;

    public ReviewController(BookStoreContext context, UserManager<DefaultUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Add(int bookId, int rating, string comment)
    {
        var userId = _userManager.GetUserId(User);

        // ✅ Check purchase
        var user = await _context.Users
            .Include(u => u.PurchasedBooks)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (!user.PurchasedBooks.Any(b => b.Id == bookId))
            return Unauthorized();

        // ❌ Prevent duplicate
        var existing = await _context.Reviews
            .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId);

        if (existing != null)
            return BadRequest("Already reviewed");

        var review = new Review
        {
            BookId = bookId,
            UserId = userId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.Now
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Store", new { id = bookId });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, int rating, string comment)
    {
        var userId = _userManager.GetUserId(User);

        var review = await _context.Reviews.FindAsync(id);

        if (review == null) return NotFound();

        if (review.UserId != userId && !User.IsInRole("Admin"))
            return Unauthorized();

        review.Rating = rating;
        review.Comment = comment;

        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Store", new { id = review.BookId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);

        var review = await _context.Reviews.FindAsync(id);

        if (review == null) return NotFound();

        if (review.UserId != userId && !User.IsInRole("Admin"))
            return Unauthorized();

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Store", new { id = review.BookId });
    }
}