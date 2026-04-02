using Chet.WebApi.Template.Contracts.User;
using Chet.WebApi.Template.Domain;
using Microsoft.EntityFrameworkCore;

namespace Chet.WebApi.Template.Data;

/// <summary>
/// 用户仓储实现类，继承自EfCoreRepository<User>并实现了IUserRepository接口
/// </summary>
public class UserRepository : EfCoreRepository<UserEnitity>, IUserRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public UserRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<UserEnitity> GetByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
