using AutoMapper;
using Chet.WebApi.Template.Contracts.Cache;
using Chet.WebApi.Template.Contracts.Security;
using Chet.WebApi.Template.Contracts.User;
using Chet.WebApi.Template.Domain.User;
using Chet.WebApi.Template.DTOs.User;
using Chet.WebApi.Template.Shared;
using Chet.WebApi.Template.Shared.Caching;
using Microsoft.Extensions.Logging;

namespace Chet.WebApi.Template.Services.User;

/// <summary>
/// 用户服务实现类，实现了 IUserService 接口
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ICacheService cacheService,
        IPasswordService passwordService,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
        _passwordService = passwordService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        _logger.LogInformation("Getting user by id: {Id}", id);

        var cacheKey = CacheKeys.Users.ById(id);

        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException(nameof(UserEntity), id);
            }
            return _mapper.Map<UserDto>(user);
        }, CacheKeys.Expiry.Medium);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        _logger.LogInformation("Getting all users");

        var cacheKey = CacheKeys.Users.All();

        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }, CacheKeys.Expiry.Medium);
    }

    public async Task<PagedResult<UserDto>> GetPagedUsersAsync(PagedRequest request)
    {
        _logger.LogInformation("Getting paged users: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);

        request.Normalize();

        var pagedUsers = await _userRepository.GetPagedAsync(request);
        var userDtos = _mapper.Map<List<UserDto>>(pagedUsers.Items);

        return new PagedResult<UserDto>(userDtos, request.PageNumber, request.PageSize, pagedUsers.Metadata.TotalCount);
    }

    public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto)
    {
        _logger.LogInformation("Creating user: {Email}", userCreateDto.Email);

        var user = _mapper.Map<UserEntity>(userCreateDto);
        user.PasswordHash = _passwordService.Hash(userCreateDto.Password);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        await _cacheService.RemoveByPatternAsync(CacheKeys.Users.Pattern);

        return _mapper.Map<UserDto>(user);
    }

    public async Task UpdateUserAsync(int id, UserUpdateDto userUpdateDto)
    {
        _logger.LogInformation("Updating user: {Id}", id);

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException(nameof(UserEntity), id);
        }

        _mapper.Map(userUpdateDto, user);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync(CacheKeys.Users.ById(id));
        await _cacheService.RemoveByPatternAsync(CacheKeys.Users.Pattern);
    }

    public async Task DeleteUserAsync(int id)
    {
        _logger.LogInformation("Deleting user: {Id}", id);

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException(nameof(UserEntity), id);
        }

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync(CacheKeys.Users.ById(id));
        await _cacheService.RemoveByPatternAsync(CacheKeys.Users.Pattern);
    }
}
