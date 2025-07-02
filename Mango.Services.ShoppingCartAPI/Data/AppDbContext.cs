using Mango.Services.ShoppingCartAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<CartHeader> CartHeaders { get; set; }
        public DbSet<CartDetails> CartDetails { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed CartHeader
            modelBuilder.Entity<CartHeader>().HasData(
                new CartHeader
                {
                    CartHeaderId = 1,
                    UserId = "1",
                    CouponCode = "WELCOME10"
                },
                new CartHeader
                {
                    CartHeaderId = 2,
                    UserId = "2",
                    CouponCode = null
                }
            );

            // Seed CartDetails
            modelBuilder.Entity<CartDetails>().HasData(
                new CartDetails
                {
                    CartDetailsId = 1,
                    CartHeaderId = 1,
                    ProductId = 101,
                    Count = 2
                },
                new CartDetails
                {
                    CartDetailsId = 2,
                    CartHeaderId = 2,
                    ProductId = 102,
                    Count = 1
                }
            );
        }
    }
}
