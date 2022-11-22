using Api.Models.Attach;
using Api.Models.User;
using Api.Services;
using AutoMapper;
using DAL.Entities;

namespace Api.Mapper.MapperActions
{
    public class PostContentMapperAction : IMappingAction<PostContent, AttachExternalModel>
    {
        private LinkGeneratorService _links;

        public PostContentMapperAction(LinkGeneratorService links)
        {
            _links = links;
        }

        public void Process(PostContent source, AttachExternalModel destinition, ResolutionContext context)
            => _links.FixContent(source, destinition);
    }
}
