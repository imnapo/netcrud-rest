using Microsoft.EntityFrameworkCore;

namespace NetCrud.Rest.Example.Models
{
    public class NetCrudDbContext : DbContext
    {
        public NetCrudDbContext(DbContextOptions<NetCrudDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
          .HasOne<Address>(ps => ps.Address)
          .WithOne(jc => jc.Customer)
          .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Product> Products { get; set; }

    }
}
