using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BookStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BookStore.Data
{
    public class BookStoreContext : IdentityDbContext<DefaultUser>
    {
        public BookStoreContext (DbContextOptions<BookStoreContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
           .HasOne(b => b.Publisher)
           .WithMany() // Publisher does NOT need PublishedBooks list
           .HasForeignKey(b => b.PublisherId)
           .OnDelete(DeleteBehavior.Restrict);

            // Reader ↔ PurchasedBooks (Many-to-Many)
            modelBuilder.Entity<DefaultUser>()
            .HasMany(u => u.PurchasedBooks)
            .WithMany(b => b.Purchasers)
            .UsingEntity(j => j.ToTable("UserPurchasedBooks"));

            modelBuilder.Entity<DefaultUser>()
            .Property(u => u.Wallet)
            .HasColumnType("decimal(18,2)");


        }


        public DbSet<Book> Books { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }

}
