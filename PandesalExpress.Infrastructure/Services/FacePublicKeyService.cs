using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PandesalExpress.Infrastructure.Configs;

namespace PandesalExpress.Infrastructure.Services;

public class FacePublicKeyService
{
    private const string CacheKey = "FaceServicePublicKeys";
    private const string RotationInfoCacheKey = "FaceServiceRotationInfo";
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FacePublicKeyService> _logger;
    private readonly JwtOptions _options;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public FacePublicKeyService(
        IMemoryCache cache,
        HttpClient httpClient,
        IOptions<JwtOptions> options,
        ILogger<FacePublicKeyService> logger
    )
    {
        _cache = cache;
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // Set internal service authentication header
        if (!string.IsNullOrEmpty(_options.InternalServiceKey)) _httpClient.DefaultRequestHeaders.Add("X-Internal-Key", _options.InternalServiceKey);
    }

    public async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync()
    {
        // Try to get from cache first
        if (!_cache.TryGetValue(CacheKey, out IEnumerable<SecurityKey>? cachedKeys) || cachedKeys == null)
            // If not in cache, refresh keys
            return await RefreshKeysAsync();

        _logger.LogDebug("Retrieved signing keys from cache");
        return cachedKeys;
    }

    public async Task<IEnumerable<SecurityKey>> RefreshKeysAsync()
    {
        await _refreshLock.WaitAsync();
        try
        {
            // Double-check cache after acquiring lock
            if (_cache.TryGetValue(CacheKey, out IEnumerable<SecurityKey>? cachedKeys) && cachedKeys != null)
                return cachedKeys;

            _logger.LogInformation("Fetching public keys from Face Recognition Service: {JwksUri}", _options.JwksUri);

            HttpResponseMessage response = await _httpClient.GetAsync(_options.JwksUri);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch JWKS. Status: {StatusCode}", response.StatusCode);
                throw new InvalidOperationException($"JWKS fetch failed with status: {response.StatusCode}");
            }

            JwksDocument? jwks = await response.Content.ReadFromJsonAsync<JwksDocument>();

            if (jwks?.Keys == null || jwks.Keys.Count > 0)
            {
                _logger.LogError("JWKS document is empty or invalid");
                throw new InvalidOperationException("JWKS document is empty or invalid");
            }

            var keys = jwks.Keys.Select(k =>
                {
                    try
                    {
                        var rsa = RSA.Create();
                        rsa.ImportParameters(
                            new RSAParameters
                            {
                                Modulus = Base64UrlEncoder.DecodeBytes(k.N),
                                Exponent = Base64UrlEncoder.DecodeBytes(k.E)
                            }
                        );

                        return new RsaSecurityKey(rsa) { KeyId = k.Kid };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to import RSA key with kid: {Kid}", k.Kid);
                        throw;
                    }
                }
            ).ToList();

            _logger.LogInformation("Successfully loaded {KeyCount} signing keys", keys.Count);

            // Fetch rotation info to set appropriate cache duration
            TimeSpan cacheExpiration = await GetCacheExpirationAsync();

            _cache.Set(CacheKey, keys, cacheExpiration);

            return keys;
        }
        finally { _refreshLock.Release(); }
    }

    private async Task<TimeSpan> GetCacheExpirationAsync()
    {
        try
        {
            string rotationInfoUri = _options.JwksUri.Replace("/jwks", "/keys/rotation-info");
            RotationInfo? rotationInfo = await _httpClient.GetFromJsonAsync<RotationInfo>(rotationInfoUri);

            if (rotationInfo?.NextRotation != null)
            {
                var nextRotation = DateTime.Parse(rotationInfo.NextRotation, CultureInfo.CurrentCulture);
                TimeSpan timeUntilRotation = nextRotation - DateTime.UtcNow;

                // Cache for 80% of the rotation interval to ensure fresh keys
                var cacheTime = TimeSpan.FromMinutes(timeUntilRotation.TotalMinutes * 0.8);

                // Minimum 1 minute, maximum 30 minutes
                cacheTime = TimeSpan.FromMinutes(Math.Max(1, Math.Min(30, cacheTime.TotalMinutes)));

                _logger.LogInformation(
                    "Setting cache expiration to {CacheTime} (Next rotation: {NextRotation})",
                    cacheTime,
                    rotationInfo.NextRotation
                );

                return cacheTime;
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to fetch rotation info, using default cache expiration"); }

        // Default fallback: 10 minutes
        return TimeSpan.FromMinutes(10);
    }

    public async Task<RotationInfo?> GetRotationInfoAsync()
    {
        try
        {
            if (_cache.TryGetValue(RotationInfoCacheKey, out RotationInfo? cached) && cached != null) return cached;

            string rotationInfoUri = _options.JwksUri.Replace("/jwks", "/keys/rotation-info");
            RotationInfo? rotationInfo = await _httpClient.GetFromJsonAsync<RotationInfo>(rotationInfoUri);

            if (rotationInfo != null) _cache.Set(RotationInfoCacheKey, rotationInfo, TimeSpan.FromMinutes(5));

            return rotationInfo;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch rotation info");
            return null;
        }
    }

    private sealed record JwksDocument([property: JsonPropertyName("keys")] List<JwkKey> Keys);

    private sealed record JwkKey(
        [property: JsonPropertyName("kty")] string Kty,
        [property: JsonPropertyName("n")] string N,
        [property: JsonPropertyName("e")] string E,
        [property: JsonPropertyName("alg")] string Alg,
        [property: JsonPropertyName("use")] string Use,
        [property: JsonPropertyName("kid")] string Kid
    );

    public sealed record RotationInfo(
        [property: JsonPropertyName("last_rotation")]
        string? LastRotation,
        [property: JsonPropertyName("next_rotation")]
        string? NextRotation,
        [property: JsonPropertyName("current_kid")]
        string? CurrentKid,
        [property: JsonPropertyName("previous_kid")]
        string? PreviousKid,
        [property: JsonPropertyName("rotation_interval_minutes")]
        double RotationIntervalMinutes
    );
}
