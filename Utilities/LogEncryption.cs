using System;
using System.IO;
using System.Security.Cryptography;

namespace MissionPlanner.Utilities
{
    public static class LogEncryption
    {
        private const string SettingKey = "encrypt_logs";
        private const string PasswordKey = "log_encryption_key";

        public static bool Enabled
        {
            get => Settings.Instance.GetBoolean(SettingKey, false);
            set => Settings.Instance[SettingKey] = value.ToString();
        }

        /// <summary>
        /// Wraps a file stream with AES encryption if enabled.
        /// Writes a 16-byte IV header, then encrypted data.
        /// </summary>
        public static Stream WrapForWriting(FileStream baseStream)
        {
            if (!Enabled) return baseStream;

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateIV();
            var key = GetOrCreateKey();

            // Write IV as first 16 bytes
            baseStream.Write(aes.IV, 0, 16);

            return new CryptoStream(baseStream,
                aes.CreateEncryptor(key, aes.IV),
                CryptoStreamMode.Write);
        }

        /// <summary>
        /// Opens an encrypted log for reading.
        /// Reads 16-byte IV header, then decrypts.
        /// </summary>
        public static Stream WrapForReading(FileStream baseStream)
        {
            var iv = new byte[16];
            baseStream.Read(iv, 0, 16);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            var key = GetOrCreateKey();

            return new CryptoStream(baseStream,
                aes.CreateDecryptor(key, iv),
                CryptoStreamMode.Read);
        }

        private static byte[] GetOrCreateKey()
        {
            var stored = Settings.Instance[PasswordKey];
            if (!string.IsNullOrEmpty(stored))
                return Convert.FromBase64String(stored);

            // Generate new random 256-bit key
            var key = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(key);

            Settings.Instance[PasswordKey] = Convert.ToBase64String(key);
            return key;
        }

        /// <summary>
        /// Set encryption key from user password
        /// </summary>
        public static void SetKeyFromPassword(string password)
        {
            using var sha = SHA256.Create();
            var key = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            Settings.Instance[PasswordKey] = Convert.ToBase64String(key);
        }
    }
}
