namespace BookStore.Models
{
    public class Receipt
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string FileUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
