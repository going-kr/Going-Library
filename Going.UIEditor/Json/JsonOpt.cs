using Going.UI.Json;
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
        }
    }
}
