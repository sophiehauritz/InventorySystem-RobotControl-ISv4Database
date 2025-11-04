using Microsoft.EntityFrameworkCore;
// Keep these so UseMySql(...) is available
using Pomelo.EntityFrameworkCore.MySql;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace InventorySystem.Models
{
    public sealed class AppDbContext : DbContext
    {
        public DbSet<Item>      Items      => Set<Item>();
        public DbSet<Order>     Orders     => Set<Order>();
        public DbSet<OrderLine> OrderLines => Set<OrderLine>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            // Adjust if you changed port/user/password
            var cs =
                "Server=127.0.0.1;Port=3306;Database=inventory;User Id=root;Password=;AllowPublicKeyRetrieval=True;SslMode=None";

            optionsBuilder.UseMySql(cs, ServerVersion.AutoDetect(cs));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TEMPORARY minimal mapping so it compiles with your current classes.

            // If your Item has NO numeric Id, use Name as the key:
            // (If you DO have an Id later, remove this line and use .HasKey(i => i.Id))
            modelBuilder.Entity<Item>().HasKey(i => i.Name);

            // We don’t know your key properties for Order / OrderLine yet.
            // Mark them keyless for now so EF doesn’t require an Id during compile.
            // (We’ll switch to real keys when we confirm property names.)
            modelBuilder.Entity<Order>().HasNoKey();
            modelBuilder.Entity<OrderLine>().HasNoKey();

            base.OnModelCreating(modelBuilder);
        }
    }
}