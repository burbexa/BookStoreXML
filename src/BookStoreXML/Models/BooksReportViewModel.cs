namespace BookStoreXML.Models
{
    public sealed class BooksReportViewModel
    {
        public required IEnumerable<Book> Books { get; init; }
        public string? LogoDataUri { get; init; }   // base64 logo for offline
        public DateTime GeneratedAt { get; init; } = DateTime.Now;
    }
}
