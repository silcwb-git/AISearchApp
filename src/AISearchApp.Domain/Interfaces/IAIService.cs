using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAIService
    {
        /// <summary>
        /// Generates a vector embedding for the provided text.
        /// </summary>
        /// <param name="text">The input text to embed.</param>
        /// <returns>A task representing the asynchronous operation, containing the embedding array.</returns>
        Task<float[]> GenerateEmbeddingAsync(string text);

        /// <summary>
        /// Generates an answer based on the provided context and question.
        /// </summary>
        /// <param name="context">The context information to base the answer on.</param>
        /// <param name="question">The user question.</param>
        /// <returns>A task representing the asynchronous operation, containing the generated answer string.</returns>
        Task<string> GenerateAnswerAsync(string context, string question);
    }
}