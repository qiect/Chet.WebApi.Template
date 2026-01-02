using Chet.WebApi.Template.Contracts;
using Microsoft.Extensions.Logging;

namespace Chet.WebApi.Template.Caching
{
    /// <summary>
    /// 空缓存服务实现类，当Redis未启用时使用
    /// 所有方法都是空实现，不执行任何实际的缓存操作
    /// </summary>
    public class NoOpCacheService : ICacheService
    {
        private readonly ILogger<NoOpCacheService> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger">日志记录器</param>
        public NoOpCacheService(ILogger<NoOpCacheService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<T> GetAsync<T>(string key)
        {
            _logger.LogDebug("NoOpCacheService: GetAsync called for key: {Key}", key);
            return Task.FromResult(default(T)!);
        }

        /// <inheritdoc />
        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            _logger.LogDebug("NoOpCacheService: SetAsync called for key: {Key}", key);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task RemoveAsync(string key)
        {
            _logger.LogDebug("NoOpCacheService: RemoveAsync called for key: {Key}", key);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string key)
        {
            _logger.LogDebug("NoOpCacheService: ExistsAsync called for key: {Key}", key);
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            _logger.LogDebug("NoOpCacheService: GetOrCreateAsync called for key: {Key}", key);
            return await factory();
        }
    }
}