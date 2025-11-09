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

            var cs =
                "Server=127.0.0.1;Port=3306;Database=inventory;User Id=root;Password=;AllowPublicKeyRetrieval=True;SslMode=None";

            optionsBuilder.UseMySql(cs, ServerVersion.AutoDetect(cs));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Item>().HasKey(i => i.Name);

            modelBuilder.Entity<Order>().HasNoKey();
            modelBuilder.Entity<OrderLine>().HasNoKey();

            base.OnModelCreating(modelBuilder);
        }
    }
}
