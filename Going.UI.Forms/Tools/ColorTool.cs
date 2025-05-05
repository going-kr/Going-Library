using Going.UI.Forms.Containers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Tools
{
    public class ColorTool
    {
        public static SKColor EnableColor(Control c, SKColor backgroundColor)
        {
            var thm = GoTheme.Current;
            SKColor ret = backgroundColor;

            if (!c.Enabled && c.Parent != null)
            {
                var parentColor = ParentColor(c.Parent);
                ret = MixColorAlpha(parentColor, backgroundColor, 255 - GoTheme.DisableAlpha);
            }

            return ret;
        }

        static SKColor ParentColor(Control c)
        {
            var thm = GoTheme.Current;
            var cBack = Util.FromArgb(c.BackColor);

            var ret = c switch
            {
                GoPanel pnl => thm.ToColor(pnl.PanelColor),
                GoBoxPanel box => thm.ToColor(box.BoxColor),
                _ => Util.FromArgb(c.BackColor),
            };

            if (!c.Enabled && c.Parent != null) ret = ParentColor(c.Parent);
            
            return ret;
        }

        #region MixColorAlpha
        public static SKColor MixColorAlpha(SKColor dest, SKColor src, int srcAlpha)
        {
            byte red = Convert.ToByte(MathTool.Constrain(MathTool.Map(srcAlpha, 0, 255, (int)dest.Red, (int)src.Red), 0.0, 255.0));
            byte green = Convert.ToByte(MathTool.Constrain(MathTool.Map(srcAlpha, 0, 255, (int)dest.Green, (int)src.Green), 0.0, 255.0));
            byte blue = Convert.ToByte(MathTool.Constrain(MathTool.Map(srcAlpha, 0, 255, (int)dest.Blue, (int)src.Blue), 0.0, 255.0));
            return Util.FromArgb(red, green, blue);
        }
        #endregion
    }
}
