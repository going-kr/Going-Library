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

namespace Going.UI.Json
{
    public class GoJsonConverter
    {
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions();
        static GoJsonConverter()
        {
            Options.PropertyNameCaseInsensitive = true;
            Options.Converters.Add(new SKRectConverter());
            Options.Converters.Add(new GoControlConverter());
            Options.Converters.Add(new GoTableLayoutControlCollectionConverter());
        }
    }

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

                    if (propertyName == "Type") typeName = reader.GetString();
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
    #region ScControlCollectionConverter
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
}
