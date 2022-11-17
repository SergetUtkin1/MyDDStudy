using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Post
    {
        public Guid PostId { get; set; }
        public string Description { get; set; } = null!;
        public DateTimeOffset CreatedDate { get; set; }

        public Guid AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;
        public virtual ICollection<PostContent> PostContent { get; set; } = null!;
        public virtual ICollection<Comment>? Comments { get; set; }
    }
}
