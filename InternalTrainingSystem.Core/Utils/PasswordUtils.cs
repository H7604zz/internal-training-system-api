using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace InternalTrainingSystem.Core.Utils
{
    public class PasswordUtils
    {
        //Tạo mật khẩu ngẫu nhiên
        public static string Generate(PasswordOptions opts)
        {
            int length = Math.Max(12, opts.RequiredLength);

            string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            string lower = "abcdefghijkmnopqrstuvwxyz";
            string digits = "0123456789";
            string nonAlnum = "!@$?_-";
            string all = upper + lower + digits + nonAlnum;

            string Take(string src)
            {
                byte[] b = new byte[1];
                RandomNumberGenerator.Fill(b);
                return src[b[0] % src.Length].ToString();
            }

            var sb = new StringBuilder();

            if (opts.RequireUppercase) sb.Append(Take(upper));
            if (opts.RequireLowercase) sb.Append(Take(lower));
            if (opts.RequireDigit) sb.Append(Take(digits));
            if (opts.RequireNonAlphanumeric) sb.Append(Take(nonAlnum));

            while (sb.Length < length) sb.Append(Take(all));
            return sb.ToString();
        }
    }
}
