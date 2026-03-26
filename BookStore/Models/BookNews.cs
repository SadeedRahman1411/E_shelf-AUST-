using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class BookNews
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔗 Publisher Info
        public string? PublisherId { get; set; }

        [ForeignKey("PublisherId")]
        public DefaultUser? Publisher { get; set; }
    }
}