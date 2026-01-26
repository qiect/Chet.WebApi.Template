# 统一API返回模型设计

## 1. 设计目标
- 提供一致的API响应格式，方便客户端处理
- 包含成功/失败状态标识，明确响应结果
- 支持数据、消息、状态码等必要字段
- 提供分页响应支持
- 保持与现有代码的兼容性
- 简化开发人员使用，提供便捷的静态工厂方法

## 2. 核心模型设计

### 2.1 通用响应包装器 `ApiResponse<T>`
```csharp
namespace Chet.WebApi.Template.Shared;

/// <summary>
/// 通用API响应包装器
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 响应状态码
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// 响应状态，true表示成功，false表示失败
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 响应消息
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// 响应时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    // 静态工厂方法...
}
```

### 2.2 分页响应模型 `PaginatedResponse<T>`
```csharp
namespace Chet.WebApi.Template.Shared;

/// <summary>
/// 分页API响应包装器
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class PaginatedResponse<T> : ApiResponse<IEnumerable<T>>
{
    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPreviousPage { get; set; }
    
    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage { get; set; }
    
    // 静态工厂方法...
}
```

## 3. 实现步骤

### 3.1 创建 Shared 项目并添加响应模型
- 在 `Chet.WebApi.Template.Shared` 项目中创建响应模型类
- 确保所有项目都能引用 Shared 项目

### 3.2 更新异常处理中间件
- 修改 `Program.cs` 中的异常处理中间件，使用 `ApiResponse<object>` 返回统一格式的错误响应

### 3.3 更新控制器
- 修改所有控制器方法，使用 `ApiResponse<T>` 包装响应
- 为需要分页的接口使用 `PaginatedResponse<T>`

### 3.4 添加扩展方法（可选）
- 为 `ControllerBase` 添加扩展方法，简化响应返回，如 `OkApi()`、`CreatedApi()`、`BadRequestApi()` 等

## 4. 使用示例

### 4.1 成功响应
```csharp
// 单个对象响应
return Ok(ApiResponse<UserDto>.Success(userDto, "User retrieved successfully"));

// 列表响应
return Ok(ApiResponse<IEnumerable<UserDto>>.Success(users, "Users retrieved successfully"));

// 分页响应
return Ok(PaginatedResponse<UserDto>.Success(users, totalCount, pageNumber, pageSize, "Users retrieved successfully"));
```

### 4.2 错误响应
```csharp
// 手动返回错误
return BadRequest(ApiResponse<object>.Error("Invalid input data", StatusCodes.Status400BadRequest));

// 异常处理中间件自动返回错误
```

## 5. 预期效果

### 5.1 成功响应示例
```json
{
  "statusCode": 200,
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "id": 1,
    "name": "admin",
    "email": "admin@example.com",
    "createdAt": "2026-01-02T11:46:26.7713129",
    "updatedAt": "2026-01-02T12:27:34.7343239"
  },
  "timestamp": "2026-01-02T16:38:43.1234567Z"
}
```

### 5.2 分页响应示例
```json
{
  "statusCode": 200,
  "success": true,
  "message": "Users retrieved successfully",
  "data": [/* 用户列表 */],
  "timestamp": "2026-01-02T16:38:43.1234567Z",
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 5.3 错误响应示例
```json
{
  "statusCode": 400,
  "success": false,
  "message": "Invalid email or password",
  "data": null,
  "timestamp": "2026-01-02T16:38:43.1234567Z"
}
```

## 6. 兼容性考虑
- 保持与现有DTO的兼容性，无需修改现有DTO结构
- 提供向后兼容的响应格式，确保现有客户端能够适应新的响应格式
- 逐步迁移现有控制器，支持混合使用旧格式和新格式

通过以上设计和实现，可以为项目提供一套统一、清晰、易用的API响应模型，提高API的一致性和可维护性，同时方便客户端开发人员处理API响应。