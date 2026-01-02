namespace Chet.WebApi.Template.Shared;

/// <summary>
/// 分页API响应包装器
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class PaginatedResponse<T> : ApiResponse<List<T>>
{
    /// <summary>
    /// 泛型响应数据
    /// </summary>
    public new List<T>? Data { get; set; }
    
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
    
    /// <summary>
    /// 分页响应静态工厂方法
    /// </summary>
    /// <param name="items">当前页数据</param>
    /// <param name="totalCount">总记录数</param>
    /// <param name="pageNumber">当前页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="message">响应消息</param>
    /// <param name="statusCode">响应状态码，默认200</param>
    /// <returns>PaginatedResponse实例</returns>
    public static PaginatedResponse<T> Ok(List<T> items, int totalCount, int pageNumber, int pageSize, string? message = null, int statusCode = 200)
    {
        // 计算总页数
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        return new PaginatedResponse<T>
        {
            StatusCode = statusCode,
            Success = true,
            Message = message,
            Data = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages
        };
    }
}