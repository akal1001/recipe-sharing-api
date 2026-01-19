using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EncryptionHelper
{
    public class EncryptionHelper
    {

        private const string key = "Fmn//nGFz234s8vsfJ2WUw==";
        public static string Encrypt(string plainText)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32, '0').Substring(0, 32));
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV();
            var iv = aes.IV;
            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            byte[] result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
            return Convert.ToBase64String(result);
        }

        public static string DecryptAES(string encryptedData, string key)
        {
            try
            {
                // Convert the key and the encrypted data to bytes
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] cipherBytes = Convert.FromBase64String(encryptedData); // Ensure it's Base64-encoded

                // Ensure the key length is appropriate for AES (128, 192, or 256 bits)
                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes.Length >= aes.KeySize / 8
                              ? keyBytes.Take(aes.KeySize / 8).ToArray()
                              : keyBytes.Concat(new byte[aes.KeySize / 8 - keyBytes.Length]).ToArray(); // Padding key if it's too short

                    aes.Mode = CipherMode.ECB; // ECB mode
                    aes.Padding = PaddingMode.PKCS7; // PKCS7 padding

                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return Encoding.UTF8.GetString(plainBytes); // Return the decrypted data
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while decrypting: " + ex.Message); // More detailed error message
            }
        }

        public static string Decrypt(string cipherText)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32, '0').Substring(0, 32));
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] encrypted = new byte[cipherBytes.Length - iv.Length];
            Buffer.BlockCopy(cipherBytes, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(cipherBytes, iv.Length, encrypted, 0, encrypted.Length);
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        public static string encriptPassword(string password, string salt)
        {

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password + salt,
                salt: System.Text.Encoding.ASCII.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return hashed;
        }

        public static byte[] generateSalt()
        {

            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;


        }
    }
}

