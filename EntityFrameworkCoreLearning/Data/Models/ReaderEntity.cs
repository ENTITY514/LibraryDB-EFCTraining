namespace EntityFrameworkCoreLearning.Data.Models
{
    public class ReaderEntity
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public ICollection<BorrowEntity> Borrows { get; set; } = [];
    }
}
