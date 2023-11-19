using Diov.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Diov.Web;

public class DistributedCacheContentAccessor(
    IContentRepository contentRepository,
    IDistributedCache distributedCache,
    IOptions<DistributedCacheEntryOptions> distributedCacheEntryOptionsAccessor,
    IOptions<JsonSerializerOptions> jsonSerializerOptionsAccessor) :
    IContentAccessor
{
    private static string CacheKeyPrefix { get; } =
        $"{nameof(Content)}:{nameof(Content.Path)}:";

    public IContentRepository ContentRepository { get; set; } =
        contentRepository;
    public IDistributedCache DistributedCache { get; } =
        distributedCache;
    public DistributedCacheEntryOptions DistributedCacheEntryOptions { get; } =
        distributedCacheEntryOptionsAccessor.Value;
    public JsonSerializerOptions JsonSerializerOptions { get; } =
        jsonSerializerOptionsAccessor.Value;

    public async Task<int> AddContentAsync(
        Content content,
        CancellationToken cancellationToken = default)
    {
        content.Id = await ContentRepository
            .AddContentAsync(
                content,
                cancellationToken)
            .ConfigureAwait(false);

        await DistributedCache
            .SetAsync(
                $"{CacheKeyPrefix}{content.Path}",
                content,
                DistributedCacheEntryOptions,
                JsonSerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);

        return content.Id;
    }

    public async Task DeleteContentAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        await ContentRepository
            .DeleteContentAsync(
                path,
                cancellationToken)
            .ConfigureAwait(false);

        await DistributedCache
            .RemoveAsync(
                $"{CacheKeyPrefix}{path}",
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Content?> GetContentAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{path}";

        var content = await DistributedCache
            .GetAsync<Content>(
                cacheKey,
                JsonSerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);

        if (content == null)
        {
            content = await ContentRepository
                .GetContentAsync(
                    path,
                    cancellationToken)
                .ConfigureAwait(false);

            if (content != null)
            {
                await DistributedCache
                    .SetAsync(
                        cacheKey,
                        content,
                        DistributedCacheEntryOptions,
                        JsonSerializerOptions,
                        cancellationToken)
                    .ConfigureAwait(false);
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
        var searchResponse = await ContentRepository
            .SearchContentAsync(
                contentSearchRequest,
                skip,
                take,
                cancellationToken)
            .ConfigureAwait(false);

        foreach (var content in searchResponse.Items)
        {
            var cacheKey = $"{CacheKeyPrefix}{content.Path}";

            if (await DistributedCache
                    .GetAsync<Content>(
                        cacheKey,
                        JsonSerializerOptions,
                        cancellationToken)
                    .ConfigureAwait(false) == null)
            {
                await DistributedCache
                    .SetAsync(
                        cacheKey,
                        content,
                        DistributedCacheEntryOptions,
                        JsonSerializerOptions,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return searchResponse;
    }

    public async Task UpdateContentAsync(
        Content content,
        CancellationToken cancellationToken = default)
    {
        await ContentRepository
            .UpdateContentAsync(
                content,
                cancellationToken)
            .ConfigureAwait(false);

        await DistributedCache
            .SetAsync(
                $"{CacheKeyPrefix}{content.Path}",
                content,
                DistributedCacheEntryOptions,
                JsonSerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);
    }
}