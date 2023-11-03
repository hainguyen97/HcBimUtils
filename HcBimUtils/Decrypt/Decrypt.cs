using System.Security.Cryptography;
using System.Text;

namespace HcBimUtils.Decrypt
{
    public static class EncryptUtils
    {
        private const string Key = "HC_Tool";

        /// <summary>
        /// Giản mã
        /// </summary>
        /// <param name="toDecrypt">Chuỗi đã mã hóa</param>
        /// <returns>Chuỗi giản mã</returns>
        public static string Decrypt(string toDecrypt)
        {
            byte[] keyArray;
            var toEncryptArray = Convert.FromBase64String(toDecrypt);

            {
                var hashMd5 = new MD5CryptoServiceProvider();
                keyArray = hashMd5.ComputeHash(Encoding.UTF8.GetBytes(Key));
            }

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = tdes.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        /// Mã hóa chuỗi có mật khẩu
        /// </summary>
        /// <param name="toEncrypt">Chuỗi cần mã hóa</param>
        /// <returns>Chuỗi đã mã hóa</returns>
        public static string Encrypt(string toEncrypt)
        {
            byte[] keyArray;
            var toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

            {
                var hashMd5 = new MD5CryptoServiceProvider();
                keyArray = hashMd5.ComputeHash(Encoding.UTF8.GetBytes(Key));
            }

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = tdes.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
    }
}