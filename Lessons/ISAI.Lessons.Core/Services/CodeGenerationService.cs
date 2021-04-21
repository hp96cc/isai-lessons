using System.Security.Cryptography;
using System.Text;

namespace Scf.Core.Services
{
    public static class CodeGenerationService
    {
        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = "ABCDEFGHJKLMNOPQRSTUVWXYZ234567890".ToCharArray();
            byte[] data = new byte[1];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();

        }
    }
}
