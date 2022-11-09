using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string PasswordHash { get; set; } = String.Empty;
        public DateTimeOffset BirthDate { get; set; }

        public virtual Avatar? Avatar { get; set; }
        public virtual ICollection<UserSession>? Sessions { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
    }
}
