using System.Threading.Tasks;

namespace AISearchApp.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResponseDto> ExecuteSearchAsync(SearchRequestDto request);
    }
}