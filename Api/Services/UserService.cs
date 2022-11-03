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

        public async Task<DAL.Entities.User> GetUserById(Guid id)
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(p => p.Id == id);
            if (dbUser != null)
            {
                return dbUser ;
            }
            else
            {
                throw new Exception("user not found");
            }
        }

        public async Task<UserModel> GetUser(Guid id)
        {
            var dbUser = await GetUserById(id);

            return _mapper.Map<UserModel>(dbUser);
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

        private TokenModel GenerateTokens(DAL.Entities.User user)
        {
            DateTime dtNow = DateTime.Now;
            var myClaims = new Claim[] {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                new Claim("id", user.Id.ToString()),
            };

            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: dtNow,
                expires: dtNow.AddMinutes(_config.LifeTime),
                claims: new Claim[] {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                new Claim("id", user.Id.ToString()),
            },
                signingCredentials: new SigningCredentials(_config.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refresh = new JwtSecurityToken(
                notBefore: dtNow,
                expires: dtNow.AddHours(_config.LifeTime),
                claims: new Claim[] {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                new Claim("id", user.Id.ToString()),
                },
                signingCredentials: new SigningCredentials(_config.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);

            return new TokenModel(encodedJwt, encodedRefresh);
        }

        public async Task<TokenModel> GetToken(string login, string password)
        {
            var userDb = await GetUserByCredentions(login, password);

            return GenerateTokens(userDb);
        }

        public async Task<TokenModel> GetTokenByRefreshToken(string refreshToken)
        {
            var validParams = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = _config.GetSymmetricSecurityKey()
            };
            var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken
                || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("invalid token");
            }

            if (claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "id")?.Value is String userIdString
                && Guid.TryParse(userIdString, out var userId))
            {
                var user = await GetUserById(userId);
                return GenerateTokens(user);
            }
            else
            {
                throw new SecurityTokenException("invalid token");
            }
        }
    }
}
