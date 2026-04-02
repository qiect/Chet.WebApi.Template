using Chet.WebApi.Template.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chet.WebApi.Template.Data.User
{
    /// <summary>
    /// 用户配置
    /// </summary>
    public class UserConfig : IEntityTypeConfiguration<UserEnitity>
    {
        public void Configure(EntityTypeBuilder<UserEnitity> builder)
        {
            builder.HasKey(e => e.Id); // 设置主键
            builder.ToTable("Users");
            builder.ToTable(e => e.HasComment("用户信息表"));

            builder.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("Name")
                    .HasComment("用户名");

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("Email")
                .HasComment("用户邮箱，用于登录和通知");

            builder.Property(e => e.PasswordHash)
                .IsRequired()
                .HasColumnName("PasswordHash")
                .HasComment("密码哈希值，使用 BCrypt 算法生成");

            builder.Property(e => e.RefreshToken)
                .HasMaxLength(500)
                .HasColumnName("RefreshToken")
                .HasComment("刷新令牌，用于获取新的访问令牌");

            builder.Property(e => e.RefreshTokenExpiryTime)
                .HasColumnName("RefreshTokenExpiryTime")
                .HasComment("刷新令牌过期时间");

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt")
                .HasComment("创建时间");

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnName("UpdatedAt")
                .HasComment("更新时间");

            builder.HasIndex(e => e.Email).IsUnique().HasDatabaseName("IX_Users_Email");
        }
    }
}
