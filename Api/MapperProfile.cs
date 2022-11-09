using Api.Models.Attach;
using Api.Models.User;
using AutoMapper;
using Common;
using DAL.Entities;

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
            CreateMap<Avatar, MetadataModel>();
        }
    }
}
