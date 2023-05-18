using Arc4u.UnitTest.Database.EfCore.Model;
using Microsoft.EntityFrameworkCore;

namespace Arc4u.UnitTest.Database.EfCore
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {

        }

        public virtual DbSet<Contract> Contracts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Reference)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate);

                entity.Property(e => e.EndDate);

                entity.Ignore(e => e.PersistChange);
            });

        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
