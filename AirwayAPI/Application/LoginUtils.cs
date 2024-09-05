using System.Security.Cryptography;
using System.Text;

namespace AirwayAPI.Application
{
    public static class LoginUtils
    {
        private static readonly string EncryptionKey = "ASDASWEF453123234";

        // Add a static logger instance
        private static readonly ILogger Logger;

        // Static constructor to initialize the logger
        static LoginUtils()
        {
            // Create a LoggerFactory and get a logger instance
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Logger = loggerFactory.CreateLogger("LoginUtils");
        }

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
            try
            {
                // Validate if the input string is a valid Base-64 string
                if (string.IsNullOrWhiteSpace(password) || !IsBase64String(password))
                {
                    throw new ArgumentException("The provided password is not a valid Base-64 string.");
                }

                byte[] cipherBytes = Convert.FromBase64String(password);

                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new(EncryptionKey, new byte[] { 0x88, 0x76, 0x61, 0x6a, 0x99, 0x4c, 0x65, 0x64, 0x76, 0x82, 0x64, 0x65, 0x45 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);

                    using MemoryStream ms = new();
                    using (CryptoStream cs = new(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        // Use a try-catch block within the CryptoStream to handle padding issues
                        try
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        catch (CryptographicException cryptoEx)
                        {
                            Logger.LogError(cryptoEx, "Cryptographic error during password decryption: Incorrect padding or data corruption.");
                            throw new CryptographicException("The input data could not be decrypted. Ensure that the data is correct and properly padded.", cryptoEx);
                        }
                    }

                    password = Encoding.Unicode.GetString(ms.ToArray());
                }

                return password;
            }
            catch (CryptographicException cryptoEx)
            {
                Logger.LogError(cryptoEx, "Cryptographic error during password decryption. Check input data for integrity.");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred during password decryption.");
                throw;
            }
        }

        private static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new byte[base64.Length];
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}
