namespace Diov.Data;

public interface IContentRepository
{
    Task<int> AddContentAsync(
        Content content,
        CancellationToken cancellationToken = default);

    Task DeleteContentAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task<Content?> GetContentAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task<SearchResponse<Content>> SearchContentAsync(
        ContentSearchRequest contentSearchRequest,
        int skip = 0,
        int take = 5,
        CancellationToken cancellationToken = default);

    Task UpdateContentAsync(
        Content content,
        CancellationToken cancellationToken = default);
}