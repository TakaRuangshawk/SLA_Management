using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

namespace EncryptData.Net
{
    public class AesEncryption
    {
        private string encryptionKey { get; set; }
        private int keySize { get; set; } // 128, 192, or 256 bits

        public AesEncryption()
        {
            
            this.encryptionKey = ComputeSha256Hash("DataOne").Substring(0, 16);
            this.keySize = 256;
        }

        public string EncryptConnectionString(string plainText)
        {
            using (Aes rijAlg = Aes.Create())
            {
                rijAlg.KeySize = keySize;
                rijAlg.Key = Encoding.UTF8.GetBytes(encryptionKey);
                rijAlg.IV = Encoding.UTF8.GetBytes(encryptionKey.Substring(0, 16));

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    }

                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        public string DecryptConnectionString(string encryptedText)
        {
            using (Aes rijAlg = Aes.Create())
            {
                rijAlg.KeySize = keySize;
                rijAlg.Key = Encoding.UTF8.GetBytes(encryptionKey);
                rijAlg.IV = Encoding.UTF8.GetBytes(encryptionKey.Substring(0, 16));

                using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(encryptedText)))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijAlg.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cryptoStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
