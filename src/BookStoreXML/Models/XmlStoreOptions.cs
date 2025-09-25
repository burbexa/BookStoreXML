namespace BookStoreXML.Models
{
    public sealed class XmlStoreOptions
    {
        public const string SectionName = "XmlStore";
        public string Path { get; set; } = "./data/bookstore.xml";
    }
}
