using BookStoreXML.Services;
using BookStoreXML.Models;
using Xunit;

namespace BookStoreXml.Tests;

public class BookXmlRepositoryTests
{
    private static string PrepareTempXml()
    {
        var dir = Path.Combine(Path.GetTempPath(), "bookstore-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        // This path points to the copied test data in the test project's output folder.
        var src = Path.Combine(AppContext.BaseDirectory, "TestData", "bookstore.sample.xml");
        var dst = Path.Combine(dir, "bookstore.xml");
        File.Copy(src, dst);

        return dst;
    }

    [Fact]
    public async Task Read_All_Parses_Multiple_Authors_And_Metadata()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });

        // ACT
        var all = await repo.GetAllAsync(default); 

        // ASSERT
        Assert.True(all.Count >= 3);

        var kick = all.First(b => b.Isbn == "9031234567897");
        Assert.Contains("Per Bothner", kick.Authors);
        Assert.Equal("web", kick.Category);
        Assert.Equal("en", kick.TitleLang);
    }

    [Fact]
    public async Task GetByIsbn_Finds_Expected_Book()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });

        // ACT
        var book = await repo.GetByIsbnAsync("9031234567897");

        // ASSERT
        Assert.NotNull(book);
        Assert.Equal("XQuery Kick Start", book!.Title);
        Assert.Contains("Per Bothner", book.Authors);
        Assert.Equal("web", book.Category);
        Assert.Equal("en", book.TitleLang);
    }

    [Fact]
    public async Task GetByIsbn_Returns_Null_When_Missing()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });

        // ACT
        var book = await repo.GetByIsbnAsync("0000000000000");

        // ASSERT
        Assert.Null(book);
    }

    [Fact]
    public async Task Add_Adds_New_Book_And_Persists()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });
        var dto = new Book(
            Isbn: "1112223334445",
            Title: "CLR via C#",
            Authors: new[] { "Jeffrey Richter" },
            Category: "programming",
            Year: 2012,
            Price: 59.99m,
            Cover: "hardcover",
            TitleLang: "en"
        );

        // ACT
        var added = await repo.AddAsync(dto);

        // ASSERT
        Assert.True(added);
        var fetched = await repo.GetByIsbnAsync(dto.Isbn);
        Assert.NotNull(fetched);
        Assert.Equal(59.99m, fetched!.Price);
    }

    [Fact]
    public async Task Add_ReturnsFalse_When_Isbn_Exists()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });

        
        var dto = new Book(
            Isbn: "9031234567897",
            Title: "Duplicate",
            Authors: new[] { "Someone" },
            Category: "web",
            Year: 2003,
            Price: 1.00m,
            Cover: null,
            TitleLang: "en"
        );

        // ACT
        var added = await repo.AddAsync(dto);

        // ASSERT
        Assert.False(added);
    }

    [Fact]
    public async Task Update_Existing_Book_Replaces_Content_And_Persists()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });
        var existingIsbn = "9043127323207"; 

        var original = await repo.GetByIsbnAsync(existingIsbn);
        Assert.NotNull(original); 

        var updated = original! with
        {
            Price = 41.50m,
            Category = "web-tech",
            Cover = "paperback",
            Title = original.Title + " (2nd Ed.)",
            Authors = new[] { "Erik T. Ray", "Co Author" }
        };

        // ACT
        var ok = await repo.UpdateAsync(existingIsbn, updated);

        // ASSERT
        Assert.True(ok);

        var reloaded = await repo.GetByIsbnAsync(existingIsbn);
        Assert.NotNull(reloaded);
        Assert.Equal(41.50m, reloaded!.Price);
        Assert.Equal("web-tech", reloaded.Category);
        Assert.Equal("paperback", reloaded.Cover);
        Assert.Equal(updated.Title, reloaded.Title);
        Assert.Contains("Co Author", reloaded.Authors);
    }

    [Fact]
    public async Task Update_ReturnsFalse_When_Book_Missing()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });

        var updated = new Book(
            Isbn: "1111111111111",
            Title: "Does Not Exist",
            Authors: new[] { "Nobody" },
            Category: "none",
            Year: 2000,
            Price: 10m,
            Cover: null,
            TitleLang: "en"
        );

        // ACT
        var ok = await repo.UpdateAsync(updated.Isbn, updated);

        // ASSERT
        Assert.False(ok);
    }

    [Fact]
    public async Task Update_Throws_When_Path_And_Payload_Isbn_Differ()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });

        var updated = new Book(
            Isbn: "1234567890123",
            Title: "Mismatch",
            Authors: new[] { "A" },
            Category: "x",
            Year: 2020,
            Price: 1m,
            Cover: null,
            TitleLang: "en"
        );

        // ACT + ASSERT
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repo.UpdateAsync("DIFFERENT_ISBN", updated));
    }

    [Fact]
    public async Task Delete_Removes_Book_And_Persists()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });
        var isbn = "9031234567897"; 
        Assert.NotNull(await repo.GetByIsbnAsync(isbn)); 

        // ACT
        var ok = await repo.DeleteAsync(isbn);

        // ASSERT
        Assert.True(ok);
        var after = await repo.GetByIsbnAsync(isbn);
        Assert.Null(after);
    }

    [Fact]
    public async Task Delete_ReturnsFalse_When_Missing()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });

        // ACT
        var ok = await repo.DeleteAsync("0000000000000");

        // ASSERT
        Assert.False(ok);
    }


    [Fact]
    public async Task AddMany_Adds_Only_New_Valid_And_Skips_Duplicates_And_Invalid()
    {
        // ARRANGE
        var path = PrepareTempXml();
        var repo = new BookXmlRepository(new XmlStoreOptions { Path = path });

        // payload:
        //  - NEW valid ? should add
        //  - DUPLICATE (already exists in sample) ? should be reported as duplicate
        //  - DUPLICATE inside payload (same ISBN as the new valid) ? reported as duplicate (payload)
        //  - INVALID: empty ISBN ? reported as invalid
        //  - INVALID: no authors ? reported as invalid
        var newIsbn = "1111111111111";
        var payload = new[]
        {
        new Book(
            Isbn: newIsbn,
            Title: "Ok A",
            Authors: new[] { "A" },
            Category: "x",
            Year: 2020,
            Price: 10m,
            Cover: null,
            TitleLang: "en"
        ),
        // duplicate already in sample xml
        new Book(
            Isbn: "9031234567897", // XQuery Kick Start
            Title: "Dup existing",
            Authors: new[] { "Someone" },
            Category: "web",
            Year: 2003,
            Price: 1m,
            Cover: null,
            TitleLang: "en"
        ),
        // duplicate inside payload (same as newIsbn)
        new Book(
            Isbn: newIsbn,
            Title: "Dup in payload",
            Authors: new[] { "A" },
            Category: "x",
            Year: 2020,
            Price: 10m,
            Cover: null,
            TitleLang: "en"
        ),
        // invalid: empty ISBN
        new Book(
            Isbn: "",
            Title: "Invalid no isbn",
            Authors: new[] { "Z" },
            Category: "x",
            Year: 2020,
            Price: 10m,
            Cover: null,
            TitleLang: "en"
        ),
        // invalid: no authors
        new Book(
            Isbn: "2222222222222",
            Title: "Invalid no authors",
            Authors: new List<string>(),  // empty
            Category: "x",
            Year: 2020,
            Price: 10m,
            Cover: null,
            TitleLang: "en"
        )
    };

        // ACT
        var res = await repo.AddManyAsync(payload);

        // ASSERT - summary
        Assert.Equal(1, res.AddedCount); // only the first valid, unique one
        Assert.Contains("9031234567897", res.DuplicateIsbns); // existed in sample
        Assert.Contains(newIsbn, res.DuplicateIsbns);          // duplicate inside payload
        Assert.Contains("", res.InvalidIsbns);                 // empty ISBN
        Assert.Contains("2222222222222", res.InvalidIsbns);    // no authors

        // ASSERT - persistence
        var added = await repo.GetByIsbnAsync(newIsbn);
        Assert.NotNull(added);
        Assert.Equal("Ok A", added!.Title);

        // ASSERT - duplicates not added again
        var existing = await repo.GetByIsbnAsync("9031234567897");
        Assert.NotNull(existing);
        Assert.Equal("XQuery Kick Start", existing!.Title);
    }



}
