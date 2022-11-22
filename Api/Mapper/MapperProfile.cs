using Api.Mapper.MapperActions;
using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using Common;
using DAL.Entities;
using System;

namespace Api.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, User>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime))
                ;
            CreateMap<User, UserModel>();
            CreateMap<User, UserAvatarModel>()
                .ForMember(d => d.PostsCount, m => m.MapFrom(s => s.Posts!.Count))
                .AfterMap<UserAvatarMapperAction>();

            CreateMap<Avatar, AttachModel>();

            CreateMap<Post, PostModel>()
                .ForMember(d => d.Id, m => m.MapFrom(s => s.PostId))
                .ForMember(d => d.Contents, m => m.MapFrom(s => s.PostContent));
            CreateMap<PostContent, AttachExternalModel>().AfterMap<PostContentMapperAction>();
            CreateMap<AttachExternalModel, PostContent>();

            CreateMap<CreatePostRequest, CreatePostModel>();
            CreateMap<MetadataModel, MetadataLinkModel>();
            CreateMap<MetadataLinkModel, PostContent>();

            CreateMap<CreatePostModel, Post>()
                .ForMember(d => d.PostId, m => m.MapFrom(d => d.Id))
                .ForMember(d => d.CreatedDate, m => m.MapFrom(s => DateTime.UtcNow))
                .ForMember(d => d.PostContent, m => m.MapFrom(d => d.Contents))
                ;

            CreateMap<CreatePostCommentRequest, CreatePostCommentModel>();
            CreateMap<CreatePostCommentModel, PostComment>()
                .ForMember(d => d.CreatedDate, m => m.MapFrom(s => DateTime.UtcNow));
            CreateMap<PostComment, PostCommentModel>();
        }
    }
}
