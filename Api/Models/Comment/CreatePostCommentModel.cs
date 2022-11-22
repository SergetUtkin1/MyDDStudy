namespace Api.Models.Comment
{
    public class CreatePostCommentModel
    {
        public Guid PostOwnerId { get; set; }
        public Guid Id { get; set; }
        public string? Text { get; set; }
        public Guid AuthorId { get; set; }
    }

    public class CreatePostCommentRequest
    {
        public Guid? PostOwnerId { get; set; }
        public Guid? AuthorId { get; set; }
        public string? Text { get; set; }
    }
}
