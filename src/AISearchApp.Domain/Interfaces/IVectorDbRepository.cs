using System.Collections.Generic;
using System.Threading.Tasks;
using AISearchApp;

namespace Domain.Interfaces
{
    public interface IVectorDbRepository
    {
        Task SaveAsync(VectorDocument document);
        Task<IEnumerable<VectorDocument>> SearchAsync(float[] embedding, int limit);
    }
}