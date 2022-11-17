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
        private Func<UserModel, string?>? _linkGenerator;

        public UserService(DataContext context, IMapper mapper, IOptions<AuthConfig> options)
        {
            _context = context;
            _mapper = mapper;
        }

        public void SetLinkGenerator(Func<UserModel, string?> linkGenerator)
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

    }
}
