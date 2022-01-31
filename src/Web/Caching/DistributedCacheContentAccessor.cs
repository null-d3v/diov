using Diov.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Diov.Web;

public class DistributedCacheContentAccessor : IContentAccessor
{
    public DistributedCacheContentAccessor(
        IContentRepository contentRepository,
        IDistributedCache distributedCache,
        IOptions<DistributedCacheEntryOptions> distributedCacheEntryOptions,
        IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        ContentRepository = contentRepository;
        DistributedCache = distributedCache;
        DistributedCacheEntryOptions = distributedCacheEntryOptions.Value;
        JsonSerializerOptions = jsonSerializerOptions.Value;
    }

    private static string CacheKeyPrefix { get; } =
        $"{nameof(Content)}:{nameof(Content.Path)}:";

    public IContentRepository ContentRepository { get; set; }
    public IDistributedCache DistributedCache { get; }
    public DistributedCacheEntryOptions DistributedCacheEntryOptions { get; }
    public JsonSerializerOptions JsonSerializerOptions { get; }

    public async Task<int> AddContentAsync(
        Content content,
        CancellationToken cancellationToken = default)
    {
        content.Id = await ContentRepository.AddContentAsync(
            content, cancellationToken);

        await DistributedCache.SetAsync(
            $"{CacheKeyPrefix}{content.Path}",
            content,
            DistributedCacheEntryOptions,
            JsonSerializerOptions,
            cancellationToken);

        return content.Id;
    }

    public async Task DeleteContentAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        await ContentRepository.DeleteContentAsync(
            path, cancellationToken);

        await DistributedCache.RemoveAsync(
            $"{CacheKeyPrefix}{path}",
            cancellationToken);
    }

    public async Task<Content?> GetContentAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{path}";

        var content = await DistributedCache.GetAsync<Content>(
            cacheKey, JsonSerializerOptions, cancellationToken);

        if (content == null)
        {
            content = await ContentRepository.GetContentAsync(
                path, cancellationToken);

            if (content != null)
            {
                await DistributedCache.SetAsync(
                    cacheKey,
                    content,
                    DistributedCacheEntryOptions,
                    JsonSerializerOptions,
                    cancellationToken);
            }
        }

        return content;
    }

    public async Task<SearchResponse<Content>> SearchContentAsync(
        ContentSearchRequest contentSearchRequest,
        int skip = 0,
        int take = 5,
        CancellationToken cancellationToken = default)
    {
        var searchResponse = await ContentRepository.SearchContentAsync(
            contentSearchRequest,
            skip,
            take,
            cancellationToken);

        foreach (var content in searchResponse.Items)
        {
            var cacheKey = $"{CacheKeyPrefix}{content.Path}";

            if (await DistributedCache.GetAsync<Content>(
                cacheKey, JsonSerializerOptions, cancellationToken) == null)
            {
                await DistributedCache.SetAsync(
                    cacheKey,
                    content,
                    DistributedCacheEntryOptions,
                    JsonSerializerOptions,
                    cancellationToken);
            }
        }

        return searchResponse;
    }

    public async Task UpdateContentAsync(
        Content content,
        CancellationToken cancellationToken = default)
    {
        await ContentRepository.UpdateContentAsync(
            content, cancellationToken);

        await DistributedCache.SetAsync(
            $"{CacheKeyPrefix}{content.Path}",
            content,
            DistributedCacheEntryOptions,
            JsonSerializerOptions,
            cancellationToken);
    }
}