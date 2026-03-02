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

        [PersonalData]
        [DataType(DataType.Date)]
        public DateTime UserCreation { get; set; } = DateTime.Now;
    }
}
