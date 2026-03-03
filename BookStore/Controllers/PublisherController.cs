using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Models;

namespace BookStore.Controllers
{
    [Authorize(Roles = "Publisher")]
    public class PublisherController : Controller
    {
        private readonly UserManager<DefaultUser> _userManager;

        public PublisherController(UserManager<DefaultUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Wallet()
        {
            var user = await _userManager
                .Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user == null)
                return NotFound();

            return View(user);
        }
    }
}