using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }
        public string Language { get; set; }

        [Required,
        MaxLength(13)]
        public string ISBN { get; set; }

        [Required,
        DataType(DataType.Date),
        Display(Name = "Date Published")]
        public DateTime DatePublished { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } 

        [Required]
        public string Author { get; set; }

        // Many readers can purchase one book
        public List<DefaultUser> Purchasers { get; set; } = new List<DefaultUser>();

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "PDF URL")]
        public string? PdfUrl { get; set; }

        public BookStatus Status { get; set; } = BookStatus.Queued;

        public string? PublisherId { get; set; }

        [ForeignKey("PublisherId")]
        public DefaultUser? Publisher { get; set; }

        public string? ReportMessage { get; set; }

        public string? ReportedByUserId { get; set; }

        
    }
}
