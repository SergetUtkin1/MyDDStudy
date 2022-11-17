using Api.Models.Attach;
using Api.Models.User;
using Api.Services;
using AutoMapper;
using Common.Consts;
using Common.Extentions;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
            if (userService != null)
                _userService.SetLinkGenerator(x =>
                Url.Action(nameof(GetUserAvatarById), new { userId = x.Id, download = false }));
        }

        [HttpPost]
        [Authorize]
        public async Task AddAvatarToUser(MetadataModel model)
        {
            string? userId = User.Claims.FirstOrDefault(u => u.Type == "id")?.Value;
            if (Guid.TryParse(userId, out var id))
            {
                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));
                if (!tempFi.Exists)
                    throw new Exception("file not found");
                else
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "attaches", model.TempId.ToString());
                    var destFi = new FileInfo(path);
                    if (destFi.Directory != null && !destFi.Directory.Exists)
                        destFi.Directory.Create();

                    System.IO.File.Copy(tempFi.FullName, path, true);

                    await _userService.AddAvatarToUser(id, model, path);
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<FileStreamResult> GetUserAvatarById(Guid userId, bool download = false)
        {
            var attach = await _userService.GetUserAvatar(userId);
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            if (download)
                return File(fs, attach.MimeType, attach.Name);
            else
                return File(fs, attach.MimeType);

        }

        [HttpGet]
        public async Task<FileResult> DownloadAvatarById(Guid userId)
        {
            var attach = await _userService.GetUserAvatar(userId);

            HttpContext.Response.ContentType = attach!.MimeType;
            FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(attach.FilePath), attach.MimeType)
            {
                FileDownloadName = attach.Name
            };

            return result;
        }

        [HttpGet]
        [Authorize]
        public async Task<UserModel> GetCurrentUser()
        {
            var id = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (id != default)
            {
                return await _userService.GetUser(id);
            }

            throw new Exception("U are not authorized");
        }

        [HttpPost]
        public async Task CreateUser(CreateUserModel model)
        {
            if (await _userService.CheckUserExist(model.Email))
                throw new Exception("user is exist");
            await _userService.CreateUser(model);

        }

        [HttpGet]
        [Authorize]
        public async Task<List<UserModel>> GetUsers()
            => await _userService.GetUsers();

        [HttpGet]
        public async Task<FileStreamResult> GetCurentUserAvatar(bool download = false)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await GetUserAvatarById(userId, download);
            }
            else
                throw new Exception("you are not authorized");

        }
    }
}
