namespace ChatWebServer
{
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;
    using System;
    using System.Security.Cryptography;

    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            Console.WriteLine($"Hashed: {hashed}");

            return hashed;
        }

    }

}
