using Api.Configs;
using Api.Models.Attach;
using Api.Models.User;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace Api.Services
{
    public class UserService
    {
        private readonly DAL.DataContext _context;
        private readonly IMapper _mapper;
        private Func<User, string?>? _linkGenerator;

        public UserService(DataContext context, IMapper mapper, IOptions<AuthConfig> options)
        {
            _context = context;
            _mapper = mapper;
        }

        public void SetLinkGenerator(Func<User, string?> linkGenerator)
        {
            _linkGenerator = linkGenerator;
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

        public async Task<UserAvatarModel> GetUser(Guid id)
            => _mapper.Map<User, UserAvatarModel>(await GetUserById(id), o => o.AfterMap(FixAvatar));

        public async Task<IEnumerable<UserAvatarModel>> GetUsers() 
            => (await _context.Users.AsNoTracking().Include(x => x.Avatar).ToListAsync())
                .Select(x => _mapper.Map<User, UserAvatarModel>(x, o => o.AfterMap(FixAvatar)));


        private void FixAvatar(User s, UserAvatarModel d)
            => d.AvatarLink = s.Avatar == null ? null : _linkGenerator?.Invoke(s);
        public async Task<AttachModel> GetUserAvatar(Guid id)
        {
            var user = await GetUserById(id);
            var attach = _mapper.Map<AttachModel>(user.Avatar);

            return attach;
        }

        public async Task<DAL.Entities.User> GetUserById(Guid id)
        {
            var dbUser = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(p => p.Id == id);
            if (dbUser != null && dbUser != default)
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

    }
}
