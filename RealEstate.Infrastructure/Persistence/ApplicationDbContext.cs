using Microsoft.EntityFrameworkCore;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opt) : base(opt) { }

            public DbSet<Property> Properties => Set<Property>();
            public DbSet<Owner> Owners => Set<Owner>();
            public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
            public DbSet<PropertyTrace> PropertyTraces => Set<PropertyTrace>();

        protected override void OnModelCreating(ModelBuilder b) {

            b.Entity<Property>(e => {
                e.ToTable("Property");
                e.HasKey(x => x.IdProperty);

                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                e.Property(x => x.Address)
                    .IsRequired()
                    .HasMaxLength(250);

                e.Property(x => x.CodeInternal)
                    .IsRequired()
                    .HasMaxLength(50);

                e.HasIndex(x => x.CodeInternal).IsUnique();

                e.Property(x => x.Price).HasColumnType("decimal(18,2)");

                e.HasIndex(x => x.Price);
                e.HasIndex(x => x.Year);

                e.HasOne(x => x.Owner)
                    .WithMany(o => o.Properties) 
                    .HasForeignKey(x => x.IdOwner)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(x => x.Images)
                    .WithOne()
                    .HasForeignKey(i => i.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(x => x.Traces)
                    .WithOne(t => t.Property)
                    .HasForeignKey(t => t.IdProperty)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<Owner>(e => {
                e.ToTable("Owner");
                e.HasKey(x => x.IdOwner);

                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                e.Property(x => x.Address).HasMaxLength(250);
                e.Property(x => x.Photo).HasMaxLength(300);
            });

            b.Entity<PropertyImage>(e => {
                e.ToTable("PropertyImage");
                e.HasKey(x => x.IdPropertyImage);

                e.Property(x => x.Url)
                    .IsRequired()
                    .HasMaxLength(300);

                e.Property(x => x.IsCover)
                    .HasDefaultValue(false);

                e.HasIndex(x => x.PropertyId);

             });

            b.Entity<PropertyTrace>(e => {
                e.ToTable("PropertyTrace");
                e.HasKey(x => x.IdPropertyTrace);

                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                e.Property(x => x.Value).HasColumnType("decimal(18,2)");
                e.Property(x => x.Tax).HasColumnType("decimal(18,2)");

                e.HasIndex(x => x.DateSale);
            });
        }
    }
}
