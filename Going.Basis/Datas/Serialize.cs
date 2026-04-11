using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Drawing;

namespace Going.Basis.Datas
{
    /// <summary>
    /// XML, 바이너리(구조체), JSON 직렬화/역직렬화 유틸리티 클래스.
    /// </summary>
    public class Serialize
    {
        #region Xml
        /// <summary>객체를 XML로 직렬화하여 파일에 저장한다.</summary>
        /// <param name="Path">저장할 파일 경로</param>
        /// <param name="obj">직렬화할 객체</param>
        /// <param name="type">객체 타입</param>
        public static void XmlSerializeToFile(string Path, object obj, Type type)
        {
            XmlSerializer xmlser = new XmlSerializer(type);
            XmlWriterSettings set = new XmlWriterSettings();
            set.Indent = true;
            XmlWriter writer = XmlWriter.Create(Path, set);
            xmlser.Serialize(writer, obj);
            writer.Close();
        }

        /// <summary>XML 파일을 역직렬화하여 객체로 반환한다.</summary>
        /// <param name="Path">XML 파일 경로</param>
        /// <param name="type">역직렬화할 타입</param>
        /// <returns>역직렬화된 객체. 파일이 없으면 null</returns>
        public static object? XmlDeserializeFromFile(string Path, Type type)
        {
            if (!File.Exists(Path)) return null;

            using var fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fs);
            var xmlser = new XmlSerializer(type);
            return xmlser.Deserialize(reader);
        }

        /// <summary>객체를 XML로 직렬화하여 바이트 배열로 반환한다.</summary>
        /// <param name="obj">직렬화할 객체</param>
        /// <param name="type">객체 타입</param>
        /// <returns>XML 데이터의 바이트 배열</returns>
        public static byte[] XmlSerialize(object obj, Type type)
        {
            XmlSerializer xmlser = new XmlSerializer(type);
            StringBuilder resultXml = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(resultXml);
            xmlser.Serialize(writer, obj);
            byte[] data = Encoding.Default.GetBytes(resultXml.ToString());
            return data;
        }

        /// <summary>바이트 배열의 XML 데이터를 역직렬화하여 객체로 반환한다.</summary>
        /// <param name="rawdata">XML 데이터 바이트 배열</param>
        /// <param name="type">역직렬화할 타입</param>
        /// <returns>역직렬화된 객체</returns>
        public static object? XmlDeserialize(byte[] rawdata, Type type)
        {
            MemoryStream ms = new MemoryStream(rawdata);
            TextReader reader = new StreamReader(ms, Encoding.Default);
            XmlSerializer xmlser = new XmlSerializer(type);
            object? obj = xmlser.Deserialize(reader);
            reader.Close();
            ms.Close();
            return obj;
        }
        #endregion

        #region Struct
        /// <summary>구조체를 바이트 배열로 직렬화한다 (Marshal 사용).</summary>
        /// <param name="obj">직렬화할 구조체</param>
        /// <returns>구조체의 바이트 배열 표현</returns>
        public static byte[] RawSerialize(object obj)
        {
            int rawSize = Marshal.SizeOf(obj);
            byte[] rawData = new byte[rawSize];
            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            IntPtr buf = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(obj, buf, false);
            handle.Free();
            return rawData;
        }

        /// <summary>바이트 배열을 구조체로 역직렬화한다 (Marshal 사용).</summary>
        /// <param name="rawdata">구조체 데이터의 바이트 배열</param>
        /// <param name="type">역직렬화할 구조체 타입</param>
        /// <returns>역직렬화된 구조체. 데이터 크기가 부족하면 null</returns>
        public static object? RawDeserialize(byte[] rawdata, Type type)
        {
            int rawSize = Marshal.SizeOf(type);
            if (rawSize > rawdata.Length) return null;
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            IntPtr buf = handle.AddrOfPinnedObject();
            object? retobj = Marshal.PtrToStructure(buf, type);
            handle.Free();
            return retobj;
        }

        /// <summary>바이트 배열의 지정 구간을 구조체로 역직렬화한다.</summary>
        /// <param name="ba">원본 바이트 배열</param>
        /// <param name="offset">시작 오프셋</param>
        /// <param name="size">읽을 바이트 수</param>
        /// <param name="type">역직렬화할 구조체 타입</param>
        /// <returns>역직렬화된 구조체. 데이터 크기가 부족하면 null</returns>
        public static object? RawDeserialize(byte[] ba, int offset, int size, Type type)
        {
            var rawdata = new byte[size];
            Array.Copy(ba, offset, rawdata, 0, size);

            int rawSize = Marshal.SizeOf(type);
            if (rawSize > rawdata.Length) return null;
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            IntPtr buf = handle.AddrOfPinnedObject();
            object? retobj = Marshal.PtrToStructure(buf, type);
            handle.Free();
            return retobj;
        }

        /// <summary>바이트 배열을 제네릭 구조체로 역직렬화한다.</summary>
        /// <typeparam name="T">역직렬화할 구조체 타입</typeparam>
        /// <param name="rawdata">구조체 데이터의 바이트 배열</param>
        /// <returns>역직렬화된 구조체. 데이터 크기가 부족하면 default</returns>
        public static T RawDeserialize<T>(byte[] rawdata) where T : struct
        {
            int rawSize = Marshal.SizeOf(typeof(T));
            if (rawSize > rawdata.Length) return default(T);
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            IntPtr buf = handle.AddrOfPinnedObject();
            T retobj = (T)Marshal.PtrToStructure(buf, typeof(T))!;
            handle.Free();
            return retobj;
        }

        /// <summary>바이트 배열의 지정 구간을 제네릭 구조체로 역직렬화한다.</summary>
        /// <typeparam name="T">역직렬화할 구조체 타입</typeparam>
        /// <param name="ba">원본 바이트 배열</param>
        /// <param name="offset">시작 오프셋</param>
        /// <param name="size">읽을 바이트 수</param>
        /// <returns>역직렬화된 구조체. 데이터 크기가 부족하면 default</returns>
        public static T RawDeserialize<T>(byte[] ba, int offset, int size)
        {
            var rawdata = new byte[size];
            Array.Copy(ba, offset, rawdata, 0, size);

            int rawSize = Marshal.SizeOf(typeof(T));
            if (rawSize > rawdata.Length) return default(T);
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            IntPtr buf = handle.AddrOfPinnedObject();
            T retobj = (T)Marshal.PtrToStructure(buf, typeof(T))!;
            handle.Free();
            return retobj;
        }

        #endregion

        #region Json
        /// <summary>객체를 JSON 문자열로 직렬화한다.</summary>
        /// <param name="obj">직렬화할 객체</param>
        /// <param name="options">JSON 직렬화 옵션 (선택)</param>
        /// <returns>JSON 문자열</returns>
        public static string JsonSerialize(object obj, JsonSerializerOptions? options = null) => JsonSerializer.Serialize(obj, options);
        /// <summary>제네릭 객체를 JSON 문자열로 직렬화한다.</summary>
        /// <typeparam name="T">직렬화할 객체 타입</typeparam>
        /// <param name="obj">직렬화할 객체</param>
        /// <param name="options">JSON 직렬화 옵션 (선택)</param>
        /// <returns>JSON 문자열</returns>
        public static string JsonSerialize<T>(T obj, JsonSerializerOptions? options = null) => JsonSerializer.Serialize(obj, options);
        /// <summary>JSON 문자열을 역직렬화하여 객체로 반환한다.</summary>
        /// <typeparam name="T">역직렬화할 타입</typeparam>
        /// <param name="json">JSON 문자열</param>
        /// <param name="options">JSON 직렬화 옵션 (선택)</param>
        /// <returns>역직렬화된 객체. 실패 시 default</returns>
        public static T? JsonDeserialize<T>(string? json, JsonSerializerOptions? options = null)
        {
            if (json != null)
            {
                try { return JsonSerializer.Deserialize<T>(json, options); }
                catch (Exception) { return default; }
            }
            else return default;
        }

        /// <summary>객체를 JSON으로 직렬화하여 파일에 저장한다.</summary>
        /// <param name="path">저장할 파일 경로</param>
        /// <param name="obj">직렬화할 객체</param>
        /// <param name="options">JSON 직렬화 옵션 (선택)</param>
        public static void JsonSerializeToFile(string path, object obj, JsonSerializerOptions? options = null) => File.WriteAllText(path, JsonSerialize(obj, options));
        /// <summary>제네릭 객체를 JSON으로 직렬화하여 파일에 저장한다.</summary>
        /// <typeparam name="T">직렬화할 객체 타입</typeparam>
        /// <param name="path">저장할 파일 경로</param>
        /// <param name="obj">직렬화할 객체</param>
        /// <param name="options">JSON 직렬화 옵션 (선택)</param>
        public static void JsonSerializeToFile<T>(string path, T obj, JsonSerializerOptions? options = null) => File.WriteAllText(path, JsonSerialize(obj, options));
        /// <summary>JSON 파일을 역직렬화하여 객체로 반환한다.</summary>
        /// <typeparam name="T">역직렬화할 타입</typeparam>
        /// <param name="Path">JSON 파일 경로</param>
        /// <param name="options">JSON 직렬화 옵션 (선택)</param>
        /// <returns>역직렬화된 객체. 파일이 없으면 default</returns>
        public static T? JsonDeserializeFromFile<T>(string Path, JsonSerializerOptions? options = null)
        {
            if (File.Exists(Path)) return JsonDeserialize<T>(File.ReadAllText(Path), options);
            else return default;
        }
        #endregion
    }
}
