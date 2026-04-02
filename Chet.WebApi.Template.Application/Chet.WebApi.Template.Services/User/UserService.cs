using AutoMapper;
using Chet.WebApi.Template.Contracts.Cache;
using Chet.WebApi.Template.Contracts.User;
using Chet.WebApi.Template.Domain;
using Chet.WebApi.Template.DTOs;
using Chet.WebApi.Template.Shared;
using Microsoft.Extensions.Logging;

namespace Chet.WebApi.Template.Services;

/// <summary>
/// 用户服务实现类，实现了 IUserService 接口
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    /// <summary>
    /// 用户缓存键模板，格式为 "users:{0}"，{0} 可以是用户ID或 "all"
    /// </summary>
    private const string UsersCacheKey = "users:{0}";

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="cacheService">缓存服务</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="logger">日志记录器</param>
    public UserService(
        IUserRepository userRepository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        _logger.LogInformation("Getting user by id: {Id}", id);

        var cacheKey = string.Format(UsersCacheKey, id);

        // 使用缓存获取用户，如果缓存不存在则从数据库获取并设置缓存
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException(nameof(UserEnitity), id);
            }
            return _mapper.Map<UserDto>(user);
        }, TimeSpan.FromMinutes(30)); // 缓存30分钟
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        _logger.LogInformation("Getting all users");

        var cacheKey = string.Format(UsersCacheKey, "all");

        // 使用缓存获取所有用户，如果缓存不存在则从数据库获取并设置缓存
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }, TimeSpan.FromMinutes(30)); // 缓存30分钟
    }

    /// <inheritdoc />
    public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto)
    {
        _logger.LogInformation("Creating user: {Email}", userCreateDto.Email);

        // 将DTO映射为实体
        var user = _mapper.Map<UserEnitity>(userCreateDto);
        // 对密码进行哈希处理
        user.PasswordHash = HashPassword(userCreateDto.Password);
        // 添加到数据库
        await _userRepository.AddAsync(user);

        // 清除所有用户缓存
        await _cacheService.RemoveAsync(string.Format(UsersCacheKey, "all"));

        // 返回创建的用户DTO
        return _mapper.Map<UserDto>(user);
    }

    /// <summary>
    /// 对密码进行哈希处理
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <returns>哈希后的密码</returns>
    private string HashPassword(string password)
    {
        // 使用BCrypt对密码进行哈希处理
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// 验证密码是否正确
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <param name="passwordHash">哈希后的密码</param>
    /// <returns>如果密码正确则返回true，否则返回false</returns>
    private bool Verify(string password, string passwordHash)
    {
        // 使用BCrypt验证密码
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    /// <inheritdoc />
    public async Task UpdateUserAsync(int id, UserUpdateDto userUpdateDto)
    {
        _logger.LogInformation("Updating user: {Id}", id);

        // 获取用户实体
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException(nameof(UserEnitity), id);
        }

        // 更新用户信息
        _mapper.Map(userUpdateDto, user);
        await _userRepository.UpdateAsync(user);

        // 清除相关缓存
        await _cacheService.RemoveAsync(string.Format(UsersCacheKey, id));
        await _cacheService.RemoveAsync(string.Format(UsersCacheKey, "all"));
    }

    /// <inheritdoc />
    public async Task DeleteUserAsync(int id)
    {
        _logger.LogInformation("Deleting user: {Id}", id);

        // 获取用户实体
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException(nameof(UserEnitity), id);
        }

        // 删除用户
        await _userRepository.DeleteAsync(user);

        // 清除相关缓存
        await _cacheService.RemoveAsync(string.Format(UsersCacheKey, id));
        await _cacheService.RemoveAsync(string.Format(UsersCacheKey, "all"));
    }
}
