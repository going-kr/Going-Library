using Going.UI.Controls;
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
        [GoProperty(PCategory.Basic, 0)] public string? IconString { get; set; }
        [GoProperty(PCategory.Basic, 1)] public string? Text { get; set; }
        public object? Tag { get; set; }
        [GoProperty(PCategory.Basic, 2)] public string? PageName { get; set; }

        internal SKRect Bounds { get; set; }

        public override string ToString() => $"{Text ?? string.Empty} / Page : {PageName}";
    }
}
