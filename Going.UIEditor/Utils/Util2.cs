using Going.UI.Enums;
using Going.UI.Managers;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UIEditor.Utils
{
    internal class Util2
    {
        public static string EllipsisPath(string path, string fontName, GoFontStyle fontStyle, float fontsize,  float maxWidth)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var sz = Util.MeasureText(path, fontName, fontStyle, fontsize);
            if (sz.Width <= maxWidth) return path;

            string ellipsis = "...";
            float ellipsisWidth = Util.MeasureText(ellipsis, fontName, fontStyle, fontsize).Width;

            int len = path.Length;
            while (len > 0)
            {
                string sub = path.Substring(0, len);
                float subWidth = Util.MeasureText(sub, fontName, fontStyle, fontsize).Width; 
                if (subWidth + ellipsisWidth <= maxWidth)
                {
                    return sub + ellipsis;
                }
                len--;
            }

            return ellipsis;
        }
    }
}
