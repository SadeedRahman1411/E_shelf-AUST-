using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class Reaction
    {
        public int Id { get; set; }

        public int BookNewsId { get; set; }

        [ForeignKey("BookNewsId")]
        public BookNews? BookNews { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public DefaultUser? User { get; set; }

        // 👍 Like = true, 👎 Dislike = false
        public bool IsLike { get; set; }
    }
}