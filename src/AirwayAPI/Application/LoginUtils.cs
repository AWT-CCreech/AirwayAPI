using System.Security.Cryptography;
using System.Text;
namespace AirwayAPI.Application;

public static class LoginUtils
{
    // It's highly recommended to retrieve the EncryptionKey from a secure configuration source
    private static readonly string EncryptionKey = "ASDASWEF453123234"; // Replace with secure retrieval
    // Encryption key is provided at runtime via Program.cs
    private static string? _encryptionKey;

    public static void SetEncryptionKey(string key)
        => _encryptionKey = key;

    // Define constants for PBKDF2
    private const int Iterations = 100_000; // Recommended minimum
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
    // Define salt size and key size
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32;  // 256 bits
    // Static logger instance
    private static readonly ILogger Logger;
    // Static constructor to initialize the logger
    static LoginUtils()
    {
        // Initialize a LoggerFactory and create a logger instance
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            // Add other logging providers if needed
        });
        Logger = loggerFactory.CreateLogger("LoginUtils");
    }
    /// <summary>
    /// Encrypts the given password using AES encryption.
    /// </summary>
    /// <param name="password">The plain text password to encrypt.</param>
    /// <returns>The encrypted password as a Base-64 encoded string, including the salt.</returns>
    public static string EncryptPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }
        byte[] clearBytes = Encoding.UTF8.GetBytes(password);
        using Aes encryptor = Aes.Create();
        // Generate a unique salt for each encryption
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Derive Key and IV from the EncryptionKey and salt
        using (var pdb = new Rfc2898DeriveBytes(EncryptionKey, salt, Iterations, HashAlgorithm))
            if (string.IsNullOrWhiteSpace(_encryptionKey))
                throw new InvalidOperationException("Encryption key is not configured.");

        // Derive Key and IV from the configured key and salt
        using (var pdb = new Rfc2898DeriveBytes(_encryptionKey, salt, Iterations, HashAlgorithm))
        {
            encryptor.Key = pdb.GetBytes(KeySize);    // 256-bit key for AES-256
            encryptor.IV = pdb.GetBytes(16);         // 128-bit IV for AES
        }
        using MemoryStream ms = new();
        using (CryptoStream cs = new(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.FlushFinalBlock();
        }
        byte[] encryptedBytes = ms.ToArray();
        // Combine salt and encrypted bytes for storage
        byte[] result = new byte[salt.Length + encryptedBytes.Length];
        Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, salt.Length, encryptedBytes.Length);
        string encryptedPassword = Convert.ToBase64String(result);
        return encryptedPassword;
    }
    /// <summary>
    /// Decrypts the given encrypted password.
    /// </summary>
    /// <param name="encryptedPassword">The encrypted password as a Base-64 encoded string, including the salt.</param>
    /// <returns>The decrypted plain text password.</returns>
    public static string DecryptPassword(string encryptedPassword)
    {
        try
        {
            // Validate if the input string is a valid Base-64 string
            if (string.IsNullOrWhiteSpace(encryptedPassword) || !IsBase64String(encryptedPassword))
            {
                throw new ArgumentException("The provided password is not a valid Base-64 string.", nameof(encryptedPassword));
            }
            byte[] cipherBytesWithSalt = Convert.FromBase64String(encryptedPassword);
            if (cipherBytesWithSalt.Length < SaltSize)
            {
                throw new ArgumentException("The encrypted password is invalid or corrupted.", nameof(encryptedPassword));
            }
            // Extract salt from the beginning of the cipher bytes
            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(cipherBytesWithSalt, 0, salt, 0, salt.Length);
            // Extract the actual cipher bytes
            int cipherTextLength = cipherBytesWithSalt.Length - SaltSize;
            byte[] cipherBytes = new byte[cipherTextLength];
            Buffer.BlockCopy(cipherBytesWithSalt, SaltSize, cipherBytes, 0, cipherTextLength);

            using Aes encryptor = Aes.Create();
            // Derive Key and IV from the EncryptionKey and extracted salt
            using (var pdb = new Rfc2898DeriveBytes(EncryptionKey, salt, Iterations, HashAlgorithm))
                if (string.IsNullOrWhiteSpace(_encryptionKey))
                    throw new InvalidOperationException("Encryption key is not configured.");

            // Derive Key and IV from the configured key and extracted salt
            using (var pdb = new Rfc2898DeriveBytes(_encryptionKey, salt, Iterations, HashAlgorithm))
            {
                encryptor.Key = pdb.GetBytes(KeySize);    // 256-bit key for AES-256
                encryptor.IV = pdb.GetBytes(16);         // 128-bit IV for AES
            }
            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
            {
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
            string decryptedPassword = Encoding.UTF8.GetString(ms.ToArray());
            return decryptedPassword;
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
    /// <summary>
    /// Validates whether a given string is a valid Base-64 encoded string.
    /// </summary>
    /// <param name="base64">The string to validate.</param>
    /// <returns>True if valid Base-64; otherwise, false.</returns>
    private static bool IsBase64String(string base64)
    {
        Span<byte> buffer = new byte[base64.Length];
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}