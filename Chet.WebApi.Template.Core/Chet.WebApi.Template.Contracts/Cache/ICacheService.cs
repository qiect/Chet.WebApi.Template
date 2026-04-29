namespace Chet.WebApi.Template.Contracts.Cache
{
    /// <summary>
    /// 缓存服务接口，定义了缓存相关的操作
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 根据键获取缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值，如果不存在则返回默认值</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 设置缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiry">过期时间，默认无过期时间</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// 根据键删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// 根据模式批量删除缓存键
        /// </summary>
        /// <param name="pattern">键模式（支持通配符 *）</param>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>如果缓存存在则返回 true，否则返回 false</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 获取匹配模式的键列表
        /// </summary>
        /// <param name="pattern">键模式（支持通配符 *）</param>
        /// <returns>匹配的键列表</returns>
        Task<string[]> GetKeysByPatternAsync(string pattern);

        /// <summary>
        /// 获取缓存值，如果不存在则使用工厂方法创建并设置缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="factory">创建缓存值的工厂方法</param>
        /// <param name="expiry">过期时间，默认无过期时间</param>
        /// <returns>缓存值</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
    }
}
