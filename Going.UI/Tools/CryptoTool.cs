using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Tools
{
    public class CryptoTool
    {
        #region Base64String
        #region Encode
        public static string? EncodeBase64String(string data, Encoding? enc = null) => Convert.ToBase64String((enc ?? Encoding.UTF8).GetBytes(data));
        public static string? EncodeBase64String(byte[] data) => Convert.ToBase64String(data);
        public static string? EncodeBase64String(SKBitmap data)
        {
            string? ret = null;
            if (data != null)
            {
                using (SKImage image = SKImage.FromBitmap(data))
                using (SKData v = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    ret = Convert.ToBase64String(v.ToArray());
                }
            }
            return ret;
        }
        public static string? EncodeBase64String(SKImage image)
        {
            string? ret = null;
            if (image != null)
            {
                using (SKData v = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    ret = Convert.ToBase64String(v.ToArray());
                }
            }
            return ret;
        }
        #endregion
        #region Decode
        public static T? DecodeBase64String<T>(string data, Encoding? enc = null)
        {
            T? ret = default;
            if (typeof(T) == typeof(string)) ret = (T)(object)(enc ?? Encoding.UTF8).GetString(Convert.FromBase64String(data));
            else if (typeof(T) == typeof(byte[])) ret = (T)(object)Convert.FromBase64String(data);
            else if (typeof(T) == typeof(SKBitmap)) ret = (T)(object)SKBitmap.Decode(Convert.FromBase64String(data));
            else if (typeof(T) == typeof(SKImage)) ret = (T)(object)SKImage.FromEncodedData(Convert.FromBase64String(data));
            return ret;
        }
        #endregion
        #endregion

        #region AES256
        public static string EncryptAES256(string input, string key, Encoding? enc = null)
        {
            enc ??= Encoding.UTF8;
            byte[] keyBytes = GetKeyBytes256(key);
            byte[] iv = GenerateIV();

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length); // IV를 앞부분에 저장
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs, enc))
                    {
                        sw.Write(input);
                    }
                    return Convert.ToBase64String(ms.ToArray()); // Base64 인코딩
                }
            }
        }

        public static string DecryptAES256(string input, string key, Encoding? enc = null)
        {
            enc ??= Encoding.UTF8;
            byte[] keyBytes = GetKeyBytes256(key);
            byte[] encryptedBytes = Convert.FromBase64String(input);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = new byte[16]; // IV 저장 공간
                Array.Copy(encryptedBytes, aes.IV, 16); // 저장된 IV 복사
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream(encryptedBytes, 16, encryptedBytes.Length - 16))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs, enc))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private static byte[] GetKeyBytes256(string key)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
        }

        #endregion

        #region AES128
        public static string EncryptAES128(string input, string key, Encoding? enc = null)
        {
            enc ??= Encoding.UTF8;
            byte[] keyBytes = GetKeyBytes128(key);
            byte[] iv = GenerateIV();

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length); // IV를 앞부분에 저장
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs, enc))
                    {
                        sw.Write(input);
                    }
                    return Convert.ToBase64String(ms.ToArray()); // Base64 인코딩
                }
            }
        }

        public static string DecryptAES128(string input, string key, Encoding? enc = null)
        {
            enc ??= Encoding.UTF8;
            byte[] keyBytes = GetKeyBytes128(key);
            byte[] encryptedBytes = Convert.FromBase64String(input);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = new byte[16]; // IV 저장 공간
                Array.Copy(encryptedBytes, aes.IV, 16); // 저장된 IV 복사
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream(encryptedBytes, 16, encryptedBytes.Length - 16))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs, enc))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private static byte[] GetKeyBytes128(string key)
        {
            using (MD5 md5 = MD5.Create()) // AES-128은 16바이트 키 필요 → MD5 해시 사용
            {
                return md5.ComputeHash(Encoding.UTF8.GetBytes(key)); // 16바이트 키 생성
            }
        }

        private static byte[] GenerateIV()
        {
            byte[] iv = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }
        #endregion
    }
}
