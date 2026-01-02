using Chet.WebApi.Template.Shared;
using Xunit;

namespace Chet.WebApi.Template.UnitTests;

/// <summary>
/// PaginatedResponse类的单元测试
/// </summary>
public class PaginatedResponseTests
{
    /// <summary>
    /// 测试PaginatedResponse.Ok方法创建成功的分页响应
    /// </summary>
    [Fact]
    public void Ok_ShouldReturnSuccessPaginatedResponse()
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
    
    /// <summary>
    /// 测试PaginatedResponse.Ok方法在最后一页时的行为
    /// </summary>
    [Fact]
    public void Ok_ShouldReturnCorrectHasNextPage_WhenLastPage()
    {
        // Arrange
        var items = new List<string> { "Item 10" };
        var totalCount = 10;
        var pageNumber = 4;
        var pageSize = 3;
        
        // Act
        var response = PaginatedResponse<string>.Ok(items, totalCount, pageNumber, pageSize);
        
        // Assert
        Assert.True(response.HasPreviousPage); // Last page, has previous
        Assert.False(response.HasNextPage); // Last page, no next
    }
    
    /// <summary>
    /// 测试PaginatedResponse.Ok方法在单页时的行为
    /// </summary>
    [Fact]
    public void Ok_ShouldReturnCorrectHasNextPage_WhenSinglePage()
    {
        // Arrange
        var items = new List<string> { "Item 1", "Item 2" };
        var totalCount = 2;
        var pageNumber = 1;
        var pageSize = 3;
        
        // Act
        var response = PaginatedResponse<string>.Ok(items, totalCount, pageNumber, pageSize);
        
        // Assert
        Assert.False(response.HasPreviousPage); // Single page, no previous
        Assert.False(response.HasNextPage); // Single page, no next
        Assert.Equal(1, response.TotalPages); // Only 1 page
    }
    
    /// <summary>
    /// 测试PaginatedResponse.Ok方法在空数据时的行为
    /// </summary>
    [Fact]
    public void Ok_ShouldHandleEmptyData()
    {
        // Arrange
        var items = new List<string>();
        var totalCount = 0;
        var pageNumber = 1;
        var pageSize = 3;
        
        // Act
        var response = PaginatedResponse<string>.Ok(items, totalCount, pageNumber, pageSize);
        
        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.TotalPages);
        Assert.False(response.HasPreviousPage);
        Assert.False(response.HasNextPage);
    }
}
