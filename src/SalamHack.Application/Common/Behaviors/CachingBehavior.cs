using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace SalamHack.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await cache.GetOrCreateAsync(
            key: request.CacheKey,
            factory: async _ =>
            {
                logger.LogDebug("Cache miss, executing handler: {CacheKey}", request.CacheKey);
                return await next();
            },
            options: new HybridCacheEntryOptions
            {
                Expiration = request.Expiration
            },
            tags: request.Tags,
            cancellationToken: ct);

        if (response is IResult result && !result.IsSuccess)
        {
            await cache.RemoveAsync(request.CacheKey, ct);
            logger.LogDebug("Cache evicted for failed result: {CacheKey}", request.CacheKey);
        }

        return response;
    }
}
