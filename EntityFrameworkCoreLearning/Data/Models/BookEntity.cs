namespace EntityFrameworkCoreLearning.Data.Models
{
    public class BookEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;

        public Guid PublisherEntityId { get; set; }
        public PublisherEntity? PublisherEntity { get; set; }

        public ICollection<GenreEntity> Genres { get; set; } = [];
    }
}
