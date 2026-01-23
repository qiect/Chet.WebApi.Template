# Serilog 配置说明

## 1. 项目中的 Serilog 配置

### 1.1 配置结构

项目的 Serilog 配置位于 `appsettings.json` 文件中的 `Serilog` 节点：

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "rollOnFileSizeLimit": true,
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  }
}
```

### 1.2 配置加载方式

Serilog 配置通过以下方式加载：

1. **Program.cs 中的配置**：
   ```csharp
   builder.Host.UseSerilog((context, configuration) =>
       context.ConfigureSerilog(configuration)
   );
   ```

2. **SerilogExtensions.cs 中的实现**：
   ```csharp
   public static void ConfigureSerilog(this HostBuilderContext context, LoggerConfiguration configuration)
   {
       configuration.ReadFrom.Configuration(context.Configuration);
   }
   ```

## 2. 配置项详细说明

### 2.1 基础配置

| 配置项 | 说明 | 示例值 |
|--------|------|--------|
| `Using` | 指定使用的 Serilog 接收器（Sinks） | `["Serilog.Sinks.Console", "Serilog.Sinks.File"]` |
| `MinimumLevel.Default` | 默认日志级别 | `Debug` |
| `MinimumLevel.Override` | 针对特定命名空间的日志级别覆盖 | `{"Microsoft": "Warning", "Microsoft.AspNetCore": "Warning"}` |

### 2.2 输出配置（WriteTo）

#### 2.2.1 控制台输出

```json
{
  "Name": "Console"
}
```

#### 2.2.2 文件输出

```json
{
  "Name": "File",
  "Args": {
    "path": "logs/log-.txt",
    "rollingInterval": "Day",
    "rollOnFileSizeLimit": true,
    "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
  }
}
```

| 参数 | 说明 | 示例值 |
|------|------|--------|
| `path` | 日志文件路径 | `logs/log-.txt` |
| `rollingInterval` | 日志滚动间隔 | `Day`（按天滚动） |
| `rollOnFileSizeLimit` | 是否按文件大小限制滚动 | `true` |
| `formatter` | 日志格式化器 | `Serilog.Formatting.Compact.CompactJsonFormatter` |

### 2.3 日志增强（Enrich）

| 增强器 | 说明 |
|--------|------|
| `FromLogContext` | 从日志上下文中获取属性 |
| `WithMachineName` | 添加机器名到日志 |
| `WithThreadId` | 添加线程ID到日志 |

## 3. 日志级别说明

Serilog 支持以下日志级别（从低到高）：

| 级别 | 说明 | 用途 |
|------|------|------|
| `Verbose` | 最详细的日志 | 详细的调试信息 |
| `Debug` | 调试信息 | 开发环境调试 |
| `Information` | 信息性消息 | 正常操作信息 |
| `Warning` | 警告信息 | 潜在问题 |
| `Error` | 错误信息 | 操作失败 |
| `Fatal` | 致命错误 | 系统崩溃 |

## 4. 代码中使用 Serilog

### 4.1 通过依赖注入使用

```csharp
public class SomeService
{
    private readonly ILogger<SomeService> _logger;

    public SomeService(ILogger<SomeService> logger)
    {
        _logger = logger;
    }

    public void DoSomething()
    {
        _logger.LogInformation("Doing something...");
        
        try
        {
            // 业务逻辑
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error doing something");
        }
    }
}
```

### 4.2 直接使用全局日志器

```csharp
Log.Information("Application started");
Log.Error(ex, "An error occurred");
```

## 5. 扩展配置

### 5.1 添加更多输出目标

可以添加更多的日志接收器，例如：

#### 5.1.1 添加 Seq 接收器

1. 安装包：`Serilog.Sinks.Seq`
2. 配置：
   ```json
   "WriteTo": [
     // 现有配置...
     {
       "Name": "Seq",
       "Args": {
         "serverUrl": "http://localhost:5341"
       }
     }
   ]
   ```

#### 5.1.2 添加 Elasticsearch 接收器

1. 安装包：`Serilog.Sinks.Elasticsearch`
2. 配置：
   ```json
   "WriteTo": [
     // 现有配置...
     {
       "Name": "Elasticsearch",
       "Args": {
         "nodeUris": "http://localhost:9200",
         "indexFormat": "logs-{0:yyyy.MM.dd}"
       }
     }
   ]
   ```

### 5.2 自定义日志格式

可以自定义控制台和文件输出的格式：

```json
"WriteTo": [
  {
    "Name": "Console",
    "Args": {
      "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j} {Exception}{NewLine}"
    }
  }
]
```

### 5.3 添加环境变量配置

可以通过环境变量覆盖配置：

```json
"MinimumLevel": {
  "Default": "Debug",
  "Override": {
    "Microsoft": "Warning",
    "Microsoft.AspNetCore": "Warning"
  }
}
```

## 6. 最佳实践

### 6.1 生产环境配置

生产环境建议：
- 将默认日志级别设置为 `Information`
- 配置适当的文件滚动策略
- 考虑添加集中式日志系统（如 Seq、Elasticsearch）
- 启用结构化日志

### 6.2 性能优化

- 避免在高频代码路径中使用详细的日志级别
- 使用结构化日志而不是字符串拼接
- 对于高流量应用，考虑使用异步接收器

### 6.3 安全考虑

- 避免记录敏感信息（密码、令牌等）
- 考虑使用日志脱敏
- 配置适当的日志文件权限

## 7. 常见问题及解决方案

### 7.1 日志文件未创建

**原因**：可能是路径权限问题或路径不存在
**解决方案**：确保应用程序有创建目录和文件的权限，或手动创建日志目录

### 7.2 日志级别不生效

**原因**：可能是配置覆盖顺序问题
**解决方案**：检查配置文件中的覆盖顺序，确保正确设置

### 7.3 结构化日志属性未显示

**原因**：可能是格式化器配置问题
**解决方案**：确保使用支持结构化日志的格式化器，如 `CompactJsonFormatter`

## 8. 配置示例

### 8.1 开发环境配置

```json
"Serilog": {
  "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
  "MinimumLevel": {
    "Default": "Debug",
    "Override": {
      "Microsoft": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "WriteTo": [
    {
      "Name": "Console",
      "Args": {
        "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j} {Exception}{NewLine}"
      }
    },
    {
      "Name": "File",
      "Args": {
        "path": "logs/development-.txt",
        "rollingInterval": "Day",
        "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
      }
    }
  ],
  "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
}
```

### 8.2 生产环境配置

```json
"Serilog": {
  "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Seq" ],
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "WriteTo": [
    {
      "Name": "Console",
      "Args": {
        "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Exception}{NewLine}"
      }
    },
    {
      "Name": "File",
      "Args": {
        "path": "logs/production-.txt",
        "rollingInterval": "Day",
        "rollOnFileSizeLimit": true,
        "fileSizeLimitBytes": 10485760,
        "retainedFileCountLimit": 7,
        "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
      }
    },
    {
      "Name": "Seq",
      "Args": {
        "serverUrl": "http://seq-server:5341"
      }
    }
  ],
  "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId", "WithEnvironmentName" ]
}
```

## 9. 配置位置建议

在 `Program.cs` 文件中，`builder.Host.UseSerilog()` 的最佳放置位置是：

```csharp
// 1. 创建 WebApplicationBuilder 实例
var builder = WebApplication.CreateBuilder(args);

// 2. 立即配置 Serilog（最佳位置）
builder.Host.UseSerilog((context, configuration) =>
    context.ConfigureSerilog(configuration)
);

// 3. 后续的配置操作
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
builder.Services.AddControllers();
// ... 其他配置
```

**原因**：
1. 尽早捕获日志，确保后续所有启动过程的日志都能被 Serilog 捕获
2. 替换默认日志系统，避免日志不一致
3. 避免配置冲突，确保日志系统先于其他服务配置
4. 确保配置读取完整，能读取到完整的日志配置

## 10. 总结

Serilog 是一个功能强大的结构化日志库，通过本配置说明，您可以：

1. 了解项目中当前的 Serilog 配置
2. 根据需要修改和扩展配置
3. 在代码中正确使用 Serilog 进行日志记录
4. 遵循最佳实践确保日志系统的可靠性和性能

通过合理配置 Serilog，您可以获得更清晰、更有用的日志信息，帮助您快速定位和解决问题。