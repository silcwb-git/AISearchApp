using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AISearchApp.Domain.Entities;
using AISearchApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AISearchApp.Infrastructure.VectorDB
{
    public class QdrantVectorDbRepository : IVectorDbRepository
    {
        private readonly QdrantClient _client;
        private readonly ILogger<QdrantVectorDbRepository> _logger;

        public QdrantVectorDbRepository(
            QdrantClient client,
            ILogger<QdrantVectorDbRepository> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task SaveAsync(VectorDocument document)
        {
            try
            {
                _logger.LogInformation("Salvando documento {DocumentId} no Qdrant", document.Id);

                var payload = new
                {
                    title = document.Title,
                    content = document.Content,
                    metadata = document.Metadata,
                    createdAt = document.CreatedAt
                };

                await _client.UpsertAsync(
                    document.Id.ToString(),
                    document.Embedding,
                    payload);

                _logger.LogInformation("Documento salvo com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar documento no Qdrant");
                throw;
            }
        }

        public async Task<IEnumerable<VectorDocument>> SearchAsync(float[] embedding, int limit)
        {
            try
            {
                _logger.LogInformation("Buscando {Limit} documentos similares", limit);

                var result = await _client.SearchAsync(embedding, limit);

                if (result?.Result == null || result.Result.Count == 0)
                {
                    _logger.LogWarning("Nenhum documento encontrado");
                    return Enumerable.Empty<VectorDocument>();
                }

                var documents = result.Result
                    .Select(point => new VectorDocument
                    {
                        Title = point.Payload?["title"]?.ToString() ?? "Sem título",
                        Content = point.Payload?["content"]?.ToString() ?? string.Empty,
                        Metadata = point.Payload?["metadata"]?.ToString() ?? string.Empty,
                        Embedding = embedding
                    })
                    .ToList();

                _logger.LogInformation("Busca concluída. {Count} documentos retornados", documents.Count);

                return documents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar documentos no Qdrant");
                throw;
            }
        }
    }
}