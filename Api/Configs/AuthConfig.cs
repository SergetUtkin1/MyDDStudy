using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api.Configs
{
    public class AuthConfig
    {
        public const string Position = "auth";
        public string Issuer { get; set; } = String.Empty;
        public string Audence { get; set; } = String.Empty;
        public string Key { get; set; } = String.Empty;
        public int LifeTime { get; set; }
        public SymmetricSecurityKey GetSymmetricSecurityKey()
            => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    }
}
