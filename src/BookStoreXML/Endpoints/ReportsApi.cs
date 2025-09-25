using BookStoreXML.Models;
using BookStoreXML.Services;
using BookStoreXML.Services.Reports;
using System.Text;

namespace BookStoreXML.Endpoints
{
    public sealed class ReportsApi : IEndpoint
    {
        public void MapEndpoints(IEndpointRouteBuilder app)
        {

            app.MapGet("/reports/booksRazor", async (
                                                IBookRepository repo,
                                                IHtmlReportService renderer,
                                                IWebHostEnvironment env,
                                                CancellationToken ct) =>
                                                 {
                                                     var books = await repo.GetAllAsync(ct);

                                                     // embed logo as base64 so the file works offline
                                                     string? dataUri = null;
                                                     var logoPath = Path.Combine(env.ContentRootPath, "Services", "Reports", "Assets","insurtix.png");
                                                     if (File.Exists(logoPath))
                                                     {
                                                         var bytes = await File.ReadAllBytesAsync(logoPath, ct);
                                                         dataUri = $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
                                                     }

                                                     var html = await renderer.RenderBooksAsync(new BooksReportViewModel
                                                     {
                                                         Books = books,
                                                         LogoDataUri = dataUri
                                                     }, ct);

                                                     var bytesOut = Encoding.UTF8.GetBytes(html);
                                                     return Results.File(bytesOut, "text/html; charset=utf-8", "books-report.html");
                                                 })
                .WithTags("Reports");
        }
    }
}
