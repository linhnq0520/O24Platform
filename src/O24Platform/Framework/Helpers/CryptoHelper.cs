namespace O24OpenAPI.Framework.Helpers
{
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Defines the <see cref="CryptoHelper" />
    /// </summary>
    public static class CryptoHelper
    {
        /// <summary>
        /// The Encrypt
        /// </summary>
        /// <param name="plainText">The plainText<see cref="string"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="iv">The iv<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string Encrypt(string plainText, string key, string iv)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = BuildKey(key, 32);
            aes.IV = BuildKey(iv, 16);

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// The Decrypt
        /// </summary>
        /// <param name="cipherText">The cipherText<see cref="string"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="iv">The iv<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string Decrypt(string cipherText, string key, string iv)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                return string.Empty;

            using var aes = Aes.Create();

            aes.Key = BuildKey(key, 32);
            aes.IV = BuildKey(iv, 16);

            using var decryptor = aes.CreateDecryptor();
            var buffer = Convert.FromBase64String(cipherText);

            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        /// <summary>
        /// Ensure key length (pad or trim)
        /// </summary>
        /// <param name="input">The input<see cref="string"/></param>
        /// <param name="length">The length<see cref="int"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        private static byte[] BuildKey(string input, int length)
        {
            var bytes = Encoding.UTF8.GetBytes(input);

            if (bytes.Length == length)
                return bytes;

            if (bytes.Length > length)
                return bytes.Take(length).ToArray();

            var result = new byte[length];
            Array.Copy(bytes, result, bytes.Length);

            return result;
        }
    }
}
