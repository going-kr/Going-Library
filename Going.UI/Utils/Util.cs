using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.Utils
{
    public class Util
    {
        #region Member Variable
        static Dictionary<string, SKTypeface> fontCache = [];
        #endregion

        #region Method
        #region From
        #region FromArgb
        public static Color FromArgb(SKColor c) => Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);

        public static SKColor FromArgb(Color c) => new SKColor(c.R, c.G, c.B, c.A);
        public static SKColor FromArgb(int c) => new SKColor(Convert.ToByte(((uint)c & 0xFF0000) >> 16), Convert.ToByte(((uint)c & 0xFF00) >> 8), Convert.ToByte(((uint)c & 0xFF)), Convert.ToByte(((uint)c & 0xFF000000) >> 24));
        public static SKColor FromArgb(byte a, byte r, byte g, byte b) => new SKColor(r, g, b, a);
        public static SKColor FromArgb(byte a, SKColor c) => new SKColor(c.Red, c.Green, c.Blue, a);
        public static SKColor FromArgb(byte r, byte g, byte b) => new SKColor(r, g, b);
        #endregion

        #region FromRect
        public static SKRect FromRect(SKRect rt) => new SKRect(rt.Left, rt.Top, rt.Right, rt.Bottom);
        public static SKRect FromRect(Rectangle rt) => new SKRect(rt.Left, rt.Top, rt.Right, rt.Bottom);
        public static SKRect FromRect(RectangleF rt) => new SKRect(rt.Left, rt.Top, rt.Right, rt.Bottom);
        public static SKRect FromRect(int x, int y, int width, int height) => FromRect(new Rectangle(x, y, width, height));
        public static SKRect FromRect(float x, float y, float width, float height) => FromRect(new RectangleF(x, y, width, height));
        public static SKRect FromRect(SKRect rt, GoPadding pad) => new SKRect(rt.Left + pad.Left, rt.Top + pad.Top, rt.Right - pad.Right, rt.Bottom - pad.Bottom);
        #endregion

        #region FromBitmap
        public static SKBitmap? FromBitmap(string path)
        {
            SKBitmap? ret = null;
            try { if (File.Exists(path)) using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read)) ret = SKBitmap.Decode(fs); }
            catch { }
            return ret;
        }
        public static SKBitmap? FromBitmap64(string base64)
        {
            SKBitmap? ret = null;
            try { ret = SKBitmap.Decode(Convert.FromBase64String(base64)); }
            catch { }
            return ret;
        }
        public static SKBitmap? FromAssemblyBitmap(Assembly asm, string name)
        {
            SKBitmap? ret = null;
            try { using (var ms = asm.GetManifestResourceStream(name)) ret = SKBitmap.Decode(ms); }
            catch { }
            return ret;
        }
        #endregion
        #endregion

        #region Text
        #region GetTypeface
        public static SKTypeface? GetTypeface(string fontName)
        {
            if (!fontCache.TryGetValue(fontName, out var typeface))
            {
                typeface = SKFontManager.Default.MatchFamily(fontName);
                fontCache[fontName] = typeface;
            }
            return typeface;
        }
        #endregion

        #region DrawText
        public static void DrawText(SKCanvas canvas, string? text, string fontName, float fontSize, SKRect bounds, SKColor color, GoContentAlignment align = GoContentAlignment.MiddleCenter, bool wordWrap = true)
        {
            var tf = GetTypeface(fontName);

            var sp = canvas.Save();
            canvas.ClipRect(bounds);

            if (!string.IsNullOrWhiteSpace(text) && tf != null)
            {
                using var p = new SKPaint { IsAntialias = true };
                using var font = new SKFont(tf, fontSize);

                var lines = !wordWrap ? LineText(text) : WrapText(text, font, bounds.Width);
                var lineHeight = font.Spacing;
                float totalTextHeight = lineHeight * lines.Count;
                float startY = bounds.Top;
                if (align == GoContentAlignment.MiddleLeft || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.MiddleRight) startY = bounds.MidY - totalTextHeight / 2F;
                else if (align == GoContentAlignment.BottomLeft || align == GoContentAlignment.BottomCenter || align == GoContentAlignment.BottomRight) startY = bounds.Bottom - totalTextHeight;

                p.IsStroke = false;
                p.Color = color;
                for (int i = 0; i < lines.Count; i++)
                {
                    float y = startY + (i * lineHeight) - font.Metrics.Ascent;
                    float textWidth = font.MeasureText(lines[i]);

                    float x = bounds.Left;
                    if (align == GoContentAlignment.TopCenter || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.BottomCenter) x = bounds.MidX - textWidth / 2;
                    else if (align == GoContentAlignment.TopRight || align == GoContentAlignment.MiddleRight || align == GoContentAlignment.BottomRight) x = bounds.Right - textWidth;

                    canvas.DrawText(lines[i], x, y, font, p);
                }
            }

            canvas.RestoreToCount(sp);
        }
        #endregion

        #region DrawIcon
        public static void DrawIcon(SKCanvas canvas, string? iconString, float iconSize, SKRect bounds, SKColor color, GoContentAlignment align = GoContentAlignment.MiddleCenter)
        {
            var fi = iconString != null ? GoIconManager.GetIcon(iconString) : null;

            if (fi != null && iconSize > 0)
            {
                using var p = new SKPaint { IsAntialias = true };
                using var font = new SKFont(fi.FontFamily, iconSize);

                var lineHeight = font.Spacing;
                float totalTextHeight = lineHeight;

                float startY = bounds.Top;
                if (align == GoContentAlignment.MiddleLeft || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.MiddleRight) startY = bounds.MidY - totalTextHeight / 2F;
                else if (align == GoContentAlignment.BottomLeft || align == GoContentAlignment.BottomCenter || align == GoContentAlignment.BottomRight) startY = bounds.Bottom - totalTextHeight;

                float y = startY - font.Metrics.Ascent;
                float textWidth = font.MeasureText(fi.IconText);
                float x = bounds.Left;
                if (align == GoContentAlignment.TopCenter || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.BottomCenter) x = bounds.MidX - textWidth / 2;
                else if (align == GoContentAlignment.TopRight || align == GoContentAlignment.MiddleRight || align == GoContentAlignment.BottomRight) x = bounds.Right - textWidth;

                p.IsStroke = false;
                p.Color = color;
                canvas.DrawText(fi.IconText, x, y, font, p);
            }
        }

        public static void DrawIcon(SKCanvas canvas, string? iconString, float iconSize, float rotate, SKRect bounds, SKColor fill, SKColor border)
        {
            var fi = iconString != null ? GoIconManager.GetIcon(iconString) : null;

            if (fi != null && iconSize > 0)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.Translate(bounds.MidX, bounds.MidY);
                    canvas.RotateDegrees(rotate);
                    using var p = new SKPaint { IsAntialias = true };
                    using var font = new SKFont(fi.FontFamily, iconSize);

                    var lineHeight = font.Spacing;
                    float totalTextHeight = lineHeight;

                    float textWidth = font.MeasureText(fi.IconText);

                    float y = -totalTextHeight / 2F - font.Metrics.Ascent;
                    float x = -textWidth / 2;

                    p.IsStroke = false;
                    p.Color = fill;
                    canvas.DrawText(fi.IconText, x, y, font, p);

                    p.IsStroke = true;
                    p.StrokeWidth = 1.5F;
                    p.Color = border;
                    canvas.DrawText(fi.IconText, x, y, font, p);
                }
            }
        }

        public static SKPath? GetIconPath(string? iconString, float iconSize, float rotate, SKRect bounds)
        {
            SKPath? ret = null;
            var fi = iconString != null ? GoIconManager.GetIcon(iconString) : null;
            if (fi != null && iconSize > 0)
            {
                using var p = new SKPaint { IsAntialias = true };
                using var font = new SKFont(fi.FontFamily, iconSize);

                var lineHeight = font.Spacing;
                float totalTextHeight = lineHeight;

                float textWidth = font.MeasureText(fi.IconText);

                float y = -totalTextHeight / 2F - font.Metrics.Ascent;
                float x = -textWidth / 2;

                ret = font.GetTextPath(fi.IconText, new SKPoint(x, y));

                var rotationMatrix = SKMatrix.CreateRotationDegrees(rotate, 0, 0); ret.Transform(rotationMatrix);
                var translateMatrix = SKMatrix.CreateTranslation(bounds.MidX, bounds.MidY); ret.Transform(translateMatrix);
                //var scaleMatrix = SKMatrix.CreateScale(1.25F,1.25F); ret.Transform(scaleMatrix);
            }
            return ret;
        }
        #endregion

        #region DrawTextIcon
        public static void DrawTextIcon(SKCanvas canvas, string? text, string fontName, float fontSize, string? iconString, float iconSize, GoDirectionHV direction, float gap, SKRect bounds, SKColor color, GoContentAlignment align = GoContentAlignment.MiddleCenter, bool wordWrap = true)
        {
            var fi = iconString != null ? GoIconManager.GetIcon(iconString) : null;
            if (fi != null && iconSize > 0)
            {
                using var font = new SKFont(fi.FontFamily, iconSize);
                font.MeasureText(fi.IconText, out var vrtico);

                var (rtIco, rtText) = TextIconBounds(text, fontName, fontSize, vrtico.Size, direction, gap, bounds, align);
                DrawIcon(canvas, iconString, iconSize, rtIco, color, align);
                if (!string.IsNullOrEmpty(text)) DrawText(canvas, text, fontName, fontSize, rtText, color, align, wordWrap);

            }
            else DrawText(canvas, text, fontName, fontSize, bounds, color, align, wordWrap);
        }

        public static void DrawTextIcon(SKCanvas canvas, string? text, string fontName, float fontSize, string? iconString, float iconSize, GoDirectionHV direction, float gap, SKRect bounds, SKColor textcolor, SKColor iconcolor, GoContentAlignment align = GoContentAlignment.MiddleCenter, bool wordWrap = true)
        {
            var fi = iconString != null ? GoIconManager.GetIcon(iconString) : null;
            if (fi != null && iconSize > 0)
            {
                using var font = new SKFont(fi.FontFamily, iconSize);
                font.MeasureText(fi.IconText, out var vrtico);

                var (rtIco, rtText) = TextIconBounds(text, fontName, fontSize, vrtico.Size, direction, gap, bounds, align);
                DrawIcon(canvas, iconString, iconSize, rtIco, iconcolor, align);
                if (!string.IsNullOrEmpty(text)) DrawText(canvas, text, fontName, fontSize, rtText, textcolor, align, wordWrap);

            }
            else DrawText(canvas, text, fontName, fontSize, bounds, textcolor, align, wordWrap);
        }
        #endregion

        #region MeasureText
        public static float MeasureText(string text, string fontName, float fontSize)
        {
            var tf = GetTypeface(fontName);
            using var font = new SKFont(tf, fontSize);
            var w = font.MeasureText(text, out var rtText);

            return w;
        }
        #endregion

        #region MeasureTextIcon
        public static SKSize MeasureTextIcon(string? text, string fontName, float fontSize, SKSize iconSize, GoDirectionHV direction, float gap, SKRect bounds, GoContentAlignment align)
        {
            var (rtIcon, rtText) = TextIconBounds(text, fontName, fontSize, iconSize, direction, gap, bounds, align);

            var l = rtText.IsEmpty ? rtIcon.Left : Math.Min(rtIcon.Left, rtText.Left);
            var t = rtText.IsEmpty ? rtIcon.Top : Math.Min(rtIcon.Top, rtText.Top);
            var r = rtText.IsEmpty ? rtIcon.Right : Math.Max(rtIcon.Right, rtText.Right);
            var b = rtText.IsEmpty ? rtIcon.Bottom : Math.Max(rtIcon.Bottom, rtText.Bottom);

            return new SKSize(r - l, b - t);
        }
        #endregion

        #region TextIconBounds
        public static (SKRect rtIcon, SKRect rtText) TextIconBounds(string? text, string fontName, float fontSize, SKSize iconSize, GoDirectionHV direction, float gap, SKRect bounds, GoContentAlignment align)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (MathTool.MakeRectangle(bounds, iconSize, align), FromRect(0, 0, 0, 0));
            }
            else
            {
                var tf = GetTypeface(fontName);
                using var font = new SKFont(tf, fontSize);

                var lines = LineText(text);
                var lineHeight = font.Spacing;
                var TH = lineHeight * lines.Count;
                var TW = lines.Max(x => font.MeasureText(x));
                var szv = direction == GoDirectionHV.Horizon ? new SKSize(iconSize.Width + gap + TW, Math.Max(TH, iconSize.Height)) : new SKSize(Math.Max(TW, iconSize.Width), iconSize.Height + gap + TH);
                var rtb = MathTool.MakeRectangle(bounds, szv, align);

                if (direction == GoDirectionHV.Horizon)
                {
                    var rtFA = FromRect(rtb.Left, rtb.MidY - iconSize.Height / 2F + 0.5F, iconSize.Width, iconSize.Height);
                    var rtTX = FromRect(rtb.Right - TW, rtb.MidY - TH / 2F, TW, TH);
                    return (rtFA, rtTX);
                }
                else
                {
                    var rtFA = FromRect(rtb.MidX - iconSize.Width / 2F, rtb.Top, iconSize.Width, iconSize.Height);
                    var rtTX = FromRect(rtb.MidX - TW / 2F, rtb.Bottom - TH, TW, TH);
                    return (rtFA, rtTX);
                }
            }
        }
        #endregion

        #region WrapText
        static List<string> WrapText(string text, SKFont font, float maxWidth)
        {
            List<string> lines = [];

            string[] rawLines = text.Replace("\r\n", "\n").Split('\n');

            foreach (string rawLine in rawLines)
            {
                string[] words = rawLine.Split(' '); // 공백 기준 단어 분리
                string currentLine = "";

                foreach (string word in words)
                {
                    string testLine = (currentLine == "") ? word : currentLine + " " + word;
                    float testWidth = font.MeasureText(testLine);

                    if (testWidth > maxWidth && currentLine != "")
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }
                    else
                    {
                        currentLine = testLine;
                    }
                }

                if (!string.IsNullOrEmpty(currentLine))
                    lines.Add(currentLine);
            }

            return lines;
        }
        #endregion

        #region LineText
        static List<string> LineText(string text)
        {
            List<string> lines = [];

            string[] rawLines = text.Replace("\r\n", "\n").Split('\n');

            foreach (string rawLine in rawLines)
            {
                if (!string.IsNullOrEmpty(rawLine))
                    lines.Add(rawLine);
            }

            return lines;
        }
        #endregion
        #endregion

        #region Box
        public static void DrawBox(SKCanvas canvas, SKRect bounds, SKColor color, GoRoundType round, float corner, bool clean = true) => DrawBox(canvas, bounds, color, color, round, corner, clean);
        public static void DrawBox(SKCanvas canvas, SKRect bounds, SKColor fillcolor, SKColor borderColor, GoRoundType round, float corner, bool clean = true)
        {
            using var p = new SKPaint { IsAntialias = true };

            if(clean)
            {
                bounds = Int(bounds);
                bounds.Offset(0.5F, 0.5F);
            }

            using var path = PathTool.Box(bounds, round, corner);

            if (fillcolor != SKColors.Transparent)
            {
                p.IsStroke = false;
                p.Color = fillcolor;
                canvas.DrawPath(path, p);

            }

            if (borderColor != SKColors.Transparent)
            {
                p.IsStroke = true;
                p.StrokeWidth = 1F;
                p.Color = borderColor;
                canvas.DrawPath(path, p);
            }
        }
        #endregion

        #region Grid
        #region Devide
        #region Grid
        public static SKRect[,] Grid(SKRect bounds, string[] cols, string[] rows)
        {
            int colCount = cols.Length;
            int rowCount = rows.Length;
            SKRect[,] grid = new SKRect[rowCount, colCount];

            float[] colWidths = ParseSizes(cols, bounds.Width);
            float[] rowHeights = ParseSizes(rows, bounds.Height);

            float y = bounds.Top;
            for (int r = 0; r < rowCount; r++)
            {
                float x = bounds.Left;
                for (int c = 0; c < colCount; c++)
                {
                    grid[r, c] = new SKRect(x, y, x + colWidths[c], y + rowHeights[r]);
                    x += colWidths[c];
                }
                y += rowHeights[r];
            }

            return grid;
        }
        #endregion
        #region Columns
        public static SKRect[] Columns(SKRect bounds, string[] cols)
        {
            int colCount = cols.Length;
            SKRect[] grid = new SKRect[colCount];

            float[] colWidths = ParseSizes(cols, bounds.Width);

            float x = bounds.Left;
            for (int c = 0; c < colCount; c++)
            {
                grid[c] = new SKRect(x, bounds.Top, x + colWidths[c], bounds.Bottom);
                x += colWidths[c];
            }

            return grid;
        }
        #endregion
        #region Rows
        public static SKRect[] Rows(SKRect bounds, string[] rows)
        {
            int rowCount = rows.Length;
            SKRect[] grid = new SKRect[rowCount];

            float[] rowHeights = ParseSizes(rows, bounds.Height);

            float y = bounds.Top;
            for (int r = 0; r < rowCount; r++)
            {
                grid[r] = new SKRect(bounds.Left, y, bounds.Right, y + rowHeights[r]);
                y += rowHeights[r];
            }

            return grid;
        }
        #endregion
        #region ParseSizes
        private static float[] ParseSizes(string[] sizes, float totalSize)
        {
            List<float> result = new List<float>();
            float totalPercentage = 0;
            float fixedTotal = 0;

            foreach (var size in sizes)
            {
                if (size.EndsWith("px"))
                {
                    float px = float.Parse(size.Replace("px", ""));
                    result.Add(px);
                    fixedTotal += px;
                }
                else if (size.EndsWith("%"))
                {
                    float percent = float.Parse(size.Replace("%", "")) / 100f;
                    result.Add(-percent); // Negative for percentage
                    totalPercentage += percent;
                }
                else
                {
                    //throw new ArgumentException("Invalid size format. Use 'px' or '%'.");
                }
            }

            float remainingSize = totalSize - fixedTotal;
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] < 0)
                    result[i] = remainingSize * -result[i];
            }

            return result.ToArray();
        }
        #endregion
        #endregion

        #region Merge
        public static SKRect Merge(SKRect[,] rts, int col, int row, int colspan, int rowspan)
        {
            var rtLT = Util.FromRect(rts[row, col]);
            var rtRB = rts[row + rowspan - 1, col + colspan - 1];
            rtLT.Right = rtRB.Right;
            rtLT.Bottom = rtRB.Bottom;

            return rtLT;
        }

        public static SKRect MergeH(SKRect[] rts, int col, int colspan)
        {
            var rtL = Util.FromRect(rts[col]);
            var rtR = rts[col + colspan - 1];
            rtL.Right = rtR.Right;

            return rtL;
        }

        public static SKRect MergeV(SKRect[] rts, int row, int rowspan)
        {
            var rtT = Util.FromRect(rts[row]);
            var rtB = rts[row + rowspan - 1];
            rtT.Bottom = rtB.Bottom;

            return rtT;
        }

        public static SKRect Merge(SKRect rt1, SKRect rt2)
        {
            var L = Math.Min(rt1.Left, rt2.Left);
            var R = Math.Max(rt1.Right, rt2.Right);
            var T = Math.Min(rt1.Top, rt2.Top);
            var B = Math.Max(rt1.Bottom, rt2.Bottom);

            return new SKRect(L, T, R, B);
        }
        #endregion
        #endregion

        #region Round
        #region Rounds
        public static GoRoundType[] Rounds(GoDirectionHV dir, GoRoundType round, int count)
        {
            var ret = new GoRoundType[count];

            for (int i = 0; i < count; i++) ret[i] = GoRoundType.Rect;

            if (ret.Length == 1)
            {
                ret[0] = round;
            }
            else if (ret.Length > 1)
            {
                var si = 0;
                var ei = count - 1;

                if (dir == GoDirectionHV.Horizon)
                {
                    switch (round)
                    {
                        case GoRoundType.L: ret[si] = GoRoundType.L; ret[ei] = GoRoundType.Rect; break;
                        case GoRoundType.R: ret[si] = GoRoundType.Rect; ret[ei] = GoRoundType.R; break;
                        case GoRoundType.T: ret[si] = GoRoundType.LT; ret[ei] = GoRoundType.RT; break;
                        case GoRoundType.B: ret[si] = GoRoundType.LB; ret[ei] = GoRoundType.RB; break;
                        case GoRoundType.LT: ret[si] = GoRoundType.LT; ret[ei] = GoRoundType.Rect; break;
                        case GoRoundType.RT: ret[si] = GoRoundType.Rect; ret[ei] = GoRoundType.RT; break;
                        case GoRoundType.LB: ret[si] = GoRoundType.LB; ret[ei] = GoRoundType.Rect; break;
                        case GoRoundType.RB: ret[si] = GoRoundType.Rect; ret[ei] = GoRoundType.RB; break;
                        case GoRoundType.All: ret[si] = GoRoundType.L; ret[ei] = GoRoundType.R; break;
                    }
                }
                else if (dir == GoDirectionHV.Vertical)
                {
                    switch (round)
                    {
                        case GoRoundType.L: ret[si] = GoRoundType.LT; ret[ei] = GoRoundType.LB; break;
                        case GoRoundType.R: ret[si] = GoRoundType.RT; ret[ei] = GoRoundType.RB; break;
                        case GoRoundType.T: ret[si] = GoRoundType.T; ret[ei] = GoRoundType.Rect; break;
                        case GoRoundType.B: ret[si] = GoRoundType.Rect; ret[ei] = GoRoundType.B; break;
                        case GoRoundType.LT: ret[si] = GoRoundType.LT; ret[ei] = GoRoundType.Rect; break;
                        case GoRoundType.RT: ret[si] = GoRoundType.RT; ret[ei] = GoRoundType.Rect; break;
                        case GoRoundType.LB: ret[si] = GoRoundType.Rect; ret[ei] = GoRoundType.LB; break;
                        case GoRoundType.RB: ret[si] = GoRoundType.Rect; ret[ei] = GoRoundType.RB; break;
                        case GoRoundType.All: ret[si] = GoRoundType.T; ret[ei] = GoRoundType.B; break;
                    }
                }
            }
            return ret;
        }
        #endregion

        #region SetRound
        public static void SetRound(SKRoundRect rt, GoRoundType round, float corner)
        {
            switch (round)
            {
                case GoRoundType.Rect: rt.SetNinePatch(rt.Rect, 0, 0, 0, 0); break;
                case GoRoundType.All: rt.SetNinePatch(rt.Rect, corner, corner, corner, corner); break;
                case GoRoundType.L: rt.SetNinePatch(rt.Rect, corner, corner, 0, corner); break;
                case GoRoundType.R: rt.SetNinePatch(rt.Rect, 0, corner, corner, corner); break;
                case GoRoundType.T: rt.SetNinePatch(rt.Rect, corner, corner, corner, 0); break;
                case GoRoundType.B: rt.SetNinePatch(rt.Rect, corner, 0, corner, corner); break;
                case GoRoundType.LT: rt.SetNinePatch(rt.Rect, corner, corner, 0, 0); break;
                case GoRoundType.RT: rt.SetNinePatch(rt.Rect, 0, corner, corner, 0); break;
                case GoRoundType.LB: rt.SetNinePatch(rt.Rect, corner, 0, 0, corner); break;
                case GoRoundType.RB: rt.SetNinePatch(rt.Rect, 0, 0, corner, corner); break;
                case GoRoundType.Ellipse: rt.SetOval(rt.Rect); break;
            }
        }
        #endregion
        #endregion

        #region Int
        public static int Int(float v) => (int)v;
        public static SKPoint Int(SKPoint v) => new SKPoint((int)v.X, (int)v.Y);
        public static SKSize Int(SKSize v) => new SKSize((int)v.Width, (int)v.Height);
        public static SKRect Int(SKRect v) => new SKRect((int)v.Left, (int)v.Top, (int)v.Right, (int)v.Bottom);
        #endregion

        #region Lamp
        public static void DrawLamp(SKCanvas canvas, SKRect bounds, SKColor OnLampColor, SKColor OffLampColor, bool OnOff, bool Shadow = true)
        {
            #region Brightness
            var vS = 0.2F;
            var vE = -0.2F;

            if (GoTheme.Current.Dark)
            {
                vS = 0.2F;
                vE = -0.2F;

                if (OnOff)
                {
                    vS = 0.5F;
                    vE = -0.2F;
                }
            }
            else
            {
                vS = 0.3F;
                vE = 0F;

                if (OnOff)
                {
                    vS = 0.6F;
                    vE = 0F;
                }
            }
            #endregion

            var cM = OnOff ? OnLampColor : OffLampColor;
            var cS = cM.BrightnessTransmit(vS);
            var cE = cM.BrightnessTransmit(vE);
            var cB = cE.BrightnessTransmit(GoTheme.Current.BorderBrightness);

            using var p = new SKPaint { IsAntialias = true };
            p.IsStroke = false;
            p.Color = cB;

            var rt = Util.Int(bounds);
            rt.Inflate(0.5F, 0.5F);

            var cx = Convert.ToSingle(MathTool.Map(0.25, 0, 1, rt.Left, rt.Right));
            var cy = Convert.ToSingle(MathTool.Map(0.25, 0, 1, rt.Top, rt.Bottom));
            using var imgf = SKImageFilter.CreateDropShadow(1, 1, 2, 2, Util.FromArgb(Convert.ToByte(GoTheme.Current.ShadowAlpha / (OnOff ? 1 : 5)), SKColors.Black));
            using var sh = SKShader.CreateRadialGradient(new SKPoint(cx, cy), rt.Width / 2F, [cS, cE], SKShaderTileMode.Clamp);

            p.Shader = sh;
            if (Shadow) p.ImageFilter = imgf;
            canvas.DrawOval(bounds, p);
            p.Shader = null;
            p.ImageFilter = null;

            p.Color = cB;
            p.IsStroke = true;
            p.StrokeWidth = 1;
            canvas.DrawOval(bounds, p);

        }
        #endregion
        #endregion
    }
}
