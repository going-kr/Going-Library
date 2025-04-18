using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Controls;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Provider;

namespace Going.UIEditor.Controls
{
    public class GoPropertyGrid : UI.Forms.Controls.GoControl
    {
        #region Properties
        #region ScrollBar
        internal bool _IsScrolling => scroll.IsTouchMoving || (scroll.IsTouchScrolling && scroll.TouchOffset != 0);
        internal bool IsScrolling { get; private set; }
        internal bool DrawScroll { get; set; } = true;
        internal bool Scrollable => scroll.ScrollView < scroll.ScrollTotal;
        public double ScrollPosition
        {
            get => scroll.ScrollPosition;
            set => scroll.ScrollPosition = value;
        }
        #endregion
        #region ItemHeight
        public int ItemHeight { get; set; } = 30;
        #endregion
        #region SelectedObjects
        public IEnumerable<object>? SelectedObjects
        {
            get => sels; 
            set
            {
                if(sels != value)
                {
                    sels = value;
                    SelectObjs(sels);
                }
            }
        }
        #endregion
        #region Font
        public string FontName => "나눔고딕";
        public GoFontStyle FontStyle => GoFontStyle.Normal;
        public float FontSize => 12;
        #endregion
        #endregion

        #region Member Variable
        private Scroll scroll = new Scroll { Direction = ScrollDirection.Vertical };
        private IEnumerable<object>? sels;

        private List<PropertyGridItem> properties = [];
        #endregion

        #region Event
        public event EventHandler? Edited;
        public event EventHandler? SelectedObjectChanged;
        #endregion

        #region Constructor
        public GoPropertyGrid()
        {
            SetStyle(ControlStyles.Selectable, true);

            scroll.GetScrollTotal = () => properties.Count * ItemHeight + 1;
            scroll.GetScrollTick = () => ItemHeight;
            scroll.GetScrollView = () => Height;
            scroll.Refresh = () => Invalidate();
        }
        #endregion

        #region Override
        #region Draw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            #region var 
            var canvas = e.Canvas;
            var thm = GoTheme.Current;
            var prj = Program.CurrentProject;
            using var p = new SKPaint { IsAntialias = true };
            using var pe = SKPathEffect.CreateDash([2, 2,], 2);

            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);  
            #endregion

            var (rtView, rtScroll, rtContent, rtsCol) = bounds();

            if(prj != null && properties.Count > 0)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    var crt = rtContent; crt.Inflate(1, 1);
                    canvas.ClipRect(crt);
                    canvas.Translate(rtContent.Left, spos + rtContent.Top); 
                    p.IsAntialias = false;

                    rtContent.Offset(0, -Convert.ToSingle(scroll.ScrollPositionWithOffset));
                    var (si, ei) = Util.FindRect(properties.Select(x => x.Bounds).ToList(), rtContent);

                    if (si >= 0 && si < properties.Count && ei >= 0 && ei < properties.Count)
                    {
                        #region Box
                        for (int i = si; i <= ei; i++)
                        {
                            var v = properties[i];
                            var t = v.Bounds.Top;
                            var b = v.Bounds.Bottom;

                            if (v.Type == PropertyGridItemType.Category)
                            {
                                p.Color = thm.Back;
                                p.IsStroke = false;
                                canvas.DrawRect(Util.FromRect(rtContent.Left, t, rtContent.Width + 1, ItemHeight), p);
                            }
                            else
                            {
                                p.Color = Util.FromArgb(36, 36, 36);
                                p.IsStroke = false;
                                canvas.DrawRect(Util.FromRect(rtsCol[0].Left, t, rtsCol[0].Width, ItemHeight), p);

                                p.Color = thm.Back;
                                p.IsStroke = false;
                                canvas.DrawRect(Util.FromRect(rtsCol[1].Left, t, rtsCol[1].Width, ItemHeight), p);
                                canvas.DrawRect(Util.FromRect(rtsCol[2].Left, t, rtsCol[2].Width, ItemHeight), p);
                            }
                        }
                        #endregion

                        #region Border
                        for (int i = si; i <= ei; i++)
                        {
                            var v = properties[i];
                            var t = v.Bounds.Top;
                            var b = v.Bounds.Bottom;

                            p.Color = Util.FromArgb(90, 90, 90);
                            p.StrokeWidth = 1;
                            p.IsStroke = true;
                            p.PathEffect = (v.Type == PropertyGridItemType.Category || (i + 1 < properties.Count && properties[i + 1].Type == PropertyGridItemType.Category) || i == properties.Count - 1) ? null : pe;
                            canvas.DrawLine(rtContent.Left, b, rtContent.Right, b, p);
                            p.PathEffect = null;
                            if (v.Type == PropertyGridItemType.Item)
                            {
                                p.PathEffect = pe;
                                canvas.DrawLine(rtsCol[0].Right, t, rtsCol[0].Right, b, p);
                                p.PathEffect = null;
                            }
                        }
                        #endregion

                        #region Content
                        for (int i = si; i <= ei; i++)
                        {
                            var v = properties[i];
                            v.Draw(canvas, rtsCol);
                        }
                        #endregion
                    }
                    p.IsAntialias = true;
                }

                scroll.Draw(canvas, rtScroll);
            }

            base.OnContentDraw(e);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(MouseEventArgs e)
        {
            var (rtView, rtScroll, rtContent, rtsCol) = bounds();
            var x = e.X;
            var y = e.Y;
            scroll.MouseDown(x, y, rtScroll);
            if (scroll.TouchMode && CollisionTool.Check(rtContent, x, y)) scroll.TouchDown(x, y);
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var (rtView, rtScroll, rtContent, rtsCol) = bounds();
            var x = e.X;
            var y = e.Y;
            scroll.MouseMove(x, y, rtScroll);
            if (scroll.TouchMode) scroll.TouchMove(x, y);
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            var x = e.X;
            var y = e.Y;
            scroll.MouseUp(x, y);
            if (scroll.TouchMode) scroll.TouchUp(x, y);
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var (rtView, rtScroll, rtContent, rtsCol) = bounds();
            var x = e.X;
            var y = e.Y;
            if (CollisionTool.Check(rtContent, x, y)) scroll.MouseWheel(x, y, e.Delta / 120F);
            Invalidate();
            base.OnMouseWheel(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            var (rtView, rtScroll, rtContent, rtsCol) = bounds();
            var x = e.X;
            var y = e.Y;
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtContent.Top - spos;

            itemLoop((i, item) =>
            {
                if (CollisionTool.Check(item.Bounds, x, ry))
                {
                     
                }
            });
            Invalidate();
            base.OnMouseClick(e);
        }
        #endregion
        #endregion

        #region Method
        #region bounds
        (SKRect rtView, SKRect rtScroll, SKRect rtContent, SKRect[] rtsCol) bounds()
        {
            var ScrollSize = Scroll.SC_WH;
            var vc = Convert.ToInt32(scroll.ScrollPositionWithOffset);

            var rtView = Util.FromRect(0, 0, Width - ScrollSize, Height);
            var rtScroll = Util.FromRect(Width - ScrollSize, 0, ScrollSize, Height);
            var rtContent = Util.FromRect(0, 0, Width - ScrollSize - 10, Height);

            var mw = 80;
            if (properties.Count > 0)
                mw = Math.Max(Convert.ToInt32(properties.Max(x => Util.MeasureText(x.Name, FontName, FontStyle, FontSize).Width)) + 20, 80);

            var rtsCol = Util.Columns(rtContent, [$"{mw}px", $"100%", $"30px"]);

            return (rtView, rtScroll, rtContent, rtsCol);
        }
        #endregion

        #region itemLoop
        void itemLoop(Action<int, PropertyGridItem> loop)
        {
            var (rtView, rtScroll, rtContent, rtsCell) = bounds();

            rtContent.Offset(0, -Convert.ToSingle(scroll.ScrollPositionWithOffset));       
            var (si, ei) = Util.FindRect(properties.Select(x => x.Bounds).ToList(), rtContent);

            if (si >= 0 && si < properties.Count && ei >= 0 && ei < properties.Count)
                for (int i = si; i <= ei; i++)
                    loop(i, properties[i]);
        }
        #endregion

        #region SelectObjs
        private void SelectObjs(IEnumerable<object>? objs)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var (rtView, rtScroll, rtContent, rtsCell) = bounds();

                foreach (var v in properties.Where(x => x is PropertyGridItemEdit)) ((PropertyGridItemEdit)v).ClearInput();

                properties.Clear();

                if (objs != null)
                {
                    #region make dic
                    var dic = new Dictionary<string, List<Obj>>();
                    foreach (var obj in objs)
                        foreach(var pi in obj.GetType().GetProperties().Where(x=>x.CanWrite && x.CanRead && Attribute.IsDefined(x, typeof(GoPropertyAttribute))))
                        {
                            if (!dic.ContainsKey(pi.Name)) dic[pi.Name] = [];

                            var attr = pi.GetCustomAttribute<GoPropertyAttribute>();
                            if (attr != null) dic[pi.Name].Add(new Obj(pi, obj, attr.Category, PCategory.Index(attr.Category), attr.Order));
                        }

                    foreach (var k in dic.Keys.ToArray())
                        if (dic[k].Count != objs.Count())
                            dic.Remove(k);

                    var keys = dic.OrderBy(x => x.Value.FirstOrDefault()?.CategoryOrder ?? int.MaxValue).ThenBy(x => x.Value.FirstOrDefault()?.PropertyOrder ?? int.MaxValue).Select(x => x.Key).ToList();
                    #endregion

                    string? cat = null;
                    float y = 0F;
                    foreach (var k in keys)
                    {
                        var os = dic[k];
                        var o = os.FirstOrDefault();
                        if (o != null)
                        {
                            #region Category
                            if (cat == null || cat != o.Category)
                            {
                                cat = o.Category;
                                properties.Add(new PropertyGridItem(this)
                                {
                                    Category = o.Category,
                                    Type = PropertyGridItemType.Category,
                                    Bounds = Util.FromRect(rtContent.Left, y, rtContent.Width, ItemHeight),
                                });
                                y += ItemHeight;
                            }
                            #endregion

                            #region Item
                            PropertyGridItem? item;

                            if (PropertyGridItem.IsEditType(o.Prop)) item = new PropertyGridItemEdit(this);
                            else if (PropertyGridItem.IsBool(o.Prop)) item = new PropertyGridItemBool(this);
                            else if (PropertyGridItem.IsEnum(o.Prop)) item = new PropertyGridItemEnum(this);
                            else if (PropertyGridItem.IsCollection(o.Prop)) item = new PropertyGridItemCollection(this);
                            else item = new PropertyGridItem(this);

                            item.Name = o.Name;
                            item.Type = PropertyGridItemType.Item;
                            item.Info = o.Prop;
                            item.Category = o.Category;
                            item.CategoryOrder = o.CategoryOrder;
                            item.PropertyOrder = o.PropertyOrder;
                            item.Bounds = Util.FromRect(rtContent.Left, y, rtContent.Width, ItemHeight);
                            properties.Add(item);
                            y += ItemHeight;
                            #endregion

                        }
                    }
                }
            }

            SelectedObjectChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
        #endregion
        #endregion
    }

    #region class : Obj
    class Obj(PropertyInfo pi, object? obj, string category, int cOrder, int pOrder)
    {
        public string Name { get; } = pi.Name;
        public string Category { get; } = category;
        public int CategoryOrder { get; } = cOrder;
        public int PropertyOrder { get; } = pOrder;
        public PropertyInfo Prop { get; } = pi;
        public object? Object { get; } = obj;
    }
    #endregion

    #region classes : PropertyGridItem
    public enum PropertyGridItemType { Category, Item }
    public class PropertyGridItem(GoPropertyGrid pg)
    {
        public string Name { get; set; } = "";
        public PropertyGridItemType Type { get; set; } = PropertyGridItemType.Item;
        public PropertyInfo? Info { get; set; }
        public string? Category { get; set; }
        public int CategoryOrder { get; set; } 
        public int PropertyOrder { get; set; } 
        public SKRect Bounds { get; set; }
        public GoPropertyGrid Grid => pg;

        #region Method
        #region Draw
        public void Draw(SKCanvas canvas, SKRect[] rtsCol)
        {
            var thm = GoTheme.Current;
            if (Type == PropertyGridItemType.Category)
            {
                var rt = Util.FromRect(Bounds.Left + 10, Bounds.Top, Bounds.Width - 10, Bounds.Height);
                Util.DrawText(canvas, $"·  {Category}", pg.FontName, pg.FontStyle, pg.FontSize, rt, thm.Fore, GoContentAlignment.MiddleLeft);
            }
            else
            {
                var rtTitle = Util.FromRect(rtsCol[0].Left, Bounds.Top, rtsCol[0].Width, Bounds.Height);
                var rtValue = Util.FromRect(rtsCol[1].Left, Bounds.Top, rtsCol[1].Width, Bounds.Height);
                var rtButton = Util.FromRect(rtsCol[2].Left, Bounds.Top, rtsCol[2].Width, Bounds.Height);

                Util.DrawText(canvas, Name, pg.FontName, pg.FontStyle, pg.FontSize, rtTitle, thm.Base5);

            }
        }
        #endregion

        #region Virtual
        public virtual void OnDrawValue(SKCanvas canvas, SKRect rtTitle, SKRect rtValue, SKRect rtButton) { }
        #endregion
        #endregion

        #region Static
        public static bool IsEditType(PropertyInfo? Info)
        {
            if (Info != null)
            {
                var tp = Info.PropertyType;
                return tp == typeof(byte) || tp == typeof(ushort) || tp == typeof(uint) || tp == typeof(ulong) ||
                       tp == typeof(sbyte) || tp == typeof(short) || tp == typeof(int) || tp == typeof(long) ||
                       tp == typeof(float) || tp == typeof(double) || tp == typeof(decimal) ||
                       tp == typeof(byte?) || tp == typeof(ushort?) || tp == typeof(uint?) || tp == typeof(ulong?) ||
                       tp == typeof(sbyte?) || tp == typeof(short?) || tp == typeof(int?) || tp == typeof(long?) ||
                       tp == typeof(float?) || tp == typeof(double?) || tp == typeof(decimal?) ||
                       tp == typeof(string) ||
                       tp == typeof(GoPadding) || tp == typeof(TimeSpan) || tp == typeof(SKRect);
            }
            else return false;
        }
        public static bool IsUseButton(PropertyInfo? Info) => IsColor(Info) || IsCollection(Info) || IsEnum(Info) || (Info?.PropertyType.IsEnum ?? false);
        public static bool IsColor(PropertyInfo? Info) => Info != null && Info.GetCustomAttribute(typeof(GoColorPropertyAttribute)) != null;
        public static bool IsCollection(PropertyInfo? Info) => Info != null && typeof(IEnumerable).IsAssignableFrom(Info.PropertyType) && Info.PropertyType != typeof(string);
        public static bool IsEnum(PropertyInfo? Info) => Info != null && Info.PropertyType.IsEnum;
        public static bool IsBool(PropertyInfo? Info) => Info != null && Info.PropertyType == typeof(bool);

        #region ValueToString
        protected string ValueToString(object? v)
        {
            var ret = "";

            if (v != null && Info != null)
            {
                var val = Info.GetValue(v);
                if (val != null)
                {
                    if (val is GoPadding pad) ret = $"{pad.Left}, {pad.Top}, {pad.Right}, {pad.Bottom}";
                    else if (val is SKRect rt) ret = $"{rt.Left}, {rt.Top}, {rt.Width}, {rt.Height}";
                    else ret = val?.ToString() ?? "";
                }
            }

            return ret;
        }
        #endregion
        #region ValueFromString
        protected object ValueFromString(string text)
        {
            object ret = "{NONE}";
            if (Info != null)
            {
                var tp = Info.PropertyType;

                #region var
                byte u8; ushort u16; uint u32; ulong u64;
                sbyte i8; short i16; int i32; long i64;
                float f1; double f2; decimal f3;
                #endregion

                #region Padding
                if (tp == typeof(GoPadding))
                {
                    float l, r, t, b;
                    var sp = text.Split(',').Select(x => x.Trim()).ToArray();
                    if (sp.Length == 4 && float.TryParse(sp[0], out l) && float.TryParse(sp[1], out t) && float.TryParse(sp[2], out r) && float.TryParse(sp[3], out b))
                        ret = new GoPadding(l, t, r, b);
                }
                #endregion
                #region Padding
                else if (tp == typeof(SKRect))
                {
                    float x, y, w, h;
                    var sp = text.Split(',').Select(x => x.Trim()).ToArray();
                    if (sp.Length == 4 && float.TryParse(sp[0], out x) && float.TryParse(sp[1], out y) && float.TryParse(sp[2], out w) && float.TryParse(sp[3], out h))
                        ret = Util.FromRect(x, y, w, h);
                }
                #endregion
                #region TimeSpan
                else if (tp == typeof(TimeSpan))
                {
                    if (TimeSpan.TryParse(text, out TimeSpan ts))
                        ret = ts;
                }
                #endregion
                #region String / Number
                else if (tp == typeof(byte) && byte.TryParse(text, out u8)) ret = u8;
                else if (tp == typeof(ushort) && ushort.TryParse(text, out u16)) ret = u16;
                else if (tp == typeof(uint) && uint.TryParse(text, out u32)) ret = u32;
                else if (tp == typeof(ulong) && ulong.TryParse(text, out u64)) ret = u64;
                else if (tp == typeof(sbyte) && sbyte.TryParse(text, out i8)) ret = i8;
                else if (tp == typeof(short) && short.TryParse(text, out i16)) ret = i16;
                else if (tp == typeof(int) && int.TryParse(text, out i32)) ret = i32;
                else if (tp == typeof(long) && long.TryParse(text, out i64)) ret = i64;
                else if (tp == typeof(float) && float.TryParse(text, out f1)) ret = f1;
                else if (tp == typeof(double) && double.TryParse(text, out f2)) ret = f2;
                else if (tp == typeof(decimal) && decimal.TryParse(text, out f3)) ret = f3;
                else if (tp == typeof(byte?)) { if (byte.TryParse(text, out u8)) ret = u8; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(ushort?)) { if (ushort.TryParse(text, out u16)) ret = u16; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(uint?)) { if (uint.TryParse(text, out u32)) ret = u32; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(ulong?)) { if (ulong.TryParse(text, out u64)) ret = u64; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(sbyte?)) { if (sbyte.TryParse(text, out i8)) ret = i8; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(short?)) { if (short.TryParse(text, out i16)) ret = i16; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(int?)) { if (int.TryParse(text, out i32)) ret = i32; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(long?)) { if (long.TryParse(text, out i64)) ret = i64; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(float?)) { if (float.TryParse(text, out f1)) ret = f1; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(double?)) { if (double.TryParse(text, out f2)) ret = f2; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(decimal?)) { if (decimal.TryParse(text, out f3)) ret = f3; else if (string.IsNullOrWhiteSpace(text)) ret = null; }
                else if (tp == typeof(string)) ret = text;
                #endregion
            }
            return ret;
        }
        #endregion
        #endregion
    }

    public class PropertyGridItemBool(GoPropertyGrid pg) : PropertyGridItem(pg) { }
    public class PropertyGridItemEnum(GoPropertyGrid pg) : PropertyGridItem(pg) { }
    public class PropertyGridItemEdit(GoPropertyGrid pg) : PropertyGridItem(pg)
    {
        public void ClearInput() { }
    }
    public class PropertyGridItemCollection(GoPropertyGrid pg) : PropertyGridItem(pg) { }
    #endregion
}
