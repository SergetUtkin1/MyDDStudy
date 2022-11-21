using Api.Configs;
using Api.Models.Attach;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public class PostService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        private Func<PostContent, string?>? _linkContentGenerator;
        private Func<User, string?>? _linkAvatarGenerator;
        public void SetLinkGenerator(Func<PostContent, string?> linkContentGenerator, Func<User, string?> linkAvatarGenerator)
        {
            _linkAvatarGenerator = linkAvatarGenerator;
            _linkContentGenerator = linkContentGenerator;
        }
        public PostService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task CreatePost(CreatePostModel model)
        {
            var dbModel = _mapper.Map<Post>(model);

            await _context.Posts.AddAsync(dbModel);
            await _context.SaveChangesAsync();

        }

        public async Task<List<PostModel>> GetPosts(int skip, int take)
        {
            var posts = await _context.Posts
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostContent).AsNoTracking().Skip(skip).Take(take).ToListAsync();

            var res = posts.Select(post =>
                new PostModel
                {
                    Author = _mapper.Map<User, UserAvatarModel>(post.Author, o => o.AfterMap(FixAvatar)),
                    Description = post.Description,
                    Id = post.PostId,
                    Contents = post.PostContent?.Select(x =>
                    _mapper.Map<PostContent, AttachExternalModel>(x, o => o.AfterMap(FixContent))).ToList()
                }).ToList();


            return res;
        }

        private void FixContent(PostContent s, AttachExternalModel d)
            => d.ContentLink = _linkContentGenerator?.Invoke(s);

        private void FixAvatar(User s, UserAvatarModel d)
            => d.AvatarLink = s.Avatar == null ? null : _linkAvatarGenerator?.Invoke(s);

        public async Task<AttachModel> GetPostContent(Guid postContentId)
        {
            var res = await _context.PostContent.FirstOrDefaultAsync(x => x.Id == postContentId);

            return _mapper.Map<AttachModel>(res);
        }
     }
}