namespace EntityFrameworkCoreLearning.Data.Models
{
    public class GenreEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<BookEntity> Books { get; set; } = [];
    }
}
