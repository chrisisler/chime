using Microsoft.EntityFrameworkCore;
using ChimeCore.Models;

#nullable disable

namespace ChimeCore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        public DbSet<Models.Chime> Chimes { get; set; }
    }
}
