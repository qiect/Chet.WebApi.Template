using Chet.WebApi.Template.Domain;

namespace Chet.WebApi.Template.Contracts.User
{
    /// <summary>
    /// 用户仓储接口，扩展了通用仓储接口，添加了用户特定的查询方法
    /// </summary>
    public interface IUserRepository : IRepository<UserEnitity>
    {
        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        /// <param name="email">用户邮箱</param>
        /// <returns>用户实体，如果不存在则返回null</returns>
        Task<UserEnitity> GetByEmailAsync(string email);
    }
}
