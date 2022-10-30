using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class HashHelper
    {
        public static string GetHash(string input)
        {
            using (var sha = SHA256.Create())
            {
                var data = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static bool Verify(string input, string hash)
        {
            var inputHash = GetHash(input);
            var comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(inputHash, hash) == 0;
        }
    }
}
