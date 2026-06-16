using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AISearchApp.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISearchApp.Infrastructure.VectorDB
{
    public class QdrantClient
    {
        private readonly HttpClient _httpClient;
        private readonly QdrantOptions _options;
        private readonly ILogger<QdrantClient> _logger;

        public QdrantClient(
            HttpClient httpClient,
            IOptions<QdrantOptions> options,
            ILogger<QdrantClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(_options.Url);
        }

        public async Task CreateCollectionAsync()
        {
            _logger.LogInformation("Criando collection '{CollectionName}' no Qdrant", _options.CollectionName);

            var request = new
            {
                vectors = new
                {
                    size = _options.VectorSize,
                    distance = "Cosine"
                }
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"/collections/{_options.CollectionName}",
                request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Collection criada com sucesso");
            }
            else
            {
                _logger.LogWarning("Collection pode já existir ou erro ao criar");
            }
        }

        public async Task UpsertAsync(string id, float[] vector, object payload)
        {
            _logger.LogInformation("Inserindo documento {DocumentId} no Qdrant", id);

            var request = new
            {
                points = new[]
                {
                    new
                    {
                        id = id,
                        vector = vector,
                        payload = payload
                    }
                }
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"/collections/{_options.CollectionName}/points",
                request);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Documento inserido com sucesso");
        }

        public async Task<SearchResult> SearchAsync(float[] vector, int limit = 5)
        {
            _logger.LogInformation("Buscando {Limit} documentos similares no Qdrant", limit);

            var request = new
            {
                vector = vector,
                limit = limit,
                with_payload = true
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"/collections/{_options.CollectionName}/points/search",
                request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SearchResult>();

            _logger.LogInformation("Busca concluída. {Count} documentos encontrados", result?.Result?.Count ?? 0);

            return result;
        }

        public class SearchResult
        {
            [JsonPropertyName("result")]
            public List<SearchPoint> Result { get; set; }
        }

        public class SearchPoint
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("score")]
            public double Score { get; set; }

            [JsonPropertyName("payload")]
            public Dictionary<string, object> Payload { get; set; }
        }
    }
}