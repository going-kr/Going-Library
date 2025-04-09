using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Json;
using Going.UIEditor.Datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UIEditor.Tools
{
    public class JsonOpt
    {
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions();
        static JsonOpt()
        {
            Options.PropertyNameCaseInsensitive = true;
            Options.Converters.Add(new ProjectConverter());
        }
    }

    #region class : ProjectConverter
    public class ProjectConverter : JsonConverter<Project>
    {
        public override Project Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var v = JsonSerializer.Deserialize<Data>(ref reader, options);

            var design = GoDesign.JsonDeserialize(v.Design);

            return new Project
            {
                Name = v.Name,
                Width = v.Width,
                Height = v.Height,
                ProjectFolder = v.ProjectFolder,
                Design = design ?? new GoDesign()
            };
        }

        public override void Write(Utf8JsonWriter writer, Project value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, new Data
            {
                Name = value.Name,
                Design = value.Design.JsonSerialize(),
                Width = value.Width,
                Height = value.Height,
                 ProjectFolder = value.ProjectFolder,
            }, options);
        }

        class Data
        {
            public string? Name { get; set; } 
            public string? Design { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string? ProjectFolder { get; set; }
        }
    }
    #endregion
}
