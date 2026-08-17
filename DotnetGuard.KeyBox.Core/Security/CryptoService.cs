using System;
using System.Security.Cryptography;
using System.Text;

namespace DotnetGuard.KeyBox.Core.Security
{
    public static class CryptoService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int KeySize = 32;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int Iterations = 210000;

        public static byte[] GenerateSalt()
        {
            return RandomNumberGenerator.GetBytes(SaltSize);
        }

        public static byte[] DeriveHash(string masterPassword, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(masterPassword),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);
        }

        public static bool VerifyMasterPassword(string masterPassword, byte[] salt, byte[] storedHash)
        {
            byte[] computedHash = DeriveHash(masterPassword, salt);
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }

        public static byte[] DeriveKey(string masterPassword, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(masterPassword),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);
        }

        public static void Encrypt(string plainText, byte[] key, out byte[] cipherText, out byte[] nonce, out byte[] tag)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            nonce = RandomNumberGenerator.GetBytes(NonceSize);
            cipherText = new byte[plainBytes.Length];
            tag = new byte[TagSize];

            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Encrypt(nonce, plainBytes, cipherText, tag);
            }
        }

        public static string Decrypt(byte[] cipherText, byte[] nonce, byte[] tag, byte[] key)
        {
            byte[] plainBytes = new byte[cipherText.Length];

            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Decrypt(nonce, cipherText, tag, plainBytes);
            }

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
