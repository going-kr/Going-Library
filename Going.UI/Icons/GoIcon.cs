using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Icons
{
    public class GoIcon
    {
        public SKTypeface FontFamily { get; private set; }
        public string IconText { get; private set; }

        public GoIcon(SKTypeface family, string icon)
        {
            this.FontFamily = family;
            this.IconText = icon;
        }
    }
}
