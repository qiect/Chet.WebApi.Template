using Chet.WebApi.Template.Shared;

namespace Chet.WebApi.Template.UnitTests;

/// <summary>
/// ApiResponse类的单元测试
/// </summary>
public class ApiResponseTests
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
    /// 测试ApiResponse&lt;T&gt;.CreateError方法创建泛型错误响应
    /// </summary>
    [Fact]
    public void GenericCreateError_ShouldReturnGenericErrorResponse()
    {
        // Arrange
        var message = "Operation failed";
        var statusCode = 500;

        // Act
        var response = ApiResponse<string>.CreateError(message, statusCode);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(message, response.Message);
        Assert.Null(response.Data);
    }
}
