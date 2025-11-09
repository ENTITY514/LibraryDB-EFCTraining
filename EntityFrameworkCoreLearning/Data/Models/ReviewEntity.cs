namespace EntityFrameworkCoreLearning.Data.Models
{
    public class ReviewEntity
    {
        public Guid Id { get; set; }
        public float Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        public Guid BookId { get; set; }
        public BookEntity? Book { get; set; }
    }
}
