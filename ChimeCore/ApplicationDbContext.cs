using Microsoft.EntityFrameworkCore;
using ChimeCore.Models;

#nullable disable

namespace ChimeCore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        public DbSet<Chime> Chimes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chime>(entity =>
            {
                entity.HasKey(_ => _.Id);

                entity.Property(_ => _.Text).IsRequired();

                entity.Property(_ => _.Id).UseIdentityColumn();
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(_ => _.Id);

                entity.Property(_ => _.Id).UseIdentityColumn();
            });
        }
    }
}
