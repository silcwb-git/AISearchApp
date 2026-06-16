using System.Threading.Tasks;
using AISearchApp.Application.DTOs;
using AISearchApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AISearchApp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            ISearchService searchService,
            ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Realiza uma busca semântica com IA generativa
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SearchResponseDto>> Search([FromBody] SearchRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Query))
            {
                _logger.LogWarning("Requisição de busca inválida");
                return BadRequest(new { error = "Query não pode ser vazia" });
            }

            try
            {
                _logger.LogInformation("Processando busca: {Query}", request.Query);

                var response = await _searchService.ExecuteSearchAsync(request);

                return Ok(response);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar busca");
                return StatusCode(500, new { error = "Erro ao processar busca", details = ex.Message });
            }
        }

        /// <summary>
        /// Health check da API
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy" });
        }
    }
}