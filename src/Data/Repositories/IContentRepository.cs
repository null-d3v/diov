using System.Threading;
using System.Threading.Tasks;

namespace Diov.Data
{
    public interface IContentRepository
    {
        Task<int> AddContentAsync(
            Content content,
            CancellationToken cancellationToken = default);

        Task DeleteContentAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<SearchResponse<Content>> SearchContentsAsync(
            ContentSearchRequest contentSearchRequest,
            int skip = 0,
            int take = 5,
            CancellationToken cancellationToken = default);

        Task UpdateContentAsync(
            Content content,
            CancellationToken cancellationToken = default);
    }
}