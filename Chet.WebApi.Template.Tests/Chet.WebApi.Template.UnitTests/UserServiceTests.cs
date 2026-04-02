using AutoMapper;
using Chet.WebApi.Template.Contracts.Cache;
using Chet.WebApi.Template.Contracts.User;
using Chet.WebApi.Template.Domain.User;
using Chet.WebApi.Template.DTOs.User;
using Chet.WebApi.Template.Services.User;
using Chet.WebApi.Template.Shared;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chet.WebApi.Template.UnitTests
{
    /// <summary>
    /// 用户服务单元测试类，用于测试UserService的各种功能
    /// 该测试类使用模拟对象来隔离被测服务与其依赖项
    /// </summary>
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;       // 模拟用户存储库
        private readonly Mock<ICacheService> _mockCacheService;          // 模拟缓存服务
        private readonly Mock<IMapper> _mockMapper;                     // 模拟对象映射器
        private readonly Mock<ILogger<UserService>> _mockLogger;        // 模拟日志服务
        private readonly UserService _userService;                      // 被测试的服务实例

        /// <summary>
        /// 构造函数，初始化测试所需的模拟对象和服务实例
        /// </summary>
        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<UserService>>();

            _userService = new UserService(
                _mockUserRepository.Object,
                _mockCacheService.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        /// <summary>
        /// 测试根据有效ID获取用户的方法
        /// 验证当提供有效用户ID时，服务应正确返回对应的用户DTO
        /// </summary>
        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ReturnsUserDto()
        {
            // Arrange - 准备测试数据和模拟行为
            var userId = 1;
            var userEntity = new UserEnitity { Id = userId, Name = "Test User", Email = "test@example.com" };
            var expectedUserDto = new UserDto { Id = userId, Name = "Test User", Email = "test@example.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };

            // 设置缓存服务的行为：绕过缓存直接调用工厂方法
            _mockCacheService.Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<UserDto>>>(), It.IsAny<TimeSpan>()))
                .Returns<string, Func<Task<UserDto>>, TimeSpan>((key, factory, expiry) => factory());
            // 设置用户存储库的行为：当调用GetByIdAsync并传入userId时，返回userEntity
            _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(userEntity);
            // 设置映射器的行为：当映射User到UserDto时，返回expectedUserDto
            _mockMapper.Setup(x => x.Map<UserDto>(userEntity)).Returns(expectedUserDto);

            // Act - 执行被测方法
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert - 验证结果是否符合预期
            Assert.Equal(expectedUserDto.Id, result.Id);
            Assert.Equal(expectedUserDto.Name, result.Name);
            Assert.Equal(expectedUserDto.Email, result.Email);
            // 验证GetByIdAsync方法被调用了一次
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
            // 验证Map方法被调用了一次
            _mockMapper.Verify(x => x.Map<UserDto>(userEntity), Times.Once);
        }

        /// <summary>
        /// 测试根据无效ID获取用户的方法
        /// 验证当提供不存在的用户ID时，服务应抛出NotFoundException
        /// </summary>
        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ThrowsNotFoundException()
        {
            // Arrange - 准备测试数据和模拟行为
            var userId = 999;
            // 设置缓存服务的行为：绕过缓存直接调用工厂方法
            _mockCacheService.Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<UserDto>>>(), It.IsAny<TimeSpan>()))
                .Returns<string, Func<Task<UserDto>>, TimeSpan>((key, factory, expiry) => factory());
            // 设置用户存储库的行为：当调用GetByIdAsync并传入userId时，返回null
            _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((UserEnitity)null);

            // Act & Assert - 验证调用方法会抛出NotFoundException
            await Assert.ThrowsAsync<NotFoundException>(() => _userService.GetUserByIdAsync(userId));
            // 验证GetByIdAsync方法被调用了一次
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        }

        /// <summary>
        /// 测试获取所有用户的方法
        /// 验证服务能够正确返回所有用户列表
        /// </summary>
        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            // Arrange - 准备测试数据和模拟行为
            var users = new List<UserEnitity>
            {
                new UserEnitity { Id = 1, Name = "User 1", Email = "user1@example.com" },
                new UserEnitity { Id = 2, Name = "User 2", Email = "user2@example.com" }
            }.AsEnumerable();

            var expectedUserDtos = new List<UserDto>
            {
                new UserDto { Id = 1, Name = "User 1", Email = "user1@example.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new UserDto { Id = 2, Name = "User 2", Email = "user2@example.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            }.AsEnumerable();

            // 设置缓存服务的行为：绕过缓存直接调用工厂方法
            _mockCacheService.Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<UserDto>>>>(), It.IsAny<TimeSpan>()))
                .Returns<string, Func<Task<IEnumerable<UserDto>>>, TimeSpan>((key, factory, expiry) => factory());
            // 设置用户存储库的行为：当调用GetAllAsync时，返回users列表
            _mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
            // 设置映射器的行为：当映射IEnumerable<User>到IEnumerable<UserDto>时，返回expectedUserDtos
            _mockMapper.Setup(x => x.Map<IEnumerable<UserDto>>(users)).Returns(expectedUserDtos);

            // Act - 执行被测方法
            var result = await _userService.GetAllUsersAsync();

            // Assert - 验证结果是否符合预期
            Assert.Equal(2, result.Count());
            // 验证GetAllAsync方法被调用了一次
            _mockUserRepository.Verify(x => x.GetAllAsync(), Times.Once);
            // 验证Map方法被调用了一次
            _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
        }

        /// <summary>
        /// 测试创建用户的方法
        /// 验证当提供有效数据时，服务应正确创建新用户并返回创建的用户DTO
        /// </summary>
        [Fact]
        public async Task CreateUserAsync_WithValidData_CreatesAndReturnsUser()
        {
            // Arrange - 准备测试数据和模拟行为
            var userCreateDto = new UserCreateDto
            {
                Name = "New User",
                Email = "newuser@example.com",
                Password = "password123"
            };

            var userEntity = new UserEnitity
            {
                Id = 1,
                Name = "New User",
                Email = "newuser@example.com"
            };

            var expectedUserDto = new UserDto
            {
                Id = 1,
                Name = "New User",
                Email = "newuser@example.com",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 设置映射器的行为：当映射UserCreateDto到User时，返回userEntity
            _mockMapper.Setup(x => x.Map<UserEnitity>(userCreateDto)).Returns(userEntity);
            // 设置用户存储库的行为：当调用AddAsync时，返回已完成的任务
            _mockUserRepository.Setup(x => x.AddAsync(userEntity)).Returns(Task.CompletedTask);
            // 设置缓存服务的行为：当调用RemoveAsync时，返回已完成的任务
            _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            // 设置映射器的行为：当映射User到UserDto时，返回expectedUserDto
            _mockMapper.Setup(x => x.Map<UserDto>(userEntity)).Returns(expectedUserDto);

            // Act - 执行被测方法
            var result = await _userService.CreateUserAsync(userCreateDto);

            // Assert - 验证结果是否符合预期
            Assert.Equal(expectedUserDto.Id, result.Id);
            Assert.Equal(expectedUserDto.Name, result.Name);
            Assert.Equal(expectedUserDto.Email, result.Email);
            // 验证映射方法被调用了一次
            _mockMapper.Verify(x => x.Map<UserEnitity>(userCreateDto), Times.Once);
            // 验证AddAsync方法被调用了一次
            _mockUserRepository.Verify(x => x.AddAsync(userEntity), Times.Once);
            // 验证缓存"all"键被移除了一次
            _mockCacheService.Verify(x => x.RemoveAsync("users:all"), Times.Once);
            // 验证映射方法被调用了一次
            _mockMapper.Verify(x => x.Map<UserDto>(userEntity), Times.Once);
        }

        /// <summary>
        /// 测试更新用户的方法
        /// 验证当提供有效数据时，服务应正确更新用户信息并清除相关缓存
        /// </summary>
        [Fact]
        public async Task UpdateUserAsync_WithValidData_UpdatesUser()
        {
            // Arrange - 准备测试数据和模拟行为
            var userId = 1;
            var userUpdateDto = new UserUpdateDto
            {
                Name = "Updated User",
                Email = "updated@example.com"
            };

            var existingUser = new UserEnitity
            {
                Id = userId,
                Name = "Old Name",
                Email = "old@example.com"
            };

            // 设置用户存储库的行为：当调用GetByIdAsync并传入userId时，返回existingUser
            _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            // 设置映射器的行为：当将userUpdateDto映射到existingUser时，更新existingUser的属性
            _mockMapper.Setup(x => x.Map(userUpdateDto, existingUser)).Callback(() =>
            {
                existingUser.Name = userUpdateDto.Name;
                existingUser.Email = userUpdateDto.Email;
            });
            // 设置用户存储库的行为：当调用UpdateAsync时，返回已完成的任务
            _mockUserRepository.Setup(x => x.UpdateAsync(existingUser)).Returns(Task.CompletedTask);
            // 设置缓存服务的行为：当调用RemoveAsync时，返回已完成的任务
            _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act - 执行被测方法
            await _userService.UpdateUserAsync(userId, userUpdateDto);

            // Assert - 验证用户信息是否正确更新
            Assert.Equal(userUpdateDto.Name, existingUser.Name);
            Assert.Equal(userUpdateDto.Email, existingUser.Email);
            // 验证GetByIdAsync方法被调用了一次
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
            // 验证Map方法被调用了一次
            _mockMapper.Verify(x => x.Map(userUpdateDto, existingUser), Times.Once);
            // 验证UpdateAsync方法被调用了一次
            _mockUserRepository.Verify(x => x.UpdateAsync(existingUser), Times.Once);
            // 验证用户特定缓存键被移除了一次
            _mockCacheService.Verify(x => x.RemoveAsync($"users:{userId}"), Times.Once);
            // 验证所有用户缓存键被移除了一次
            _mockCacheService.Verify(x => x.RemoveAsync("users:all"), Times.Once);
        }

        /// <summary>
        /// 测试使用无效ID更新用户的方法
        /// 验证当尝试更新不存在的用户时，服务应抛出NotFoundException
        /// </summary>
        [Fact]
        public async Task UpdateUserAsync_WithInvalidId_ThrowsNotFoundException()
        {
            // Arrange - 准备测试数据和模拟行为
            var userId = 999;
            var userUpdateDto = new UserUpdateDto
            {
                Name = "Updated User",
                Email = "updated@example.com"
            };

            // 设置用户存储库的行为：当调用GetByIdAsync并传入userId时，返回null
            _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((UserEnitity)null);

            // Act & Assert - 验证调用方法会抛出NotFoundException
            await Assert.ThrowsAsync<NotFoundException>(() => _userService.UpdateUserAsync(userId, userUpdateDto));
            // 验证GetByIdAsync方法被调用了一次
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        }

        /// <summary>
        /// 测试删除用户的方法
        /// 验证当提供有效ID时，服务应正确删除用户并清除相关缓存
        /// </summary>
        [Fact]
        public async Task DeleteUserAsync_WithValidId_DeletesUser()
        {
            // Arrange - 准备测试数据和模拟行为
            var userId = 1;
            var existingUser = new UserEnitity
            {
                Id = userId,
                Name = "User to Delete",
                Email = "delete@example.com"
            };

            // 设置用户存储库的行为：当调用GetByIdAsync并传入userId时，返回existingUser
            _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            // 设置用户存储库的行为：当调用DeleteAsync时，返回已完成的任务
            _mockUserRepository.Setup(x => x.DeleteAsync(existingUser)).Returns(Task.CompletedTask);
            // 设置缓存服务的行为：当调用RemoveAsync时，返回已完成的任务
            _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act - 执行被测方法
            await _userService.DeleteUserAsync(userId);

            // Assert - 验证方法调用次数
            // 验证GetByIdAsync方法被调用了一次
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
            // 验证DeleteAsync方法被调用了一次
            _mockUserRepository.Verify(x => x.DeleteAsync(existingUser), Times.Once);
            // 验证用户特定缓存键被移除了一次
            _mockCacheService.Verify(x => x.RemoveAsync($"users:{userId}"), Times.Once);
            // 验证所有用户缓存键被移除了一次
            _mockCacheService.Verify(x => x.RemoveAsync("users:all"), Times.Once);
        }

        /// <summary>
        /// 测试使用无效ID删除用户的方法
        /// 验证当尝试删除不存在的用户时，服务应抛出NotFoundException
        /// </summary>
        [Fact]
        public async Task DeleteUserAsync_WithInvalidId_ThrowsNotFoundException()
        {
            // Arrange - 准备测试数据和模拟行为
            var userId = 999;
            // 设置用户存储库的行为：当调用GetByIdAsync并传入userId时，返回null
            _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((UserEnitity)null);

            // Act & Assert - 验证调用方法会抛出NotFoundException
            await Assert.ThrowsAsync<NotFoundException>(() => _userService.DeleteUserAsync(userId));
            // 验证GetByIdAsync方法被调用了一次
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        }
    }
}