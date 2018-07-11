using LibraryExercise.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryExercise.Persistence
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<Patron> Patrons { get; set; }
    }
}