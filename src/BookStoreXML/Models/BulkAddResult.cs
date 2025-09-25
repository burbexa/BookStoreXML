namespace BookStoreXML.Models
{
    public sealed record BulkAddResult(
       int AddedCount,
       IReadOnlyList<string> DuplicateIsbns,
       IReadOnlyList<string> InvalidIsbns
   );
}
