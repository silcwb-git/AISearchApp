namespace AISearchApp.Dtos
{
    public class SearchRequestDto
    {
        public string Query { get; set; } = string.Empty;
        public int TopResults { get; set; } = 5;
    }
}