using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diov.Web;

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

    public static T? Get<T>(
        this IDistributedCache distributedCache,
        string key,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
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

    public static async Task<T?> GetAsync<T>(
        this IDistributedCache distributedCache,
        string key,
        JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        jsonSerializerOptions ??= DefaultJsonSerializerOptions;

        var bytes = await distributedCache
            .GetAsync(
                key,
                cancellationToken)
            .ConfigureAwait(false);
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
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
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
        JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        jsonSerializerOptions ??= DefaultJsonSerializerOptions;

        await distributedCache
            .SetAsync(
                key,
                JsonSerializer.SerializeToUtf8Bytes(
                    value, jsonSerializerOptions),
                distributedCacheEntryOptions,
                cancellationToken)
            .ConfigureAwait(false);
    }
}