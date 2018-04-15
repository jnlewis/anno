using System;
using System.Security.Cryptography;
using System.Text;

namespace Anno.Api.Core.Utility
{
    public static class HashUtility
    {
        public static string SHA256(string text)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] message = Encoding.UTF8.GetBytes(text);
            byte[] hashValue = algorithm.ComputeHash(message);

            return ConvertBytesToHex(hashValue);
        }

        public static string GenerateHash()
        {
            return HashString(Guid.NewGuid().ToString());
        }

        #region Private Methods

        private static string ConvertBytesToHex(byte[] value)
        {
            return BitConverter.ToString(value).Replace("-", "");
        }

        private static byte[] ConvertHexToBytes(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }
        
        private static string HashString(string text)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);

            char[] hash2 = new char[16];

            // Note that here we are wasting bits of hash! 
            // But it isn't really important, because hash.Length == 32
            for (int i = 0; i < hash2.Length; i++)
            {
                hash2[i] = chars[hash[i] % chars.Length];
            }

            return new string(hash2);
        }

        #endregion

    }
}