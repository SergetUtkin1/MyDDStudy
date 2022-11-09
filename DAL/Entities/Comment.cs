using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;
        public virtual Avatar? Avatar { get; set; }
        public string Text { get; set; } = null!;
        public DateTimeOffset CreatedDate { get; set; }

        public virtual Post PostOwner { get; set; } = null!;
        public Guid PostOwnerId { get; set; }
    }
}
