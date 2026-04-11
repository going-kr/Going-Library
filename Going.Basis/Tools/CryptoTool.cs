using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Tools
{
    /// <summary>
    /// 암호화/인코딩 유틸리티 클래스.
    /// Base64 인코딩/디코딩, AES-256/AES-128 암호화/복호화(CBC 모드, PKCS7 패딩)를 제공한다.
    /// </summary>
    public class CryptoTool
    {
        #region Base64String
        #region Encode
        /// <summary>문자열을 Base64로 인코딩한다.</summary>
        /// <param name="data">인코딩할 문자열</param>
        /// <param name="enc">문자 인코딩 (기본값: UTF-8)</param>
        /// <returns>Base64 인코딩된 문자열</returns>
        public static string? EncodeBase64String(string data, Encoding? enc = null) => Convert.ToBase64String((enc ?? Encoding.UTF8).GetBytes(data));
        /// <summary>바이트 배열을 Base64로 인코딩한다.</summary>
        /// <param name="data">인코딩할 바이트 배열</param>
        /// <returns>Base64 인코딩된 문자열</returns>
        public static string? EncodeBase64String(byte[] data) => Convert.ToBase64String(data);
        #endregion
        #region Decode
        /// <summary>Base64 문자열을 디코딩한다.</summary>
        /// <typeparam name="T">반환 타입 (string 또는 byte[])</typeparam>
        /// <param name="data">Base64 인코딩된 문자열</param>
        /// <param name="enc">문자 인코딩 (기본값: UTF-8). T가 string일 때 사용</param>
        /// <returns>디코딩된 결과</returns>
        public static T? DecodeBase64String<T>(string data, Encoding? enc = null)
        {
            T? ret = default;
            if (typeof(T) == typeof(string)) ret = (T)(object)(enc ?? Encoding.UTF8).GetString(Convert.FromBase64String(data));
            else if (typeof(T) == typeof(byte[])) ret = (T)(object)Convert.FromBase64String(data);
            return ret;
        }
        #endregion
        #endregion

        #region AES256
        /// <summary>AES-256(CBC, PKCS7) 방식으로 문자열을 암호화한다. IV는 결과 앞에 포함된다.</summary>
        /// <param name="input">암호화할 평문</param>
        /// <param name="key">암호화 키 (SHA-256 해시로 256비트 키 생성)</param>
        /// <param name="enc">문자 인코딩 (기본값: UTF-8)</param>
        /// <returns>Base64 인코딩된 암호문 (IV 포함)</returns>
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

        /// <summary>AES-256(CBC, PKCS7) 방식으로 암호문을 복호화한다. 입력 앞 16바이트를 IV로 사용한다.</summary>
        /// <param name="input">Base64 인코딩된 암호문 (IV 포함)</param>
        /// <param name="key">복호화 키 (SHA-256 해시로 256비트 키 생성)</param>
        /// <param name="enc">문자 인코딩 (기본값: UTF-8)</param>
        /// <returns>복호화된 평문</returns>
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
        /// <summary>AES-128(CBC, PKCS7) 방식으로 문자열을 암호화한다. IV는 결과 앞에 포함된다.</summary>
        /// <param name="input">암호화할 평문</param>
        /// <param name="key">암호화 키 (MD5 해시로 128비트 키 생성)</param>
        /// <param name="enc">문자 인코딩 (기본값: UTF-8)</param>
        /// <returns>Base64 인코딩된 암호문 (IV 포함)</returns>
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

        /// <summary>AES-128(CBC, PKCS7) 방식으로 암호문을 복호화한다. 입력 앞 16바이트를 IV로 사용한다.</summary>
        /// <param name="input">Base64 인코딩된 암호문 (IV 포함)</param>
        /// <param name="key">복호화 키 (MD5 해시로 128비트 키 생성)</param>
        /// <param name="enc">문자 인코딩 (기본값: UTF-8)</param>
        /// <returns>복호화된 평문</returns>
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
