using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    public class GoListItem
    {
        public string? IconString { get; set; }
        public string? Text { get; set; }
        public object? Tag { get; set; }

        internal SKRect Bounds { get; set; }

        public override string ToString() => Text ?? string.Empty;
    }

    public class ListItemEventArgs(GoListItem itm) : EventArgs
    {
        public GoListItem Item { get; } = itm;
    }
}
