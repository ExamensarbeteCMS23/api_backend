using api_backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api_backend.Contexts
{
    public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<BookingCleanerEntity> BookingCleaner { get; set; }
        public DbSet<BookingEntity> Bookings { get; set; }
        public DbSet<EmployeeEntity> Employees { get; set; }
        public DbSet<CustomerAddressEntity> CustomerAddresses { get; set; }
        public DbSet<CustomerEntity> Customers { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Kompisitnyckel för BookingCleanerEntity
            modelBuilder.Entity<BookingCleanerEntity>()
                .HasKey(bc => new { bc.BookingId, bc.CleanerId });

            // Booking -> BookingCleaner
            modelBuilder.Entity<BookingCleanerEntity>()
                .HasOne(bc => bc.Booking)
                .WithMany(b => b.BookingCleaners)
                .HasForeignKey(bc => bc.BookingId);

            // Cleaner -> BookingCleaner
            modelBuilder.Entity<BookingCleanerEntity>()
                .HasOne(bc => bc.Cleaner)
                .WithMany(c => c.BookingCleaners)
                .HasForeignKey(bc => bc.CleanerId);

            // CustomerAddress -> Customers
            modelBuilder.Entity<CustomerAddressEntity>()
                .HasMany(ca => ca.Customers)
                .WithOne()
                .HasForeignKey(c => c.AddressId);

            // Role -> Cleaners
            modelBuilder.Entity<RoleEntity>()
                .HasMany(r => r.Cleaners)
                .WithOne(c => c.Role)
                .HasForeignKey(c => c.RoleId);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(a => a.Employee)
                .WithOne()
                .HasForeignKey<ApplicationUser>(a => a.EmployeeId)
                .IsRequired();
        }

    }
}
