namespace AISearchApp.Configuration
{
    public class QdrantOptions
    {
        public string Url { get; set; } = string.Empty;

        public string CollectionName { get; set; } = "documents";

        public int VectorSize { get; set; } = 1536;
    }
}