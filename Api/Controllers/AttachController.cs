using Api.Models.Attach;
using Api.Services;
using Common.Consts;
using Common.Extentions;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AttachController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly UserService _userService;

        public AttachController(PostService postService, UserService userService)
        {
            _postService = postService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<List<MetadataModel>> UploadFiles([FromForm] List<IFormFile> files)
        {
            var resourses = new List<MetadataModel>();
            foreach (var file in files)
            {
                resourses.Add(await UploadFile(file));
            }

            return resourses;
        }


        [HttpGet]
        [Route("{postContentId}")]
        public async Task<FileStreamResult> GetPostContent(Guid postContentId, bool download = false)
            => RenderAttach(await _postService.GetPostContent(postContentId), download);

        [HttpGet]
        [Route("{userId}")]
        public async Task<FileStreamResult> GetUserAvatarById(Guid userId, bool download = false)
            => RenderAttach(await _userService.GetUserAvatar(userId), download);

        [HttpGet]
        public async Task<FileStreamResult> GetCurentUserAvatar(bool download = false)
            => await GetUserAvatarById(User.GetClaimValue<Guid>(ClaimNames.Id), download);

        private async Task<MetadataModel> UploadFile(IFormFile file)
        {
            var tempPath = Path.GetTempPath();
            var meta = new MetadataModel()
            {
                TempId = Guid.NewGuid(),
                Name = file.FileName,
                MimeType = file.ContentType,
                Size = file.Length,
            };

            var newPath = Path.Combine(tempPath, meta.TempId.ToString());

            var fileInfo = new FileInfo(newPath);

            if (fileInfo.Exists)
            {
                throw new Exception("File exists");
            }
            else
            {
                if (fileInfo.Directory == null)
                {
                    throw new Exception("Directory is null");
                }
                else
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                using (var stream = System.IO.File.Create(newPath))
                {
                    await file.CopyToAsync(stream);
                }

                return meta;
            }
        }

        private  FileStreamResult RenderAttach(AttachModel attach, bool download = false)
        {
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            var ext = Path.GetExtension(attach.Name);
            if (download)
                return File(fs, attach.MimeType, $"{attach.Id}{ext}");
            else
                return File(fs, attach.MimeType);
        }
    }
}
