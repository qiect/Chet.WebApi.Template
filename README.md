# Chet.WebApi.Template

一个基于.NET 8的现代化Web API模板框架，支持单体服务和微服务架构，提供完整的身份认证、缓存、日志等功能。

## 技术栈

- **框架**: .NET 8
- **ORM**: Entity Framework Core
- **数据库**: SQLite (本地开发)
- **身份认证**: JWT
- **缓存**: Redis (可选)
- **日志**: Serilog
- **对象映射**: AutoMapper
- **API文档**: Swagger/OpenAPI
- **测试**: Xunit

## 架构设计

### 分层架构

项目采用经典的分层架构设计，各层之间通过依赖注入实现解耦：

- **Core层**: 包含领域模型、接口定义和共享类
- **Application层**: 包含业务逻辑、DTO和映射配置
- **Infrastructure层**: 包含数据访问、缓存、配置和日志实现
- **API层**: 包含控制器、中间件和启动配置
- **Tests层**: 包含单元测试和集成测试

### 单体服务 vs 微服务

#### 单体服务

- 所有功能模块部署在一个应用中
- 适合中小型应用，开发和部署简单
- 代码集中管理，便于维护

#### 微服务

- 各功能模块独立部署，通过API通信
- 适合大型应用，具有更好的扩展性和容错性
- 可根据业务需求独立扩展各服务

## 快速开始

### 前置条件

- .NET 8 SDK
- SQLite (自动嵌入，无需额外安装)
- Redis (可选，应用会在Redis不可用时自动降级)

### 安装步骤

1. **克隆仓库**
   ```bash
   git clone https://github.com/your-repo/Chet.WebApi.Template.git
   cd Chet.WebApi.Template
   ```

2. **配置数据库连接**
   修改`Chet.WebApi.Template.Api/appsettings.json`中的数据库连接字符串（SQLite）：
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=ChetWebApiTemplate.db"
   }
   ```

3. **安装依赖**
   ```bash
   dotnet restore
   ```

4. **运行数据库迁移**
   ```bash
   dotnet ef database update --project Chet.WebApi.Template.Data --startup-project Chet.WebApi.Template.Api
   ```

5. **启动应用**
   ```bash
   dotnet run --project Chet.WebApi.Template.Api
   ```

6. **访问API文档**
   打开浏览器访问：http://localhost:5021
   应用会自动重定向到Swagger UI页面

## 项目结构

```
Chet.WebApi.Template/
├── Chet.WebApi.Template.Core/           # 核心层
│   ├── Chet.WebApi.Template.Contracts/   # 接口定义
│   ├── Chet.WebApi.Template.Domain/      # 领域模型
│   └── Chet.WebApi.Template.Shared/      # 共享类和异常
├── Chet.WebApi.Template.Application/    # 应用层
│   ├── Chet.WebApi.Template.DTOs/        # 数据传输对象
│   ├── Chet.WebApi.Template.Mapping/     # AutoMapper配置
│   └── Chet.WebApi.Template.Services/     # 业务逻辑实现
├── Chet.WebApi.Template.Infrastructure/ # 基础设施层
│   ├── Chet.WebApi.Template.Caching/     # 缓存实现
│   ├── Chet.WebApi.Template.Configuration/ # 配置管理
│   ├── Chet.WebApi.Template.Data/        # 数据库访问
│   └── Chet.WebApi.Template.Logging/     # 日志配置
├── Chet.WebApi.Template.Api/            # API层
│   ├── Controllers/                     # API控制器
│   └── Program.cs                       # 应用启动配置
└── Chet.WebApi.Template.Tests/          # 测试层
    ├── Chet.WebApi.Template.UnitTests/    # 单元测试
    └── Chet.WebApi.Template.IntegrationTests/ # 集成测试
```

## 功能模块

### 身份认证

- 用户名/密码登录
- JWT访问令牌
- 刷新令牌机制
- 基于角色的授权

### 用户管理

- 获取用户列表
- 获取单个用户
- 创建用户
- 更新用户
- 删除用户

### 缓存管理

- Redis缓存实现
- 缓存自动过期
- 缓存键前缀管理

### 日志管理

- 结构化日志
- 多环境日志配置
- 控制台和文件日志

## 配置说明

### 应用配置

主要配置文件：`appsettings.json`

- **数据库连接**: `ConnectionStrings.DefaultConnection`
- **JWT配置**: `AppSettings.Jwt`
- **Redis配置**: `AppSettings.Redis`
- **Serilog配置**: `Serilog`

### JWT配置

```json
"Jwt": {
  "SecretKey": "YourSecretKeyForJWTAuthentication1234567890",
  "Issuer": "Chet.WebApi.Template",
  "Audience": "Chet.WebApi.Template",
  "AccessTokenExpirationInMinutes": 30,
  "RefreshTokenExpirationInDays": 7
}
```

### Redis配置

```json
"Redis": {
  "ConnectionString": "localhost:6379",
  "InstanceName": "ChetWebApiTemplate:"
}
```

## 部署方式

### 本地开发

```bash
dotnet run --project Chet.WebApi.Template.Api
```

### Docker部署

1. **构建镜像**
   ```bash
docker build -t chet-webapi-template .
   ```

2. **运行容器**
   ```bash
docker run -p 5021:80 chet-webapi-template
   ```

### 发布到IIS

1. **发布应用**
   ```bash
dotnet publish -c Release -o ./publish
   ```

2. **部署到IIS**
   - 创建新网站
   - 设置物理路径为publish目录
   - 配置应用池为.NET 8

## 开发指南

### 添加新功能模块

1. **定义领域模型** (Core.Domain)
2. **定义接口** (Core.Contracts)
3. **实现仓储** (Infrastructure.Data)
4. **实现业务逻辑** (Application.Services)
5. **添加DTO和映射** (Application.DTOs, Application.Mapping)
6. **添加控制器** (Api.Controllers)

### 数据库迁移

```bash
# 添加迁移
dotnet ef migrations add InitialCreate --project Chet.WebApi.Template.Data --startup-project Chet.WebApi.Template.Api

# 应用迁移
dotnet ef database update --project Chet.WebApi.Template.Data --startup-project Chet.WebApi.Template.Api

# 删除迁移
dotnet ef migrations remove --project Chet.WebApi.Template.Data --startup-project Chet.WebApi.Template.Api
```

## 测试说明

### 运行单元测试

```bash
dotnet test --project Chet.WebApi.Template.Tests/Chet.WebApi.Template.UnitTests
```

### 运行集成测试

```bash
dotnet test --project Chet.WebApi.Template.Tests/Chet.WebApi.Template.IntegrationTests
```

## API文档

应用启动后，访问 http://localhost:5021/swagger 查看API文档。

## 许可证

MIT License

## 贡献

欢迎提交Issue和Pull Request！

## 联系方式

- 项目主页: https://github.com/your-repo/Chet.WebApi.Template
- 问题反馈: https://github.com/your-repo/Chet.WebApi.Template/issues

## 更新日志

### v1.0.0
- 初始化项目
- 实现JWT身份认证
- 实现用户管理功能
- 集成Redis缓存
- 集成Serilog日志
- 支持Swagger文档
- 提供完整的测试框架
