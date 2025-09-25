namespace BookStoreXML.Models
{
    public sealed record Book(
      string Isbn,
      string Title,
      IReadOnlyList<string> Authors,
      string Category,
      int Year,
      decimal Price,
      string? Cover = null,
      string? TitleLang = null
  );

    public sealed record BookDto(
    string Isbn,
    string Title,
    List<string> Authors,
    string Category,
    int Year,
    decimal Price,
    string? Cover = null,
    string? TitleLang = null
)
    {
        public Book ToDomain() => new(Isbn.Trim(), Title.Trim(), Authors, Category.Trim(), Year, Price, Cover?.Trim(), TitleLang?.Trim());
        public static BookDto FromDomain(Book b) => new(b.Isbn, b.Title, b.Authors.ToList(), b.Category, b.Year, b.Price, b.Cover, b.TitleLang);
    }
}
