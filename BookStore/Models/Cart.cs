using BookStore.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

namespace BookStore.Models
{
    public class Cart
    {
        private readonly BookStoreContext _context;

        public Cart(BookStoreContext context)
        {
            _context = context;
        }

        public string Id { get; set; }

        public List<CartItem> CartItems { get; set; }


        public static Cart GetCart(IServiceProvider services)
        {
            var httpContext = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext;
            var context = services.GetService<BookStoreContext>();

            // 1. Try to get the CartId from a COOKIE instead of Session
            string cartId = httpContext.Request.Cookies["CartId"];

            // 2. If the cookie doesn't exist, create a new ID and save it in a 30-day cookie
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();

                CookieOptions options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true, // Security: prevents JS access
                    IsEssential = true // Required for the app to function
                };

                httpContext.Response.Cookies.Append("CartId", cartId, options);
            }

            return new Cart(context) { Id = cartId };
        }
        public List<CartItem> GetAllCartItems()
        {
            return CartItems ??
               (CartItems = _context.CartItems.Where(ci => ci.CartId == Id)
               .Include(ci => ci.Book)
               .ToList());
        }

        public CartItem GetCartItem(Book book)
        {
            return _context.CartItems.SingleOrDefault(ci =>
            ci.Book.Id == book.Id && ci.CartId == Id);
        }
        public void AddToCart(Book book)
        {
            var cartItem = _context.CartItems.FirstOrDefault(ci =>
        ci.Book.Id == book.Id && ci.CartId == Id);


            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    Book = book,
                    CartId = Id
                };

                _context.CartItems.Add(cartItem);
                _context.SaveChanges();
            }

        }
        public void RemoveFromCart(int cartItemId)
        {
            var cartItem = _context.CartItems.FirstOrDefault(ci => ci.Id == cartItemId && ci.CartId == Id);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }
        }

        // Add this helper to check if a book is in the cart
        public bool IsBookInCart(int bookId)
        {
            return _context.CartItems.Any(ci => ci.Book.Id == bookId && ci.CartId == Id);
        }

        public decimal GetCartTotal()
        {
            return _context.CartItems.Where(ci => ci.CartId == Id)
                .Select(ci => ci.Book.Price)
                .Sum();
        }

    }
}
