using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Managers;
using Going.UI.Utils;
using Going.UIEditor.Windows;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Going.UIEditor.Utils
{
    internal class Util2
    {
        #region EllipsisPath
        public static string EllipsisPath(string path, string fontName, GoFontStyle fontStyle, float fontsize, float maxWidth)
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
        #endregion

        #region GetAnchors
        public static Anchor[] GetAnchors(GoControl c, SKRect rect)
        {
            if (c is GoTitleBar || c is GoSideBar || c is GoFooter || c is GoPage || c is GoWindow)
                return [];
            else
                return [
                    new Anchor { Name = "move", Position = new SKPoint(rect.MidX, rect.MidY), Control = c },
                    new Anchor { Name = "lt", Position = new SKPoint(rect.Left, rect.Top), Control = c },
                    new Anchor { Name = "rt", Position = new SKPoint(rect.Right, rect.Top), Control = c },
                    new Anchor { Name = "lb", Position = new SKPoint(rect.Left, rect.Bottom), Control = c },
                    new Anchor { Name = "rb", Position = new SKPoint(rect.Right, rect.Bottom), Control = c },
                    new Anchor { Name = "l", Position = new SKPoint(rect.Left, rect.MidY), Control = c },
                    new Anchor { Name = "r", Position = new SKPoint(rect.Right, rect.MidY), Control = c },
                    new Anchor { Name = "t", Position = new SKPoint(rect.MidX, rect.Top), Control = c },
                    new Anchor { Name = "b", Position = new SKPoint(rect.MidX, rect.Bottom), Control = c },
                ];
        }
        #endregion 

    }
}
