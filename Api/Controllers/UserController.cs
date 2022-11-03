using Api.Models;
using Api.Services;
using AutoMapper;
using DAL;
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
            => await _userService.CreateUser(model);

        [HttpGet]
        [Authorize]
        public async Task<List<UserModel>> GetUsers()
            => await _userService.GetUsers();
    }
}
