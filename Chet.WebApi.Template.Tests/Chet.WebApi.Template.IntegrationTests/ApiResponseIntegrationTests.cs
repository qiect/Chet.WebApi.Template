using Chet.WebApi.Template.Shared;
using Xunit;

namespace Chet.WebApi.Template.IntegrationTests;

/// <summary>
/// API响应格式的集成测试
/// </summary>
public class ApiResponseIntegrationTests
{
    /// <summary>
    /// 测试ApiResponse.Ok方法创建成功响应
    /// </summary>
    [Fact]
    public void Ok_ShouldReturnSuccessResponse()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var message = "Operation successful";
        var statusCode = 200;
        
        // Act
        var response = ApiResponse.Ok(data, message, statusCode);
        
        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(message, response.Message);
        Assert.Equal(data, response.Data);
    }
    
    /// <summary>
    /// 测试ApiResponse.Error方法创建错误响应
    /// </summary>
    [Fact]
    public void Error_ShouldReturnErrorResponse()
    {
        // Arrange
        var message = "Operation failed";
        var statusCode = 500;
        
        // Act
        var response = ApiResponse.Error(message, statusCode);
        
        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(message, response.Message);
        Assert.Null(response.Data);
    }
    
    /// <summary>
    /// 测试ApiResponse.NoContent方法创建无内容响应
    /// </summary>
    [Fact]
    public void NoContent_ShouldReturnNoContentResponse()
    {
        // Arrange
        var message = "No content available";
        var statusCode = 204;
        
        // Act
        var response = ApiResponse.NoContent(message, statusCode);
        
        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(message, response.Message);
        Assert.Null(response.Data);
    }
    
    /// <summary>
    /// 测试ApiResponse&lt;T&gt;.CreateSuccess方法创建泛型成功响应
    /// </summary>
    [Fact]
    public void GenericCreateSuccess_ShouldReturnGenericSuccessResponse()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var message = "Operation successful";
        var statusCode = 200;
        
        // Act
        var response = ApiResponse<object>.CreateSuccess(data, message, statusCode);
        
        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(message, response.Message);
        Assert.Equal(data, response.Data);
    }
    
    /// <summary>
    /// 测试PaginatedResponse.Ok方法创建成功的分页响应
    /// </summary>
    [Fact]
    public void PaginatedOk_ShouldReturnSuccessPaginatedResponse()
    {
        // Arrange
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };
        var totalCount = 10;
        var pageNumber = 1;
        var pageSize = 3;
        var message = "Items retrieved successfully";
        var statusCode = 200;
        
        // Act
        var response = PaginatedResponse<string>.Ok(items, totalCount, pageNumber, pageSize, message, statusCode);
        
        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(message, response.Message);
        Assert.Equal(items, response.Data);
        Assert.Equal(totalCount, response.TotalCount);
        Assert.Equal(pageNumber, response.PageNumber);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Equal(4, response.TotalPages); // 10 items / 3 per page = 4 pages
        Assert.False(response.HasPreviousPage); // First page, no previous
        Assert.True(response.HasNextPage); // First page, has next
    }
}
