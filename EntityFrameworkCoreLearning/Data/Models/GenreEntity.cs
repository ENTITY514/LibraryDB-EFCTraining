namespace EntityFrameworkCoreLearning.Data.Models
{
    public class GenreEntity
    {
        public Guid Id { get; set; }
        public ICollection<BookEntity> Books { get; set; } = [];
    }
}
