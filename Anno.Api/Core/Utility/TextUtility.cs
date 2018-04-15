using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Anno.Api.Core.Utility
{
    public static class TextUtility
    {
        public static string EncodeBase64(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string DecodeBase64(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string RandomAlphanumeric(int length)
        {
            //string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string chars = "ABCDEFGHKJLMNOPQRSTUVWXYZ123456789";  //Exclude I and 0 zero
            Random random = new Random();

            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}