using Api.Configs;
using Api.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Services
{
    public class UserService
    {
        private readonly DAL.DataContext _context;
        private readonly IMapper _mapper;
        private readonly AuthConfig _config;

        public UserService(DataContext context, IMapper mapper, IOptions<AuthConfig> options)
        {
            _context = context;
            _mapper = mapper;
            _config = options.Value;
        }

        public async Task CreateUser(CreateUserModel model)
        {
            var DBuser = _mapper.Map<DAL.Entities.User>(model);
            await _context.AddAsync(DBuser);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserModel>> GetUsers()
        {
            return await _context.Users.AsNoTracking().ProjectTo<UserModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        private async Task<DAL.Entities.User> GetUserByCredentions(string login, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower().Trim() == login);

            if (user == null)
                throw new Exception("user not found");

            if (!HashHelper.Verify(password, user.PasswordHash))
                throw new Exception("password is not correct");

            return user;
        }

        public async Task<TokenModel> GetToken(string login, string password)
        {
            var user = await GetUserByCredentions(login, password);

            var myClaims = new Claim[]
            {
                new Claim("displayName", user.Name),
                new Claim("Id", user.Id.ToString()),
            };

            var dtNow = DateTime.Now;

            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: dtNow,
                expires: dtNow.AddMinutes(_config.LifeTime),
                claims: myClaims,
                signingCredentials: new SigningCredentials(_config.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new TokenModel(encodedJwt);
        }
    }
}
