# 📚 BookStore XML Management

A modern .NET 8 Web API for managing a bookstore catalog stored in XML format. Features comprehensive CRUD operations, bulk operations, and beautiful HTML reports with optional branding.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()

## ✨ Features

- 📚 **Complete CRUD Operations** - Create, read, update, and delete books with ISBN as unique identifier
- 📦 **Bulk Operations** - Add multiple books at once with validation and duplicate detection
- 💾 **XML Storage** - Atomic writes with async I/O and pretty-printed output
- 🔒 **Thread Safety** - Single-writer async file lock to prevent data corruption
- 🧪 **Comprehensive Testing** - xUnit tests with isolated test data
- 🖨️ **HTML Reports** - Beautiful downloadable reports with optional INSURTIX branding
- 🚀 **Modern API** - Built with .NET 8 Minimal APIs and Swagger documentation

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Your favorite IDE (Visual Studio, VS Code, Rider, etc.)

### Installation & Running

```bash
# Clone the repository
git clone https://github.com/burbexa/BookStoreXML.git
cd "BookStore XML management"

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the application
dotnet run --project src/BookStoreXML/BookStoreXML.csproj
```

The API will be available at:
- **API**: `https://localhost:7xxx` or `http://localhost:5xxx`
- **Swagger UI**: `https://localhost:7xxx/swagger` (Development only)

## ⚙️ Configuration

The XML store path is resolved in the following order:

1. **Environment Variable**: `XML_STORE_PATH`
2. **Configuration File**: `appsettings.json` → `XmlStoreOptions:Path`

### Example Configuration

```json
{
  "XmlStoreOptions": {
    "Path": "data/bookstore.xml"
  }
}
```

> 💡 **Note**: If the XML file doesn't exist, it will be created automatically on first use.

## 📊 Data Format

### XML Structure

```xml
<bookstore>
  <book category="programming" cover="hardcover">
    <isbn>9780132350884</isbn>
    <title lang="en">Clean Code</title>
    <author>Robert C. Martin</author>
    <year>2008</year>
    <price>42.50</price>
  </book>
</bookstore>
```

### API Data Transfer Object (DTO)

```json
{
  "isbn": "9780132350884",
  "title": "Clean Code",
  "authors": ["Robert C. Martin"],
  "category": "programming",
  "year": 2008,
  "price": 42.50,
  "cover": "hardcover",
  "titleLang": "en"
}
```

## 🔌 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/books` | List all books |
| `GET` | `/books/{isbn}` | Get a specific book by ISBN |
| `POST` | `/books` | Add a single book |
| `POST` | `/books/batch` | Add multiple books (bulk operation) |
| `PUT` | `/books/{isbn}` | Update a book (ISBN must match) |
| `DELETE` | `/books/{isbn}` | Delete a book |
| `GET` | `/reports/books` | Download HTML report |

### Example: Bulk Add Books

**Request** (`POST /books/batch`):
```json
[
  {
    "isbn": "9780132350884",
    "title": "Clean Code",
    "authors": ["Robert C. Martin"],
    "category": "programming",
    "year": 2008,
    "price": 42.50,
    "cover": "hardcover",
    "titleLang": "en"
  },
  {
    "isbn": "9031234567897",
    "title": "XQuery Kick Start",
    "authors": ["James McGovern", "Per Bothner"],
    "category": "web",
    "year": 2003,
    "price": 49.99,
    "cover": null,
    "titleLang": "en"
  }
]
```

**Response**:
```json
{
  "added": 1,
  "duplicates": ["9031234567897"],
  "invalid": []
}
```

## 📋 Validation Rules

- **ISBN**: Required, unique identifier
- **Title**: Required, non-empty string
- **Authors**: Required, at least one author
- **Price**: Must be >= 0
- **Year**: Must be between 0 and 3000
- **Category**: Required, non-empty string

## 🖨️ HTML Reports

### Features

- **Endpoint**: `GET /reports/books`
- **Format**: Standalone HTML file
- **Styling**: Modern, responsive design
- **Template**: Razor-based templating with RazorLight

### Usage

```bash
# Download the report
curl -o books-report.html "https://localhost:7xxx/reports/books"
```

## 🧪 Testing

Run the test suite:

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests for specific project
dotnet test tests/BookStoreXml.Tests/BookStoreXml.Tests.csproj
```

### Test Features

- **Isolation**: Uses temporary copies of sample XML data
- **Coverage**: Tests all CRUD operations and edge cases
- **Validation**: Tests input validation and error handling
- **Bulk Operations**: Tests bulk add with various scenarios

## 🏗️ Architecture

### Project Structure

```
src/BookStoreXML/
├── Endpoints/           # API endpoint definitions
├── Models/             # Data models and DTOs
├── Services/           # Business logic and repositories
│   └── Reports/        # HTML report generation
├── Utils/              # Utility classes (file locking)
└── data/               # XML data storage
```

### Key Components

- **`BookXmlRepository`**: XML-based data access layer
- **`AsyncFileLock`**: Thread-safe file operations
- **`RazorReportService`**: HTML report generation
- **`BooksApi`**: RESTful API endpoints

## 🔧 Implementation Details

### XML I/O

- **Loading**: Uses `LoadOptions.None` to avoid preserving old whitespace
- **Saving**: Pretty-printed output with proper indentation
- **Atomicity**: Uses temporary files and `File.Replace` for safe updates

### Thread Safety

- **Single Writer**: Async file lock prevents concurrent write operations
- **Read Operations**: Multiple concurrent reads are allowed
- **Data Integrity**: Ensures XML file consistency during updates

### Bulk Operations

- **Validation**: All inputs validated before processing
- **Deduplication**: Removes duplicates within payload and against existing data
- **Efficiency**: Single-pass processing for optimal performance

## 🛠️ Tech Stack

- **.NET 8** - Latest LTS version with Minimal APIs
- **LINQ to XML** - XML data manipulation
- **RazorLight** - Server-side Razor templating
- **xUnit** - Unit testing framework
- **Swagger/OpenAPI** - API documentation
- **Microsoft.Extensions.DependencyInjection** - Dependency injection

## 📝 Development

### Building from Source

```bash
# Restore packages
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Run tests
dotnet test

# Publish for deployment
dotnet publish --configuration Release --output ./publish
```


**Made with ❤️ using .NET 8**
