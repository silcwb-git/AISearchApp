using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AISearchApp.Domain.Interfaces;
using AISearchApp.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace AISearchApp.Infrastructure.AI
{
    public class OpenAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly AIOptions _options;
        private readonly ILogger<OpenAIService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public OpenAIService(
            HttpClient httpClient,
            IOptions<AIOptions> options,
            ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            // Configurar retry policy: 3 tentativas com backoff exponencial
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            $"Retry {retryCount} após {timespan.TotalSeconds}s. Erro: {outcome.Exception?.Message}");
                    });

            // Configurar header de autenticação
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Gerando embedding para texto de {Length} caracteres", text.Length);

                var request = new
                {
                    input = text,
                    model = _options.EmbeddingModel
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "https://api.openai.com/v1/embeddings",
                    request);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();

                if (result?.Data == null || result.Data.Count == 0)
                {
                    throw new InvalidOperationException("Nenhum embedding foi retornado pela API");
                }

                _logger.LogInformation("Embedding gerado com sucesso. Dimensão: {Dimension}", result.Data[0].Embedding.Length);

                return result.Data[0].Embedding;
            });
        }

        public async Task<string> GenerateAnswerAsync(string context, string question)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Gerando resposta para pergunta: {Question}", question);

                var systemPrompt = "Você é um assistente de IA que responde perguntas com base APENAS no contexto fornecido. " +
                    "Se a resposta não estiver no contexto, diga que não sabe. Seja conciso e direto.";

                var userPrompt = $"Contexto:\n{context}\n\nPergunta: {question}";

                var request = new
                {
                    model = _options.ChatModel,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.7,
                    max_tokens = 500
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "https://api.openai.com/v1/chat/completions",
                    request);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ChatResponse>();

                if (result?.Choices == null || result.Choices.Count == 0)
                {
                    throw new InvalidOperationException("Nenhuma resposta foi retornada pela API");
                }

                var answer = result.Choices[0].Message.Content;

                _logger.LogInformation("Resposta gerada com sucesso");

                return answer;
            });
        }

        // Classes para deserialização JSON
        private class EmbeddingResponse
        {
            [JsonPropertyName("data")]
            public List<EmbeddingData> Data { get; set; }

            public class EmbeddingData
            {
                [JsonPropertyName("embedding")]
                public float[] Embedding { get; set; }
            }
        }

        private class ChatResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; }

            public class Choice
            {
                [JsonPropertyName("message")]
                public Message Message { get; set; }
            }

            public class Message
            {
                [JsonPropertyName("content")]
                public string Content { get; set; }
            }
        }
    }
}