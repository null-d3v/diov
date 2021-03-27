using Diov.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Diov.Web
{
    public class DistributedCacheContentAccessor : IContentAccessor
    {
        private static readonly string cacheKeyPrefix =
            $"{nameof(Content)}:{nameof(Content.Path)}:";

        public DistributedCacheContentAccessor(
            IContentRepository contentRepository,
            IDistributedCache distributedCache,
            IOptions<DistributedCacheEntryOptions> distributedCacheEntryOptions,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            ContentRepository = contentRepository ??
                throw new ArgumentNullException(
                    nameof(contentRepository));
            DistributedCache = distributedCache ??
                throw new ArgumentNullException(
                    nameof(distributedCache));
            DistributedCacheEntryOptions = distributedCacheEntryOptions?.Value ??
                throw new ArgumentNullException(
                    nameof(distributedCacheEntryOptions));
            JsonSerializerOptions = jsonSerializerOptions?.Value ??
                throw new ArgumentNullException(
                    nameof(jsonSerializerOptions));
        }

        public IContentRepository ContentRepository { get; set; }
        public IDistributedCache DistributedCache { get; }
        public DistributedCacheEntryOptions DistributedCacheEntryOptions { get; }
        public JsonSerializerOptions JsonSerializerOptions { get; }

        public async Task<int> AddContentAsync(
            Content content,
            CancellationToken cancellationToken = default)
        {
            var id = await ContentRepository.AddContentAsync(
                content, cancellationToken);

            await DistributedCache.SetAsync(
                $"{cacheKeyPrefix}{content.Path}",
                content,
                DistributedCacheEntryOptions,
                JsonSerializerOptions,
                cancellationToken);

            return id;
        }

        public async Task DeleteContentAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            await ContentRepository.DeleteContentAsync(
                path, cancellationToken);

            await DistributedCache.RemoveAsync(
                $"{cacheKeyPrefix}{path}",
                cancellationToken);
        }

        public async Task<Content> GetContentAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{cacheKeyPrefix}{path}";

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
                var cacheKey = $"{cacheKeyPrefix}{content.Path}";

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
                $"{cacheKeyPrefix}{content.Path}",
                content,
                DistributedCacheEntryOptions,
                JsonSerializerOptions,
                cancellationToken);
        }
    }
}