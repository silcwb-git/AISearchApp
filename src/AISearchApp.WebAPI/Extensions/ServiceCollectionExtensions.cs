using AISearchApp.Application.Interfaces;
using AISearchApp.Application.Services;
using AISearchApp.Domain.Interfaces;
using AISearchApp.Infrastructure.AI;
using AISearchApp.Infrastructure.Configuration;
using AISearchApp.Infrastructure.VectorDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;

namespace AISearchApp.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ISearchService, SearchService>();
            return services;
        }

        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurar opções
            services.Configure<AIOptions>(configuration.GetSection("AI"));
            services.Configure<QdrantOptions>(configuration.GetSection("Qdrant"));

            // Registrar cliente Qdrant
            services.AddScoped<QdrantClient>();

            // Registrar repositório
            services.AddScoped<IVectorDbRepository, QdrantVectorDbRepository>();

            // Configurar HttpClient para OpenAI com retry policy
            services.AddHttpClient<OpenAIService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Registrar serviço de IA
            services.AddScoped<IAIService, OpenAIService>();

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        System.TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync<HttpResponseMessage>(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: System.TimeSpan.FromSeconds(30));
        }
    }
}