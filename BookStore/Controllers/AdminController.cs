using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly BookStoreContext _context;

        public AdminController(BookStoreContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Reported()
        {
            var books = await _context.Books
                .Where(b => b.Status == BookStatus.Reported)
                .ToListAsync();

            return View(books);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptReport(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                book.Status = BookStatus.Report_Ack;

                var reporterId = book.ReportedByUserId;
                var publisherId = book.PublisherId;

                // reporter
                _context.Notifications.Add(new Notification
                {
                    UserId = reporterId,
                    Message = $"Your report for '{book.Title}' was accepted",
                    //Link = "/Store/Details/" + book.Id
                });

                // publisher
                _context.Notifications.Add(new Notification
                {
                    UserId = publisherId,
                    Message = $"Your book '{book.Title}' was reported and accepted",
                    Link = "/Books/Details/" + book.Id
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Reported");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyReport(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                book.Status = BookStatus.Allowed;

                var reporterId = book.ReportedByUserId;

                _context.Notifications.Add(new Notification
                {
                    UserId = reporterId,
                    Message = $"Your report for '{book.Title}' was rejected",
                    Link = "/Store/Details/" + book.Id
                });

                book.ReportMessage = null;
                book.ReportedByUserId = null;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Reported");
        }

        public async Task<IActionResult> Halted()
        {
            var books = await _context.Books
                .Where(b => b.Status == BookStatus.Report_Ack)
                .ToListAsync();

            return View(books);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unban(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                book.Status = BookStatus.Allowed;

                var publisherId = book.PublisherId;
                var reporterId = book.ReportedByUserId;

                _context.Notifications.Add(new Notification
                {
                    UserId = publisherId,
                    Message = $"Your book '{book.Title}' has been unbanned",
                    Link = "/Books/Details/" + book.Id
                });

                _context.Notifications.Add(new Notification
                {
                    UserId = reporterId,
                    Message = $"Book '{book.Title}' you reported has been unbanned"
                });

                book.ReportMessage = null;
                book.ReportedByUserId = null;


                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Halted");
        }
    }
}
