using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class DefaultUser : IdentityUser
    {
        //[PersonalData]
       // public int Id { get; set; }

        [PersonalData]
        public string? FirstName { get; set; }

        [PersonalData]
        public string? LastName { get; set; }

        [PersonalData]
        public string? ProfileImageUrl { get; set; }

        public List<Book> PurchasedBooks { get; set; } = new List<Book>();

        public decimal Wallet { get; set; } = 0.00m;

        [PersonalData]
        [DataType(DataType.Date)]
        public DateTime UserCreation { get; set; } = DateTime.Now;
    }
}
