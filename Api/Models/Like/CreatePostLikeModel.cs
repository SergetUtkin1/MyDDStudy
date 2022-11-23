namespace Api.Models.Like
{
    public class CreatePostLikeModel
    {
        public Guid Id { get; set; }
        public Guid? AuthorId { get; set; }
        public Guid PostOwnerId { get; set; }
    }

    public class CreatePostLikeRequest
    {
        public Guid? AuthorId { get; set; }
        public Guid PostOwnerId { get; set; }
    }
}
