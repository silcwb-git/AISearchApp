using System;

namespace AISearchApp.Domain.Entities
{
    public class VectorDocument : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public float[] Embedding { get; set; }
        public string Metadata { get; set; }

        public VectorDocument()
        {
        }

        public VectorDocument(string title, string content, float[] embedding, string metadata)
        {
            Title = title;
            Content = content;
            Embedding = embedding;
            Metadata = metadata;
        }
    }
}