using Api.Configs;
using Api.Models.Attach;
using Api.Models.Token;
using Api.Models.User;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using Common.Consts;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Services
{
    public class AuthService
    {
        private readonly DAL.DataContext _context;
        private readonly IMapper _mapper;
        private readonly AuthConfig _config;

        public AuthService(DataContext context, IMapper mapper, IOptions<AuthConfig> options)
        {
            _context = context;
            _mapper = mapper;
            _config = options.Value;
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
                new Claim(ClaimNames.Id, userSession.User.Id.ToString()),
                new Claim(ClaimNames.SessionId, userSession.Id.ToString()),
            },
                signingCredentials: new SigningCredentials(_config.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refresh = new JwtSecurityToken(
                notBefore: dtNow,
                expires: DateTime.Now.AddHours(_config.LifeTime),
                claims: new Claim[] {
                new Claim(ClaimsIdentity.DefaultNameClaimType,  userSession.User.Name),
                new Claim(ClaimNames.RefreshToken, userSession.RefreshToken.ToString()),
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
