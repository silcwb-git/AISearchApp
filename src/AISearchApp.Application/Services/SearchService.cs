using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AISearchApp.Application.DTOs;
using AISearchApp.Application.Interfaces;
using AISearchApp.Domain.Interfaces;

namespace AISearchApp.Application.Services
{
    /// <summary>
    /// Serviço de busca que implementa o padrão RAG (Retrieval-Augmented Generation)
    /// Orquestra: Embedding → Busca Vetorial → Enriquecimento com IA → Resposta
    /// </summary>
    public class SearchService : ISearchService
    {
        private readonly IAIService _aiService;
        private readonly IVectorDbRepository _vectorDbRepository;
        private readonly ILogger<SearchService> _logger;

        public SearchService(
            IAIService aiService,
            IVectorDbRepository vectorDbRepository,
            ILogger<SearchService> logger)
        {
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
            _vectorDbRepository = vectorDbRepository ?? throw new ArgumentNullException(nameof(vectorDbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executa o fluxo completo de busca RAG
        /// </summary>
        public async Task<SearchResponseDto> ExecuteSearchAsync(SearchRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                throw new ArgumentException("A consulta não pode ser nula ou vazia.", nameof(request));
            }

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Iniciando fluxo RAG para a consulta: {Query}", request.Query);

            try
            {
                // ETAPA 1: Gerar embedding da query
                _logger.LogInformation("Etapa 1/4: Gerando embedding da consulta...");
                var queryEmbedding = await _aiService.GenerateEmbeddingAsync(request.Query);

                if (queryEmbedding == null || queryEmbedding.Length == 0)
                {
                    throw new InvalidOperationException("Falha ao gerar embedding para a consulta.");
                }

                _logger.LogInformation("✓ Embedding gerado com sucesso. Dimensão: {Dimension}", queryEmbedding.Length);

                // ETAPA 2: Buscar documentos similares
                _logger.LogInformation("Etapa 2/4: Buscando documentos relevantes no banco vetorial...");
                var topK = request.TopResults > 0 ? request.TopResults : 5;
                var searchResults = await _vectorDbRepository.SearchAsync(queryEmbedding, topK);

                if (searchResults == null || !searchResults.Any())
                {
                    _logger.LogWarning("Nenhum documento relevante encontrado para a consulta: {Query}", request.Query);
                    return new SearchResponseDto
                    {
                        Query = request.Query,
                        Answer = "Não foram encontrados documentos relevantes para responder à consulta.",
                        Sources = new List<string>(),
                        ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                    };
                }

                _logger.LogInformation("✓ {Count} documentos encontrados", searchResults.Count());

                // ETAPA 3: Construir contexto
                _logger.LogInformation("Etapa 3/4: Construindo contexto com os documentos recuperados...");
                var context = string.Join("\n\n---\n\n", searchResults.Select(r => r.Content));
                _logger.LogInformation("✓ Contexto construído com {Length} caracteres", context.Length);

                // ETAPA 4: Gerar resposta com IA
                _logger.LogInformation("Etapa 4/4: Gerando resposta com IA generativa...");
                var answer = await _aiService.GenerateAnswerAsync(request.Query, context);
                _logger.LogInformation("✓ Resposta gerada com sucesso");

                stopwatch.Stop();

                // Retornar resultado
                var response = new SearchResponseDto
                {
                    Query = request.Query,
                    Answer = answer,
                    Sources = searchResults.Select(r => r.Metadata).ToList(),
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                };

                _logger.LogInformation("✓ Fluxo RAG concluído com sucesso em {ElapsedMs}ms", response.ProcessingTimeMs);
                return response;
            }
            catch (ArgumentException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex, "Validação falhou no fluxo RAG");
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erro ao executar fluxo RAG para a consulta: {Query}. Tempo: {ElapsedMs}ms", 
                    request.Query, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}