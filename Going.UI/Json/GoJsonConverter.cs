using Going.UI.Containers;
using Going.UI.Controls;
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

namespace Going.UI.Json
{
    public class GoJsonConverter
    {
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions();
        static GoJsonConverter()
        {
            Options.PropertyNameCaseInsensitive = true;
            Options.Converters.Add(new SKColorConverter());
            Options.Converters.Add(new SKRectConverter());
            Options.Converters.Add(new SKBitmapConverter());
            Options.Converters.Add(new SKImageConverter());
            Options.Converters.Add(new GoControlConverter());
            Options.Converters.Add(new GoPagesConverter());
            Options.Converters.Add(new GoTableLayoutControlCollectionConverter());
            Options.Converters.Add(new GoGridLayoutControlCollectionConverter());
        }
    }

    #region SKColorConverter
    public class SKColorConverter : JsonConverter<SKColor>
    {
        public override SKColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new SKColor(reader.GetUInt32());
        public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options) => writer.WriteNumberValue((uint)value);
    }
    #endregion
    #region SKRectConverter
    public class SKRectConverter : JsonConverter<SKRect>
    {
        public override SKRect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            float l = 0, r = 0, t = 0, b = 0;
            var vs = s.Split(',').Select(x => Convert.ToSingle(x)).ToArray();
            if (vs != null && vs.Length == 4) { l = vs[0]; t = vs[1]; r = vs[2]; b = vs[3]; }
            return new SKRect(l, t, r, b);
        }
        public override void Write(Utf8JsonWriter writer, SKRect value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.Left},{value.Top},{value.Right},{value.Bottom}");
        }
    }
    #endregion
    #region SKBitmapConverter 
    public class SKBitmapConverter : JsonConverter<SKBitmap>
    {
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
    public class SKImageConverter : JsonConverter<SKImage>
    {
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
    public class GoControlConverter : JsonConverter<IGoControl>
    {
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

                        var type = Type.GetType(typeName);
                        if (type == null)
                            throw new JsonException($"Unknown type: {typeName}");

                        value = (IGoControl)JsonSerializer.Deserialize(ref reader, type, options);
                    }
                }
            }

            return value ?? throw new JsonException("Invalid JSON for ScControl.");
        }

        public override void Write(Utf8JsonWriter writer, IGoControl value, JsonSerializerOptions options)
        {
            
            writer.WriteStartObject();
            writer.WritePropertyName("Type");
            writer.WriteStringValue(value.GetType().AssemblyQualifiedName);
            writer.WritePropertyName("Value");
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
            writer.WriteEndObject();
        }
    }
    #endregion
    #region GoPagesConverter
    public class GoPagesConverter : JsonConverter<Dictionary<string, GoPage>>
    {
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

                        var type = Type.GetType(typeName);
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

        public override void Write(Utf8JsonWriter writer, Dictionary<string, GoPage> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key);

                writer.WriteStartObject();
                writer.WriteString("Type", kvp.Value.GetType().AssemblyQualifiedName);
                writer.WritePropertyName("Value");
                JsonSerializer.Serialize(writer, kvp.Value, kvp.Value.GetType(), options);
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }
    }
    #endregion
    #region GoTableLayoutControlCollectionConverter
    public class GoTableLayoutControlCollectionConverter : JsonConverter<GoTableLayoutControlCollection>
    {
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
    public class GoGridLayoutControlCollectionConverter : JsonConverter<GoGridLayoutControlCollection>
    {
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
