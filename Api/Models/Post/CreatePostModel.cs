using Api.Models.User;
using DAL.Entities;

namespace Api.Models.Post
{
    public class CreatePostModel
    {
        public Guid PostId { get; set; }
        public string Description { get; set; } = null!;
        public DateTimeOffset CreatedDate { get; set; }

        public UserModel Author { get; set; } = null!;
        public  ICollection<PostContent> PostContent { get; set; } = null!;
    }
}
