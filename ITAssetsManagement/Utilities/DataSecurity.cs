using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace ITAssetsManagement.Utility
{
    public static class DataSecurity
    {
        private static readonly string Key = "&%#@?,:*";
        public static string Encrypt(string plainText)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(Key.Substring(0, 8));
            byte[] iv = { 18, 52, 86, 120, 144, 171, 205, 239 };
            using var des = DES.Create();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(
                ms,
                des.CreateEncryptor(keyBytes, iv),
                CryptoStreamMode.Write);
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            cs.Write(inputBytes, 0, inputBytes.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }
        public static string Decrypt(string cipherText)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(Key.Substring(0, 8));
            byte[] iv = { 18, 52, 86, 120, 144, 171, 205, 239 };
            using var des = DES.Create();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(
                ms,
                des.CreateDecryptor(keyBytes, iv),
                CryptoStreamMode.Write);
            byte[] inputBytes = Convert.FromBase64String(cipherText);
            cs.Write(inputBytes, 0, inputBytes.Length);
            cs.FlushFinalBlock();
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
