using System.Threading.Tasks;

namespace Diov.Data
{
    public interface IContentRepository
    {
        Task<int> AddContentAsync(Content content);
        Task DeleteContentAsync(int id);
        Task<SearchResponse<Content>> SearchContentsAsync(
            ContentSearchRequest contentSearchRequest,
            int pageIndex = 0,
            int pageSize = 5);
        Task UpdateContentAsync(Content content);
    }
}