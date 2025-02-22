using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Themes
{
    public abstract class GoTheme
    {
        #region Const
        internal const int DisableAlpha = 180;
        #endregion

        #region Properties
        public bool Dark { get; set; }

        public SKColor Fore { get; set; }
        public SKColor Back { get; set; }
        public SKColor Window { get; set; }
        public SKColor WindowBorder { get; set; }
        public SKColor Point { get; set; }
        public SKColor Title { get; set; }

        public SKColor Base0 { get; set; }
        public SKColor Base1 { get; set; }
        public SKColor Base2 { get; set; }
        public SKColor Base3 { get; set; }
        public SKColor Base4 { get; set; }
        public SKColor Base5 { get; set; }

        public SKColor ScrollBar { get; set; }
        public SKColor ScrollCursor { get; set; }

        public SKColor Danger { get; set; }
        public SKColor Warning { get; set; }
        public SKColor Good { get; set; }

        public SKColor Hignlight { get; set; }
        public SKColor Error { get; set; }
        public SKColor Select { get; set; }

        public int Corner { get; set; } = 5;
        public int Alpha { get; set; }
        public byte ShadowAlpha { get; set; }
        public float DownBrightness { get; set; }
        public float BorderBrightness { get; set; }
        public float HoverBorderBrightness { get; set; }
        public float HoverFillBrightness { get; set; }
        public float StageLineBrightness { get; set; }

        public bool TouchMode { get; set; } = true;
        #endregion

        #region Static
        public static GoTheme DarkTheme = new DarkTheme();

        public static GoTheme Current { get; set; } = DarkTheme;
        #endregion

        #region Color(string)
        public SKColor ToColor(string? color)
        {
            SKColor ret = Fore;
            if (color != null)
            {
                var lsv = color.Split('-');
                if (lsv.Length > 0)
                {
                    var main = lsv[0];
                    switch (main.ToLower())
                    {
                        case "fore": ret = Fore; break;
                        case "back": ret = Back; break;
                        case "window": ret = Window; break;
                        case "windowborder": ret = WindowBorder; break;
                        case "point": ret = Point; break;
                        case "title": ret = Title; break;

                        case "base0": ret = Base0; break;
                        case "base1": ret = Base1; break;
                        case "base2": ret = Base2; break;
                        case "base3": ret = Base3; break;
                        case "base4": ret = Base4; break;
                        case "base5": ret = Base5; break;

                        case "danger": ret = Danger; break;
                        case "warning": ret = Warning; break;
                        case "good": ret = Good; break;

                        case "hignlight": ret = Hignlight; break;
                        case "error": ret = Error; break;
                        case "select": ret = Select; break;

                        #region default
                        default:
                            {
                                var vs = main?.Split(',').Select(x => x.Trim()).ToArray();

                                if (main != null && main.StartsWith("#") && main.Length == 7
                                    && byte.TryParse(main.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var r1)
                                    && byte.TryParse(main.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var g1)
                                    && byte.TryParse(main.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var b1))
                                    ret = Util.FromArgb(r1, g1, b1);
                                else if (main != null && main.StartsWith("#") && main.Length == 9
                                    && byte.TryParse(main.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var a2)
                                    && byte.TryParse(main.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var r2)
                                    && byte.TryParse(main.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var g2)
                                    && byte.TryParse(main.Substring(7, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var b2))
                                    ret = Util.FromArgb(a2, r2, g2, b2);
                                else if (vs?.Length == 4 && byte.TryParse(vs[0], out byte a3) && byte.TryParse(vs[1], out byte r3) && byte.TryParse(vs[2], out byte g3) && byte.TryParse(vs[3], out byte b3))
                                    ret = Util.FromArgb(a3, r3, g3, b3);
                                else if (vs?.Length == 3 && byte.TryParse(vs[0], out byte r4) && byte.TryParse(vs[1], out byte g4) && byte.TryParse(vs[2], out byte b4))
                                    ret = Util.FromArgb(r4, g4, b4);
                                else if (int.TryParse(main, out int n))
                                    ret = Util.FromArgb(n);
                                else if (main != null)
                                    ret = Util.FromArgb(Color.FromName(main));
                            }
                            break;
                            #endregion
                    }

                    if (lsv.Length == 2)
                    {
                        var sub = lsv[1];
                        if (sub.ToLower() == "dark") ret = Util.FromArgb(ret.Alpha, Convert.ToByte(MathTool.Constrain(ret.Red - 15, 0, 255)), Convert.ToByte(MathTool.Constrain(ret.Green - 15, 0, 255)), Convert.ToByte(MathTool.Constrain(ret.Blue - 15, 0, 255)));
                        else if (sub.ToLower() == "light") ret = Util.FromArgb(ret.Alpha, Convert.ToByte(MathTool.Constrain(ret.Red + 15, 0, 255)), Convert.ToByte(MathTool.Constrain(ret.Green + 15, 0, 255)), Convert.ToByte(MathTool.Constrain(ret.Blue + 15, 0, 255)));

                    }
                }
            }
            return ret;
        }
        #endregion

    }

    public class DarkTheme : GoTheme
    {
        public DarkTheme()
        {
            Dark = true;
            Alpha = 60;

            Fore = SKColors.White;
            Back = Util.FromArgb(50, 50, 50);
            Window = Util.FromArgb(32, 32, 32);
            WindowBorder = Util.FromArgb(90, 90, 90);
            Point = SKColors.DarkRed;
            Title = Util.FromArgb(70, 70, 70);

            Base0 = Util.FromArgb(0, 0, 0);
            Base1 = Util.FromArgb(30, 30, 30);
            Base2 = Util.FromArgb(60, 60, 60);
            Base3 = Util.FromArgb(90, 90, 90);
            Base4 = Util.FromArgb(120, 120, 120);
            Base5 = Util.FromArgb(150, 150, 150);

            Danger = SKColors.DarkRed;
            Warning = SKColors.DarkOrange;
            Good = SKColors.Green;

            Hignlight = SKColors.Cyan;
            Error = SKColors.Red;
            Select = SKColors.Teal;

            ScrollBar = Base1;
            ScrollCursor = Base3;
            
            Corner = 5;
            DownBrightness = -0.25F;
            BorderBrightness = -0.3F;
            HoverBorderBrightness = 0.5F;
            HoverFillBrightness = 0.15F;
            ShadowAlpha = 180;
        }
    }
}
