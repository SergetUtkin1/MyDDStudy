using Api.Models.User;

namespace Api.Models.Like
{
    public class PostLikeModel
    {
        public Guid Id { get; set; }
        public UserAvatarModel Author { get; set; } = null!;
    }
}
