using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Buffers.Text;
using Going.UI.ImageCanvas;
using Going.UI.Design;
using System.Reflection;

namespace Going.UI.Json
{
    /// <summary>
    /// Going.UI 컨트롤의 JSON 직렬화/역직렬화를 위한 변환기 설정 클래스입니다.
    /// </summary>
    public class GoJsonConverter
    {
        /// <summary>
        /// JSON 직렬화/역직렬화에 사용되는 옵션을 가져옵니다.
        /// </summary>
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions();
        /// <summary>
        /// 등록된 컨트롤 타입 이름과 타입 매핑 사전을 가져옵니다.
        /// </summary>
        public static Dictionary<string, Type> ControlTypes { get; } = [];

        // [GoNumericAlias] 마킹된 1-arity generic IGoControl 자동 등록용 numeric 변형 목록 (소문자 alias + PascalCase alias)
        private static readonly (string lower, string pascal, Type argType)[] _numericVariants =
        [
            ("byte",    "Byte",    typeof(byte)),
            ("sbyte",   "SByte",   typeof(sbyte)),
            ("short",   "Int16",   typeof(short)),
            ("ushort",  "UInt16",  typeof(ushort)),
            ("int",     "Int32",   typeof(int)),
            ("uint",    "UInt32",  typeof(uint)),
            ("long",    "Int64",   typeof(long)),
            ("ulong",   "UInt64",  typeof(ulong)),
            ("float",   "Single",  typeof(float)),
            ("double",  "Double",  typeof(double)),
            ("decimal", "Decimal", typeof(decimal)),
        ];

        static GoJsonConverter()
        {
            Options.PropertyNameCaseInsensitive = true;
            Options.WriteIndented = true;
            Options.Converters.Add(new SKColorConverter());
            Options.Converters.Add(new SKRectConverter());
            Options.Converters.Add(new SKBitmapConverter());
            Options.Converters.Add(new SKImageConverter());
            Options.Converters.Add(new GoControlConverter());
            Options.Converters.Add(new GoPagesConverter());
            Options.Converters.Add(new GoWindowsConverter());
            Options.Converters.Add(new GoTableLayoutControlCollectionConverter());
            Options.Converters.Add(new GoGridLayoutControlCollectionConverter());

            try
            {
                var tps = typeof(GoControl).Assembly.GetTypes();

                foreach(var v in tps.Where(x => x.GetInterface("IGoControl") != null))
                {
                    if(v.IsGenericTypeDefinition && v.GetGenericArguments().Length == 1
                        && v.IsDefined(typeof(GoNumericAliasAttribute), inherit: false))
                    {
                        // [GoNumericAlias] 마킹된 1-arity generic IGoControl: numeric × 11종 자동 등록
                        var nameBase = v.Name.Substring(0, v.Name.IndexOf('`'));
                        foreach (var (lower, pascal, argType) in _numericVariants)
                        {
                            try
                            {
                                var closeType = v.MakeGenericType(argType);
                                ControlTypes.Add($"{nameBase}<{lower}>", closeType);
                                ControlTypes.Add($"{nameBase}<{pascal}>", closeType);
                            }
                            catch (ArgumentException) { /* constraint 위반 skip */ }
                        }
                    }
                    else if (!v.IsGenericType) ControlTypes.Add(ControlName(v), v);
                }
            }
            catch { }
        }

        #region NumberTypeName
        static string NumberTypeName(Type t)
        {
            var s = "";
            if (t == typeof(byte)) s = "byte";
            else if (t == typeof(ushort)) s = "ushort";
            else if (t == typeof(uint)) s = "uint";
            else if (t == typeof(ulong)) s = "ulong";
            else if (t == typeof(sbyte)) s = "sbyte";
            else if (t == typeof(short)) s = "short";
            else if (t == typeof(int)) s = "int";
            else if (t == typeof(long)) s = "long";
            else if (t == typeof(float)) s = "float";
            else if (t == typeof(double)) s = "double";
            else if (t == typeof(decimal)) s = "decimal";
            return s;
        }
        #endregion
        #region ControlName
        internal static string ControlName(Type x)
        {
            var ret = x.Name;
            if(x.IsGenericType)
            {
                ret = x.Name.Split('`').FirstOrDefault() + $"<T>";
            }
            return ret;
        }
        #endregion
    }

    #region SKColorConverter
    /// <summary>
    /// SKColor와 JSON 간의 변환을 처리하는 변환기입니다. 색상을 uint 값으로 직렬화합니다.
    /// </summary>
    public class SKColorConverter : JsonConverter<SKColor>
    {
        /// <inheritdoc/>
        public override SKColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new SKColor(reader.GetUInt32());
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options) => writer.WriteNumberValue((uint)value);
    }
    #endregion
    #region SKRectConverter
    /// <summary>
    /// SKRect와 JSON 간의 변환을 처리하는 변환기입니다. 사각형을 "Left,Top,Right,Bottom" 문자열로 직렬화합니다.
    /// </summary>
    public class SKRectConverter : JsonConverter<SKRect>
    {
        /// <inheritdoc/>
        public override SKRect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            float l = 0, r = 0, t = 0, b = 0;
            var vs = s.Split(',').Select(x => Convert.ToSingle(x)).ToArray();
            if (vs != null && vs.Length == 4) { l = vs[0]; t = vs[1]; r = vs[2]; b = vs[3]; }
            return new SKRect(l, t, r, b);
        }
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, SKRect value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.Left},{value.Top},{value.Right},{value.Bottom}");
        }
    }
    #endregion
    #region SKBitmapConverter
    /// <summary>
    /// SKBitmap과 JSON 간의 변환을 처리하는 변환기입니다. 비트맵을 Base64 문자열로 직렬화합니다.
    /// </summary>
    public class SKBitmapConverter : JsonConverter<SKBitmap>
    {
        /// <inheritdoc/>
        public override SKBitmap? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var base64String = reader.GetString();
                if (string.IsNullOrEmpty(base64String)) return null;

                byte[] imageBytes = Convert.FromBase64String(base64String);
                using var stream = new SKMemoryStream(imageBytes);
                return SKBitmap.Decode(stream);
            }
            throw new JsonException("Invalid format for SKBitmap");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, SKBitmap value, JsonSerializerOptions options)
        {
            using var stream = new SKDynamicMemoryWStream();
            value.Encode(stream, SKEncodedImageFormat.Png, 100);
            byte[] imageBytes = stream.DetachAsData().ToArray();
            string base64String = Convert.ToBase64String(imageBytes);
            writer.WriteStringValue(base64String);
        }
    }
    #endregion
    #region SKImageConverter
    /// <summary>
    /// SKImage와 JSON 간의 변환을 처리하는 변환기입니다. 이미지를 Base64 문자열로 직렬화합니다.
    /// </summary>
    public class SKImageConverter : JsonConverter<SKImage>
    {
        /// <inheritdoc/>
        public override SKImage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var base64String = reader.GetString();
                if (string.IsNullOrEmpty(base64String)) return null;

                byte[] imageBytes = Convert.FromBase64String(base64String);
                return SKImage.FromEncodedData(imageBytes);
            }
            throw new JsonException("Invalid format for SKBitmap");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, SKImage value, JsonSerializerOptions options)
        {
            using var data = value.Encode(SKEncodedImageFormat.Png, 100);
            byte[] bytes = data.ToArray();
            string base64 = Convert.ToBase64String(bytes);
            writer.WriteStringValue(base64);
        }
    }
    #endregion
    #region GoControlConverter
    /// <summary>
    /// IGoControl 인터페이스 구현체와 JSON 간의 다형성 변환을 처리하는 변환기입니다.
    /// </summary>
    public class GoControlConverter : JsonConverter<IGoControl>
    {
        /// <inheritdoc/>
        public override IGoControl Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            string typeName = null;
            IGoControl value = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) break;
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();

                    if (propertyName == "Type")
                    {
                        typeName = reader.GetString();
                    }
                    else if (propertyName == "Value")
                    {
                        if (typeName == null)
                            throw new JsonException("Type information is missing.");

                        var type = GoJsonConverter.ControlTypes.TryGetValue(typeName, out var tp) ? tp : null;
                        if (type == null)
                            throw new JsonException($"Unknown type: {typeName}");

                        value = (IGoControl)JsonSerializer.Deserialize(ref reader, type, options);
                    }
                }
            }

            return value ?? throw new JsonException("Invalid JSON for ScControl.");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, IGoControl value, JsonSerializerOptions options)
        {
            var tp = value.GetType();
            var mp = GoJsonConverter.ControlTypes.FirstOrDefault(x => x.Value == tp);
            if (!string.IsNullOrWhiteSpace(mp.Key))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteStringValue(mp.Key);
                writer.WritePropertyName("Value");
                JsonSerializer.Serialize(writer, value, tp, options);
                writer.WriteEndObject();
            }
        }
    }
    #endregion
    #region GoPagesConverter
    /// <summary>
    /// GoPage 딕셔너리와 JSON 간의 변환을 처리하는 변환기입니다.
    /// </summary>
    public class GoPagesConverter : JsonConverter<Dictionary<string, GoPage>>
    {
        /// <inheritdoc/>
        public override Dictionary<string, GoPage> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new Dictionary<string, GoPage>();

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject");

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)                    return result;
                if (reader.TokenType != JsonTokenType.PropertyName)                    throw new JsonException("Expected PropertyName");

                string key = reader.GetString();
                reader.Read(); 

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Expected StartObject for GoPage entry");

                string typeName = null;
                GoPage pageInstance = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException("Expected PropertyName inside page entry");

                    string propName = reader.GetString();
                    reader.Read();

                    if (propName == "Type")
                    {
                        typeName = reader.GetString();
                    }
                    else if (propName == "Value")
                    {
                        if (typeName == null)
                            throw new JsonException("Type is missing in GoPage entry");

                        var type = GoJsonConverter.ControlTypes.TryGetValue(typeName, out var tp) ? tp : null;
                        if (type == null)
                            throw new JsonException($"Unknown GoPage type: {typeName}");

                        pageInstance = (GoPage)JsonSerializer.Deserialize(ref reader, type, options);
                    }
                }

                if (pageInstance == null)
                    throw new JsonException("Value is missing in GoPage entry");

                result[key] = pageInstance;
            }

            throw new JsonException("Unexpected end of GoPages JSON");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Dictionary<string, GoPage> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                var tp = kvp.Value.GetType();
                var mp = GoJsonConverter.ControlTypes.FirstOrDefault(x => x.Value == tp);

                if (!string.IsNullOrWhiteSpace(mp.Key))
                {
                    writer.WritePropertyName(kvp.Key);
                    writer.WriteStartObject();
                    writer.WriteString("Type", mp.Key);
                    writer.WritePropertyName("Value");
                    JsonSerializer.Serialize(writer, kvp.Value, tp, options);
                    writer.WriteEndObject();
                }
            }

            writer.WriteEndObject();
        }
    }
    #endregion
    #region GoWindowsConverter
    /// <summary>
    /// GoWindow 딕셔너리와 JSON 간의 변환을 처리하는 변환기입니다.
    /// </summary>
    public class GoWindowsConverter : JsonConverter<Dictionary<string, GoWindow>>
    {
        /// <inheritdoc/>
        public override Dictionary<string, GoWindow> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new Dictionary<string, GoWindow>();

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject");

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) return result;
                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected PropertyName");

                string key = reader.GetString();
                reader.Read();

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Expected StartObject for GoWindow entry");

                string typeName = null;
                GoWindow pageInstance = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException("Expected PropertyName inside page entry");

                    string propName = reader.GetString();
                    reader.Read();

                    if (propName == "Type")
                    {
                        typeName = reader.GetString();
                    }
                    else if (propName == "Value")
                    {
                        if (typeName == null)
                            throw new JsonException("Type is missing in GoWindow entry");

                        var type = GoJsonConverter.ControlTypes.TryGetValue(typeName, out var tp) ? tp : null;
                        if (type == null)
                            throw new JsonException($"Unknown GoWindow type: {typeName}");

                        pageInstance = (GoWindow)JsonSerializer.Deserialize(ref reader, type, options);
                    }
                }

                if (pageInstance == null)
                    throw new JsonException("Value is missing in GoWindow entry");

                result[key] = pageInstance;
            }

            throw new JsonException("Unexpected end of GoWindow JSON");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Dictionary<string, GoWindow> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                var tp = kvp.Value.GetType();
                var mp = GoJsonConverter.ControlTypes.FirstOrDefault(x => x.Value == tp);

                if (!string.IsNullOrWhiteSpace(mp.Key))
                {
                    writer.WritePropertyName(kvp.Key);
                    writer.WriteStartObject();
                    writer.WriteString("Type", mp.Key);
                    writer.WritePropertyName("Value");
                    JsonSerializer.Serialize(writer, kvp.Value, tp, options);
                    writer.WriteEndObject();
                }
            }

            writer.WriteEndObject();
        }
    }
    #endregion
    #region GoTableLayoutControlCollectionConverter
    /// <summary>
    /// GoTableLayoutControlCollection과 JSON 간의 변환을 처리하는 변환기입니다.
    /// </summary>
    public class GoTableLayoutControlCollectionConverter : JsonConverter<GoTableLayoutControlCollection>
    {
        /// <inheritdoc/>
        public override GoTableLayoutControlCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = JsonSerializer.Deserialize<Data>(ref reader, options);

            var v = new GoTableLayoutControlCollection();
            if (list != null)
            {
                foreach (var i in list.indexes) v.Indexes.Add(i.Key, i.Value);
                foreach (var c in list.ls) v.Controls.Add(c);
            }
            return v;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, GoTableLayoutControlCollection value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, new Data { ls = value.Controls, indexes = value.Indexes }, options);
        }

        class Data
        {
            public Dictionary<Guid, GoTableIndex> indexes { get; set; } = [];
            public List<IGoControl> ls { get; set; } = [];
        }
    }
    #endregion
    #region GoGridLayoutControlCollectionConverter
    /// <summary>
    /// GoGridLayoutControlCollection과 JSON 간의 변환을 처리하는 변환기입니다.
    /// </summary>
    public class GoGridLayoutControlCollectionConverter : JsonConverter<GoGridLayoutControlCollection>
    {
        /// <inheritdoc/>
        public override GoGridLayoutControlCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = JsonSerializer.Deserialize<Data>(ref reader, options);

            var v = new GoGridLayoutControlCollection();
            if (list != null)
            {
                foreach (var i in list.indexes) v.Indexes.Add(i.Key, i.Value);
                foreach (var c in list.ls) v.Controls.Add(c);
            }
            return v;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, GoGridLayoutControlCollection value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, new Data { ls = value.Controls, indexes = value.Indexes }, options);
        }

        class Data
        {
            public Dictionary<Guid, GoGridIndex> indexes { get; set; } = [];
            public List<IGoControl> ls { get; set; } = [];
        }
    }
    #endregion

}
