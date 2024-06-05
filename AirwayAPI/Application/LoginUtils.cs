using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AirwayAPI.Application
{
    public static class LoginUtils
    {
        private static string EncryptionKey = "ASDASWEF453123234";
        public static string encryptPassword(string password)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(password);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new(EncryptionKey, new byte[] { 0x88, 0x76, 0x61, 0x6a, 0x99, 0x4c, 0x65, 0x64, 0x76, 0x82, 0x64, 0x65, 0x45 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new();
                using (CryptoStream cs = new(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                password = Convert.ToBase64String(ms.ToArray());
            }
            return password;
        }

        public static string decryptPassword(string password)
        {
            byte[] cipherBytes = Convert.FromBase64String(password);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new(EncryptionKey, new byte[] { 0x88, 0x76, 0x61, 0x6a, 0x99, 0x4c, 0x65, 0x64, 0x76, 0x82, 0x64, 0x65, 0x45 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new();
                using (CryptoStream cs = new(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                password = Encoding.Unicode.GetString(ms.ToArray());
            }
            return password;
        }
    }
}
