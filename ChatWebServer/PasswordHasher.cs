using System.Security.Cryptography;
namespace ChatWebServer
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            if (String.IsNullOrEmpty(password)) return String.Empty;

            using (HashAlgorithm algorithm = SHA256.Create())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = algorithm.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}
