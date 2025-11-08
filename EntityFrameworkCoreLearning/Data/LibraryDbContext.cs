using EntityFrameworkCoreLearning.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCoreLearning.Data
{
    public class LibraryDbContext:DbContext
    {
        public DbSet<BookEntity> Books { get; set; }
        public DbSet<GenreEntity> Genres { get; set; }
        public DbSet<PublisherEntity> Publishers { get; set; }
    }
}
