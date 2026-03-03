using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using Stripe;


namespace BookStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly BookStoreContext _context;
        private readonly Cart _cart;
        private readonly UserManager<DefaultUser> _userManager;

        public CartController(BookStoreContext context, Cart cart, UserManager<DefaultUser> userManager)
        {
            _cart = cart;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var items = _cart.GetAllCartItems();
            _cart.CartItems = items;

            return View(_cart);
        }

        public IActionResult AddToCart(int id)
        {
            var selectBook = GetBookbyId(id);

            if (selectBook != null)
            {
                _cart.AddToCart(selectBook);
            }

            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int id)
        {
            _cart.RemoveFromCart(id);
            return RedirectToAction("Index");
        }

        public Book GetBookbyId(int id)
        {
            return _context.Books.FirstOrDefault(b => b.Id == id);
        }

        /*

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> Purchase()
        {
            var userId = _userManager.GetUserId(User);

            var user = await _context.Users
                .Include(u => u.PurchasedBooks)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var cartItems = _cart.GetAllCartItems();

            if (!cartItems.Any())
                return RedirectToAction("Index");

            foreach (var item in cartItems)
            {
                // Check duplicate in DB properly
                if (!user.PurchasedBooks.Any(b => b.Id == item.Book.Id))
                {
                    user.PurchasedBooks.Add(item.Book);

                    // 🔥 Load publisher explicitly
                    var publisher = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == item.Book.PublisherId);

                    if (publisher != null)
                    {
                        publisher.Wallet += item.Book.Price;
                    }
                }
            }

            // Clear cart
            foreach (var item in cartItems)
            {
                _context.CartItems.Remove(item);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Store");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> PurchaseSingle(int bookId)
        {
            var userId = _userManager.GetUserId(User);

            var user = await _context.Users
                .Include(u => u.PurchasedBooks)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                return RedirectToAction("Index", "Store");

            if (!user.PurchasedBooks.Any(b => b.Id == bookId))
            {
                user.PurchasedBooks.Add(book);

                var publisher = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == book.PublisherId);

                if (publisher != null)
                {
                    publisher.Wallet += book.Price;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("PurchasedBooks", "Reader");
        }
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> Purchase()
        {
            var cartItems = _cart.GetAllCartItems();

            if (!cartItems.Any())
                return RedirectToAction("Index");

            var domain = "https://localhost:7198";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                SuccessUrl = domain + "/Cart/SuccessCart",
                CancelUrl = domain + "/Cart/Index",
                LineItems = new List<SessionLineItemOptions>()
            };

            foreach (var item in cartItems)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(item.Book.Price * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Book.Title
                        }
                    },
                    Quantity = 1
                });
            }

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> SuccessCart()
        {
            var userId = _userManager.GetUserId(User);

            var user = await _context.Users
                .Include(u => u.PurchasedBooks)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var cartItems = _cart.GetAllCartItems();

            foreach (var item in cartItems)
            {
                if (!user.PurchasedBooks.Any(b => b.Id == item.Book.Id))
                {
                    user.PurchasedBooks.Add(item.Book);

                    var publisher = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == item.Book.PublisherId);

                    if (publisher != null)
                        publisher.Wallet += item.Book.Price;
                }

                _context.CartItems.Remove(item);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("PurchasedBooks", "Reader");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> PurchaseSingle(int bookId)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                return RedirectToAction("Index", "Store");

            var domain = "https://localhost:7198";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                SuccessUrl = domain + $"/Cart/SuccessSingle?bookId={bookId}",
                CancelUrl = domain + "/Store/Details/" + bookId,
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(book.Price * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = book.Title
                    }
                },
                Quantity = 1
            }
        }
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> SuccessSingle(int bookId)
        {
            var userId = _userManager.GetUserId(User);

            var user = await _context.Users
                .Include(u => u.PurchasedBooks)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);

            if (book != null && !user.PurchasedBooks.Any(b => b.Id == bookId))
            {
                user.PurchasedBooks.Add(book);

                var publisher = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == book.PublisherId);

                if (publisher != null)
                    publisher.Wallet += book.Price;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("PurchasedBooks", "Reader");
        }
    }
}
