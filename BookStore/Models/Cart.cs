using BookStore.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
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

        public string UserId { get; set; }   // FK

        [ForeignKey("UserId")]
        public DefaultUser User { get; set; }

        public List<CartItem> CartItems { get; set; }


        public static Cart GetCart(IServiceProvider services)
        {
            var httpContext = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext;
            var context = services.GetService<BookStoreContext>();

            var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            return new Cart(context) { Id = userId };
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
