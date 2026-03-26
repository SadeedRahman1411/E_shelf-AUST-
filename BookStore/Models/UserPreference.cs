using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class UserPreference
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required]
        public string FavoriteGenre { get; set; }

        [ForeignKey("UserId")]
        public DefaultUser? User { get; set; }
    }
}