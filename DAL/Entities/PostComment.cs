using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class PostComment : Comment
    {
        public virtual Post PostOwner { get; set; } = null!;
        public Guid PostOwnerId { get; set; }
    }
}
