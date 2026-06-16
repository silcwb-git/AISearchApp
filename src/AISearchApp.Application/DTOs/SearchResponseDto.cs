using System.Collections.Generic;
namespace AISearchApp.Dtos
{
    public class SearchResponseDto
    {
        public string Answer { get; set; }
        public List<string> Sources { get; set; }
        public double Confidence { get; set; }
        public long ProcessingTimeMs { get; set; }

        public SearchResponseDto()
        {
            Sources = new List<string>();
        }
    }
}