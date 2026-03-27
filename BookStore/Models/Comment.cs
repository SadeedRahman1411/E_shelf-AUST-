using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class Comment
    {
        // 🔑 Primary Key
        [Key]
        public int Id { get; set; }

        // 🔗 Relation with BookNews
        [Required]
        public int BookNewsId { get; set; }

        [ForeignKey("BookNewsId")]
        public BookNews? BookNews { get; set; }

        // 🔗 Relation with User
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public DefaultUser? User { get; set; }

        // 💬 Comment Content
        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        // 🕒 Created Time
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔁 Self-reference (Reply System)
        public int? ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        public Comment? ParentComment { get; set; }

        // 📌 Child Replies
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}