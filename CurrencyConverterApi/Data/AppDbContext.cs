using CurrencyConverterApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverterApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts)
            : base(opts) { }

        public DbSet<User> Users { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<User>()
              .HasIndex(u => u.Email)
              .IsUnique();

            mb.Entity<OtpCode>()
              .HasOne(o => o.User)
              .WithMany()
              .HasForeignKey(o => o.UserId);
        }
    }
}
