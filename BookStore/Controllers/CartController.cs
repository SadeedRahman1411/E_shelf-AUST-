using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class CartController : Controller
    {
        private readonly BookStoreContext _context;
        private readonly Cart _cart;

        public CartController(BookStoreContext context, Cart cart)
        {
            _cart = cart;
            _context = context;

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
    }
}
