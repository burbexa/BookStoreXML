using BookStoreXML.Models;

namespace BookStoreXML.Services.Reports
{
    public interface IHtmlReportService
    {
        Task<string> RenderBooksAsync(BooksReportViewModel model, CancellationToken ct);
    }
}
