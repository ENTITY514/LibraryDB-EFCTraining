using EntityFrameworkCoreLearning.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCoreLearning.Data
{
    public class LibraryDbContext:DbContext
    {
        public DbSet<BookEntity> Books { get; set; }
        public DbSet<GenreEntity> Genres { get; set; }
        public DbSet<PublisherEntity> Publishers { get; set; }
        public DbSet<ReaderEntity> Readers { get; set; }
        public DbSet<BorrowEntity> Borrows { get; set; }

        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);

            modelBuilder.Entity<BookEntity>()
                .Property(book => book.Year)
                .IsRequired();

                entity.ToTable("books", t =>
                {
                    t.HasCheckConstraint(
                        name: "CK_Book_Year_ValidRange",
                        sql: "year >= 1600 AND year <= EXTRACT(YEAR FROM NOW())"
                    );
                });

                entity.HasOne(book => book.PublisherEntity) 
                    .WithMany(publisher => publisher.Books)
                    .HasForeignKey(book => book.PublisherEntityId) 
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(book => book.Genres)
                    .WithMany(genre => genre.Books);
            });

            modelBuilder.Entity<PublisherEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<GenreEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<ReaderEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<BorrowEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Reader).WithMany(r => r.Borrows).HasForeignKey(e=>e.ReaderId);
                entity.HasOne(e => e.Book).WithMany(b => b.Borrows).HasForeignKey(e => e.BookId);
            });
        }
    }
}
