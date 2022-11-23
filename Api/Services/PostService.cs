using Api.Configs;
using Api.Models.Attach;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using Common.Consts;
using Common.Extentions;
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

        public PostService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task CreatePost(CreatePostRequest request)
        {
            var model = _mapper.Map<CreatePostModel>(request);

            model.Contents.ForEach(x =>
            {
                x.AuthorId = model.AuthorId;
                x.FilePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "attaches",
                    x.TempId.ToString());

                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), x.TempId.ToString()));
                if (tempFi.Exists)
                {
                    var destFi = new FileInfo(x.FilePath);
                    if (destFi.Directory != null && !destFi.Directory.Exists)
                        destFi.Directory.Create();

                    File.Move(tempFi.FullName, x.FilePath, true);
                }
            });




            var dbModel = _mapper.Map<Post>(model);
            await _context.Posts.AddAsync(dbModel);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PostModel>> GetPosts(int skip, int take)
        {
            var posts = await _context.Posts
                .Include(x => x.PostLikes!).ThenInclude(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostComments!).ThenInclude(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostContent).AsNoTracking().Skip(skip).Take(take)
                .Select(x => _mapper.Map<PostModel>(x))
                .ToListAsync();


            return posts;
        }

        public async Task<PostModel> GetPostById(Guid id)
        {
            var post = await _context.Posts
                  .Include(x => x.Author).ThenInclude(x => x.Avatar)
                  .Include(x => x.PostContent).AsNoTracking()
                  .Where(x => x.PostId == id)
                  .Select(x => _mapper.Map<PostModel>(x))
                  .FirstOrDefaultAsync();
            if (post == null)
                throw new Exception("Post not Found");

            return post;
        }

        public async Task<AttachModel> GetPostContent(Guid postContentId)
        {
            var res = await _context.PostContent.FirstOrDefaultAsync(x => x.Id == postContentId);

            return _mapper.Map<AttachModel>(res);
        }
     }
}