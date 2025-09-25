using BookStoreXML.Models;
using RazorLight;

namespace BookStoreXML.Services.Reports
{
    public sealed class RazorReportService : IHtmlReportService
    {
        private readonly RazorLightEngine _engine;
        public RazorReportService(IWebHostEnvironment env)
        {
            var templatesDir = Path.Combine(env.ContentRootPath, "Services", "Reports","Templates");
            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(templatesDir)
                .UseMemoryCachingProvider()
                .Build();
        }

        public Task<string> RenderBooksAsync(BooksReportViewModel model, CancellationToken ct) =>
            _engine.CompileRenderAsync("BooksReport.cshtml", model);
    }
}
