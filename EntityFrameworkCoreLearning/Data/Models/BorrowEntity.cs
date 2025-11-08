namespace EntityFrameworkCoreLearning.Data.Models
{
    public class BorrowEntity
    {
        public Guid Id { get; set; }
        public DateOnly BorrowDate { get; set; }
        public DateOnly? ReturnDate { get; set; }

        public ReaderEntity? Reader { get; set; }
        public required Guid ReaderId { get; set; }
        public BookEntity? Book { get; set; }
        public required Guid BookId { get; set; }
    }
}
