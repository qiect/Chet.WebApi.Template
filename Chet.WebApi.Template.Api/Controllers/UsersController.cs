using Chet.WebApi.Template.Contracts;
using Chet.WebApi.Template.DTOs;
using Chet.WebApi.Template.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chet.WebApi.Template.Api.Controllers;

/// <summary>
/// 用户控制器，处理用户的CRUD操作
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    /// <summary>
    /// 用户服务，用于处理用户相关的业务逻辑
    /// </summary>
    private readonly IUserService _userService;
    
    /// <summary>
    /// 日志记录器，用于记录控制器操作日志
    /// </summary>
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userService">用户服务</param>
    /// <param name="logger">日志记录器</param>
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有用户信息
    /// </summary>
    /// <returns>用户列表</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Getting all users");
        var users = await _userService.GetAllUsersAsync();
        return Ok(Chet.WebApi.Template.Shared.ApiResponse.Ok(users, "Users retrieved successfully"));
    }

    /// <summary>
    /// 根据ID获取用户信息
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户信息</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        _logger.LogInformation("Getting user with id: {Id}", id);
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(Chet.WebApi.Template.Shared.ApiResponse.Ok(user, "User retrieved successfully"));
    }

    /// <summary>
    /// 创建新用户
    /// </summary>
    /// <param name="userCreateDto">用户创建信息</param>
    /// <returns>创建的用户信息</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(UserCreateDto userCreateDto)
    {
        _logger.LogInformation("Creating new user");
        var user = await _userService.CreateUserAsync(userCreateDto);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, Chet.WebApi.Template.Shared.ApiResponse.Ok(user, "User created successfully", StatusCodes.Status201Created));
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="userUpdateDto">用户更新信息</param>
    /// <returns>更新结果</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userUpdateDto)
    {
        _logger.LogInformation("Updating user with id: {Id}", id);
        await _userService.UpdateUserAsync(id, userUpdateDto);
        return Ok(Chet.WebApi.Template.Shared.ApiResponse.NoContent("User updated successfully"));
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        _logger.LogInformation("Deleting user with id: {Id}", id);
        await _userService.DeleteUserAsync(id);
        return Ok(Chet.WebApi.Template.Shared.ApiResponse.NoContent("User deleted successfully"));
    }
}