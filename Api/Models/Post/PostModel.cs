using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Like;
using Api.Models.User;

namespace Api.Models.Post
{
    public class PostModel
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public UserAvatarModel Author { get; set; } = null!;
        public List<AttachExternalModel>? Contents { get; set; } = new List<AttachExternalModel>();//1 11
        public List<PostCommentModel>? PostComments { get; set; } = new List<PostCommentModel>();
        public List<PostLikeModel>? PostLikes { get; set; } = new List<PostLikeModel>();
        public bool IsLiked { get; set; } = false;
        public int LikeCount { get; set; }
    }
}
