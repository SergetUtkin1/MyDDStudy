using Api.Models.Attach;
using Api.Models.User;
using Api.Services;
using AutoMapper;
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
        public async Task<FileResult> GetUserAvatarById(Guid id)
        {
            var avatar = await _userService.GetUserAvatar(id);

            return File(System.IO.File.ReadAllBytes(avatar!.FilePath), avatar.MimeType);
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
            string? userId = User.Claims.FirstOrDefault(u => u.Type == "id")?.Value;
            if (Guid.TryParse(userId, out var id))
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
    }
}
