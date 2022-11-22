using Api.Models.User;
using Api.Services;
using AutoMapper;
using DAL.Entities;
using System.Diagnostics;

namespace Api.Mapper.MapperActions
{
    public class UserAvatarMapperAction : IMappingAction<User, UserAvatarModel>
    {
        private LinkGeneratorService _links;

        public UserAvatarMapperAction(LinkGeneratorService links)
        {
            _links = links;
        }

        public void Process(User source, UserAvatarModel destinition, ResolutionContext context)
            => _links.FixAvatar(source, destinition);
    }
}
