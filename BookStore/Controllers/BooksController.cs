using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;
using BookStore.Services;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace BookStore.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookStoreContext _context;
        private readonly GoogleDriveService _driveService;

        public BooksController(BookStoreContext context, GoogleDriveService driveService)
        {
            _context = context;
            _driveService = driveService;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return View(await _context.Books.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Title,Description,Language,ISBN,DatePublished,Price,Author")] Book book,
            IFormFile imageFile, IFormFile pdfFile)
        {
            if (!ModelState.IsValid)
                return View(book);

            if (imageFile != null && imageFile.Length > 0)
            {
                string extension = Path.GetExtension(imageFile.FileName);
                string fileName = $"{Guid.NewGuid()}{extension}";

                using var stream = imageFile.OpenReadStream();
                string imageUrl = await _driveService.UploadFileAsync(stream, fileName, imageFile.ContentType);

                book.ImageUrl = imageUrl;
            }

            if (pdfFile != null && pdfFile.Length > 0)
            {
                book.PdfUrl = await _driveService.UploadFileAsync(pdfFile.OpenReadStream(),
                    $"{Guid.NewGuid()}.pdf", "application/pdf");
            }

            _context.Add(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
       int id,
       [Bind("Id,Title,Description,Language,ISBN,DatePublished,Price,Author")] Book updatedBook,
       IFormFile? imageFile,
       IFormFile? pdfFile) // Added pdfFile parameter
        {
            if (id != updatedBook.Id) return NotFound();

            if (!ModelState.IsValid) return View(updatedBook);

            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null) return NotFound();

            try
            {
                // 1. Update text fields
                existingBook.Title = updatedBook.Title;
                existingBook.Description = updatedBook.Description;
                existingBook.Language = updatedBook.Language;
                existingBook.ISBN = updatedBook.ISBN;
                existingBook.DatePublished = updatedBook.DatePublished;
                existingBook.Price = updatedBook.Price;
                existingBook.Author = updatedBook.Author;

                // 2. ONLY update Image if a new one is uploaded
                if (imageFile != null && imageFile.Length > 0)
                {
                    string extension = Path.GetExtension(imageFile.FileName);
                    string fileName = $"{existingBook.Id}_{Guid.NewGuid()}{extension}";
                    using var stream = imageFile.OpenReadStream();
                    existingBook.ImageUrl = await _driveService.UploadFileAsync(stream, fileName, imageFile.ContentType);
                }
                // If imageFile is null, existingBook.ImageUrl remains what it was in the DB

                // 3. ONLY update PDF if a new one is uploaded
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    string fileName = $"{existingBook.Id}_{Guid.NewGuid()}.pdf";
                    using var stream = pdfFile.OpenReadStream();
                    existingBook.PdfUrl = await _driveService.UploadFileAsync(stream, fileName, "application/pdf");
                }
                // If pdfFile is null, existingBook.PdfUrl remains what it was in the DB

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Books.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
                return NotFound();

            return View(book);//generates the front end pages folder
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book != null)
                _context.Books.Remove(book);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
