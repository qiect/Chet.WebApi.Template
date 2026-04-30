using Chet.WebApi.Template.Contracts.User;
using Chet.WebApi.Template.DTOs.User;
using Chet.WebApi.Template.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Chet.WebApi.Template.Api.Controllers;

/// <summary>
/// 用户管理控制器（Users Controller）
/// <para>
/// 提供用户资源的完整CRUD（创建、读取、更新、删除）操作。
/// 所有端点都需要JWT身份验证，确保只有已认证的用户才能访问。
/// </para>
/// </summary>
/// <remarks>
/// <para>端点列表：</para>
/// <list type="table">
///   <listheader>
///     <term>方法</term>
///     <description>端点</description>
///     <description>说明</description>
///   </listheader>
///   <item>
///     <term>GET</term>
///     <description>/api/v1/users</description>
///     <description>获取所有用户列表</description>
///   </item>
///   <item>
///     <term>GET</term>
///     <description>/api/v1/users/{id}</description>
///     <description>根据ID获取单个用户详情</description>
///   </item>
///   <item>
///     <term>POST</term>
///     <description>/api/v1/users</description>
///     <description>创建新用户</description>
///   </item>
///   <item>
///     <term>PUT</term>
///     <description>/api/v1/users/{id}</description>
///     <description>更新指定用户的信息</description>
///   </item>
///   <item>
///     <term>DELETE</term>
///     <description>/api/v1/users/{id}</description>
///     <description>删除指定用户</description>
///   </item>
/// </list>
/// 
/// <para>权限要求：</para>
/// <para>
/// 所有端点都需要在请求头中携带有效的JWT令牌：
/// <code>Authorization: Bearer &lt;access_token&gt;</code>
/// </para>
/// 
/// <para>响应格式：</para>
/// <para>
/// 所有响应都遵循统一的API响应包装格式（ApiResponse），
/// 包含success、message、data、statusCode四个字段。
/// </para>
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[SwaggerTag("提供用户管理相关的API接口，包括获取、创建、更新和删除用户")]
public class UsersController : ControllerBase
{
    /// <summary>
    /// 用户服务实例
    /// <para>负责处理用户相关的业务逻辑：查询、创建、更新、删除等操作</para>
    /// </summary>
    private readonly IUserService _userService;

    /// <summary>
    /// 日志记录器实例
    /// <para>用于记录用户管理操作的审计日志和错误信息</para>
    /// </summary>
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// 初始化用户控制器的新实例
    /// </summary>
    /// <param name="userService">用户服务接口，提供用户CRUD业务功能</param>
    /// <param name="logger">日志记录器，用于记录操作审计信息</param>
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有用户信息
    /// <para>
    /// 返回系统中所有用户的列表。支持分页查询（未来扩展）。
    /// 结果按创建时间倒序排列。
    /// </para>
    /// </summary>
    /// <returns>
    /// 200 OK - 返回用户列表数组
    /// </returns>
    /// <remarks>
    /// <para>请求示例：</para>
    /// <code>
    /// GET /api/v1/users
    /// Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
    /// </code>
    /// 
    /// <para>响应示例（200）：</para>
    /// <code>
    /// {
    ///   "success": true,
    ///   "message": "Users retrieved successfully",
    ///   "data": [
    ///     { "id": 1, "name": "张三", "email": "zhangsan@example.com" },
    ///     { "id": 2, "name": "李四", "email": "lisi@example.com" }
    ///   ],
    ///   "statusCode": 200
    /// }
    /// </code>
    /// </remarks>
    /// <response code="200">成功返回用户列表</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse.Ok(users, "Users retrieved successfully"));
    }

    /// <summary>
    /// 根据ID获取用户信息
    /// <para>
    /// 通过用户唯一标识符获取单个用户的详细信息。
    /// 如果指定的用户ID不存在，将返回404 Not Found。
    /// </para>
    /// </summary>
    /// <param name="id">用户唯一标识符（主键ID）</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>200 OK - 成功返回用户详细信息</description></item>
///   <item><description>404 Not Found - 指定ID的用户不存在</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>请求示例：</para>
    /// <code>
    /// GET /api/v1/users/1
    /// Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
    /// </code>
    /// 
    /// <para>响应示例（200）：</para>
    /// <code>
    /// {
    ///   "success": true,
    ///   "message": "User retrieved successfully",
    ///   "data": {
    ///     "id": 1,
    ///     "name": "张三",
    ///     "email": "zhangsan@example.com",
    ///     "createdAt": "2026-04-29T10:00:00Z"
    ///   },
    ///   "statusCode": 200
    /// }
    /// </code>
    /// 
    /// <para>响应示例（404）：</para>
    /// <code>
    /// {
    ///   "success": false,
    ///   "message": "User with ID 999 not found",
    ///   "data": null,
    ///   "statusCode": 404
    /// }
    /// </code>
    /// </remarks>
    /// <response code="200">成功返回用户详情</response>
    /// <response code="404">用户不存在</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(ApiResponse.Ok(user, "User retrieved successfully"));
    }

    /// <summary>
    /// 创建新用户
    /// <para>
    /// 在系统中创建新的用户账户。系统会自动进行数据验证，
    /// 包括邮箱格式检查和唯一性校验。密码会使用BCrypt加密存储。
    /// </para>
    /// </summary>
    /// <param name="userCreateDto">用户创建数据传输对象，包含姓名、邮箱、密码</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>201 Created - 创建成功，返回新用户信息和Location头</description></item>
    ///   <item><description>400 Bad Request - 输入数据验证失败或邮箱已存在</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>请求示例：</para>
    /// <code>
    /// POST /api/v1/users
    /// Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
    /// Content-Type: application/json
    /// 
    /// {
    ///   "name": "王五",
    ///   "email": "wangwu@example.com",
    ///   "password": "Secure@123456"
    /// }
    /// </code>
    /// 
    /// <para>响应示例（201）：</para>
    /// <code>
    /// HTTP/1.1 201 Created
    /// Location: /api/v1/users/3
    /// 
    /// {
    ///   "success": true,
    ///   "message": "User created successfully",
    ///   "data": {
    ///     "id": 3,
    ///     "name": "王五",
    ///     "email": "wangwu@example.com"
    ///   },
    ///   "statusCode": 201
    /// }
    /// </code>
    /// </remarks>
    /// <response code="201">用户创建成功</response>
    /// <response code="400">请求参数无效</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(UserCreateDto userCreateDto)
    {
        var user = await _userService.CreateUserAsync(userCreateDto);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ApiResponse.Ok(user, "User created successfully", StatusCodes.Status201Created));
    }

    /// <summary>
    /// 更新用户信息
    /// <para>
    /// 更新指定ID的用户信息。只更新提供的字段，未提供的字段保持不变。
    /// 注意：此接口不允许修改邮箱和密码，这些需要通过专门的接口处理。
    /// </para>
    /// </summary>
    /// <param name="id">要更新的用户唯一标识符</param>
    /// <param name="userUpdateDto">用户更新数据传输对象，包含需要修改的字段</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>204 No Content - 更新成功</description></item>
    ///   <item><description>400 Bad Request - 输入数据验证失败</description></item>
    ///   <item><description>404 Not Found - 指定ID的用户不存在</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>请求示例：</para>
    /// <code>
    /// PUT /api/v1/users/1
    /// Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
    /// Content-Type: application/json
    /// 
    /// {
    ///   "name": "张三丰",
    ///   "email": "zhangsanfeng@example.com"
    /// }
    /// </code>
    /// 
    /// <para>响应示例（204）：</para>
    /// <code>
    /// {
    ///   "success": true,
    ///   "message": "User updated successfully",
    ///   "data": null,
    ///   "statusCode": 204
    /// }
    /// </code>
    /// </remarks>
    /// <response code="204">更新成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">用户不存在</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userUpdateDto)
    {
        await _userService.UpdateUserAsync(id, userUpdateDto);
        return Ok(ApiResponse.NoContent("User updated successfully"));
    }

    /// <summary>
    /// 删除用户
    /// <para>
    /// 从系统中永久删除指定ID的用户。此操作不可逆，
    /// 建议在生产环境中使用软删除或增加二次确认机制。
    /// </para>
    /// </summary>
    /// <param name="id">要删除的用户唯一标识符</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>204 No Content - 删除成功</description></item>
    ///   <item><description>404 Not Found - 指定ID的用户不存在</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>请求示例：</para>
    /// <code>
    /// DELETE /api/v1/users/3
    /// Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
    /// </code>
    /// 
    /// <para>响应示例（204）：</para>
    /// <code>
    /// {
    ///   "success": true,
    ///   "message": "User deleted successfully",
    ///   "data": null,
    ///   "statusCode": 204
    /// }
    /// </code>
    /// 
    /// <para>⚠️ 安全警告：</para>
    /// <para>
    /// 此接口执行硬删除操作。在生产环境中建议：
    /// <list type="bullet">
    ///   <item><description>实现软删除（设置DeletedAt字段而非物理删除）</description></item>
    ///   <item><description>添加权限控制（仅管理员可删除用户）</description></item>
    ///   <item><description>添加操作确认机制（防止误操作）</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <response code="204">删除成功</response>
    /// <response code="404">用户不存在</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(ApiResponse.NoContent("User deleted successfully"));
    }
}
