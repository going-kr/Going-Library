using Going.UI.Containers;
using Going.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Design
{
    public class GoWindow : GoContainer
    {
        [JsonInclude]
        public override List<IGoControl> Childrens { get; } = [];

        [JsonConstructor]
        public GoWindow(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoWindow() { }
    }
}
