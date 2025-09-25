using BookStoreXML.Models;
using BookStoreXML.Services;

namespace BookStoreXML.Endpoints.Books;

public sealed class BooksApi : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/books").WithTags("Books");

        group.MapGet("/", async (IBookRepository repo, CancellationToken ct) =>
        {
            var items = await repo.GetAllAsync(ct);
            return Results.Ok(items.Select(BookDto.FromDomain));
        }).WithSummary("Get all books");

        group.MapGet("/{isbn}", async (string isbn, IBookRepository repo, CancellationToken ct) =>
        {
            var b = await repo.GetByIsbnAsync(isbn, ct);
            return b is null ? Results.NotFound() : Results.Ok(BookDto.FromDomain(b));
        }).WithSummary("Get book by isbn");

        group.MapPost("/", async (BookDto dto, IBookRepository repo, CancellationToken ct) =>
        {
            var ok = await repo.AddAsync(dto.ToDomain(), ct);
            return ok
                ? Results.Created($"/books/{dto.Isbn}", dto)
                : Results.Conflict(new { message = "Book with this ISBN already exists." });
        })
             .WithSummary("Single book add");

        // POST /books/batch  (bulk add)
        group.MapPost("/batch", async (List<BookDto> dtos, IBookRepository repo, CancellationToken ct) =>
        {
            if (dtos is null || dtos.Count == 0)
                return Results.BadRequest(new { message = "Provide a non-empty array of books." });

            var result = await repo.AddManyAsync(dtos.Select(d => d.ToDomain()), ct);

            var resp = new BulkAddResult(
                AddedCount: result.AddedCount,
                DuplicateIsbns: result.DuplicateIsbns,
                InvalidIsbns: result.InvalidIsbns
            );

            return Results.Ok(resp);
        })
        .WithSummary("Bulk add books")
        .WithDescription("Accepts a JSON array of books. Adds unique, valid items; returns a summary.");




        group.MapPut("/{isbn}", async (string isbn, BookDto dto, IBookRepository repo, CancellationToken ct) =>
        {
            if (!string.Equals(isbn, dto.Isbn, StringComparison.Ordinal))
                return Results.BadRequest(new { message = "Path ISBN must match payload ISBN." });

            var ok = await repo.UpdateAsync(isbn, dto.ToDomain(), ct);
            return ok ? Results.NoContent() : Results.NotFound();
        }).WithSummary("Update a book by isbn");

        group.MapDelete("/{isbn}", async (string isbn, IBookRepository repo, CancellationToken ct) =>
        {
            var ok = await repo.DeleteAsync(isbn, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        }).WithSummary("Delete a book by isbn");
    }
}
