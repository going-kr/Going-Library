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
    public class Seriealize
    {
        #region Xml
        public static void XmlSerializeToFile(string Path, object obj, Type type)
        {
            XmlSerializer xmlser = new XmlSerializer(type);
            XmlWriterSettings set = new XmlWriterSettings();
            set.Indent = true;
            XmlWriter writer = XmlWriter.Create(Path, set);
            xmlser.Serialize(writer, obj);
            writer.Close();
        }

        public static object XmlDeserializeFromFile(string Path, Type type)
        {
            object ret = null;
            if (File.Exists(Path))
            {
                using (FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    byte[] data = new byte[fs.Length];
                    int nlen = fs.Read(data, 0, data.Length);

                    System.IO.MemoryStream ms = new System.IO.MemoryStream(data);
                    System.IO.TextReader reader = new System.IO.StreamReader(ms);
                    XmlSerializer xmlser = new XmlSerializer(type);
                    ret = xmlser.Deserialize(reader);
                    reader.Close();
                    ms.Close();
                }
            }
            return ret;
        }

        public static byte[] XmlSerialize(object obj, Type type)
        {
            XmlSerializer xmlser = new XmlSerializer(type);
            StringBuilder resultXml = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(resultXml);
            xmlser.Serialize(writer, obj);
            byte[] data = Encoding.Default.GetBytes(resultXml.ToString());
            return data;
        }

        public static object XmlDeserialize(byte[] rawdata, Type type)
        {
            MemoryStream ms = new MemoryStream(rawdata);
            TextReader reader = new StreamReader(ms, Encoding.Default);
            XmlSerializer xmlser = new XmlSerializer(type);
            object obj = xmlser.Deserialize(reader);
            reader.Close();
            ms.Close();
            return obj;
        }
        #endregion

        #region Struct
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

        public static object RawDeserialize(byte[] rawdata, Type type)
        {
            int rawSize = Marshal.SizeOf(type);
            if (rawSize > rawdata.Length) return null;
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            IntPtr buf = handle.AddrOfPinnedObject();
            object retobj = Marshal.PtrToStructure(buf, type);
            handle.Free();
            return retobj;
        }

        public static object RawDeserialize(byte[] ba, int offset, int size, Type type)
        {
            var rawdata = new byte[size];
            Array.Copy(ba, offset, rawdata, 0, size);

            int rawSize = Marshal.SizeOf(type);
            if (rawSize > rawdata.Length) return null;
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            IntPtr buf = handle.AddrOfPinnedObject();
            object retobj = Marshal.PtrToStructure(buf, type);
            handle.Free();
            return retobj;
        }

        public static T RawDeserialize<T>(byte[] rawdata) where T : struct
        {
            int rawSize = Marshal.SizeOf(typeof(T));
            if (rawSize > rawdata.Length) return default(T);
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            IntPtr buf = handle.AddrOfPinnedObject();
            T retobj = (T)Marshal.PtrToStructure(buf, typeof(T));
            handle.Free();
            return retobj;
        }

        public static T RawDeserialize<T>(byte[] ba, int offset, int size)
        {
            var rawdata = new byte[size];
            Array.Copy(ba, offset, rawdata, 0, size);

            int rawSize = Marshal.SizeOf(typeof(T));
            if (rawSize > rawdata.Length) return default(T);
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            IntPtr buf = handle.AddrOfPinnedObject();
            T retobj = (T)Marshal.PtrToStructure(buf, typeof(T));
            handle.Free();
            return retobj;
        }

        #endregion

        #region Json
        public static string JsonSerialize(object obj, JsonSerializerOptions? options = null) => JsonSerializer.Serialize(obj, options);
        public static string JsonSerialize<T>(T obj, JsonSerializerOptions? options = null) => JsonSerializer.Serialize(obj, options);
        public static T? JsonDeserialize<T>(string? json, JsonSerializerOptions? options = null)
        {
            if (json != null)
            {
                try { return JsonSerializer.Deserialize<T>(json, options); }
                catch (Exception) { return default; }
            }
            else return default;
        }

        public static void JsonSerializeToFile(string path, object obj, JsonSerializerOptions? options = null) => File.WriteAllText(path, JsonSerialize(obj, options));
        public static void JsonSerializeToFile<T>(string path, T obj, JsonSerializerOptions? options = null) => File.WriteAllText(path, JsonSerialize(obj, options));
        public static T? JsonDeserializeFromFile<T>(string Path, JsonSerializerOptions? options = null)
        {
            if (File.Exists(Path)) return JsonDeserialize<T>(File.ReadAllText(Path), options);
            else return default;
        }
        #endregion
    }
}
