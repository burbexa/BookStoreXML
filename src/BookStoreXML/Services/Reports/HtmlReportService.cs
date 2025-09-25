//using BookStoreXML.Models;
//using System.Text;

//namespace BookStoreXML.Services.Reports
//{
//    public sealed class HtmlReportService : IHtmlReportService
//    {
//        public Task<string> RenderBooksAsync(BooksReportViewModel model, CancellationToken ct)
//        {
//            throw new NotImplementedException();
//        }

//        public string RenderBooksTable(IEnumerable<Book> books)
//        {
//            var sb = new StringBuilder();
//            sb.Append("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\">");
//            sb.Append("<title>Books Report</title>");
//            sb.Append("<style>body{font-family:system-ui,Segoe UI,Arial}table{border-collapse:collapse;width:100%}");
//            sb.Append("th,td{border:1px solid #ddd;padding:8px}th{background:#f3f4f6;text-align:left}");
//            sb.Append("tr:nth-child(even){background:#fafafa}</style></head><body>");
//            sb.Append("<h1>Bookstore — Books</h1><table><thead><tr>");
//            sb.Append("<th>Title</th><th>Author(s)</th><th>Category</th><th>Year</th><th>Price</th><th>ISBN</th>");
//            sb.Append("</tr></thead><tbody>");
//            foreach (var b in books)
//            {
//                var authors = string.Join(", ", b.Authors);
//                sb.Append("<tr>");
//                sb.Append($"<td>{Html(b.Title)}</td>");
//                sb.Append($"<td>{Html(authors)}</td>");
//                sb.Append($"<td>{Html(b.Category)}</td>");
//                sb.Append($"<td>{b.Year}</td>");
//                sb.Append($"<td>{b.Price:0.##}</td>");
//                sb.Append($"<td>{Html(b.Isbn)}</td>");
//                sb.Append("</tr>");
//            }
//            sb.Append("</tbody></table></body></html>");
//            return sb.ToString();

//            static string Html(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
//        }
//    }
//}
