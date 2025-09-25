using BookStoreXML.Models;

namespace BookStoreXML.Services
{
    public interface IBookRepository
    {
        Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default);
        Task<Book?> GetByIsbnAsync(string isbn, CancellationToken ct = default);
        Task<bool> AddAsync(Book book, CancellationToken ct = default);             
        Task<bool> UpdateAsync(string isbn, Book updated, CancellationToken ct = default); 
        Task<bool> DeleteAsync(string isbn, CancellationToken ct = default);
        Task<BulkAddResult> AddManyAsync(IEnumerable<Book> books, CancellationToken ct = default);
    }


}
