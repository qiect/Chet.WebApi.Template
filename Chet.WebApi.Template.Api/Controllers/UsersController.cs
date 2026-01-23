using Chet.WebApi.Template.Contracts;
using Chet.WebApi.Template.DTOs;
using Chet.WebApi.Template.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Chet.WebApi.Template.Api.Controllers;

/// <summary>
/// 用户控制器，处理用户的CRUD操作
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("提供用户管理相关的API接口，包括获取、创建、更新和删除用户")]
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
    /// <remarks>
    /// 示例响应：
    /// 
    ///     GET /api/Users
    ///     {
    ///         "success": true,
    ///         "data": [
    ///             {
    ///                 "id": 1,
    ///                 "email": "user1@example.com",
    ///                 "name": "John Doe"
    ///             },
    ///             {
    ///                 "id": 2,
    ///                 "email": "user2@example.com",
    ///                 "name": "Jane Smith"
    ///             }
    ///         ],
    ///         "message": "Users retrieved successfully",
    ///         "statusCode": 200
    ///     }
    /// </remarks>
    /// <response code="200">获取成功，返回用户列表</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Getting all users");
        var users = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse.Ok(users, "Users retrieved successfully"));
    }

    /// <summary>
    /// 根据ID获取用户信息
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户信息</returns>
    /// <remarks>
    /// 示例响应：
    /// 
    ///     GET /api/Users/1
    ///     {
    ///         "success": true,
    ///         "data": {
    ///             "id": 1,
    ///             "email": "user@example.com",
    ///             "name": "John Doe"
    ///         },
    ///         "message": "User retrieved successfully",
    ///         "statusCode": 200
    ///     }
    /// </remarks>
    /// <response code="200">获取成功，返回用户信息</response>
    /// <response code="404">用户不存在</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        _logger.LogInformation("Getting user with id: {Id}", id);
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(ApiResponse.Ok(user, "User retrieved successfully"));
    }

    /// <summary>
    /// 创建新用户
    /// </summary>
    /// <param name="userCreateDto">用户创建信息，包含用户邮箱和名称</param>
    /// <returns>创建的用户信息</returns>
    /// <remarks>
    /// 示例请求：
    /// 
    ///     POST /api/Users
    ///     {
    ///         "email": "newuser@example.com",
    ///         "name": "New User"
    ///     }
    /// </remarks>
    /// <response code="201">创建成功，返回创建的用户信息</response>
    /// <response code="400">创建失败，输入无效</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(UserCreateDto userCreateDto)
    {
        _logger.LogInformation("Creating new user with email: {Email}", userCreateDto.Email);
        var user = await _userService.CreateUserAsync(userCreateDto);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ApiResponse.Ok(user, "User created successfully", StatusCodes.Status201Created));
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="userUpdateDto">用户更新信息，包含用户名称</param>
    /// <returns>更新结果</returns>
    /// <remarks>
    /// 示例请求：
    /// 
    ///     PUT /api/Users/1
    ///     {
    ///         "name": "Updated Name"
    ///     }
    /// </remarks>
    /// <response code="204">更新成功</response>
    /// <response code="404">用户不存在</response>
    /// <response code="400">更新失败，输入无效</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userUpdateDto)
    {
        _logger.LogInformation("Updating user with id: {Id}", id);
        await _userService.UpdateUserAsync(id, userUpdateDto);
        return Ok(ApiResponse.NoContent("User updated successfully"));
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>删除结果</returns>
    /// <remarks>
    /// 示例请求：
    /// 
    ///     DELETE /api/Users/1
    /// </remarks>
    /// <response code="204">删除成功</response>
    /// <response code="404">用户不存在</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        _logger.LogInformation("Deleting user with id: {Id}", id);
        await _userService.DeleteUserAsync(id);
        return Ok(ApiResponse.NoContent("User deleted successfully"));
    }
}