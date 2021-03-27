namespace Diov.Web
{
    using Microsoft.Extensions.Caching.Distributed;
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    public static class DistributedCacheExtensions
    {
        static DistributedCacheExtensions()
        {
            DefaultJsonSerializerOptions = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling
                    .AllowReadingFromString,
                PropertyNameCaseInsensitive = true,
            };
        }

        private static JsonSerializerOptions DefaultJsonSerializerOptions { get; }

        public static T Get<T>(
            this IDistributedCache distributedCache,
            string key,
            JsonSerializerOptions jsonSerializerOptions = null)
        {
            if (distributedCache == null)
            {
                throw new ArgumentNullException(
                    nameof(distributedCache));
            }
            jsonSerializerOptions ??= DefaultJsonSerializerOptions;

            var bytes = distributedCache.Get(key);
            if (bytes == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(
                bytes,
                jsonSerializerOptions);
        }

        public static async Task<T> GetAsync<T>(
            this IDistributedCache distributedCache,
            string key,
            JsonSerializerOptions jsonSerializerOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (distributedCache == null)
            {
                throw new ArgumentNullException(
                    nameof(distributedCache));
            }
            jsonSerializerOptions ??= DefaultJsonSerializerOptions;

            var bytes = await distributedCache.GetAsync(
                key, cancellationToken);
            if (bytes == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(
                bytes,
                jsonSerializerOptions);
        }

        public static void Set(
            this IDistributedCache distributedCache,
            string key,
            object value,
            DistributedCacheEntryOptions distributedCacheEntryOptions,
            JsonSerializerOptions jsonSerializerOptions = null)
        {
            if (distributedCache == null)
            {
                throw new ArgumentNullException(
                    nameof(distributedCache));
            }
            jsonSerializerOptions ??= DefaultJsonSerializerOptions;

            distributedCache.Set(
                key,
                JsonSerializer.SerializeToUtf8Bytes(
                    value, jsonSerializerOptions),
                distributedCacheEntryOptions);
        }

        public static async Task SetAsync(
            this IDistributedCache distributedCache,
            string key,
            object value,
            DistributedCacheEntryOptions distributedCacheEntryOptions,
            JsonSerializerOptions jsonSerializerOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (distributedCache == null)
            {
                throw new ArgumentNullException(
                    nameof(distributedCache));
            }
            jsonSerializerOptions ??= DefaultJsonSerializerOptions;

            await distributedCache.SetAsync(
                key,
                JsonSerializer.SerializeToUtf8Bytes(
                    value, jsonSerializerOptions),
                distributedCacheEntryOptions,
                cancellationToken);
        }
    }
}