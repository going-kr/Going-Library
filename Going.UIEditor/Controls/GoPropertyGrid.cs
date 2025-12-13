using Going.UI.Collections;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Forms;
using Going.UI.Forms.Controls;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using Going.UIEditor.Forms;
using Going.UIEditor.Forms.Editors;
using Going.UIEditor.Utils;
using Going.UIEditor.Windows;
using SkiaSharp;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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
        internal double ScrollPositionWithOffset => scroll.ScrollPositionWithOffset;
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
        public EditorWindow? SelectedEditor { get; set; }
        public IEnumerable<object>? SelectedObjects
        {
            get => sels;
            set
            {
                if (sels != value)
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

        private int pw, ph;
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
            scroll.Refresh = () => Invoke(Invalidate);
        }
        #endregion

        #region Override
        #region Draw
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            #region var 
            var canvas = e.Canvas;
            var thm = GoThemeW.Current;
            var prj = Program.CurrentProject;
            using var p = new SKPaint { IsAntialias = true };
            using var pe = SKPathEffect.CreateDash([2, 2,], 2);

            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            #endregion

            var (rtView, rtScroll, rtContent, rtsCol) = bounds();

            if (prj != null && properties.Count > 0)
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
                                canvas.DrawLine(rtsCol[1].Right, t, rtsCol[1].Right, b, p);
                                p.PathEffect = null;
                            }
                        }
                        #endregion

                        #region Content
                        for (int i = si; i <= ei; i++)
                        {
                            var v = properties[i];
                            v.Draw(canvas, thm, rtsCol);
                        }
                        #endregion
                    }
                    p.IsAntialias = true;
                }

                scroll.Draw(canvas, thm, rtScroll);
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
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtContent.Top - spos;

            itemLoop((i, item) => item.MouseDown(x, ry, rtsCol));

            #region Scroll
            scroll.MouseDown(x, y, rtScroll);
            if (Scroll.TouchMode && CollisionTool.Check(rtContent, x, y)) scroll.TouchDown(x, y);
            #endregion

            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var (rtView, rtScroll, rtContent, rtsCol) = bounds();
            var x = e.X;
            var y = e.Y;
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtContent.Top - spos;

            itemLoop((i, item) => item.MouseMove(x, ry, rtsCol));

            #region Scroll
            scroll.MouseMove(x, y, rtScroll);
            if (Scroll.TouchMode) scroll.TouchMove(x, y);
            #endregion

            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            var (rtView, rtScroll, rtContent, rtsCol) = bounds();
            var x = e.X;
            var y = e.Y;
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtContent.Top - spos;

            itemLoop((i, item) => item.MouseUp(x, ry, rtsCol));

            #region Scroll
            scroll.MouseUp(x, y);
            if (Scroll.TouchMode) scroll.TouchUp(x, y);
            #endregion
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var (rtView, rtScroll, rtContent, rtsCol) = bounds();
            var x = e.X;
            var y = e.Y;
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);
            var ry = y - rtContent.Top - spos;

            itemLoop((i, item) => item.MouseWheel(x, ry, rtsCol));

            #region Scroll
            if (CollisionTool.Check(rtContent, x, y)) scroll.MouseWheel(x, y, e.Delta / 120F);
            #endregion
            Invalidate();
            base.OnMouseWheel(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            var (rtView, rtScroll, rtContent, rtsCol) = bounds();
            var spos = Convert.ToSingle(scroll.ScrollPositionWithOffset);

            itemLoop((i, item) => item.MouseMove(-1, -1, rtsCol));

            Invalidate();
            base.OnMouseLeave(e);
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

            /*
            var mw = 80;
            if (properties.Count > 0)
                mw = Math.Max(Convert.ToInt32(properties.Max(x => Util.MeasureText(x.Name, FontName, FontStyle, FontSize).Width)) + 20, 80);
            var rtsCol = Util.Columns(rtContent, [$"{mw}px", $"100%", $"30px"]);
            */

            var rtsCol = Util.Columns(rtContent, [$"50%", $"50%", $"30px"]);

            return (rtView, rtScroll, rtContent, rtsCol);
        }
        #endregion

        #region itemLoop
        void itemLoop(Action<int, PropertyGridItem> loop)
        {
            var (rtView, rtScroll, rtContent, rtsCell) = bounds();

            if(pw != Width || ph != Height)
            {
                pw = Width;
                ph = Height;

                var y = 0F;
                foreach (var v in properties)
                {
                    v.Bounds = Util.FromRect(rtContent.Left, y, rtContent.Width, ItemHeight);
                    y += ItemHeight;
                }
            }

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

                foreach (var v in properties.Where(x => x is PropertyGridItemEdit))
                    if (v is PropertyGridItemEdit i)
                    {
                        i.ClearInput();
                        i.Dispose();
                    }

                properties.Clear();

                if (objs != null)
                {
                    #region make dic
                    var dic = new Dictionary<string, List<Obj>>();
                    foreach (var obj in objs)
                        foreach (var pi in obj.GetType().GetProperties().Where(x => x.CanWrite && x.CanRead && Attribute.IsDefined(x, typeof(GoPropertyAttribute))))
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

                    #region var
                    string? cat = null;
                    float y = 0F;

                    pw = Width;
                    ph = Height;
                    #endregion

                    #region Id
                    if (!objs.Any(x => x?.GetType().GetInterface("Going.UI.Controls.IGoControl") == null) && objs.Count() > 0)
                    {
                        properties.Add(new PropertyGridItem(this) { Category = PCategory.ID, Type = PropertyGridItemType.Category, Bounds = Util.FromRect(rtContent.Left, y, rtContent.Width, ItemHeight), });
                        y += ItemHeight;
                        properties.Add(new PropertyGridItemID(this)
                        {
                            Name = "ID",
                            Type = PropertyGridItemType.Item,
                            Info = null,
                            Category = PCategory.ID,
                            CategoryOrder = PCategory.Index(PCategory.ID),
                            PropertyOrder = 0,
                            Bounds = Util.FromRect(rtContent.Left, y, rtContent.Width, ItemHeight),
                        });
                        y += ItemHeight;
                        properties.Add(new PropertyGridItemCType(this)
                        {
                            Name = "Type",
                            Type = PropertyGridItemType.Item,
                            Info = null,
                            Category = PCategory.ID,
                            CategoryOrder = PCategory.Index(PCategory.ID),
                            PropertyOrder = 1,
                            Bounds = Util.FromRect(rtContent.Left, y, rtContent.Width, ItemHeight),
                        });
                        y += ItemHeight;
                    }
                    #endregion

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
                            else if (PropertyGridItem.IsSizes(o.Prop)) item = new PropertyGridItemSizes(this);
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
    #region Base
    public enum PropertyGridItemType { Category, Item }
    public class PropertyGridItem(GoPropertyGrid pg)
    {
        #region Properties
        public string Name { get; set; } = "";
        public PropertyGridItemType Type { get; set; } = PropertyGridItemType.Item;
        public PropertyInfo? Info { get; set; }
        public string? Category { get; set; }
        public int CategoryOrder { get; set; }
        public int PropertyOrder { get; set; }
        public SKRect Bounds { get; set; }
        public GoPropertyGrid Grid => pg;

        public bool ItemDown { get; private set; } = false;
        public bool ValueDown { get; private set; } = false;
        public bool ButtonDown { get; private set; } = false;
        public bool ButtonHover { get; private set; } = false;
        #endregion 

        #region Method
        #region Draw
        public void Draw(SKCanvas canvas, GoTheme thm, SKRect[] rtsCol)
        {
            if (Type == PropertyGridItemType.Category)
            {
                var rt = Util.FromRect(Bounds.Left + 10, Bounds.Top, Bounds.Width - 10, Bounds.Height);
                Util.DrawText(canvas, $"·  {Category}", pg.FontName, pg.FontStyle, pg.FontSize, rt, thm.Fore, GoContentAlignment.MiddleLeft);
            }
            else
            {
                var (rtTitle, rtValue, rtButton) = bounds(rtsCol);

                Util.DrawText(canvas, Util2.EllipsisPath(Name, pg.FontName, pg.FontStyle, pg.FontSize, rtTitle.Width - 20), pg.FontName, pg.FontStyle, pg.FontSize, rtTitle, thm.Base5);

                OnDrawValue(canvas, thm, rtTitle, rtValue, rtButton);
            }
        }
        #endregion

        #region Mouse
        public void MouseDown(float x, float y, SKRect[] rtsCol)
        {
            if (Type == PropertyGridItemType.Item && CollisionTool.Check(Bounds, x, y))
            {
                ItemDown = true;

                var (rtTitle, rtValue, rtButton) = bounds(rtsCol);
                if (CollisionTool.Check(rtValue, x, y)) ValueDown = true;
                else if (CollisionTool.Check(rtButton, x, y)) ButtonDown = true;
            }
        }

        public void MouseMove(float x, float y, SKRect[] rtsCol)
        {
            if (Type == PropertyGridItemType.Item )
            {
                var (rtTitle, rtValue, rtButton) = bounds(rtsCol);
                ButtonHover = CollisionTool.Check(rtButton, x, y);
            }
        }

        public void MouseUp(float x, float y, SKRect[] rtsCol)
        {
            var valueout = true;
            var (rtTitle, rtValue, rtButton) = bounds(rtsCol);

            if (Type == PropertyGridItemType.Item && (CollisionTool.Check(Bounds, x, y) || ItemDown))
            {
                ItemDown = false;

                if (ValueDown)
                {
                    ValueDown = false;
                    if (CollisionTool.Check(rtValue, x, y))
                    {
                        OnValueClick(rtValue, rtButton);
                        valueout = false;
                    }
                }

                if (ButtonDown)
                {
                    ButtonDown = false;
                    if (IsUseButton(Info) && CollisionTool.Check(rtButton, x, y))
                    {
                        OnButtonClick(rtValue, rtButton);
                    }
                }

            }
            
            if (valueout) OnValueOutsideClick(rtValue, rtButton);

        }

        public void MouseWheel(float x, float y, SKRect[] rtsCol)
        {
            if (Type == PropertyGridItemType.Item)
            {
                var (rtTitle, rtValue, rtButton) = bounds(rtsCol);
                OnMouseWheel(rtValue, rtButton);
            }
        }
        #endregion

        #region Tools
        (SKRect rtTitle, SKRect rtValue, SKRect rtButton) bounds(SKRect[] rtsCol)
        {
            var rtTitle = Util.FromRect(rtsCol[0].Left, Bounds.Top, rtsCol[0].Width, Bounds.Height);
            var rtValue = Util.FromRect(rtsCol[1].Left, Bounds.Top, rtsCol[1].Width, Bounds.Height);
            var rtButton = Util.FromRect(rtsCol[2].Left, Bounds.Top, rtsCol[2].Width, Bounds.Height);
            return (rtTitle, rtValue, rtButton);
        }
        #endregion

        #region Virtual
        protected virtual void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtTitle, SKRect rtValue, SKRect rtButton) { }
        protected virtual void OnValueClick(SKRect rtValue, SKRect rtButton) { }
        protected virtual void OnValueOutsideClick(SKRect rtValue, SKRect rtButton) { }
        protected virtual void OnButtonClick(SKRect rtValue, SKRect rtButton) { }
        protected virtual void OnMouseWheel(SKRect rtValue, SKRect rtButton) { }
        #endregion

        #region SetValue
        protected void SetValue(object? c, PropertyInfo Info, object? value)
        {
            if (pg != null && Info != null && c != null)
            {
                if (pg.SelectedEditor != null)
                {
                    if (c != null && c.GetType().GetProperty(Info.Name) is PropertyInfo info2)
                    {
                        var ovalue = info2.GetValue(c);
                        pg.SelectedEditor.EditObject(c, info2, ovalue, value);
                    }
                    pg.Invalidate();
                    pg.SelectedEditor.Invalidate();
                }
                else
                {
                    if (c != null && c.GetType().GetProperty(Info.Name) is PropertyInfo info2)
                    {
                        var ovalue = info2.GetValue(c);
                        Info.SetValue(c, value);
                    }

                    pg.Invalidate();
                }
            }
        }
        #endregion

        #region SetSizes
        protected void SetSizes(object? c, PropertyInfo Info, List<SizesItem> items)
        {
            if (pg != null && Info != null && c != null)
            {
                if (pg.SelectedEditor != null)
                {
                    if (c != null && c.GetType().GetProperty(Info.Name) is PropertyInfo info2)
                    {
                        var ovalue = info2.GetValue(c);
                        pg.SelectedEditor.EditSizesTable(c, info2,  ovalue, items);
                    }
                    pg.Invalidate();
                    pg.SelectedEditor.Invalidate();
                }
                else
                {
                    if (c != null && c.GetType().GetProperty(Info.Name) is PropertyInfo info2)
                    {
                        var ovalue = info2.GetValue(c);
                        var nvalue = items.Where(x => !x.IsDelete).OrderBy(x => x.Idx).Select(x => x.ToValue()).ToList();
                        Info.SetValue(c, nvalue);

                        if (c is GoGridLayoutPanelRow row) row.ExtraData = items;
                    }

                    pg.Invalidate();
                }
            }
        }
        #endregion

        #region SelectedObjectLoop
        protected void SelectedObjectLoop(Action < object> loop)
        {
            if (Info != null && Grid.SelectedObjects != null)
            {
                if (Grid.SelectedEditor != null)
                {
                    Grid.SelectedEditor.TransAction(() =>
                    {
                        foreach (var obj in Grid.SelectedObjects) loop(obj);
                    });
                }
                else
                {
                    foreach (var obj in Grid.SelectedObjects) loop(obj);
                }
            }
        }
        #endregion

        #region GetObjValue
        protected object? GetObjValue(object o, PropertyInfo info)
        {
            object? ret = null;
            var tp = o.GetType();
            if (tp == info.DeclaringType) ret = info.GetValue(o);
            else
            {
                var info2 = tp.GetProperty(info.Name);
                if (info2 != null && info.PropertyType == info2.PropertyType) ret = info2.GetValue(o);
            }

            return ret;
        }
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
        public static bool IsUseButton(PropertyInfo? Info) => IsCollection(Info) || IsEnum(Info) || IsSizes(Info) || IsEnum(Info) || IsImage(Info) || IsFontName(Info) || IsMultiLine(Info);
        public static bool IsCollection(PropertyInfo? Info) => Info != null && typeof(IEnumerable).IsAssignableFrom(Info.PropertyType) && Info.PropertyType != typeof(string) && !Attribute.IsDefined(Info, typeof(GoSizesPropertyAttribute));
        public static bool IsSizes(PropertyInfo? Info) => Info != null && typeof(IEnumerable).IsAssignableFrom(Info.PropertyType) && Info.PropertyType != typeof(string) && Attribute.IsDefined(Info, typeof(GoSizesPropertyAttribute));
        public static bool IsEnum(PropertyInfo? Info) => Info != null && Info.PropertyType.IsEnum;
        public static bool IsBool(PropertyInfo? Info) => Info != null && Info.PropertyType == typeof(bool);
        public static bool IsImage(PropertyInfo? Info) => Info != null && Attribute.IsDefined(Info, typeof(GoImagePropertyAttribute));
        public static bool IsFontName(PropertyInfo? Info) => Info != null && Attribute.IsDefined(Info, typeof(GoFontNamePropertyAttribute));
        public static bool IsMultiLine(PropertyInfo? Info) => Info != null && Attribute.IsDefined(Info, typeof(GoMultiLinePropertyAttribute));

        #region ValueToString
        protected string ValueToString(object? v)
        {
            var ret = "";

            if (v != null && Info != null && v.GetType().GetProperty(Info.Name) is PropertyInfo info2)
            {
                var val = info2.GetValue(v);
                if (val != null)
                {
                    if (val is GoPadding pad) ret = $"{pad.Left}, {pad.Top}, {pad.Right}, {pad.Bottom}";
                    else if (val is SKRect rt) ret = $"{rt.Left}, {rt.Top}, {rt.Width}, {rt.Height}";
                    else if (val is SKColor color)
                    {
                        if (color.Alpha == 255)
                            ret = $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
                        else
                            ret = $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}{color.Alpha:X2}";
                    }
                    else if (val is TimeSpan ts) ret = ts.ToString();
                    else if (val is DateTime time) ret = time.ToString();
                    else if (IsCollection(Info) && val is IEnumerable<object> ls) ret = $"Count : {ls.Count()}";
                    else if (IsSizes(Info) && val is IEnumerable<object> ls2) ret = $"Count : {ls2.Count()}";
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
                #region SKRect
                else if (tp == typeof(SKRect))
                {
                    float x, y, w, h;
                    var sp = text.Split(',').Select(x => x.Trim()).ToArray();
                    if (sp.Length == 4 && float.TryParse(sp[0], out x) && float.TryParse(sp[1], out y) && float.TryParse(sp[2], out w) && float.TryParse(sp[3], out h))
                        ret = Util.FromRect(x, y, w, h);
                }
                #endregion
                #region SKColor
                else if (tp == typeof(SKColor))
                {
                    ret = GoThemeW.Current.ToColor(text);
                }
                #endregion
                #region TimeSpan
                else if (tp == typeof(TimeSpan))
                {
                    if (TimeSpan.TryParse(text, out TimeSpan ts))
                        ret = ts;
                }
                #endregion
                #region DateTime
                else if (tp == typeof(DateTime))
                {
                    if (DateTime.TryParse(text, out DateTime dt)) ret = dt;
                }
                #endregion
                #region Sizes
                else if (tp == typeof(List<string>) && Attribute.IsDefined(Info, typeof(GoSizesPropertyAttribute)))
                {
                    ret = text.Split(',').Select(x => x.Trim()).ToList();
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
                else if (tp == typeof(byte?)) { if (byte.TryParse(text, out u8)) ret = u8; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(ushort?)) { if (ushort.TryParse(text, out u16)) ret = u16; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(uint?)) { if (uint.TryParse(text, out u32)) ret = u32; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(ulong?)) { if (ulong.TryParse(text, out u64)) ret = u64; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(sbyte?)) { if (sbyte.TryParse(text, out i8)) ret = i8; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(short?)) { if (short.TryParse(text, out i16)) ret = i16; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(int?)) { if (int.TryParse(text, out i32)) ret = i32; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(long?)) { if (long.TryParse(text, out i64)) ret = i64; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(float?)) { if (float.TryParse(text, out f1)) ret = f1; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(double?)) { if (double.TryParse(text, out f2)) ret = f2; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(decimal?)) { if (decimal.TryParse(text, out f3)) ret = f3; else if (string.IsNullOrWhiteSpace(text)) ret = "{NONE}"; }
                else if (tp == typeof(string)) ret = text ?? "{NONE}";
                #endregion
            }
            return ret;
        }
        #endregion
        #endregion
    }
    #endregion

    #region ID
    public class PropertyGridItemID(GoPropertyGrid pg) : PropertyGridItem(pg)
    {
        #region Draw
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtTitle, SKRect rtValue, SKRect rtButton)
        {
            base.OnDrawValue(canvas, thm, rtTitle, rtValue, rtButton);

            var s = Util2.EllipsisPath(Grid.SelectedObjects?.Count() == 1 && Grid.SelectedObjects?.FirstOrDefault() is IGoControl c ? c.Id.ToString() : "", Grid.FontName, Grid.FontStyle, Grid.FontSize, rtValue.Width - 20);
            Util.DrawText(canvas, s, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtValue, thm.Base5);
        }
        #endregion
    }
    #endregion
    #region Type
    public class PropertyGridItemCType(GoPropertyGrid pg) : PropertyGridItem(pg)
    {
        #region Draw
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtTitle, SKRect rtValue, SKRect rtButton)
        {
            base.OnDrawValue(canvas, thm, rtTitle, rtValue, rtButton);

            var s = Grid.SelectedObjects?.Count() == 1 && Grid.SelectedObjects?.FirstOrDefault() is IGoControl c ? c.GetType().Name : "";
            Util.DrawText(canvas, s, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtValue, thm.Base5);
        }
        #endregion
    }
    #endregion
    #region Bool
    public class PropertyGridItemBool(GoPropertyGrid pg) : PropertyGridItem(pg)
    {
        #region Override
        #region Draw
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtTitle, SKRect rtValue, SKRect rtButton)
        {
            base.OnDrawValue(canvas, thm, rtTitle, rtValue, rtButton);

            var vs = Grid.SelectedObjects?.Select(x => ValueToString(x));
            var lk = vs?.ToLookup(x => x);
            var s = lk?.Count == 1 ? (lk.FirstOrDefault()?.Key ?? "") : "";

            Util.DrawText(canvas, s, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtValue, thm.Base5);
        }
        #endregion

        #region Click
        protected override void OnValueClick(SKRect rtValue, SKRect rtButton)
        {
            if (Info != null && Grid.SelectedObjects != null)
            {
                var vs = Grid.SelectedObjects.Select(x => GetObjValue(x, Info));
                var lk = vs.ToLookup(x => x);
                var val = lk.Count == 1 ? (lk.FirstOrDefault()?.Key ?? null) : null;

                SelectedObjectLoop((obj) => SetValue(obj, Info, (val is bool b && !b)));
            }
            base.OnValueClick(rtValue, rtButton);
        }
        #endregion
        #endregion
    }
    #endregion
    #region Enum
    public class PropertyGridItemEnum(GoPropertyGrid pg) : PropertyGridItem(pg)
    {
        #region Override
        #region Draw
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtTitle, SKRect rtValue, SKRect rtButton)
        {
            base.OnDrawValue(canvas, thm, rtTitle, rtValue, rtButton);

            #region Value
            var vs = Grid.SelectedObjects?.Select(x => ValueToString(x));
            var lk = vs?.ToLookup(x => x);
            var s = lk?.Count == 1 ? (lk.FirstOrDefault()?.Key ?? "") : "";

            Util.DrawText(canvas, s, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtValue, thm.Base5);
            #endregion

            #region Button
            if(IsUseButton(Info))
            {
                var vrt = rtButton;
                var c = (ButtonHover ? thm.Fore : thm.Base5).BrightnessTransmit(ButtonDown ? thm.DownBrightness : 0);
                if (ButtonDown) vrt.Offset(0, 1);
                Util.DrawIcon(canvas, "fa-ellipsis", 12, vrt, c);
            }
            #endregion
        }
        #endregion

        #region Button
        protected override void OnButtonClick(SKRect rtValue, SKRect rtButton)
        {
            if (Info != null && Grid.SelectedObjects != null)
            {
                var tp = Info.PropertyType;


                var values = Enum.GetValues(tp).Cast<object>().Select(x => new GoListItem { Text = x.ToString(), Tag = x }).ToList();

                var vs = Grid.SelectedObjects.Select(x => GetObjValue(x, Info));
                var lk = vs.ToLookup(x => x);
                var val = lk.Count == 1 ? (lk.FirstOrDefault()?.Key ?? null) : null;
                var ret = Program.SelBox.ShowRadio(Name, (values.Count > 10 ? 4 : (values.Count > 3 ? 3 : 1)), values, values.FirstOrDefault(x => object.Equals(x.Tag, val)));
                if (ret != null && ret.Tag != null)
                {
                    SelectedObjectLoop((obj) => SetValue(obj, Info, ret.Tag));
                }

            }
            base.OnButtonClick(rtValue, rtButton);
        }
        #endregion
        #endregion
    }
    #endregion
    #region Edit
    public class PropertyGridItemEdit  : PropertyGridItem , IDisposable
    {
        #region Member Variable
        private TextBox txt;
        #endregion

        #region Constructor
        public PropertyGridItemEdit(GoPropertyGrid pg) : base(pg)
        {
            txt = new TextBox { Font = new Font("나눔고딕", 9), TextAlign = HorizontalAlignment.Center, BorderStyle = BorderStyle.None, Visible = false };
            pg.Controls.Add(txt);
            #region  txt.TextChanged 
            txt.TextChanged += (o, s) =>
            {
                if (Info != null)
                {
                    var tp = Info.PropertyType;
                    if (tp == typeof(float) || tp == typeof(double) || tp == typeof(decimal))
                    {
                        #region Floating
                        var selectionStart = txt.SelectionStart;
                        var selectionLength = txt.SelectionLength;

                        var newText = String.Empty;
                        bool bComma = false;
                        foreach (var c in txt.Text.ToCharArray())
                        {
                            if (Char.IsDigit(c) || Char.IsControl(c) || (c == '.' && !bComma && txt.Text != ".") || (newText.Length == 0 && (c == '-' || c == '+'))) newText += c;
                            if (c == '.' && txt.Text != ".") bComma = true;
                        }
                        txt.Text = newText;
                        txt.SelectionStart = selectionStart <= txt.Text.Length ? selectionStart : txt.Text.Length;
                        #endregion
                    }
                    else if (tp == typeof(sbyte) || tp == typeof(short) || tp == typeof(int) || tp == typeof(long) ||
                           tp == typeof(byte) || tp == typeof(ushort) || tp == typeof(uint) || tp == typeof(ulong))
                    {
                        #region Number
                        var selectionStart = txt.SelectionStart;
                        var selectionLength = txt.SelectionLength;

                        var newText = String.Empty;
                        foreach (var c in txt.Text.ToCharArray())
                        {
                            if (Char.IsDigit(c) || Char.IsControl(c) || (newText.Length == 0 && (c == '-' || c == '+'))) newText += c;
                        }
                        txt.Text = newText;
                        txt.SelectionStart = selectionStart <= txt.Text.Length ? selectionStart : txt.Text.Length;
                        #endregion
                    }
                }
            };
            #endregion
            #region txt.KeyDown
            txt.KeyDown += (o, s) =>
            {
                if (Info != null)
                {
                    if (IsEditType(Info) || IsSizes(Info))
                    {
                        if (s.KeyCode == Keys.Enter) CompleteInput();
                        if (s.KeyCode == Keys.Escape) ClearInput();
                    }
                }
            };
            #endregion
            txt.LostFocus += (o, s) => ClearInput();
        }
        #endregion

        #region Override
        #region Draw
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtTitle, SKRect rtValue, SKRect rtButton)
        {
            base.OnDrawValue(canvas, thm, rtTitle, rtValue, rtButton);

            #region Value
            if (!txt.Visible)
            {
                var vs = Grid.SelectedObjects?.Select(x => ValueToString(x));
                var lk = vs?.ToLookup(x => x);
                var s = lk?.Count == 1 ? (lk.FirstOrDefault()?.Key ?? "") : "";

                Util.DrawText(canvas, s, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtValue, thm.Base5);
            }
            #endregion

            #region Button
            if (IsUseButton(Info))
            {
                var vrt = rtButton;
                var c = (ButtonHover ? thm.Fore : thm.Base5).BrightnessTransmit(ButtonDown ? thm.DownBrightness : 0);
                if (ButtonDown) vrt.Offset(0, 1);
                Util.DrawIcon(canvas, "fa-ellipsis", 12, vrt, c);
            }
            #endregion
        }
        #endregion

        #region Click
        protected override void OnValueClick(SKRect rtValue, SKRect rtButton)
        {
            SetInput(rtValue);

            base.OnValueClick(rtValue, rtButton);
        }
        #endregion

        #region Button
        protected override void OnButtonClick(SKRect rtValue, SKRect rtButton)
        {
            if (IsImage(Info) && Info != null)
            {
                var ret = Program.ImageSelector.ShowImageSelect();
                if (ret != null && Grid.SelectedObjects != null)
                {
                    object? vv = ret.Name;
                    if (vv is string s && s == "{NONE}") vv = null;
                    SelectedObjectLoop((obj) => SetValue(obj, Info, vv));
                }
            }
            else if (Info != null && IsFontName(Info))
            {
                #region Font
                var ret = Program.FontSelector.ShowFontSelect(Info.Name);
                if (ret != null && Grid.SelectedObjects != null)
                {
                    object? vv = ret.Name;
                    if (vv is string s && s == "{NONE}") vv = "나눔고딕";
                    SelectedObjectLoop((obj) => SetValue(obj, Info, vv));
                }
                #endregion
            }
            else if (IsMultiLine(Info) && Info != null)
            {
                var ret = Program.MultiLineInputor.ShowString(Info.Name, (Grid.SelectedObjects?.Count() == 1 ? Info.GetValue(Grid.SelectedObjects.First()) as string : null));
                if (ret != null && Grid.SelectedObjects != null)
                {
                    object? vv = ret;
                    SelectedObjectLoop((obj) => SetValue(obj, Info, vv));
                }
            }
            else SetInput(rtValue);
            base.OnButtonClick(rtValue, rtButton);
        }
        #endregion

        protected override void OnMouseWheel(SKRect rtValue, SKRect rtButton) => ClearInput();
        protected override void OnValueOutsideClick(SKRect rtValue, SKRect rtButton) => ClearInput();
        #endregion

        #region Method
        #region SetInput
        void SetInput(SKRect rtValue)
        {
            if (Info != null && Grid.SelectedObjects != null && ((Info.Name == "Name" && Grid.SelectedObjects.Count() == 1) || Info.Name != "Name"))
            {
                var vs = Grid.SelectedObjects?.Select(x => ValueToString(x));
                var lk = vs?.ToLookup(x => x);
                var s = lk?.Count == 1 ? (lk.FirstOrDefault()?.Key ?? "") : "";

                var thm = GoThemeW.Current;
                txt.ForeColor = Util.FromArgb(thm.Base5);
                txt.BackColor = Util.FromArgb(thm.Back);
                txt.Text = s;
                txt.Visible = true;
                var srt = rtValue;
                srt.Offset(0, Convert.ToSingle(Grid.ScrollPositionWithOffset));
                SetBounds(srt);

                Grid.BeginInvoke(new Action(() => { txt.Focus(); txt.SelectAll(); }));
            }
        }
        #endregion
        #region ClearInput
        public void ClearInput() => txt.Visible = false;
        #endregion
        #region CompleteInput
        public void CompleteInput()
        {
            if (Info != null && Grid.SelectedObjects != null && ((Info.Name == "Name" && Grid.SelectedObjects.Count() == 1) || Info.Name != "Name"))
            {
                object? vv = ValueFromString(txt.Text);
                if (vv is string s && s == "{NONE}") vv = null;

                if (Info.Name == "Name")
                {
                    var obj = Grid.SelectedObjects.First();
                    #region Rename
                    var p = Program.CurrentProject;
                    if (p != null && obj is GoPage p1 && Info.Name == "Name" && vv is string newname1 && !p.Design.Pages.Values.Any(x => x != p1 && x.Name == newname1))
                    {
                        SetValue(obj, Info, vv);

                        var ls = p.Design.Pages.Values.ToList();
                        p.Design.Pages.Clear();
                        foreach (var vp in ls) p.Design.AddPage(vp);

                        if (Program.MainForm.DockPanel.Contents.FirstOrDefault(x => x is ExplorerWindow) is ExplorerWindow ew) ew.RefreshTreeView();
                    }
                    else if (p != null && obj is GoWindow w2 && Info.Name == "Name" && vv is string newname2 && !p.Design.Windows.Values.Any(x => x != w2 && x.Name == newname2))
                    {
                        SetValue(obj, Info, vv);

                        var ls = p.Design.Windows.Values.ToList();
                        p.Design.Windows.Clear();
                        foreach (var vp in ls) p.Design.Windows.Add(vp.Name!, vp);

                        if (Program.MainForm.DockPanel.Contents.FirstOrDefault(x => x is ExplorerWindow) is ExplorerWindow ew) ew.RefreshTreeView();
                    }
                    else
                    {
                        SetValue(obj, Info, vv);
                    }
                    #endregion
                }
                else SelectedObjectLoop((obj) => SetValue(obj, Info, vv));
            }

            Grid?.BeginInvoke(new Action(() =>
            {
                var vs = Grid.SelectedObjects?.Select(x => ValueToString(x));
                var lk = vs?.ToLookup(x => x);
                var s = lk?.Count == 1 ? (lk.FirstOrDefault()?.Key ?? "") : "";

                txt.Text = s;
                txt.Focus();
                txt.SelectAll();
            }));
        }
        #endregion

        #region SetBounds
        void SetBounds(SKRect rtValue)
        {
            var rt = MathTool.MakeRectangle(Util.Int(rtValue), new SKSize(Convert.ToInt32(rtValue.Width - 20), txt.Height));
            txt.Bounds = new Rectangle(Convert.ToInt32(rt.Left), Convert.ToInt32(rt.Top), Convert.ToInt32(rt.Width), Convert.ToInt32(rt.Height));
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            txt?.Dispose();
        }
        #endregion
        #endregion
    }
    #endregion
    #region Collection
    public class PropertyGridItemCollection(GoPropertyGrid pg) : PropertyGridItem(pg)
    {
        #region Override
        #region Draw
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtTitle, SKRect rtValue, SKRect rtButton)
        {
            base.OnDrawValue(canvas, thm, rtTitle, rtValue, rtButton);
        
            #region Value
            var vs = Grid.SelectedObjects?.Select(x => ValueToString(x));
            var lk = vs?.ToLookup(x => x);
            var s = lk?.Count == 1 ? (lk.FirstOrDefault()?.Key ?? "") : "";

            Util.DrawText(canvas, s, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtValue, thm.Base5);
            #endregion

            #region Button
            if (IsUseButton(Info))
            {
                var vrt = rtButton;
                var c = (ButtonHover ? thm.Fore : thm.Base5).BrightnessTransmit(ButtonDown ? thm.DownBrightness : 0);
                if (ButtonDown) vrt.Offset(0, 1);
                Util.DrawIcon(canvas, "fa-ellipsis", 12, vrt, c);
            }
            #endregion
        }
        #endregion

        #region Button
        protected override void OnButtonClick(SKRect rtValue, SKRect rtButton)
        {
            if (Info != null && Grid.SelectedObjects != null)
            {
                var tp = Info.PropertyType;

                var vs = Grid.SelectedObjects.Select(x => GetObjValue(x, Info));
                var lk = vs.ToLookup(x => x);
                var val = lk.Count == 1 ? (lk.FirstOrDefault()?.Key ?? null) : null;

                if (IsCollection(Info))
                {
                    var type = Info.PropertyType.GetGenericArguments().FirstOrDefault();
                    if (type == typeof(GoGridLayoutPanelRow)) SetValue(Info, val as IEnumerable<GoGridLayoutPanelRow>);
                    else if (type == typeof(GoButtonItem)) SetValue(Info, val as IEnumerable<GoButtonItem>);
                    else if (type == typeof(GoButtonsItem)) SetValue(Info, val as IEnumerable<GoButtonsItem>);
                    else if (type == typeof(GoListItem)) SetValue(Info, val as IEnumerable<GoListItem>);
                    else if (type == typeof(GoMenuItem)) SetValue(Info, val as IEnumerable<GoMenuItem>);
                    else if (type == typeof(GoToolCategory)) SetValue(Info, val as IEnumerable<GoToolCategory>);
                    else if (type == typeof(GoToolItem)) SetValue(Info, val as IEnumerable<GoToolItem>);
                    else if (type == typeof(GoTreeNode)) SetValue(Info, val as IEnumerable<GoTreeNode>);
                    else if (type == typeof(GoGraphSeries)) SetValue(Info, val as IEnumerable<GoGraphSeries>);
                    else if (type == typeof(GoLineGraphSeries)) SetValue(Info, val as IEnumerable<GoLineGraphSeries>);
                    else if (type == typeof(GoTabPage)) SetValue(Info, val as IEnumerable<GoTabPage>);
                    else if (type == typeof(GoSubPage)) SetValue(Info, val as IEnumerable<GoSubPage>);
                    else throw new Exception("invalid type");

                    Grid.Invalidate();
                }
            }
            base.OnButtonClick(rtValue, rtButton);
        }
        #endregion
        #endregion

        #region SetValue 
        void SetValue<T>(PropertyInfo info, IEnumerable<T>? val)
        {
            var tp = info.PropertyType;
            using (var dlg = new FormCollectionEditor<T>())
            {
                if (val is IEnumerable<GoGridLayoutPanelRow> ls) foreach (var v in ls) v.ExtraData = null;

                var ret = dlg.ShowCollectionEditor($"{info.Name} Editor", val);
                if (ret != null && Grid?.SelectedObjects != null)
                    SelectedObjectLoop((obj) =>
                    {
                        if (typeof(T) == typeof(GoGridLayoutPanelRow) && ret is IEnumerable<GoGridLayoutPanelRow> rows) 
                        {

                        }
                        else if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var va = new List<T>();
                            va.AddRange(ret);
                            SetValue(obj, info, va);
                        }
                        else if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(ObservableList<>))
                        {
                            var va = new ObservableList<T>();
                            va.AddRange(ret);
                            SetValue(obj, info, va);
                        }
                    });

            }
        }
        #endregion
    }
    #endregion
    #region Sizes
    public class PropertyGridItemSizes(GoPropertyGrid pg) : PropertyGridItem(pg)
    {
        #region Override
        #region Draw
        protected override void OnDrawValue(SKCanvas canvas, GoTheme thm, SKRect rtTitle, SKRect rtValue, SKRect rtButton)
        {
            base.OnDrawValue(canvas, thm, rtTitle, rtValue, rtButton);

            #region Value
            var vs = Grid.SelectedObjects?.Select(x => ValueToString(x));
            var lk = vs?.ToLookup(x => x);
            var s = lk?.Count == 1 ? (lk.FirstOrDefault()?.Key ?? "") : "";

            Util.DrawText(canvas, s, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtValue, thm.Base5);
            #endregion

            #region Button
            if (IsUseButton(Info))
            {
                var vrt = rtButton;
                var c = (ButtonHover ? thm.Fore : thm.Base5).BrightnessTransmit(ButtonDown ? thm.DownBrightness : 0);
                if (ButtonDown) vrt.Offset(0, 1);
                Util.DrawIcon(canvas, "fa-ellipsis", 12, vrt, c);
            }
            #endregion
        }
        #endregion

        #region Button
        protected override void OnButtonClick(SKRect rtValue, SKRect rtButton)
        {
            if (Info != null && Grid.SelectedObjects != null)
            {
                var tp = Info.PropertyType;

                var vs = Grid.SelectedObjects.Select(x => GetObjValue(x, Info));
                var lk = vs.ToLookup(x => x);
                var val = lk.Count == 1 ? (lk.FirstOrDefault()?.Key ?? null) : null;

                if (IsSizes(Info) && val is List<string> items)
                {
                    using(var dlg = new FormSizesEditor())
                    {
                        var ret = dlg.ShowSizesEditor(Info.Name, items);
                        if(ret != null)
                        {
                            var itms = ret.Where(x => !x.IsDelete).OrderBy(x => x.Idx).Select(x => x.ToValue()).ToArray();
                            if (itms != null && Util.ValidSizes(itms))
                            {
                                SelectedObjectLoop((obj) => SetSizes(obj, Info, ret));
                                Grid.Invalidate();
                            }
                        }
                    }
                }
            }
            base.OnButtonClick(rtValue, rtButton);
        }
        #endregion
        #endregion

        #region Control
        public class TblControl(GoTableIndex idx, IGoControl control)
        {
            public int Col => idx.Column;
            public int Row => idx.Row;
            public int ColSpan => idx.ColSpan;
            public int RowSpan => idx.RowSpan;
            public GoTableIndex Index => idx;
            public IGoControl Control => control;
        }

        public class GrdControl(GoGridIndex idx, IGoControl control)
        {
            public int Col => idx.Column;
            public int Row => idx.Row;
            public GoGridIndex Index => idx;
            public IGoControl Control => control;
        }
        #endregion
    }
    #endregion
    #endregion
}
