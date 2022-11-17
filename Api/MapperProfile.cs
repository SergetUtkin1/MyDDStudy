using Api.Models.Attach;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using Common;
using DAL.Entities;
using System;

namespace Api
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, DAL.Entities.User>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime))
                ;
            CreateMap<DAL.Entities.User, UserModel>();

            CreateMap<Avatar, AttachModel>();

            //CreateMap<User, UserAvatarModel>()
            //                .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDay))
            //                .ForMember(d => d.PostsCount, m => m.MapFrom(s => s.Posts!.Count))
            //                .AfterMap<UserAvatarMapperAction>();

            CreateMap<PostContent, AttachModel>();

            CreateMap<MetadataModel, PostContent>();
            CreateMap<MetaWithPath, PostContent>();

            CreateMap<CreatePostModel, Post>()
                .ForMember(d => d.CreatedDate, m => m.MapFrom(s => DateTime.UtcNow))
                .ForMember(d => d.PostContent, m => m.MapFrom(d => d.Contents))
                ;
        }
    }
}
