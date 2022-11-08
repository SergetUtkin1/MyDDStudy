using Api.Configs;
using Api.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using DAL.Entities;
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

        public async Task<bool> CheckUserExist(string email)
        {

            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());

        }

        public async Task CreateUser(CreateUserModel model)
        {
            var DBuser = _mapper.Map<DAL.Entities.User>(model);
            await _context.AddAsync(DBuser);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var dbUser = await GetUserById(id);
            if (dbUser != null)
            {
                _context.Users.Remove(dbUser);
                await _context.SaveChangesAsync();
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

        public async Task<Avatar?> GetUserAvatar(Guid id)
        {
            var userDb = await GetUserById(id);
            if(userDb.Avatar == null)
            {
                throw new Exception("User doesn't have avatar");
                // вообще наверное тут бы attach я бы присвоил костыль в виде аватара-заглушки, но не буду
            }

            var attach =  await _context.Avatars.FirstOrDefaultAsync(x => x.Id == userDb.Avatar.Id);
            if(attach == null)
            {
                throw new Exception("This avatar doesn't exist");
            }

            return attach;
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

        public async Task<DAL.Entities.User> GetUserById(Guid id)
        {
            var dbUser = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(p => p.Id == id);
            if (dbUser != null)
            {
                return dbUser ;
            }
            else
            {
                throw new Exception("user not found");
            }
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel meta, string filePath)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                var avatar = new Avatar { Author = user, 
                    MimeType = meta.MimeType, 
                    FilePath = filePath, 
                    Name = meta.Name, 
                    Size = meta.Size 
                };
                user.Avatar = avatar;

                await _context.SaveChangesAsync();
            }

        }

        public async Task<DAL.Entities.UserSession> GetSessionById(Guid id)
        {
            var dbSession = await _context.UserSessions.FirstOrDefaultAsync(x => x.Id == id);
            if (dbSession != null)
            {
                return dbSession;
            }
            else
            {
                throw new Exception("session is not found");
            }
        }

        private async Task<DAL.Entities.UserSession> GetSessionByRefreshToken(Guid refreshToken)
        {
            var dbSession = await _context.UserSessions.Include(x => x.User).FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
            if (dbSession != null)
            {
                return dbSession;
            }
            else
            {
                throw new Exception("session is not found");
            }
        }

        private TokenModel GenerateTokens(DAL.Entities.UserSession userSession)
        {
            DateTime dtNow = DateTime.Now;

            if (userSession.User == null)
            {
                throw new Exception("It's a kind of magic");
            }

            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: dtNow,
                expires: DateTime.Now.AddMinutes(_config.LifeTime),
                claims: new Claim[] {
                new Claim(ClaimsIdentity.DefaultNameClaimType,  userSession.User.Name),
                new Claim("id", userSession.User.Id.ToString()),
                new Claim("sessionId", userSession.Id.ToString()),
            },
                signingCredentials: new SigningCredentials(_config.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refresh = new JwtSecurityToken(
                notBefore: dtNow,
                expires: DateTime.Now.AddHours(_config.LifeTime),
                claims: new Claim[] {
                new Claim(ClaimsIdentity.DefaultNameClaimType,  userSession.User.Name),
                new Claim("refreshToken", userSession.RefreshToken.ToString()),
                },
                signingCredentials: new SigningCredentials(_config.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);

            return new TokenModel(encodedJwt, encodedRefresh);
        }

        public async Task<TokenModel> GetToken(string login, string password)
        {
            var userDb = await GetUserByCredentions(login, password);
            var sessionDb = await _context.UserSessions.AddAsync(new DAL.Entities.UserSession()
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                RefreshToken = Guid.NewGuid(),
                User = userDb,
                
            });
            await _context.SaveChangesAsync();

            return GenerateTokens(sessionDb.Entity);
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

            if (claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "refreshToken")?.Value is String refreshIdString
                && Guid.TryParse(refreshIdString, out var refreshId))
            {
                var session = await GetSessionByRefreshToken(refreshId);
                if (!session.IsActive)
                {
                    throw new Exception("session is not active");
                }

                var user = session.User;

                session.RefreshToken = Guid.NewGuid();
                await _context.SaveChangesAsync();

                return GenerateTokens(session);
            }
            else
            {
                throw new SecurityTokenException("invalid token");
            }
        }
    }
}
