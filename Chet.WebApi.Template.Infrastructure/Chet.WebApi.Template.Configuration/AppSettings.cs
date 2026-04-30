namespace Chet.WebApi.Template.Configuration
{
    /// <summary>
    /// 应用程序配置类，用于映射appsettings.json文件
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string? ConnectionStrings { get; set; }

        /// <summary>
        /// JWT配置设置
        /// </summary>
        public JwtSettings? Jwt { get; set; }

        /// <summary>
        /// Redis配置设置
        /// </summary>
        public RedisSettings? Redis { get; set; }
    }

    /// <summary>
    /// JWT配置设置类
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// 是否启用JWT身份认证功能
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// JWT签名密钥，用于生成和验证令牌
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// JWT签名密钥，用于生成和验证令牌
        /// </summary>
        public string? SecretKey { get; set; }

        /// <summary>
        /// JWT令牌发行者
        /// </summary>
        public string? Issuer { get; set; }

        /// <summary>
        /// JWT令牌受众
        /// </summary>
        public string? Audience { get; set; }

        /// <summary>
        /// 访问令牌过期时间（分钟）
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; }

        /// <summary>
        /// 刷新令牌过期时间（天）
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; }
    }

    /// <summary>
    /// Redis配置设置类
    /// </summary>
    public class RedisSettings
    {
        /// <summary>
        /// 是否启用Redis缓存功能
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Redis连接字符串
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Redis实例名称，用于缓存键前缀
        /// </summary>
        public string? InstanceName { get; set; }
    }
}
