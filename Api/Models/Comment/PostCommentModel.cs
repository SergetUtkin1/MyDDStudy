using Api.Models.User;
using DAL.Entities;

namespace Api.Models.Comment
{
    public class PostCommentModel
    {
        public Guid Id { get; set; }
        public UserAvatarModel Author { get; set; } = null!;
        public string Text { get; set; } = null!;
        public DateTimeOffset CreatedDate { get; set; }
    }
}
