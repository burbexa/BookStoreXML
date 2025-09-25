using BookStoreXML.Models;
using BookStoreXML.Utils;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BookStoreXML.Services
{
    public sealed class BookXmlRepository : IBookRepository
    {
        private const int BufferSize = 4096;
        private const int MaxYear = 3000;
        private const int MinYear = 0;
        
        private readonly string _filePath;
        private readonly AsyncFileLock _lock = new();

        public BookXmlRepository(XmlStoreOptions opts)
        {
            _filePath = Environment.GetEnvironmentVariable("XML_STORE_PATH")?.Trim()
                     ?? opts.Path;
            EnsureStoreExists(); 
        }

        
        public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default)
        {
            var doc = await LoadAsync(ct);
            var root = doc.Root ?? new XElement("bookstore");
            return root.Elements("book").Select(ParseBook).ToList();
        }

       

        public async Task<Book?> GetByIsbnAsync(string isbn, CancellationToken ct = default)
        {
            var doc = await LoadAsync(ct);
            var root = doc.Root ?? new XElement("bookstore");

            var el = root.Elements("book")
                         .FirstOrDefault(b =>
                             string.Equals((string?)b.Element("isbn"), isbn, StringComparison.Ordinal));

            return el is null ? null : ParseBook(el);
        }

        public async Task<bool> AddAsync(Book book, CancellationToken ct = default)
        {
            Validate(book);
            using (await _lock.AcquireWriteAsync(ct))
            {
                var doc = await LoadAsync(ct);
                var root = doc.Root!;

                var map = CreateBookIndex(root);

                if (map.ContainsKey(book.Isbn)) return false;

                root.Add(SerializeBook(book));
                await SaveAsync(doc, ct);
                return true;
            }
        }

        public async Task<BulkAddResult> AddManyAsync(IEnumerable<Book> books, CancellationToken ct = default)
        {
            var incoming = (books ?? Enumerable.Empty<Book>()).ToList();

            // 1) Validate each payload item
            var invalid = new List<string>();
            foreach (var b in incoming)
            {
                try { Validate(b); }
                catch (Exception ex) 
                { 
                    invalid.Add(b?.Isbn ?? string.Empty); 
                    // Could log the specific validation error here
                }
            }

            // Keep only valid items
            var valid = incoming.Where(b => b != null && !invalid.Contains(b.Isbn, StringComparer.Ordinal)).ToList();

            // 2) De-duplicate inside the payload (ISBN)
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var payloadDupes = new List<string>();
            var deduped = new List<Book>(valid.Count);

            foreach (var b in valid)
            {
                if (seen.Add(b.Isbn))
                    deduped.Add(b);
                else
                    payloadDupes.Add(b.Isbn);
            }

            // 3) Single-writer block: add only non-existing
            using (await _lock.AcquireWriteAsync(ct))
            {
                var doc = await LoadAsync(ct);
                var root = doc.Root!;

                var existing = CreateBookIndex(root);

                var duplicates = new List<string>();
                var added = 0;

                foreach (var b in deduped)
                {
                    if (existing.ContainsKey(b.Isbn))
                    {
                        duplicates.Add(b.Isbn);
                        continue;
                    }

                    var el = SerializeBook(b);
                    root.Add(el);
                    existing[b.Isbn] = el;   // keep index consistent
                    added++;
                }

                if (added > 0)
                    await SaveAsync(doc, ct);

                // Merge external duplicates with payload duplicates
                duplicates.AddRange(payloadDupes);

                return new BulkAddResult(added, duplicates, invalid);
            }
        }

        public async Task<bool> UpdateAsync(string isbn, Book updated, CancellationToken ct = default)
        {
            if (!string.Equals(isbn, updated.Isbn, StringComparison.Ordinal))
                throw new ArgumentException("Path ISBN must match payload ISBN.", nameof(isbn));

            Validate(updated);

            using (await _lock.AcquireWriteAsync(ct))
            {
                var doc = await LoadAsync(ct);
                var root = doc.Root!;

                var map = CreateBookIndex(root);

                if (!map.TryGetValue(isbn, out var existing))
                    return false;

                existing!.ReplaceWith(SerializeBook(updated));
                await SaveAsync(doc, ct);
                return true;
            }
        }

        public async Task<bool> DeleteAsync(string isbn, CancellationToken ct = default)
        {
            using (await _lock.AcquireWriteAsync(ct))
            {
                var doc = await LoadAsync(ct);
                var root = doc.Root!;

                var map = CreateBookIndex(root);

                if (!map.TryGetValue(isbn, out var existing))
                    return false;

                existing!.Remove();
                await SaveAsync(doc, ct);
                return true;
            }
        }

        // --- helpers ---

        private static Dictionary<string, XElement> CreateBookIndex(XElement root) =>
            root.Elements("book")
                .ToDictionary(
                    b => (string?)b.Element("isbn") ?? string.Empty,
                    b => b,
                    StringComparer.Ordinal);

        private static void Validate(Book b)
        {
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (string.IsNullOrWhiteSpace(b.Isbn)) throw new ArgumentException("ISBN is required");
            if (string.IsNullOrWhiteSpace(b.Title)) throw new ArgumentException("Title is required");
            if (b.Authors is null || b.Authors.Count == 0) throw new ArgumentException("At least one author is required");
            if (b.Year < MinYear || b.Year > MaxYear) throw new ArgumentOutOfRangeException(nameof(b.Year));
            if (b.Price < 0) throw new ArgumentOutOfRangeException(nameof(b.Price));
        }


        private static Book ParseBook(XElement b)
        {
            var category = (string?)b.Attribute("category") ?? string.Empty;
            var cover = (string?)b.Attribute("cover");
            var isbn = (string?)b.Element("isbn") ?? string.Empty;

            var titleEl = b.Element("title");
            var title = (string?)titleEl ?? string.Empty;
            var titleLang = (string?)titleEl?.Attribute("lang");

            var authors = b.Elements("author")
                .Select(a => (string?)a ?? string.Empty)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var year = (int?)b.Element("year") ?? 0;
            var price = (decimal?)b.Element("price") ?? 0m;

            return new Book(isbn, title, authors, category, year, price, cover, titleLang);
        }

        private static XElement SerializeBook(Book b)
        {
            var titleEl = new XElement("title", b.Title);
            if (!string.IsNullOrWhiteSpace(b.TitleLang))
                titleEl.SetAttributeValue("lang", b.TitleLang);

            var el = new XElement("book",
                new XElement("isbn", b.Isbn),
                titleEl,
                b.Authors.Select(a => new XElement("author", a)),
                new XElement("year", b.Year),
                new XElement("price", b.Price)
            );

            if (!string.IsNullOrWhiteSpace(b.Category))
                el.SetAttributeValue("category", b.Category);
            if (!string.IsNullOrWhiteSpace(b.Cover))
                el.SetAttributeValue("cover", b.Cover);

            return el;
        }

        private void EnsureStoreExists()
        {
            // Normalize & ensure directory exists
            var full = Path.GetFullPath(_filePath);
            var dir = Path.GetDirectoryName(full);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Create an empty store on first run
            if (!File.Exists(full))
            {
                var doc = new XDocument(new XElement("bookstore"));
                doc.Save(full);
            }
        }

        private async Task<XDocument> LoadAsync(CancellationToken ct)
        {
            using var fs = new FileStream(
                _filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: BufferSize, useAsync: true);

            return await XDocument.LoadAsync(fs, LoadOptions.None, ct);
        }


        private static readonly XmlWriterSettings PrettyXmlSettings = new()
        {
            Indent = true,
            IndentChars = "  ",           
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = false,
            Async = true
        };


        private async Task SaveAsync(XDocument doc, CancellationToken ct)
        {
            var tmp = _filePath + ".tmp";

            await using (var fs = new FileStream(
                tmp, FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: BufferSize, useAsync: true))
            await using (var xw = XmlWriter.Create(fs, PrettyXmlSettings))
            {
                
                doc.Declaration ??= new XDeclaration("1.0", "utf-8", null);

                await doc.SaveAsync(xw, ct);   
                await xw.FlushAsync();
            }

            try
            {
                File.Replace(tmp, _filePath, destinationBackupFileName: null);
            }
            catch (IOException ex)
            {
                
                File.Copy(tmp, _filePath, overwrite: true);
                File.Delete(tmp);
            }
        }

    }
}
