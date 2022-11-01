namespace Api.Models
{
    public class TokenModel
    {
        public string Jwt { get; set; }

        public TokenModel(string jwt)
        {
            Jwt = jwt;
        }
    }
}
