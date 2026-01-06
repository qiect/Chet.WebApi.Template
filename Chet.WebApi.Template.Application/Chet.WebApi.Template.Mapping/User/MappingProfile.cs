using AutoMapper;
using Chet.WebApi.Template.Domain;
using Chet.WebApi.Template.DTOs;

namespace Chet.WebApi.Template.Mapping;

/// <summary>
/// AutoMapper配置类，用于定义实体和DTO之间的映射关系
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// 初始化映射配置，定义所有实体和DTO之间的映射规则
    /// </summary>
    public MappingProfile()
    {
        // 用户实体到用户DTO的映射
        CreateMap<User, UserDto>();

        // 用户创建DTO到用户实体的映射
        CreateMap<UserCreateDto, User>();

        // 用户更新DTO到用户实体的映射
        CreateMap<UserUpdateDto, User>();

        // 注册DTO到用户实体的映射
        CreateMap<RegisterDto, User>();
    }
}
