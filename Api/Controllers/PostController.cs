using Api.Models.Attach;
using Api.Models.Post;
using Api.Services;
using Common.Consts;
using Common.Extentions;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]

    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        public PostController(PostService postService, LinkGeneratorService links)
        {
            _postService = postService;

            links.LinkAvatarGenerator = x =>
                Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatarById), new
                {
                    userId = x.Id,
                });

            links.LinkContentGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetPostContent), new
                {
                    postContentId = x.Id,
                }); 
        }

        [HttpGet]
        public async Task<List<PostModel>> GetPosts(int skip = 0, int take = 10)
            => await _postService.GetPosts(skip, take);

        [HttpPost]
        public async Task CreatePost(CreatePostRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("not authorize");

            var model = new CreatePostModel
            {
                AuthorId = userId,
                
                Description = request.Description,
                Contents = request.Contents.Select(x =>
                new MetadataLinkModel(x, q => Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "attaches",
                    q.TempId.ToString()), userId)).ToList()
            };

            model.Contents.ForEach(x =>
            {
                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), x.TempId.ToString()));
                if (tempFi.Exists)
                {
                    var destFi = new FileInfo(x.FilePath);
                    if (destFi.Directory != null && !destFi.Directory.Exists)
                        destFi.Directory.Create();

                    System.IO.File.Copy(tempFi.FullName, x.FilePath, true);
                    tempFi.Delete();
                }

            });

            await _postService.CreatePost(model);

        }
    }
}
