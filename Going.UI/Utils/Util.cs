#define MeasureA
using Going.UI.Containers;
using Going.UI.Controls;
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
    /// <summary>
    /// Going.UI 프레임워크의 핵심 유틸리티 클래스입니다. 색상 변환, 사각형 생성, 폰트 관리, 텍스트/아이콘 그리기, 레이아웃 계산 등의 기능을 제공합니다.
    /// </summary>
    public class Util
    {
        /// <summary>
        /// 이미지 샘플링에 사용되는 기본 옵션입니다.
        /// </summary>
        public readonly static SKSamplingOptions Sampling = new(SKCubicResampler.Mitchell);
        /// <summary>
        /// 시스템 폰트 캐시 딕셔너리입니다.
        /// </summary>
        public static Dictionary<string, Dictionary<GoFontStyle, SKTypeface>> FontCache { get; } = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// 외부 폰트 딕셔너리입니다.
        /// </summary>
        public static Dictionary<string, Dictionary<GoFontStyle, SKTypeface>> ExternalFonts { get; } = new(StringComparer.OrdinalIgnoreCase);

        private static SKTypeface DefaultFontR;
        private static SKTypeface DefaultFontB;

        static Util()
        {
            #region DefaultFont
            {
                var asm = typeof(Util).Assembly;
                var names = asm.GetManifestResourceNames();
                using (var ms = asm.GetManifestResourceStream("Going.UI.Resources.NanumGothic.ttf")) DefaultFontR = SKTypeface.FromStream(ms);
                using (var ms = asm.GetManifestResourceStream("Going.UI.Resources.NanumGothicBold.ttf")) DefaultFontB = SKTypeface.FromStream(ms);

                ExternalFonts["나눔고딕"] = [];
                ExternalFonts["나눔고딕"][GoFontStyle.Normal] = DefaultFontR;
                ExternalFonts["나눔고딕"][GoFontStyle.Bold] = DefaultFontB;
            }
            #endregion
        }

        #region Member Variable
        #endregion

        #region Method
        #region From
        #region FromArgb
        /// <summary>
        /// SKColor를 System.Drawing.Color로 변환합니다.
        /// </summary>
        public static Color FromArgb(SKColor c) => Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);

        /// <summary>
        /// System.Drawing.Color를 SKColor로 변환합니다.
        /// </summary>
        public static SKColor FromArgb(Color c) => new SKColor(c.R, c.G, c.B, c.A);
        /// <summary>
        /// 정수 색상 값을 SKColor로 변환합니다.
        /// </summary>
        public static SKColor FromArgb(int c) => new SKColor(Convert.ToByte(((uint)c & 0xFF0000) >> 16), Convert.ToByte(((uint)c & 0xFF00) >> 8), Convert.ToByte(((uint)c & 0xFF)), Convert.ToByte(((uint)c & 0xFF000000) >> 24));
        /// <summary>
        /// ARGB 바이트 값으로 SKColor를 생성합니다.
        /// </summary>
        public static SKColor FromArgb(byte a, byte r, byte g, byte b) => new SKColor(r, g, b, a);
        /// <summary>
        /// 기존 색상에 알파 값을 적용한 SKColor를 생성합니다.
        /// </summary>
        public static SKColor FromArgb(byte a, SKColor c) => new SKColor(c.Red, c.Green, c.Blue, a);
        /// <summary>
        /// RGB 바이트 값으로 SKColor를 생성합니다.
        /// </summary>
        public static SKColor FromArgb(byte r, byte g, byte b) => new SKColor(r, g, b);
        #endregion

        #region FromRect
        /// <summary>
        /// SKRect를 복사합니다.
        /// </summary>
        public static SKRect FromRect(SKRect rt) => new SKRect(rt.Left, rt.Top, rt.Right, rt.Bottom);
        /// <summary>
        /// Rectangle을 SKRect로 변환합니다.
        /// </summary>
        public static SKRect FromRect(Rectangle rt) => new SKRect(rt.Left, rt.Top, rt.Right, rt.Bottom);
        /// <summary>
        /// RectangleF를 SKRect로 변환합니다.
        /// </summary>
        public static SKRect FromRect(RectangleF rt) => new SKRect(rt.Left, rt.Top, rt.Right, rt.Bottom);
        /// <summary>
        /// 정수 좌표와 크기로 SKRect를 생성합니다.
        /// </summary>
        public static SKRect FromRect(int x, int y, int width, int height) => FromRect(new Rectangle(x, y, width, height));
        /// <summary>
        /// 실수 좌표와 크기로 SKRect를 생성합니다.
        /// </summary>
        public static SKRect FromRect(float x, float y, float width, float height) => FromRect(new RectangleF(x, y, width, height));
        /// <summary>
        /// SKRect에 패딩을 적용한 SKRect를 생성합니다.
        /// </summary>
        public static SKRect FromRect(SKRect rt, GoPadding pad) => new SKRect(rt.Left + pad.Left, rt.Top + pad.Top, rt.Right - pad.Right, rt.Bottom - pad.Bottom);
        #endregion

        #region FromBitmap
        /// <summary>
        /// 파일 경로에서 SKBitmap을 로드합니다.
        /// </summary>
        /// <param name="path">이미지 파일 경로</param>
        /// <returns>로드된 비트맵, 실패 시 null</returns>
        public static SKBitmap? FromBitmap(string path)
        {
            SKBitmap? ret = null;
            try { if (File.Exists(path)) using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read)) ret = SKBitmap.Decode(fs); }
            catch { }
            return ret;
        }
        /// <summary>
        /// Base64 문자열에서 SKBitmap을 로드합니다.
        /// </summary>
        /// <param name="base64">Base64로 인코딩된 이미지 데이터</param>
        /// <returns>로드된 비트맵, 실패 시 null</returns>
        public static SKBitmap? FromBitmap64(string base64)
        {
            SKBitmap? ret = null;
            try { ret = SKBitmap.Decode(Convert.FromBase64String(base64)); }
            catch { }
            return ret;
        }
        /// <summary>
        /// 어셈블리 리소스에서 SKBitmap을 로드합니다.
        /// </summary>
        /// <param name="asm">리소스가 포함된 어셈블리</param>
        /// <param name="name">리소스 이름</param>
        /// <returns>로드된 비트맵, 실패 시 null</returns>
        public static SKBitmap? FromAssemblyBitmap(Assembly asm, string name)
        {
            SKBitmap? ret = null;
            try { using (var ms = asm.GetManifestResourceStream(name)) ret = SKBitmap.Decode(ms); }
            catch { }
            return ret;
        }
        #endregion

        #region FromImage
        /// <summary>
        /// 파일 경로에서 SKImage를 로드합니다.
        /// </summary>
        /// <param name="path">이미지 파일 경로</param>
        /// <returns>로드된 이미지, 실패 시 null</returns>
        public static SKImage? FromImage(string path)
        {
            SKImage? ret = null;
            try { if (File.Exists(path)) ret = SKImage.FromEncodedData(File.ReadAllBytes(path)); }
            catch { }
            return ret;
        }
        /// <summary>
        /// Base64 문자열에서 SKImage를 로드합니다.
        /// </summary>
        /// <param name="base64">Base64로 인코딩된 이미지 데이터</param>
        /// <returns>로드된 이미지, 실패 시 null</returns>
        public static SKImage? FromImage64(string base64)
        {
            SKImage? ret = null;
            try { ret = SKImage.FromEncodedData(Convert.FromBase64String(base64)); }
            catch { }
            return ret;
        }
        /// <summary>
        /// 어셈블리 리소스에서 SKImage를 로드합니다.
        /// </summary>
        /// <param name="asm">리소스가 포함된 어셈블리</param>
        /// <param name="name">리소스 이름</param>
        /// <returns>로드된 이미지, 실패 시 null</returns>
        public static SKImage? FromAssemblyImage(Assembly asm, string name)
        {
            SKImage? ret = null;
            try { using (var ms = asm.GetManifestResourceStream(name)) ret = SKImage.FromEncodedData(ms); }
            catch { }
            return ret;
        }
        #endregion
        #endregion

        #region Text
        #region SetExternalFonts
        /// <summary>
        /// 외부 폰트를 등록합니다. 기존 외부 폰트는 해제됩니다.
        /// </summary>
        /// <param name="fonts">폰트 이름과 폰트 데이터 바이트 배열 목록의 딕셔너리</param>
        public static void SetExternalFonts(Dictionary<string, List<byte[]>> fonts)
        {
            List<SKTypeface> disposeItems = [];
            foreach(var fontName in ExternalFonts.Keys)
                if(fontName != "나눔고딕")
                    disposeItems.AddRange(ExternalFonts[fontName].Values);

            ExternalFonts.Clear();
            ExternalFonts["나눔고딕"] = [];
            ExternalFonts["나눔고딕"][GoFontStyle.Normal] = DefaultFontR;
            ExternalFonts["나눔고딕"][GoFontStyle.Bold] = DefaultFontB;

            foreach (var fontName in fonts.Keys)
            {
                if (!ExternalFonts.TryGetValue(fontName, out var typefaces)) ExternalFonts[fontName] = [];
                foreach (var fs in fonts[fontName])
                {
                    try
                    {
                        using var ms = new MemoryStream(fs);
                        var typeface = SKTypeface.FromStream(ms);
                        if (typeface != null)
                        {
                            if(typeface.IsBold && typeface.IsItalic)
                                ExternalFonts[fontName].Add(GoFontStyle.BoldItalic, typeface);
                            else if (typeface.IsBold)
                                ExternalFonts[fontName].Add(GoFontStyle.Bold, typeface);
                            else if (typeface.IsItalic)
                                ExternalFonts[fontName].Add(GoFontStyle.Italic, typeface);
                            else
                                ExternalFonts[fontName].Add(GoFontStyle.Normal, typeface);
                        }
                    }
                    catch { }
                }
            }

            foreach (var v in disposeItems) v.Dispose();
        }

        /// <summary>
        /// 등록된 외부 폰트를 해제합니다. 기본 나눔고딕 폰트는 유지됩니다.
        /// </summary>
        public static void UnloadExternalFonts()
        {
            List<SKTypeface> disposeItems = [];

            foreach (var fontName in ExternalFonts.Keys)
                if (fontName != "나눔고딕")
                    disposeItems.AddRange(ExternalFonts[fontName].Values);

            foreach (var v in disposeItems) v.Dispose();
        }
        #endregion

        #region GetFontStyle
        static SKFontStyle GetFontStyle(GoFontStyle fontStyle)
        {
            var r = SKFontStyle.Normal;
            switch (fontStyle)
            {
                case GoFontStyle.Normal: r = SKFontStyle.Normal; break;
                case GoFontStyle.Bold: r = SKFontStyle.Bold; break;
                case GoFontStyle.Italic: r = SKFontStyle.Italic; break;
                case GoFontStyle.BoldItalic: r = SKFontStyle.BoldItalic; break;
            }
            return r;
        }
        #endregion

        #region GetTypeface
        /// <summary>
        /// 폰트 이름과 스타일에 해당하는 SKTypeface를 가져옵니다.
        /// </summary>
        /// <param name="fontName">폰트 이름</param>
        /// <param name="fontStyle">폰트 스타일</param>
        /// <returns>해당하는 SKTypeface</returns>
        public static SKTypeface GetTypeface(string fontName, GoFontStyle fontStyle)
        {
            SKTypeface ret = DefaultFontR;

            try
            {
                if (ExternalFonts.TryGetValue(fontName, out var typefaces) && typefaces.Count > 0)
                {
                    if (typefaces.TryGetValue(fontStyle, out var tp1)) ret = tp1;
                    else ret = typefaces.TryGetValue(GoFontStyle.Normal, out var tp2) ? tp2 : typefaces.Values.First();
                }
                else
                {
                    if (!FontCache.ContainsKey(fontName)) FontCache[fontName] = [];
                    if (FontCache.TryGetValue(fontName, out var ls))
                    {
                        if (ls.TryGetValue(fontStyle, out var tp)) ret = tp;
                        else
                        {
                            ret = SKFontManager.Default.MatchFamily(fontName, GetFontStyle(fontStyle));
                            FontCache[fontName][fontStyle] = ret;
                        }
                    }
                }
            }
            catch { }

            return ret;
        }
        #endregion

        #region DrawText
        /// <summary>
        /// 캔버스에 텍스트를 그립니다.
        /// </summary>
        /// <param name="canvas">SkiaSharp 캔버스</param>
        /// <param name="text">출력할 텍스트</param>
        /// <param name="fontName">폰트 이름</param>
        /// <param name="fontStyle">폰트 스타일</param>
        /// <param name="fontSize">폰트 크기</param>
        /// <param name="bounds">텍스트를 그릴 영역</param>
        /// <param name="color">텍스트 색상</param>
        /// <param name="align">텍스트 정렬</param>
        /// <param name="wordWrap">자동 줄바꿈 여부</param>
        public static void DrawText(SKCanvas canvas, string? text, string fontName, GoFontStyle fontStyle, float fontSize, SKRect bounds, SKColor color, GoContentAlignment align = GoContentAlignment.MiddleCenter, bool wordWrap = true)
        {
            var tf = GetTypeface(fontName, fontStyle);

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
#if MeasureA
                    float textWidth = font.MeasureText(lines[i]) + 1;
#else
                    font.MeasureText(lines[i], out var vszrt); var textWidth = vszrt.Width;
#endif
                    float x = bounds.Left;
                    if (align == GoContentAlignment.TopCenter || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.BottomCenter) x = bounds.MidX - textWidth / 2;
                    else if (align == GoContentAlignment.TopRight || align == GoContentAlignment.MiddleRight || align == GoContentAlignment.BottomRight) x = bounds.Right - textWidth;

                    canvas.DrawText(lines[i], x, y, font, p);
                }
            }

            canvas.RestoreToCount(sp);
        }

        /// <summary>
        /// 캔버스에 테두리가 있는 텍스트를 그립니다.
        /// </summary>
        public static void DrawText(SKCanvas canvas, string? text, string fontName, GoFontStyle fontStyle, float fontSize, SKRect bounds, SKColor color, SKColor bordercolor, float bordersize=1, GoContentAlignment align = GoContentAlignment.MiddleCenter, bool wordWrap = true)
        {
            var tf = GetTypeface(fontName, fontStyle);

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

                p.StrokeJoin = SKStrokeJoin.Round;
                p.IsStroke = true;
                p.StrokeWidth = bordersize;
                p.Color = bordercolor;
                p.StrokeMiter = bordersize;
                p.StrokeCap = SKStrokeCap.Round;
                for (int i = 0; i < lines.Count; i++)
                {
                    float y = startY + (i * lineHeight) - font.Metrics.Ascent;
#if MeasureA
                    float textWidth = font.MeasureText(lines[i]) + 1;
#else
                    font.MeasureText(lines[i], out var vszrt); var textWidth = vszrt.Width;
#endif
                    float x = bounds.Left;
                    if (align == GoContentAlignment.TopCenter || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.BottomCenter) x = bounds.MidX - textWidth / 2;
                    else if (align == GoContentAlignment.TopRight || align == GoContentAlignment.MiddleRight || align == GoContentAlignment.BottomRight) x = bounds.Right - textWidth;
   
                    canvas.DrawText(lines[i], x, y, font, p);
                     
                }

                p.IsStroke = false;
                p.Color = color;
                for (int i = 0; i < lines.Count; i++)
                {
                    float y = startY + (i * lineHeight) - font.Metrics.Ascent;
#if MeasureA
                    float textWidth = font.MeasureText(lines[i]) + 1;
#else
                    font.MeasureText(lines[i], out var vszrt); var textWidth = vszrt.Width;
#endif
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
        /// <summary>
        /// 캔버스에 아이콘을 그립니다.
        /// </summary>
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
#if MeasureA
                float textWidth = font.MeasureText(fi.IconText);
#else
                font.MeasureText(fi.IconText, out var vszrt); var textWidth = vszrt.Width;
#endif

                float x = bounds.Left;
                if (align == GoContentAlignment.TopCenter || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.BottomCenter) x = bounds.MidX - textWidth / 2;
                else if (align == GoContentAlignment.TopRight || align == GoContentAlignment.MiddleRight || align == GoContentAlignment.BottomRight) x = bounds.Right - textWidth;

                p.IsStroke = false;
                p.Color = color;
                canvas.DrawText(fi.IconText, x, y, font, p);
            }
        }

        /// <summary>지정된 영역에 아이콘을 그립니다.</summary>
        /// <param name="canvas">대상 캔버스.</param>
        /// <param name="iconString">아이콘 식별 문자열.</param>
        /// <param name="iconSize">아이콘 크기.</param>
        /// <param name="bounds">그릴 영역.</param>
        /// <param name="color">아이콘 채움 색상.</param>
        /// <param name="colorborder">아이콘 테두리 색상.</param>
        /// <param name="align">정렬 방식.</param>
        public static void DrawIcon(SKCanvas canvas, string? iconString, float iconSize, SKRect bounds, SKColor color, SKColor colorborder, GoContentAlignment align = GoContentAlignment.MiddleCenter)
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
#if MeasureA
                float textWidth = font.MeasureText(fi.IconText);
#else
                font.MeasureText(fi.IconText, out var vszrt); var textWidth = vszrt.Width;
#endif

                float x = bounds.Left;
                if (align == GoContentAlignment.TopCenter || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.BottomCenter) x = bounds.MidX - textWidth / 2;
                else if (align == GoContentAlignment.TopRight || align == GoContentAlignment.MiddleRight || align == GoContentAlignment.BottomRight) x = bounds.Right - textWidth;

                if (color != SKColors.Transparent)
                {
                    p.IsStroke = false;
                    p.Color = color;
                    canvas.DrawText(fi.IconText, x, y, font, p);
                }

                if (colorborder != SKColors.Transparent)
                {
                    p.IsStroke = true;
                    p.StrokeWidth = 1;
                    p.Color = colorborder;
                    canvas.DrawText(fi.IconText, x, y, font, p);
                }

            }
        }

        /// <summary>지정된 영역에 회전된 아이콘을 그립니다.</summary>
        /// <param name="canvas">대상 캔버스.</param>
        /// <param name="iconString">아이콘 식별 문자열.</param>
        /// <param name="iconSize">아이콘 크기.</param>
        /// <param name="rotate">회전 각도(도).</param>
        /// <param name="bounds">그릴 영역.</param>
        /// <param name="fill">아이콘 채움 색상.</param>
        /// <param name="border">아이콘 테두리 색상.</param>
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
#if MeasureA
                    float textWidth = font.MeasureText(fi.IconText);
#else
                    font.MeasureText(fi.IconText, out var vszrt); var textWidth = vszrt.Width;
#endif
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

        /// <summary>지정된 영역에 아이콘을 테두리 크기와 함께 그립니다.</summary>
        /// <param name="canvas">대상 캔버스.</param>
        /// <param name="iconString">아이콘 식별 문자열.</param>
        /// <param name="iconSize">아이콘 크기.</param>
        /// <param name="bounds">그릴 영역.</param>
        /// <param name="color">아이콘 채움 색상.</param>
        /// <param name="colorborder">아이콘 테두리 색상.</param>
        /// <param name="borderSize">테두리 크기.</param>
        /// <param name="align">정렬 방식.</param>
        public static void DrawIcon(SKCanvas canvas, string? iconString, float iconSize, SKRect bounds, SKColor color, SKColor colorborder, int borderSize, GoContentAlignment align = GoContentAlignment.MiddleCenter)
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
#if MeasureA
                float textWidth = font.MeasureText(fi.IconText);
#else
                font.MeasureText(fi.IconText, out var vszrt); var textWidth = vszrt.Width;
#endif

                float x = bounds.Left;
                if (align == GoContentAlignment.TopCenter || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.BottomCenter) x = bounds.MidX - textWidth / 2;
                else if (align == GoContentAlignment.TopRight || align == GoContentAlignment.MiddleRight || align == GoContentAlignment.BottomRight) x = bounds.Right - textWidth;

                if (colorborder != SKColors.Transparent)
                {
                    p.IsStroke = true;
                    p.StrokeWidth = borderSize;
                    p.Color = colorborder;
                    canvas.DrawText(fi.IconText, x, y, font, p);
                }

                if (color != SKColors.Transparent)
                {
                    p.IsStroke = false;
                    p.Color = color;
                    canvas.DrawText(fi.IconText, x, y, font, p);
                }
            }
        }

        /// <summary>아이콘의 <see cref="SKPath"/>를 생성합니다.</summary>
        /// <param name="iconString">아이콘 식별 문자열.</param>
        /// <param name="iconSize">아이콘 크기.</param>
        /// <param name="rotate">회전 각도(도).</param>
        /// <param name="bounds">대상 영역.</param>
        /// <returns>아이콘을 나타내는 <see cref="SKPath"/>. 아이콘이 없을 경우 null.</returns>
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

#if MeasureA
                float textWidth = font.MeasureText(fi.IconText);
#else
                font.MeasureText(fi.IconText, out var vszrt); var textWidth = vszrt.Width;
#endif

                float y = -totalTextHeight / 2F - font.Metrics.Ascent;
                float x = -textWidth / 2;

                ret = font.GetTextPath(fi.IconText, new SKPoint(x, y));

                var rotationMatrix = SKMatrix.CreateRotationDegrees(rotate, 0, 0); ret.Transform(rotationMatrix);
                var translateMatrix = SKMatrix.CreateTranslation(bounds.MidX, bounds.MidY); ret.Transform(translateMatrix);
            }
            return ret;
        }
        #endregion

        #region DrawTextIcon
        /// <summary>
        /// 캔버스에 텍스트와 아이콘을 함께 그립니다.
        /// </summary>
        public static void DrawTextIcon(SKCanvas canvas, string? text, string fontName, GoFontStyle fontStyle, float fontSize, string? iconString, float iconSize, GoDirectionHV direction, float gap, SKRect bounds, SKColor color, GoContentAlignment align = GoContentAlignment.MiddleCenter, bool wordWrap = true)
        {
            var fi = iconString != null ? GoIconManager.GetIcon(iconString) : null;
            if (fi != null && iconSize > 0)
            {
                using var font = new SKFont(fi.FontFamily, iconSize);
#if MeasureA
                var w = font.MeasureText(fi.IconText, out var vrtico); var vrtsz = new SKSize(w, font.Spacing);
#else
                font.MeasureText(fi.IconText, out var vrtico); var vrtsz = vrtico.Size;
#endif

                var (rtIco, rtText) = TextIconBounds(text, fontName, fontStyle, fontSize, vrtico.Size, direction, gap, bounds, align);
                DrawIcon(canvas, iconString, iconSize, rtIco, color, align);
                if (!string.IsNullOrEmpty(text)) DrawText(canvas, text, fontName, fontStyle, fontSize, rtText, color, align, wordWrap);

            }
            else DrawText(canvas, text, fontName, fontStyle, fontSize, bounds, color, align, wordWrap);
        }

        /// <summary>텍스트와 아이콘을 함께 그립니다.</summary>
        /// <param name="canvas">대상 캔버스.</param>
        /// <param name="text">텍스트.</param>
        /// <param name="fontName">폰트 이름.</param>
        /// <param name="fontStyle">폰트 스타일.</param>
        /// <param name="fontSize">폰트 크기.</param>
        /// <param name="iconString">아이콘 식별 문자열.</param>
        /// <param name="iconSize">아이콘 크기.</param>
        /// <param name="direction">아이콘과 텍스트의 배치 방향.</param>
        /// <param name="gap">아이콘과 텍스트 사이 간격.</param>
        /// <param name="bounds">그릴 영역.</param>
        /// <param name="textcolor">텍스트 색상.</param>
        /// <param name="iconcolor">아이콘 색상.</param>
        /// <param name="align">정렬 방식.</param>
        /// <param name="wordWrap">단어 단위 줄 바꿈 여부.</param>
        public static void DrawTextIcon(SKCanvas canvas, string? text, string fontName, GoFontStyle fontStyle, float fontSize, string? iconString, float iconSize, GoDirectionHV direction, float gap, SKRect bounds, SKColor textcolor, SKColor iconcolor, GoContentAlignment align = GoContentAlignment.MiddleCenter, bool wordWrap = true)
        {
            var fi = iconString != null ? GoIconManager.GetIcon(iconString) : null;
            if (fi != null && iconSize > 0)
            {
                using var font = new SKFont(fi.FontFamily, iconSize);
#if MeasureA
                var w = font.MeasureText(fi.IconText, out var vrtico); var vrtsz = new SKSize(w, font.Spacing);
#else
                font.MeasureText(fi.IconText, out var vrtico); var vrtsz = vrtico.Size;
#endif
                var (rtIco, rtText) = TextIconBounds(text, fontName, fontStyle, fontSize, vrtico.Size, direction, gap, bounds, align);
                DrawIcon(canvas, iconString, iconSize, rtIco, iconcolor, align);
                if (!string.IsNullOrEmpty(text)) DrawText(canvas, text, fontName, fontStyle, fontSize, rtText, textcolor, align, wordWrap);

            }
            else DrawText(canvas, text, fontName, fontStyle, fontSize, bounds, textcolor, align, wordWrap);
        }
        #endregion

        #region MeasureText
        /// <summary>
        /// 텍스트의 렌더링 크기를 측정합니다.
        /// </summary>
        public static SKSize MeasureText(string text, string fontName, GoFontStyle fontStyle, float fontSize)
        {
            var tf = GetTypeface(fontName, fontStyle);
            using var font = new SKFont(tf, fontSize);
            var lines = LineText(text);
            var lineHeight = font.Spacing;
            var TH = lineHeight * lines.Count;
#if MeasureA
            var TW = lines.Count > 0 ? lines.Max(x => font.MeasureText(x)) + 1 : 0;
#else
            var TW = lines.Max(x => { font.MeasureText(x, out var vszrt); return vszrt.Width; });
#endif
            return new SKSize(TW, TH);
        }
        #endregion

        #region MeasureTextIcon
        /// <summary>
        /// 텍스트와 아이콘을 합친 렌더링 크기를 측정합니다.
        /// </summary>
        public static SKSize MeasureTextIcon(string? text, string fontName, GoFontStyle fontStyle, float fontSize, string? iconString, float iconSize, GoDirectionHV direction, float gap)
        {
            var ico = GoIconManager.GetIcon(iconString);
            if (ico != null)
            {
                using var ft = new SKFont(ico.FontFamily, iconSize);
#if MeasureA
                var w = ft.MeasureText(ico.IconText);
#else
                ft.MeasureText(ico.IconText, out var vszrt); var w = vszrt.Width;
#endif
                var iconSz = new SKSize(w, ft.Spacing);

                if (string.IsNullOrEmpty(text))
                {
                    return iconSz;
                }
                else
                {
                    var tf = GetTypeface(fontName, fontStyle);
                    using var font = new SKFont(tf, fontSize);

                    var lines = LineText(text);
                    var lineHeight = font.Spacing;
                    var TH = lineHeight * lines.Count;
#if MeasureA
                    var TW = lines.Count > 0 ? lines.Max(x => font.MeasureText(x)) + 1 : 0;
#else
                    var TW = lines.Max(x => { font.MeasureText(x, out var vszrt); return vszrt.Width; });
#endif
                    var szv = direction == GoDirectionHV.Horizon ?
                        new SKSize(iconSz.Width + gap + TW, Math.Max(TH, iconSz.Height)) :
                        new SKSize(Math.Max(TW, iconSz.Width), iconSz.Height + gap + TH);

                    return szv;
                }

            }
            else
            {
                var tf = GetTypeface(fontName, fontStyle);
                using var font = new SKFont(tf, fontSize);

                var lines = LineText(text);
                var lineHeight = font.Spacing;
                var TH = lineHeight * lines.Count;
#if MeasureA
                var TW = lines.Count > 0 ? lines.Max(x => font.MeasureText(x)) + 1 : 0;
#else
                var TW = lines.Max(x => { font.MeasureText(x, out var vszrt); return vszrt.Width; });
#endif
                return new SKSize(TW, font.Spacing);
            }
        }
        #endregion

        #region TextIconBounds
        /// <summary>텍스트와 아이콘의 영역을 계산합니다.</summary>
        /// <param name="text">텍스트.</param>
        /// <param name="fontName">폰트 이름.</param>
        /// <param name="fontStyle">폰트 스타일.</param>
        /// <param name="fontSize">폰트 크기.</param>
        /// <param name="iconSize">아이콘 크기.</param>
        /// <param name="direction">아이콘과 텍스트의 배치 방향.</param>
        /// <param name="gap">아이콘과 텍스트 사이 간격.</param>
        /// <param name="bounds">전체 영역.</param>
        /// <param name="align">정렬 방식.</param>
        /// <returns>아이콘 영역과 텍스트 영역의 튜플.</returns>
        public static (SKRect rtIcon, SKRect rtText) TextIconBounds(string? text, string fontName, GoFontStyle fontStyle, float fontSize, SKSize iconSize, GoDirectionHV direction, float gap, SKRect bounds, GoContentAlignment align)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (MathTool.MakeRectangle(bounds, iconSize, align), FromRect(0, 0, 0, 0));
            }
            else
            {
                var tf = GetTypeface(fontName, fontStyle);
                using var font = new SKFont(tf, fontSize);

                var lines = LineText(text);
                var lineHeight = font.Spacing;
                var TH = lineHeight * lines.Count;
#if MeasureA
                var TW = lines.Count > 0 ? lines.Max(x => font.MeasureText(x)) + 1 : 0;
#else
                var TW = lines.Max(x => { font.MeasureText(x, out var vszrt); return vszrt.Width; });
#endif
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

            if (text != null)
            {
                string[] rawLines = text.Replace("\r\n", "\n").Split('\n');

                foreach (string rawLine in rawLines)
                {
                    string[] words = rawLine.Split(' '); // 공백 기준 단어 분리
                    string currentLine = "";

                    foreach (string word in words)
                    {
                        string testLine = (currentLine == "") ? word : currentLine + " " + word;
#if MeasureA
                        float testWidth = font.MeasureText(testLine);
#else
                    font.MeasureText(testLine, out var vszrt); float testWidth = vszrt.Width;
#endif
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
            }

            return lines;
        }
        #endregion

        #region LineText
        static List<string> LineText(string text)
        {
            List<string> lines = [];

            if (text != null)
            {
                string[] rawLines = text.Replace("\r\n", "\n").Split('\n');

                foreach (string rawLine in rawLines)
                {
                    if (!string.IsNullOrEmpty(rawLine))
                        lines.Add(rawLine);
                }
            }

            return lines;
        }
        #endregion
        #endregion

        #region Box
        /// <summary>
        /// 캔버스에 버튼을 그립니다. 플랫, 그라데이션 등의 스타일을 지원합니다.
        /// </summary>
        public static void DrawButton(SKCanvas canvas, GoTheme thm, SKRect bounds, SKColor fillcolor, SKColor borderColor, GoRoundType round, float corner, bool clean = true, float borderSize = 1F, GoButtonFillStyle style = GoButtonFillStyle.Flat, bool bDown = false)
        {
            using var p = new SKPaint();
            p.IsAntialias = true;

            if (clean)
            {
                bounds = Int(bounds);
                bounds.Offset(0.5F, 0.5F);
            }

            if (style == GoButtonFillStyle.Flat)
            {
                #region Flat
                if (fillcolor != SKColors.Transparent)
                {
                    p.IsStroke = false;
                    p.Color = fillcolor;
                    if (round == GoRoundType.Ellipse) canvas.DrawOval(bounds, p);
                    else
                    {
                        SKRoundRect rtr = new SKRoundRect(bounds, corner);
                        SetRound(rtr, round, corner);
                        canvas.DrawRoundRect(rtr, p);
                    }
                }

                if (borderColor != SKColors.Transparent)
                {
                    p.IsStroke = true;
                    p.StrokeWidth = borderSize;
                    p.Color = borderColor;

                    float strokeHalf = p.StrokeWidth / 2f;
                    bounds.Inflate(-strokeHalf, -strokeHalf);
                    if (round == GoRoundType.Ellipse) canvas.DrawOval(bounds, p);
                    else
                    {
                        SKRoundRect rtr = new SKRoundRect(bounds, corner);
                        SetRound(rtr, round, corner);
                        canvas.DrawRoundRect(rtr, p);
                    }
                }
                #endregion
            }
            else if(style == GoButtonFillStyle.Gradient)
            {
                #region Gradient
                if (fillcolor != SKColors.Transparent)
                {
                    using var sh = SKShader.CreateLinearGradient(new SKPoint(bounds.Left, bounds.Top), new SKPoint(bounds.Left, bounds.Bottom),
                                                               !bDown ? [fillcolor.BrightnessTransmit(thm.GradientLightBrightness), fillcolor.BrightnessTransmit(thm.GradientDarkBrightness)] : [fillcolor.BrightnessTransmit(thm.GradientDarkBrightness), fillcolor.BrightnessTransmit(thm.GradientLightBrightness)], 
                                                                SKShaderTileMode.Clamp);
                    p.IsStroke = false;
                    p.Shader = sh;
                    if (round == GoRoundType.Ellipse) canvas.DrawOval(bounds, p);
                    else
                    {
                        SKRoundRect rtr = new SKRoundRect(bounds, corner);
                        SetRound(rtr, round, corner);
                        canvas.DrawRoundRect(rtr, p);
                    }
                    p.Shader = null;
                }

                if (borderColor != SKColors.Transparent)
                {
                    p.IsStroke = true;
                    p.StrokeWidth = borderSize;
                    p.Color = borderColor;

                    float strokeHalf = p.StrokeWidth / 2f;
                    bounds.Inflate(-strokeHalf, -strokeHalf);
                    if (round == GoRoundType.Ellipse) canvas.DrawOval(bounds, p);
                    else
                    {
                        SKRoundRect rtr = new SKRoundRect(bounds, corner);
                        SetRound(rtr, round, corner);
                        canvas.DrawRoundRect(rtr, p);
                    }
                }
                #endregion
            }
            else if(style == GoButtonFillStyle.Emboss)
            {
                #region Emboss
                if (fillcolor != SKColors.Transparent)
                {
                    #region Fill
                    p.IsStroke = false;
                    p.Color = fillcolor;
                    if (round == GoRoundType.Ellipse) canvas.DrawOval(bounds, p);
                    else
                    {
                        SKRoundRect rtr = new SKRoundRect(bounds, corner);
                        SetRound(rtr, round, corner);
                        canvas.DrawRoundRect(rtr, p);
                    }
                    #endregion

                    #region Bevel
                    var vbnd = bounds; vbnd.Inflate(-(borderSize / 2F * 3F), -(borderSize / 2F * 3F));
                    var cp = new SKPoint(vbnd.MidX, vbnd.MidY);
                    var lt = new SKPoint(vbnd.Left, vbnd.Top);
                    var rb = new SKPoint(vbnd.Right, vbnd.Bottom);

                    var a = Convert.ToSingle(90 - MathTool.GetAngle(lt, rb));
                    var d = Convert.ToSingle(MathTool.GetDistance(cp, lt));

                    SKPoint gs = MathTool.GetPointWithAngle(cp, a + 180, d);
                    SKPoint ge = MathTool.GetPointWithAngle(cp, a, d);

                    using var sh = SKShader.CreateLinearGradient(gs, ge, !bDown ? [fillcolor.BrightnessTransmit(thm.GradientLightBrightness), fillcolor.BrightnessTransmit(thm.GradientLightBrightness), fillcolor.BrightnessTransmit(thm.GradientDarkBrightness), fillcolor.BrightnessTransmit(thm.GradientDarkBrightness)] :
                                                                                  [fillcolor.BrightnessTransmit(thm.GradientDarkBrightness), fillcolor.BrightnessTransmit(thm.GradientDarkBrightness), fillcolor.BrightnessTransmit(thm.GradientLightBrightness), fillcolor.BrightnessTransmit(thm.GradientLightBrightness)],
                                                                                  [0F, 0.5F, 0.5F, 1F], SKShaderTileMode.Clamp);
                   

                    p.StrokeWidth = borderSize + (borderSize / 2F);               
                    p.IsStroke = true;
                    p.Shader = sh;

                    if (round == GoRoundType.Ellipse) canvas.DrawOval(vbnd, p);
                    else
                    {
                        SKRoundRect rtr = new SKRoundRect(vbnd, corner);
                        SetRound(rtr, round, corner);
                        canvas.DrawRoundRect(rtr, p);
                    }

                    p.Shader = null;
                    #endregion
                }

                if (borderColor != SKColors.Transparent)
                {
                    #region Boarder
                    p.IsStroke = true;
                    p.StrokeWidth = borderSize;
                    p.Color = borderColor;

                    float strokeHalf = p.StrokeWidth / 2f;
                    bounds.Inflate(-strokeHalf, -strokeHalf);
                    if (round == GoRoundType.Ellipse) canvas.DrawOval(bounds, p);
                    else
                    {
                        SKRoundRect rtr = new SKRoundRect(bounds, corner);
                        SetRound(rtr, round, corner);
                        canvas.DrawRoundRect(rtr, p);
                    }
                    #endregion
                }
                #endregion
            }
        }

        /// <summary>
        /// Elevation 단계(0~5)에 따른 드롭 그림자를 박스 영역 외곽에 그립니다.
        /// 부모 컨테이너에서 자식 그리기 전에 호출되어, 자식 ClipRect 밖에 그림자가 spread됩니다.
        /// </summary>
        /// <param name="canvas">SkiaSharp 캔버스</param>
        /// <param name="bounds">컨트롤 영역 (그림자 외곽 기준)</param>
        /// <param name="elevation">Elevation 레벨 (0=없음, 1~5=점진적 강도)</param>
        /// <param name="round">모서리 라운드 타입</param>
        /// <param name="corner">모서리 라운드 반경</param>
        /// <param name="shadowColor">그림자 색상</param>
        /// <param name="shadowAlpha">그림자 베이스 알파 (0~255)</param>
        public static void DrawElevationShadow(SKCanvas canvas, SKRect bounds, int elevation, GoRoundType round, float corner, SKColor shadowColor, byte shadowAlpha)
        {
            if (elevation <= 0) return;
            elevation = Math.Min(5, elevation);

            // Level 1~5에 대한 (offsetX, offsetY, blur, alpha 가중치)
            // 좌상단에서 빛이 떨어진다고 가정 → 우하단으로 그림자
            (float ox, float oy, float blur, float w)[] table =
            [
                (0, 0, 0, 0),        // 0 (사용 안함)
                (1, 1, 3, 0.40f),    // 1
                (1, 2, 6, 0.55f),    // 2
                (2, 4, 10, 0.70f),   // 3
                (3, 6, 14, 0.85f),   // 4
                (4, 8, 20, 1.00f),   // 5
            ];
            var (ox, oy, blur, w) = table[elevation];

            byte alpha = (byte)Math.Clamp(shadowAlpha * w, 0, 255);
            var sc = new SKColor(shadowColor.Red, shadowColor.Green, shadowColor.Blue, alpha);

            using var p = new SKPaint { IsAntialias = true, Color = sc };
            p.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2f);

            var rt = new SKRect(bounds.Left + ox, bounds.Top + oy, bounds.Right + ox, bounds.Bottom + oy);

            if (round == GoRoundType.Rect || corner <= 0)
            {
                canvas.DrawRect(rt, p);
            }
            else if (round == GoRoundType.Ellipse)
            {
                canvas.DrawOval(rt, p);
            }
            else
            {
                var rrect = new SKRoundRect(rt, corner);
                SetRound(rrect, round, corner);
                canvas.DrawRoundRect(rrect, p);
            }
        }

        /// <summary>
        /// 캔버스에 단색 박스를 그립니다.
        /// </summary>
        public static void DrawBox(SKCanvas canvas, SKRect bounds, SKColor color, GoRoundType round, float corner, bool clean = true, float borderSize = 1F) => DrawBox(canvas, bounds, color, color, round, corner, clean, borderSize);
        /// <summary>
        /// 캔버스에 채우기 및 테두리 색상이 있는 박스를 그립니다.
        /// </summary>
        public static void DrawBox(SKCanvas canvas, SKRect bounds, SKColor fillcolor, SKColor borderColor, GoRoundType round, float corner, bool clean = true, float borderSize = 1F)
        {
            using var p = new SKPaint();
            p.IsAntialias = true;

            if (clean)
            {
                bounds = Int(bounds);
                bounds.Offset(0.5F, 0.5F);
            }

            if (fillcolor != SKColors.Transparent)
            {
                p.IsStroke = false;
                p.Color = fillcolor;

                if (round == GoRoundType.Ellipse)
                    canvas.DrawOval(bounds, p);
                else
                {
                    SKRoundRect rtr = new SKRoundRect(bounds, corner);
                    SetRound(rtr, round, corner);
                    canvas.DrawRoundRect(rtr, p);
                }
            }

            if (borderColor != SKColors.Transparent)
            {
                p.IsStroke = true;
                p.StrokeWidth = borderSize;
                p.Color = borderColor;
                
                float strokeHalf = p.StrokeWidth / 2f;
                bounds.Inflate(-strokeHalf, -strokeHalf);

                if (round == GoRoundType.Ellipse)
                    canvas.DrawOval(bounds, p);
                else
                {
                    SKRoundRect rtr = new SKRoundRect(bounds, corner);
                    SetRound(rtr, round, corner);
                    canvas.DrawRoundRect(rtr, p);
                }
            }
        }

        /// <summary>둥근 사각형 박스를 그립니다.</summary>
        /// <param name="canvas">대상 캔버스.</param>
        /// <param name="bounds">박스 영역.</param>
        /// <param name="fillcolor">채움 색상.</param>
        /// <param name="borderColor">테두리 색상.</param>
        public static void DrawBox(SKCanvas canvas, SKRoundRect bounds, SKColor fillcolor, SKColor borderColor)
        {
            using var p = new SKPaint { IsAntialias = true };

            if (fillcolor != SKColors.Transparent)
            {
                p.IsStroke = false;
                p.Color = fillcolor;
                canvas.DrawRoundRect(bounds, p);
            }

            if (borderColor != SKColors.Transparent)
            {
                p.IsStroke = true;
                p.StrokeWidth = 1F;
                p.Color = borderColor;

                float strokeHalf = p.StrokeWidth / 2f;
                bounds.Inflate(-strokeHalf, -strokeHalf);
                canvas.DrawRoundRect(bounds, p);
            }
        }

        /// <summary>둥근 사각형 박스를 지정된 테두리 두께로 그립니다.</summary>
        /// <param name="canvas">대상 캔버스.</param>
        /// <param name="bounds">박스 영역.</param>
        /// <param name="fillcolor">채움 색상.</param>
        /// <param name="borderColor">테두리 색상.</param>
        /// <param name="borderSize">테두리 두께.</param>
        public static void DrawBox(SKCanvas canvas, SKRoundRect bounds, SKColor fillcolor, SKColor borderColor, float borderSize)
        {
            using var p = new SKPaint { IsAntialias = true };

            if (fillcolor != SKColors.Transparent)
            {
                p.IsStroke = false;
                p.Color = fillcolor;
                canvas.DrawRoundRect(bounds, p);

            }

            if (borderColor != SKColors.Transparent)
            {
                p.IsStroke = true;
                p.StrokeWidth = borderSize;
                p.Color = borderColor;

                float strokeHalf = p.StrokeWidth / 2f;
                bounds.Inflate(-strokeHalf, -strokeHalf);
                canvas.DrawRoundRect(bounds, p);
            }
        }
        #endregion

        #region Circle
        /// <summary>
        /// 캔버스에 원을 그립니다.
        /// </summary>
        public static void DrawCircle(SKCanvas canvas, SKRect bounds, SKColor color, bool clean = true)
        {
            using var p = new SKPaint();
            p.IsAntialias = true;

            if (clean)
            {
                bounds = Int(bounds);
                bounds.Offset(0.5F, 0.5F);
            }

            p.IsStroke = false;
            p.Color = color;
            canvas.DrawOval(bounds, p);
        }
        /// <summary>
        /// 캔버스에 지정된 중심과 반지름으로 원을 그립니다.
        /// </summary>
        public static void DrawCircle(SKCanvas canvas, float x, float y, float radius, SKColor color, bool clean = true)
        {
            using var p = new SKPaint();
            p.IsAntialias = true;

            if (clean)
            {
                x = Int(x);
                y = Int(y);
                radius = Int(radius);
            }

            p.IsStroke = false;
            p.Color = color;
            canvas.DrawCircle(x, y, radius, p);
        }
        #endregion

        #region Grid
        #region Devide
        #region Base
        #region Grid
        /// <summary>
        /// 사각형을 열과 행으로 분할하여 그리드 셀 배열을 생성합니다.
        /// </summary>
        /// <param name="bounds">분할할 사각형</param>
        /// <param name="cols">열 크기 정의 배열 (예: "100px", "1*", "2*")</param>
        /// <param name="rows">행 크기 정의 배열</param>
        /// <returns>그리드 셀 사각형 2차원 배열</returns>
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
        /// <summary>
        /// 사각형을 열로 분할합니다.
        /// </summary>
        /// <param name="bounds">분할할 사각형</param>
        /// <param name="cols">열 크기 정의 배열</param>
        /// <returns>열 사각형 배열</returns>
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
        /// <summary>
        /// 사각형을 행으로 분할합니다.
        /// </summary>
        /// <param name="bounds">분할할 사각형</param>
        /// <param name="rows">행 크기 정의 배열</param>
        /// <returns>행 사각형 배열</returns>
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
                if (size.EndsWith("px") && float.TryParse(size.Replace("px", ""), out var val1))
                {
                    float px = val1;
                    result.Add(px);
                    fixedTotal += px;
                }
                else if (size.EndsWith("%")&& float.TryParse(size.Replace("%", ""), out var val2))
                {
                    float percent = val2 / 100f;
                    result.Add(-percent); 
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
        #region ValidSizes
        /// <summary>크기 문자열 배열이 유효한지 확인합니다.</summary>
        /// <param name="sizes">검사할 크기 문자열 배열(예: "100px", "50%").</param>
        /// <returns>모든 크기가 유효하면 true, 그렇지 않으면 false.</returns>
        public static bool ValidSizes(string[] sizes)
        {
            List<bool> ret = [];
            foreach (var size in sizes)
            {
                if (size.EndsWith("px") && float.TryParse(size.Replace("px", ""), out _))
                {
                    ret.Add(true);
                }
                else if (size.EndsWith("%") && float.TryParse(size.Replace("%", ""), out _))
                {
                    ret.Add(true);
                }
            }
            return ret.Count == sizes.Length;
        }
        #endregion
        #endregion

        #region MinimumSize
        #region Grid
        /// <summary>열과 행 크기 정보를 기반으로 2차원 그리드 영역을 계산합니다.</summary>
        /// <param name="bounds">전체 영역.</param>
        /// <param name="cols">열 크기 문자열 배열.</param>
        /// <param name="rows">행 크기 문자열 배열.</param>
        /// <param name="minCols">열 최소 크기 배열.</param>
        /// <param name="minRows">행 최소 크기 배열.</param>
        /// <returns>행 x 열 크기의 <see cref="SKRect"/> 2차원 배열.</returns>
        public static SKRect[,] Grid(SKRect bounds, string[] cols, string[] rows, float[] minCols, float[] minRows)
        {
            if (cols.Length != minCols.Length) throw new ArgumentException("cols와 minCols 길이는 같아야 합니다.");
            if (rows.Length != minRows.Length) throw new ArgumentException("rows와 minRows 길이는 같아야 합니다.");

            int colCount = cols.Length;
            int rowCount = rows.Length;
            SKRect[,] grid = new SKRect[rowCount, colCount];

            float[] colWidths = ParseSizes(cols, bounds.Width, minCols);
            float[] rowHeights = ParseSizes(rows, bounds.Height, minRows);

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
        /// <summary>열 크기 정보를 기반으로 열 영역을 계산합니다.</summary>
        /// <param name="bounds">전체 영역.</param>
        /// <param name="cols">열 크기 문자열 배열.</param>
        /// <param name="minCols">열 최소 크기 배열.</param>
        /// <returns>열 개수 크기의 <see cref="SKRect"/> 배열.</returns>
        public static SKRect[] Columns(SKRect bounds, string[] cols, float[] minCols)
        {
            if (cols.Length != minCols.Length) throw new ArgumentException("cols와 minCols 길이는 같아야 합니다.");

            int colCount = cols.Length;
            SKRect[] grid = new SKRect[colCount];

            float[] colWidths = ParseSizes(cols, bounds.Width, minCols);

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
        /// <summary>행 크기 정보를 기반으로 행 영역을 계산합니다.</summary>
        /// <param name="bounds">전체 영역.</param>
        /// <param name="rows">행 크기 문자열 배열.</param>
        /// <param name="minRows">행 최소 크기 배열.</param>
        /// <returns>행 개수 크기의 <see cref="SKRect"/> 배열.</returns>
        public static SKRect[] Rows(SKRect bounds, string[] rows, float[] minRows)
        {
            if (rows.Length != minRows.Length) throw new ArgumentException("rows와 minRows 길이는 같아야 합니다.");

            int rowCount = rows.Length;
            SKRect[] grid = new SKRect[rowCount];

            float[] rowHeights = ParseSizes(rows, bounds.Height, minRows);

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
        /// <summary>크기 문자열 배열을 실제 크기 값으로 변환합니다.</summary>
        /// <param name="sizes">크기 문자열 배열(예: "100px", "50%").</param>
        /// <param name="totalSize">전체 크기.</param>
        /// <param name="minimumSizes">최소 크기 배열.</param>
        /// <returns>계산된 크기 값 배열.</returns>
        public static float[] ParseSizes(string[] sizes, float totalSize, float[] minimumSizes)
        {
            List<float> result = new List<float>();
            float totalPercentage = 0;
            float fixedTotal = 0;

            for (int i = 0; i < sizes.Length; i++)
            {
                string size = sizes[i];
                if (size.EndsWith("px") && float.TryParse(size.Replace("px", ""), out var val1))
                {
                    float px = Math.Max(val1, minimumSizes[i]);
                    result.Add(px);
                    fixedTotal += px;
                }
                else if (size.EndsWith("%") && float.TryParse(size.Replace("%", ""), out var val2))
                {
                    float percent = val2 / 100f;
                    result.Add(-percent);
                    totalPercentage += percent;
                }
                else
                {
                    result.Add(minimumSizes[i]);
                    fixedTotal += minimumSizes[i];
                }
            }

            float remainingSize = totalSize - fixedTotal;

            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] < 0)
                {
                    float desired = remainingSize * -result[i];
                    result[i] = Math.Max(desired, minimumSizes[i]);
                }
            }

            return result.ToArray();
        }
        #endregion
        #endregion
        #endregion

        #region Merge
        /// <summary>2차원 그리드에서 지정된 범위의 셀들을 병합한 영역을 반환합니다.</summary>
        /// <param name="rts">그리드 영역 배열.</param>
        /// <param name="col">시작 열 인덱스.</param>
        /// <param name="row">시작 행 인덱스.</param>
        /// <param name="colspan">병합할 열 개수.</param>
        /// <param name="rowspan">병합할 행 개수.</param>
        /// <returns>병합된 <see cref="SKRect"/>.</returns>
        public static SKRect Merge(SKRect[,] rts, int col, int row, int colspan, int rowspan)
        {
            var rtLT = Util.FromRect(rts[row, col]);
            var rtRB = rts[row + rowspan - 1, col + colspan - 1];
            rtLT.Right = rtRB.Right;
            rtLT.Bottom = rtRB.Bottom;

            return rtLT;
        }

        /// <summary>열 배열에서 지정된 범위의 영역들을 가로 방향으로 병합합니다.</summary>
        /// <param name="rts">열 영역 배열.</param>
        /// <param name="col">시작 열 인덱스.</param>
        /// <param name="colspan">병합할 열 개수.</param>
        /// <returns>병합된 <see cref="SKRect"/>.</returns>
        public static SKRect MergeH(SKRect[] rts, int col, int colspan)
        {
            var rtL = Util.FromRect(rts[col]);
            var rtR = rts[col + colspan - 1];
            rtL.Right = rtR.Right;

            return rtL;
        }

        /// <summary>행 배열에서 지정된 범위의 영역들을 세로 방향으로 병합합니다.</summary>
        /// <param name="rts">행 영역 배열.</param>
        /// <param name="row">시작 행 인덱스.</param>
        /// <param name="rowspan">병합할 행 개수.</param>
        /// <returns>병합된 <see cref="SKRect"/>.</returns>
        public static SKRect MergeV(SKRect[] rts, int row, int rowspan)
        {
            var rtT = Util.FromRect(rts[row]);
            var rtB = rts[row + rowspan - 1];
            rtT.Bottom = rtB.Bottom;

            return rtT;
        }

        /// <summary>
        /// 두 사각형을 합쳐 두 사각형을 모두 포함하는 최소 사각형을 반환합니다.
        /// </summary>
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
        /// <summary>
        /// 방향과 모서리 타입에 따라 분할된 각 항목의 모서리 둥글기 배열을 생성합니다.
        /// </summary>
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
        /// <summary>
        /// SKRoundRect에 모서리 둥글기를 설정합니다.
        /// </summary>
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
        /// <summary>
        /// float 값을 정수로 변환합니다.
        /// </summary>
        public static int Int(float v) => Convert.ToInt32(v);
        /// <summary>
        /// SKPoint의 좌표를 정수로 변환합니다.
        /// </summary>
        public static SKPoint Int(SKPoint v) => new SKPoint(Convert.ToInt32(v.X), Convert.ToInt32(v.Y));
        /// <summary>
        /// SKSize의 크기를 정수로 변환합니다.
        /// </summary>
        public static SKSize Int(SKSize v) => new SKSize(Convert.ToInt32(v.Width), Convert.ToInt32(v.Height));
        /// <summary>
        /// SKRect의 좌표를 정수로 변환합니다.
        /// </summary>
        public static SKRect Int(SKRect v) => new SKRect(Convert.ToInt32(v.Left), Convert.ToInt32(v.Top), Convert.ToInt32(v.Right), Convert.ToInt32(v.Bottom));
        #endregion

        #region Lamp
        /// <summary>
        /// 캔버스에 램프(표시등)를 그립니다.
        /// </summary>
        public static void DrawLamp(SKCanvas canvas, GoTheme thm, SKRect bounds, SKColor OnLampColor, SKColor OffLampColor, bool OnOff, bool Shadow = true)
        {
            #region Brightness
            var vS = 0.2F;
            var vE = -0.2F;

            if (thm.Dark)
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
            var cB = cE.BrightnessTransmit(thm.BorderBrightness);

            using var p = new SKPaint { IsAntialias = true };
            p.IsStroke = false;
            p.Color = cB;

            var rt = Util.Int(bounds);
            rt.Inflate(0.5F, 0.5F);

            var cx = Convert.ToSingle(MathTool.Map(0.25, 0, 1, rt.Left, rt.Right));
            var cy = Convert.ToSingle(MathTool.Map(0.25, 0, 1, rt.Top, rt.Bottom));
            using var imgf = SKImageFilter.CreateDropShadow(1, 1, 2, 2, Util.FromArgb(Convert.ToByte(thm.ShadowAlpha / (OnOff ? 1 : 5)), SKColors.Black));
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

        #region FindRect
        /// <summary>
        /// 사각형 목록에서 대상 사각형과 겹치는 범위의 시작/끝 인덱스를 이진 탐색으로 찾습니다.
        /// </summary>
        public static (int StartIndex, int EndIndex) FindRect(List<SKRect> rects, SKRect targetRect)
        {
            int start = BinarySearch(rects, targetRect.Top, true);
            int end = BinarySearch(rects, targetRect.Bottom, false);

            return (start, end);
        }

        private static int BinarySearch(List<SKRect> rects, float value, bool findStart)
        {
            int left = 0, right = rects.Count - 1, result = -1;

            while (left <= right)
            {
                int mid = (left + right) / 2;

                if (findStart)
                {
                    if (rects[mid].Bottom >= value)
                    {
                        result = mid;
                        right = mid - 1;  
                    }
                    else
                    {
                        left = mid + 1;
                    }
                }
                else
                {
                    if (rects[mid].Top <= value)
                    {
                        result = mid;
                        left = mid + 1;  
                    }
                    else
                    {
                        right = mid - 1;
                    }
                }
            }

            return result;
        }
        #endregion

        #region Color
        /// <summary>
        /// SKColor를 HSV(색상, 채도, 명도) double 값으로 변환합니다.
        /// </summary>
        public static void ToHsvDouble(SKColor color, out double h, out double s, out double v)
        {
            double EPSILON = 0.001;

            var r = color.Red / 255.0;
            var g = color.Green / 255.0;
            var b = color.Blue / 255.0;

            var min = Math.Min(Math.Min(r, g), b);
            var max = Math.Max(Math.Max(r, g), b);
            var delta = max - min;

            h = 0;
            s = 0;
            v = max;

            if (Math.Abs(delta) > EPSILON)
            {
                s = delta / max;

                var deltaR = (((max - r) / 6f) + (delta / 2f)) / delta;
                var deltaG = (((max - g) / 6f) + (delta / 2f)) / delta;
                var deltaB = (((max - b) / 6f) + (delta / 2f)) / delta;

                if (Math.Abs(r - max) < EPSILON)
                    h = deltaB - deltaG;
                else if (Math.Abs(g - max) < EPSILON)
                    h = (1f / 3f) + deltaR - deltaB;
                else // b == max
                    h = (2f / 3f) + deltaG - deltaR;

                if (h < 0f)
                    h += 1f;
                if (h > 1f)
                    h -= 1f;
            }

            h = h * 360f;
            s = s * 100f;
            v = v * 100f;
        }

        /// <summary>
        /// HSV(색상, 채도, 명도) double 값을 SKColor로 변환합니다.
        /// </summary>
        public static SKColor FromHsvDouble(double h, double s, double v)
        {
            double EPSILON = 0.001;

            h = h / 360.0;
            s = s / 100.0;
            v = v / 100.0;

            var r = v;
            var g = v;
            var b = v;

            if (Math.Abs(s) > EPSILON)
            {
                h = h * 6f;
                if (Math.Abs(h - 6f) < EPSILON) h = 0f;

                var hInt = (int)h;
                var v1 = v * (1f - s);
                var v2 = v * (1f - s * (h - hInt));
                var v3 = v * (1f - s * (1f - (h - hInt)));

                if (hInt == 0)
                {
                    r = v;
                    g = v3;
                    b = v1;
                }
                else if (hInt == 1)
                {
                    r = v2;
                    g = v;
                    b = v1;
                }
                else if (hInt == 2)
                {
                    r = v1;
                    g = v;
                    b = v3;
                }
                else if (hInt == 3)
                {
                    r = v1;
                    g = v2;
                    b = v;
                }
                else if (hInt == 4)
                {
                    r = v3;
                    g = v1;
                    b = v;
                }
                else
                {
                    r = v;
                    g = v1;
                    b = v2;
                }
            }

            return new SKColor(Convert.ToByte(r * 255.0), Convert.ToByte(g * 255.0), Convert.ToByte(b * 255.0));
        }
        #endregion

        #region FontSize
        /// <summary>
        /// 자동 폰트 크기를 높이 기반으로 계산합니다.
        /// </summary>
        public static float? FontSize(GoAutoFontSize sz, float height)
        {
            float? ret = null;

            switch(sz)
            {
                case GoAutoFontSize.XXS: ret = height * 0.2F; break;
                case GoAutoFontSize.XS: ret = height * 0.3F; break;
                case GoAutoFontSize.S: ret = height * 0.4F; break;
                case GoAutoFontSize.M: ret = height * 0.5F; break;
                case GoAutoFontSize.L: ret = height * 0.6F; break;
                case GoAutoFontSize.XL: ret = height * 0.7F; break;
                case GoAutoFontSize.XXL: ret = height * 0.8F; break;
            }

            return ret;
        }
        #endregion
        #endregion

        #region AllControls
        /// <summary>
        /// 컨테이너 내의 모든 컨트롤을 재귀적으로 수집합니다.
        /// </summary>
        public static List<IGoControl> AllControls(IGoContainer? container)
        {
            var ls = new List<IGoControl>();
            if (container != null)
                foreach (var c in container.Childrens)
                {
                    ls.Add(c);
                    if (c is GoTabControl tab) { foreach (var p in tab.TabPages) ls.AddRange(AllControls(p)); }
                    else if (c is GoSwitchPanel sw) { foreach (var p in sw.Pages) ls.AddRange(AllControls(p)); }
                    else if (c is IGoContainer vp) { ls.AddRange(AllControls(vp)); }
                }
            return ls;
        }

        /// <summary>
        /// 탭 페이지 내의 모든 컨트롤을 재귀적으로 수집합니다.
        /// </summary>
        public static List<IGoControl> AllControls(GoTabPage container)
        {
            var ls = new List<IGoControl>();
            if (container != null)
                foreach (var c in container.Childrens)
                {
                    ls.Add(c);
                    if (c is GoTabControl tab) { foreach (var p in tab.TabPages) ls.AddRange(AllControls(p)); }
                    else if (c is GoSwitchPanel sw) { foreach (var p in sw.Pages) ls.AddRange(AllControls(p)); }
                    else if (c is IGoContainer vp) { ls.AddRange(AllControls(vp)); }
                }
            return ls;
        }

        /// <summary>
        /// 서브 페이지 내의 모든 컨트롤을 재귀적으로 수집합니다.
        /// </summary>
        public static List<IGoControl> AllControls(GoSubPage container)
        {
            var ls = new List<IGoControl>();
            if (container != null)
                foreach (var c in container.Childrens)
                {
                    ls.Add(c);
                    if (c is GoTabControl tab) { foreach (var p in tab.TabPages) ls.AddRange(AllControls(p)); }
                    else if (c is GoSwitchPanel sw) { foreach (var p in sw.Pages) ls.AddRange(AllControls(p)); }
                    else if (c is IGoContainer vp) { ls.AddRange(AllControls(vp)); }

                }
            return ls;
        }
        #endregion
    }
}
