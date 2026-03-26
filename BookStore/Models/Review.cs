namespace BookStore.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public string UserId { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DefaultUser User { get; set; }
    }
}
