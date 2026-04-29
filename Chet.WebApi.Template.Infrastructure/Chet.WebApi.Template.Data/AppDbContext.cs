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
        public DbSet<UserEntity> Users { get; set; }

        /// <summary>
        /// 配置实体映射和关系
        /// </summary>
        /// <param name="modelBuilder">模型构建器</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 从当前程序集自动应用所有实现了 IEntityTypeConfiguration<T> 接口的配置类
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
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
