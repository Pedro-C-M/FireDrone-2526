using StackExchange.Redis;
using System.Text.Json;

namespace CentralBackend.Services
{
    /// <summary>
    /// Servicio de caché distribuida con Redis para mejorar el rendimiento
    /// del sistema evitando consultas repetidas a la base de datos.
    /// </summary>
    public class RedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
            _logger = logger;
        }

        /// <summary>
        /// Obtiene un valor de la caché deserializado al tipo especificado
        /// </summary>
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _db.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                {
                    _logger.LogDebug("Cache MISS for key: {Key}", key);
                    return default;
                }

                _logger.LogDebug("Cache HIT for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache key: {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// Almacena un valor serializado en la caché con tiempo de expiración opcional
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);

                // Use When.Always and configure expiry if provided
                if (expiry.HasValue)
                {
                    await _db.StringSetAsync(key, json, expiry.Value, when: When.Always);
                }
                else
                {
                    await _db.StringSetAsync(key, json, when: When.Always);
                }

                _logger.LogDebug("Cache SET for key: {Key}, Expiry: {Expiry}min", key, expiry?.TotalMinutes ?? -1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
            }
        }

        /// <summary>
        /// Elimina una clave específica de la caché
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
                _logger.LogDebug("Cache REMOVE for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        /// <summary>
        /// Elimina todas las claves que coincidan con un patrón
        /// Útil para invalidar grupos de cachés relacionados
        /// </summary>
        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var endpoints = _redis.GetEndPoints();
                if (endpoints.Length == 0)
                {
                    _logger.LogWarning("No Redis endpoints available for pattern removal");
                    return;
                }

                var server = _redis.GetServer(endpoints.First());
                var keys = server.Keys(pattern: pattern).ToArray();

                if (keys.Any())
                {
                    await _db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cache REMOVE by pattern: {Pattern}, Keys removed: {Count}", pattern, keys.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
            }
        }

        /// <summary>
        /// Verifica si una clave existe en la caché
        /// </summary>
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if cache key exists: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Obtiene información de conexión de Redis para monitoreo
        /// </summary>
        public string GetConnectionStatus()
        {
            try
            {
                return _redis.IsConnected ? "Connected" : "Disconnected";
            }
            catch
            {
                return "Error";
            }
        }
    }
}
