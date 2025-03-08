using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    public class GoMenuItem
    {
        public string? IconString { get; set; }
        public string? Text { get; set; }
        public object? Tag { get; set; }
        public string? PageName { get; set; }

        internal SKRect Bounds { get; set; }

        public override string ToString() => Text ?? string.Empty;
    }
}
