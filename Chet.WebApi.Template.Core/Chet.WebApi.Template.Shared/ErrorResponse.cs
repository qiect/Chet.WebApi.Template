namespace Chet.WebApi.Template.Shared
{
    /// <summary>
    /// API错误响应类，用于封装错误信息并返回给客户端
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// 错误消息，描述发生的错误
        /// </summary>
        public string? Message { get; set; }
        
        /// <summary>
        /// HTTP状态码，表示错误类型
        /// </summary>
        public int StatusCode { get; set; }
        
        /// <summary>
        /// 错误堆栈跟踪，仅在开发环境中返回
        /// </summary>
        public string? StackTrace { get; set; }
        
        /// <summary>
        /// 验证错误字典，键为字段名，值为该字段的错误信息数组
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }
}
