namespace Application.Interfaces;

/// <summary>
/// Abstraction over a distributed or in-process cache.
/// Session tokens are stored with a 15-minute sliding expiry (DR-002).
/// </summary>
public interface ICacheService
{
    /// <summary>Gets a cached value, extending the sliding TTL on each access.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Stores a value. When <paramref name="expiry"/> is null the implementation applies its default TTL.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>Removes a cached value.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Returns true when the key is present in the cache.</summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
