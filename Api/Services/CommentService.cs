using Api.Models.Comment;
using AutoMapper;
using DAL;
using DAL.Entities;

namespace Api.Services
{
    public class CommentService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;

        public CommentService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task CreateComment(CreatePostCommentRequest request)
        {
            var model = _mapper.Map<CreatePostCommentModel>(request);

            var dbModel = _mapper.Map<PostComment>(model);
            await _context.PostComment.AddAsync(dbModel);
            await _context.SaveChangesAsync();
        }
    }
}
