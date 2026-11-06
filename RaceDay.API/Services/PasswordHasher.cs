using System.Security.Cryptography;

namespace RaceDay.API.Services
{
    /// <summary>
    /// Hashes and verifies passwords using PBKDF2-SHA256 with a random salt
    /// per user. Passwords are never stored or logged in their original form,
    /// satisfying the Part 2 requirement.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;      // 128-bit salt
        private const int HashSize = 32;      // 256-bit derived key
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
    }
}
