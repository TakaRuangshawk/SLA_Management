using System.Security.Cryptography;
using System.Text;

namespace SLA_Management.Data
{
    public class DecryptConfig
    {
        public DecryptConfig() { }

        public string EnsureKeySize(string key)
        {
            // AES supports key sizes of 128, 192, or 256 bits (16, 24, or 32 bytes)
            if (key.Length < 16)
            {
                key = key.PadRight(16, '\0');
            }
            else if (key.Length < 24)
            {
                key = key.PadRight(24, '\0');
            }
            else if (key.Length < 32)
            {
                key = key.PadRight(32, '\0');
            }
            else if (key.Length > 32)
            {
                key = key.Substring(0, 32);
            }

            return key;
        }


        public string DecryptString(string cipherText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256; // Set the key size explicitly
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16]; // Use the same IV used during encryption

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
