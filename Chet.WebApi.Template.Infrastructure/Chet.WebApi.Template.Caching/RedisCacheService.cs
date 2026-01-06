using Chet.WebApi.Template.Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Chet.WebApi.Template.Caching
{
    /// <summary>
    /// Redis缓存服务实现类，实现了ICacheService接口
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionMultiplexer">Redis连接多路复用器</param>
        /// <param name="logger">日志记录器</param>
        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
        {
            _database = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (value.IsNull)
                {
                    return default;
                }
                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from Redis cache for key: {Key}", key);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, json, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value to Redis cache for key: {Key}", key);
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from Redis cache for key: {Key}", key);
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists in Redis cache: {Key}", key);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            var value = await GetAsync<T>(key);
            if (value != null)
            {
                return value;
            }

            value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, expiry);
            }

            return value;
        }
    }
}
