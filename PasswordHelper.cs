using System.Text;
using System.Security.Cryptography;

namespace MARS_MELA
{
    public class PasswordHelper
    {

        public string ComputeSha512Hash(string rawPassword)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(rawPassword);
                byte[] hashBytes = sha512.ComputeHash(bytes);

                // Convert hash bytes to hex string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

    }
}
