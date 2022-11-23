using Api.Models.Comment;
using Api.Models.Like;
using AutoMapper;
using DAL;
using DAL.Entities;

namespace Api.Services
{
    public class LikeService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;

        public LikeService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task CreatePostLike(CreatePostLikeRequest request)
        {
            var model = _mapper.Map<CreatePostLikeModel>(request);

            var dbModel = _mapper.Map<PostLike>(model);
            await _context.PostLikes.AddAsync(dbModel);
            await _context.SaveChangesAsync();
        }
    }
}
