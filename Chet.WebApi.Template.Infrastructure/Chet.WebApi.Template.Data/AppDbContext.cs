using Chet.WebApi.Template.Domain;
using Chet.WebApi.Template.Domain.User;
using Microsoft.EntityFrameworkCore;

namespace Chet.WebApi.Template.Data
{
    /// <summary>
    /// EF Core 数据库上下文类，用于管理实体和数据库交互
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">数据库上下文配置选项</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// 表示数据库中的 Users 表
        /// </summary>
        public DbSet<UserEnitity> Users { get; set; }

        /// <summary>
        /// 配置实体映射和关系
        /// </summary>
        /// <param name="modelBuilder">模型构建器</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置用户实体
            modelBuilder.Entity<UserEnitity>(entity =>
            {
                entity.HasKey(e => e.Id); // 设置主键
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255); // 配置 Email 属性
                entity.HasIndex(e => e.Email).IsUnique(); // 为 Email 添加唯一索引
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100); // 配置 Name 属性
                entity.Property(e => e.PasswordHash).IsRequired(); // 配置 PasswordHash 属性
                entity.Property(e => e.CreatedAt).IsRequired(); // 配置 CreatedAt 属性
                entity.Property(e => e.UpdatedAt).IsRequired(); // 配置 UpdatedAt 属性
            });
        }

        /// <summary>
        /// 重写基类方法，用于自动设置实体的创建和更新时间
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>影响的行数</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 自动设置创建和更新时间
            var entities = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entities)
            {
                var entity = (BaseEntity)entityEntry.Entity;
                entity.UpdatedAt = DateTime.Now; // 设置更新时间为当前 北京 时间

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.Now; // 新建实体时，设置创建时间为当前 北京 时间
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
