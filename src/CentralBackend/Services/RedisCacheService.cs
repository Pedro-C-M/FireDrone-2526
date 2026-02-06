using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CentralBackend.Services
{
    /// Servicio de caché distribuida con Redis para mejorar el rendimiento
    /// del sistema evitando consultas repetidas a la base de datos.
    public class RedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;


        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        protected RedisCacheService() { }//Para los moqs

        /// Obtiene un valor de la caché deserializado al tipo especificado
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _db.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                {
                    _logger.LogInformation("?? [Redis] Cache MISS for key: {Key}", key);
                    return default;
                }

                _logger.LogInformation("? [Redis] Cache HIT for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [Redis] Error getting cache key: {Key}", key);
                return default;
            }
        }

        /// Almacena un valor serializado en la caché con tiempo de expiración opcional
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);

                // Use When.Always and configure expiry if provided
                if (expiry.HasValue)
                {
                    await _db.StringSetAsync(key, json, expiry.Value, when: When.Always);
                }
                else
                {
                    await _db.StringSetAsync(key, json, when: When.Always);
                }

                _logger.LogInformation("?? [Redis] Cache SET for key: {Key}, Expiry: {Expiry}min", key, expiry?.TotalMinutes ?? -1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [Redis] Error setting cache key: {Key}", key);
            }
        }

        /// Elimina una clave específica de la caché
        public async virtual Task RemoveAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
                _logger.LogInformation("??? [Redis] Cache REMOVE for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [Redis] Error removing cache key: {Key}", key);
            }
        }

        /// Elimina todas las claves que coincidan con un patrón
        /// Útil para invalidar grupos de cachés relacionados
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

        /// Verifica si una clave existe en la caché
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


        /// Obtiene información de conexión de Redis para monitoreo
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
