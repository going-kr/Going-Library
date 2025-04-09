using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Dialogs;
using Going.UI.Enums;
using Going.UI.Forms.Input;
using Going.UI.Managers;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UI.Forms.Dialogs
{
    public partial class GoInputBox : GoForm
    {
        #region Const
        float minw = 200;
        float minh = 140;
        #endregion

        #region Properties
        public string OkText { get => btnOk.Text; set => btnOk.Text = value; }
        public string CancelText { get => btnCancel.Text; set => btnCancel.Text = value; }

        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;
        
        public int ItemHeight { get; set; } = 40;
        public int ItemTitleWidth { get; set; } = 80;
        public int ItemValueWidth { get; set; } = 150;
        #endregion

        #region Member Varaible
        GoTableLayoutPanel tpnl;
        GoTableLayoutPanel tpnl2;
        GoButton btnOk, btnCancel;
        #endregion

        #region Constructor
        public GoInputBox()
        {
            InitializeComponent();

            this.Width = Convert.ToInt32(minw);
            this.Height = Convert.ToInt32(minh);

            TitleIconString = "fa-pen-to-square";
            TitleIconSize = 18;

            tpnl = new GoTableLayoutPanel { Fill = true, Margin = new GoPadding(10), Columns = ["34%", "33%", "33%"], Rows = ["100%", "40px"] };
            tpnl2 = new GoTableLayoutPanel { Fill = true, Margin = new GoPadding(0), };
            btnOk = new GoButton { Fill = true, Text = "확인" };
            btnCancel = new GoButton { Fill = true, Text = "취소" };

            tpnl.Childrens.Add(tpnl2, 0, 0, 3, 1);
            tpnl.Childrens.Add(btnOk, 1, 1);
            tpnl.Childrens.Add(btnCancel, 2, 1);

            btnOk.ButtonClicked += (o, s) => DialogResult = DialogResult.OK;
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;

            GoInputEventer.Current.InputString += (c, bounds, callback, value) =>
            {
                bounds.Offset(tpnl.Left + tpnl2.Left + c.Left, tpnl.Top + tpnl2.Top + c.Top);
                if (tpnl2.Childrens.Contains(c) && c is GoInputString vc)
                    FormsInputManager.Current.InputString(this, c, bounds, FontName, FontStyle, FontSize, vc.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, value);
            };

            GoInputEventer.Current.InputNumber += (c, bounds, callback, type, value, min, max) =>
            {
                bounds.Offset(tpnl.Left + tpnl2.Left + c.Left, tpnl.Top + tpnl2.Top + c.Top);
                if (tpnl2.Childrens.Contains(c))
                {
                    if (type == typeof(byte) && c is GoInputNumber<byte> vc1)
                        FormsInputManager.Current.InputNumber<byte>(this, c, bounds, FontName, FontStyle, FontSize, vc1.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(sbyte) && c is GoInputNumber<sbyte> vc2)
                        FormsInputManager.Current.InputNumber<sbyte>(this, c, bounds, FontName, FontStyle, FontSize, vc2.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);

                    else if (type == typeof(ushort) && c is GoInputNumber<ushort> vc3)
                        FormsInputManager.Current.InputNumber<ushort>(this, c, bounds, FontName, FontStyle, FontSize, vc3.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(short) && c is GoInputNumber<short> vc4)
                        FormsInputManager.Current.InputNumber<short>(this, c, bounds, FontName, FontStyle, FontSize, vc4.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);

                    else if (type == typeof(uint) && c is GoInputNumber<uint> vc5)
                        FormsInputManager.Current.InputNumber<uint>(this, c, bounds, FontName, FontStyle, FontSize, vc5.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(int) && c is GoInputNumber<int> vc6)
                        FormsInputManager.Current.InputNumber<int>(this, c, bounds, FontName, FontStyle, FontSize, vc6.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);

                    else if (type == typeof(ulong) && c is GoInputNumber<ulong> vc7)
                        FormsInputManager.Current.InputNumber<ulong>(this, c, bounds, FontName, FontStyle, FontSize, vc7.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(long) && c is GoInputNumber<long> vc9)
                        FormsInputManager.Current.InputNumber<long>(this, c, bounds, FontName, FontStyle, FontSize, vc9.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);

                    else if (type == typeof(float) && c is GoInputNumber<float> vc10)
                        FormsInputManager.Current.InputNumber<float>(this, c, bounds, FontName, FontStyle, FontSize, vc10.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(double) && c is GoInputNumber<double> vc11)
                        FormsInputManager.Current.InputNumber<double>(this, c, bounds, FontName, FontStyle, FontSize, vc11.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                    else if (type == typeof(decimal) && c is GoInputNumber<decimal> vc12)
                        FormsInputManager.Current.InputNumber<decimal>(this, c, bounds, FontName, FontStyle, FontSize, vc12.ValueColor, TextColor, (v) => { callback(v); Invalidate(); }, spkey, type, value, min, max);
                }
            };
        }
        #endregion

        #region Override
        public override void OnContentDraw(Going.UI.Forms.Controls.ContentDrawEventArgs e)
        {
            tpnl.Bounds = Util.FromRect(Util.FromRect(0, 0, ClientSize.Width, ClientSize.Height), new GoPadding(10));
            using (new SKAutoCanvasRestore(e.Canvas))
            {
                e.Canvas.Translate(tpnl.Left, tpnl.Top);
                tpnl.FireDraw(e.Canvas);
            }
            base.OnContentDraw(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            tpnl.FireMouseDown(e.X - tpnl.Left, e.Y - tpnl.Top, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            tpnl.FireMouseMove(e.X - tpnl.Left, e.Y - tpnl.Top);
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            tpnl.FireMouseUp(e.X - tpnl.Left, e.Y - tpnl.Top, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            tpnl.FireMouseMove(-1, -1);
            Invalidate();
            base.OnMouseLeave(e);
        }
        #endregion

        #region Method
        #region Show[Class]
        public T? ShowInputBox<T>(string title, int columnCount = 1, T? value = null, Dictionary<string, InputBoxInfo>? infos = null) where T : class
        {
            T? ret = null;
            this.Text = title;

            #region var
            var ps = typeof(T).GetProperties();
            var props = ps.Where(x => CheckProp(x, infos != null && infos.TryGetValue(x.Name, out InputBoxInfo? value) ? value : null)).ToList();
            var rowCount = Convert.ToInt32(Math.Ceiling((double)props.Count / (double)columnCount));
            var csz = 100F / columnCount;
            var rsz = 100F / rowCount;
            #endregion
            #region Size
            var TitleHeight = 23;
            var w = Convert.ToInt32(Math.Max(10 + (columnCount * (ItemTitleWidth + ItemValueWidth + 10)) + 10, minw));
            var h = Convert.ToInt32(Math.Max(TitleHeight + 10 + (rowCount * (ItemHeight + 6)) + 10 + 40 + 10, minh));
            this.Width = w;
            this.Height = h;
            #endregion
            #region Layout
            var lsc = new List<string>(); for (int i = 0; i < columnCount; i++) lsc.Add($"{csz}%");
            var lsr = new List<string>(); for (int i = 0; i < rowCount; i++) lsr.Add($"{rsz}%");

            tpnl2.Childrens.Clear();
            tpnl2.Columns = lsc;
            tpnl2.Rows = lsr;
            foreach (var v in props)
            {
                #region var
                var p = (infos != null && infos.ContainsKey(v.Name) ? infos[v.Name] : null);

                var name = p?.Title ?? v.Name;
                var count = tpnl2.Childrens.Count();
                var col = count % columnCount;
                var row = count / columnCount;
                var min = p?.Minimum;
                var max = p?.Maximum;
                var format = p?.FormatString;
                #endregion

                #region Items
                if (p != null && p.Items != null && p.Items.Count > 0)
                {
                    #region Selector
                    var c = new GoInputCombo
                    {
                        Fill = true,
                        Title = name,
                        TitleSize = ItemTitleWidth,
                        Items = p.Items,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                    };
                    c.DropDownOpening += (o, s) => { s.Cancel = true; OpenDropDown(c); };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null)
                    {
                        var val = v.GetValue((object)value);
                        var itm = c.Items.Where(x => val != null && val.Equals(x.Tag)).FirstOrDefault();
                        if (itm != null) c.SelectedIndex = c.Items.IndexOf(itm);
                    }
                    #endregion
                }
                else if (v.PropertyType == typeof(sbyte))
                {
                    #region byte
                    var c = new GoInputNumber<sbyte>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToSByte(min.Value) : null,
                        Maximum = max != null ? Convert.ToSByte(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is sbyte vv ? vv : (sbyte)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(short))
                {
                    #region short
                    var c = new GoInputNumber<short>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToInt16(min.Value) : null,
                        Maximum = max != null ? Convert.ToInt16(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is short vv ? vv : (short)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(int))
                {
                    #region int
                    var c = new GoInputNumber<int>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToInt32(min.Value) : null,
                        Maximum = max != null ? Convert.ToInt32(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is int vv ? vv : (int)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(long))
                {
                    #region long
                    var c = new GoInputNumber<long>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToInt64(min.Value) : null,
                        Maximum = max != null ? Convert.ToInt64(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is long vv ? vv : (long)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(byte))
                {
                    #region byte
                    var c = new GoInputNumber<byte>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToByte(min.Value) : null,
                        Maximum = max != null ? Convert.ToByte(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is byte vv ? vv : (byte)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(ushort))
                {
                    #region ushort
                    var c = new GoInputNumber<ushort>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToUInt16(min.Value) : null,
                        Maximum = max != null ? Convert.ToUInt16(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is ushort vv ? vv : (ushort)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(uint))
                {
                    #region uint
                    var c = new GoInputNumber<uint>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToUInt32(min.Value) : null,
                        Maximum = max != null ? Convert.ToUInt32(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is uint vv ? vv : (uint)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(ulong))
                {
                    #region ulong
                    var c = new GoInputNumber<ulong>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToUInt64(min.Value) : null,
                        Maximum = max != null ? Convert.ToUInt64(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is ulong vv ? vv : (ulong)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(float))
                {
                    #region float
                    var c = new GoInputNumber<float>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToSingle(min.Value) : null,
                        Maximum = max != null ? Convert.ToSingle(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is float vv ? vv : (float)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(double))
                {
                    #region double
                    var c = new GoInputNumber<double>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToDouble(min.Value) : null,
                        Maximum = max != null ? Convert.ToDouble(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is double vv ? vv : (double)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(decimal))
                {
                    #region decimal
                    var c = new GoInputNumber<decimal>
                    {
                        Fill = true,
                        Title = name,
                        Minimum = min != null ? Convert.ToDecimal(min.Value) : null,
                        Maximum = max != null ? Convert.ToDecimal(max.Value) : null,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                        FormatString = format
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is decimal vv ? vv : (decimal)0);
                    #endregion
                }
                else if (v.PropertyType == typeof(string))
                {
                    #region string
                    var c = new GoInputString
                    {
                        Fill = true,
                        Title = name,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is string vv ? vv : "");
                    #endregion
                }
                else if (v.PropertyType == typeof(bool))
                {
                    #region bool
                    var c = new GoInputBoolean
                    {
                        Fill = true,
                        Title = name,
                        TitleSize = ItemTitleWidth,
                        OnText = p?.OnText ?? "ON",
                        OffText = p?.OffText ?? "OFF",
                        Tag = new InputBoxTag() { prop = v, attr = p },
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    if (value != null) c.Value = (v.GetValue(value) is bool vv ? vv : false);
                    #endregion
                }
                else if (v.PropertyType.IsEnum)
                {
                    #region enum
                    var c = new GoInputCombo
                    {
                        Fill = true,
                        Title = name,
                        TitleSize = ItemTitleWidth,
                        Tag = new InputBoxTag() { prop = v, attr = p },
                    };
                    c.SetInvalidate(Invalidate);
                    tpnl2.Childrens.Add(c, col, row);
                    c.Items.AddRange(Enum.GetValues(v.PropertyType).Cast<object>().Select(x => new GoListItem() { Text = x.ToString(), Tag = x }));
                    c.DropDownOpening += (o, s) => { s.Cancel = true; OpenDropDown(c); };

                    if (value != null)
                    {
                        var val = v.GetValue((object)value);
                        var itm = c.Items.Where(x => val != null && val.Equals(x.Tag)).FirstOrDefault();
                        if (itm != null) c.SelectedIndex = c.Items.IndexOf(itm);
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            if (this.ShowDialog() == DialogResult.OK)
            {
                var v = Activator.CreateInstance<T>();
                #region Value
                foreach (var c in tpnl2.Childrens.Cast<GoControl>())
                {
                    if (c != null && c.Tag is InputBoxTag tag)
                    {
                        var p = tag.prop;
                        var info = tag.attr;
                        if (p != null)
                        {
                            if (c is GoInputNumber<byte> c1) p.SetValue(v, c1.Value);
                            else if (c is GoInputNumber<ushort> c2) p.SetValue(v, c2.Value);
                            else if (c is GoInputNumber<uint> c3) p.SetValue(v, c3.Value);
                            else if (c is GoInputNumber<ulong> c4) p.SetValue(v, c4.Value);
                            else if (c is GoInputNumber<sbyte> c5) p.SetValue(v, c5.Value);
                            else if (c is GoInputNumber<short> c6) p.SetValue(v, c6.Value);
                            else if (c is GoInputNumber<int> c7) p.SetValue(v, c7.Value);
                            else if (c is GoInputNumber<long> c8) p.SetValue(v, c8.Value);
                            else if (c is GoInputNumber<float> c9) p.SetValue(v, c9.Value);
                            else if (c is GoInputNumber<double> c10) p.SetValue(v, c10.Value);
                            else if (c is GoInputNumber<decimal> c11) p.SetValue(v, c11.Value);
                            else if (c is GoInputString c12) p.SetValue(v, c12.Value);
                            else if (c is GoInputBoolean c13) p.SetValue(v, c13.Value);
                            else if (c is GoInputCombo c14)
                            {
                                if (c14.SelectedIndex >= 0 && c14.SelectedIndex < c14.Items.Count)
                                {
                                    var vt = c14.Items[c14.SelectedIndex].Tag;
                                    p.SetValue(v, c14.Items[c14.SelectedIndex].Tag);
                                }
                            }
                        }
                    }
                }
                #endregion
                ret = v;
            }

            return ret;
        }
        #endregion
        #region Show[Type]
        public string? ShowString(string title, string? value = null)
        {
            string? ret = null;
            this.Text = title;

            var w = Convert.ToInt32(Math.Max(Width, minw));
            var h = Convert.ToInt32(Math.Max(Height, minh));
            this.Width = w;
            this.Height = h;

            tpnl2.Columns = ["100%"];
            tpnl2.Rows = ["100%"];
            tpnl2.Childrens.Clear();
            tpnl2.Childrens.Add(new GoInputString { Fill = true, Value = value ?? "" }, 0, 0);

            if(this.ShowDialog() == DialogResult.OK)
            {
                ret = tpnl2.Childrens[0] is GoInputString c ? c.Value : null;
            }

            return ret;
        }

        public T? ShowNumber<T>(string title, T? value = null, T? min = null, T? max = null) where T : struct
        {
            T? ret = null;
            this.Text = title;
 
            var w = Convert.ToInt32(Math.Max(Width, minw));
            var h = Convert.ToInt32(Math.Max(Height, minh));
            this.Width = w;
            this.Height = h;

            tpnl2.Columns = ["100%"];
            tpnl2.Rows = ["100%"];
            tpnl2.Childrens.Clear();
            tpnl2.Childrens.Add(new GoInputNumber<T> { Fill = true, Value = value ?? default, Minimum = min, Maximum = max }, 0, 0);

            if (this.ShowDialog() == DialogResult.OK)
            {
                ret = tpnl2.Childrens[0] is GoInputNumber<T> c ? c.Value : null;
            }

            return ret;
        }

        public bool? ShowBool(string title, bool value = false, string onText = "True", string offText = "False")
        {
            bool? ret = null;
            this.Text = title;

            var w = Convert.ToInt32(Math.Max(Width, minw));
            var h = Convert.ToInt32(Math.Max(Height, minh));
            this.Width = w;
            this.Height = h;

            tpnl2.Columns = ["100%"];
            tpnl2.Rows = ["100%"];
            tpnl2.Childrens.Clear();
            tpnl2.Childrens.Add(new GoInputBoolean { Fill = true, Value = value, OnText = onText, OffText = offText }, 0, 0);

            if (this.ShowDialog() == DialogResult.OK)
            {
                ret = tpnl2.Childrens[0] is GoInputBoolean c ? c.Value : null;
            }

            return ret;
        }
        #endregion

        #region CheckProp
        bool CheckProp(PropertyInfo prop, InputBoxInfo? attr)
        {
            bool ret = false;
            var p = attr;

            if (p != null && p.Items != null && p.Items.Count > 0) ret = true;
            if (prop.PropertyType == typeof(sbyte)) ret = true;
            else if (prop.PropertyType == typeof(short)) ret = true;
            else if (prop.PropertyType == typeof(int)) ret = true;
            else if (prop.PropertyType == typeof(long)) ret = true;
            else if (prop.PropertyType == typeof(byte)) ret = true;
            else if (prop.PropertyType == typeof(ushort)) ret = true;
            else if (prop.PropertyType == typeof(uint)) ret = true;
            else if (prop.PropertyType == typeof(ulong)) ret = true;
            else if (prop.PropertyType == typeof(float)) ret = true;
            else if (prop.PropertyType == typeof(double)) ret = true;
            else if (prop.PropertyType == typeof(decimal)) ret = true;
            else if (prop.PropertyType == typeof(string)) ret = true;
            else if (prop.PropertyType == typeof(bool)) ret = true;
            else if (prop.PropertyType.IsEnum) ret = true;

            return ret && prop.CanWrite && prop.CanRead && !Attribute.IsDefined(prop, typeof(InputBoxIgnoreAttribute));
        }
        #endregion

        #region DropDown
        GoComboBoxDropDownWindow? dwnd;

        void Opened(GoInputCombo cmb)
        {
            if (dwnd != null)
            {
                var sel = cmb.SelectedIndex >= 0 && cmb.SelectedIndex < cmb.Items.Count ? cmb.Items[cmb.SelectedIndex] : null;
                dwnd.Set(FontName, FontStyle, FontSize, cmb.ItemHeight, cmb.MaximumViewCount, cmb.Items, sel, (item) =>
                {
                    if (item != null)
                        cmb.SelectedIndex = cmb.Items.IndexOf(item);
                });
            }
        }

        #region var
        bool closedWhileInControl;

        DropWindowState DropState { get; set; }
        bool CanDrop
        {
            get
            {
                if (dwnd != null) return false;
                if (dwnd == null && closedWhileInControl)
                {
                    closedWhileInControl = false;
                    return false;
                }

                return !closedWhileInControl;
            }
        }
        #endregion
        #region Method
        #region Freeze
        internal void FreezeDropDown(bool remainVisible)
        {
            if (dwnd != null)
            {
                dwnd.Freeze = true;
                if (!remainVisible) dwnd.Visible = false;
            }
        }

        internal void UnFreezeDropDown()
        {
            if (dwnd != null)
            {
                dwnd.Freeze = false;
                if (!dwnd.Visible) dwnd.Visible = true;
            }
        }
        #endregion
        #region Open
        private void OpenDropDown(GoInputCombo cmb)
        {
            var rtValue = cmb.Areas()["Value"];
            this.Move += (o, s) => { if (dwnd != null) dwnd.Bounds = GetDropDownBounds(cmb, rtValue); };

            dwnd = new();
            dwnd.Bounds = GetDropDownBounds(cmb, rtValue);
            dwnd.DropStateChanged += (o, s) => { DropState = s.DropState; };
            dwnd.FormClosed += (o, s) =>
            {
                if (dwnd != null && !dwnd.IsDisposed) dwnd.Dispose();
                dwnd = null;
                closedWhileInControl = (this.RectangleToScreen(this.ClientRectangle).Contains(Cursor.Position));
                DropState = DropWindowState.Closed;
                this.Invalidate();
            };

            Opened(cmb);

            DropState = DropWindowState.Dropping;
            dwnd.Show(this);
            DropState = DropWindowState.Dropped;
            this.Invalidate();
        }
        #endregion
        #region Close
        public void CloseDropDown()
        {
            if (dwnd != null)
            {
                DropState = DropWindowState.Closing;
                dwnd.Freeze = false;
                dwnd.Close();
            }
        }
        #endregion
        #region Bounds
        private Rectangle GetDropDownBounds(GoInputCombo cmb, SKRect rtValue)
        {
            int n = cmb.Items.Count;
            Point pt = PointToScreen(new Point(Convert.ToInt32(rtValue.Left + cmb.Left + tpnl.Left), Convert.ToInt32(rtValue.Bottom + cmb.Top + tpnl.Top + 1)));
            if (cmb.MaximumViewCount != -1) n = cmb.Items.Count > cmb.MaximumViewCount ? cmb.MaximumViewCount : cmb.Items.Count;
            Size inflatedDropSize = new Size(Convert.ToInt32(rtValue.Width), n * cmb.ItemHeight + 2);
            Rectangle screenBounds = new Rectangle(pt, inflatedDropSize);
            Rectangle workingArea = Screen.GetWorkingArea(screenBounds);

            if (screenBounds.X < workingArea.X) screenBounds.X = workingArea.X;
            if (screenBounds.Y < workingArea.Y) screenBounds.Y = workingArea.Y;

            if (screenBounds.Right > workingArea.Right && workingArea.Width > screenBounds.Width) screenBounds.X = workingArea.Right - screenBounds.Width;
            if (screenBounds.Bottom > workingArea.Bottom && workingArea.Height > screenBounds.Height) screenBounds.Y = Convert.ToInt32(pt.Y - rtValue.Height - screenBounds.Height);
            return screenBounds;
        }
        #endregion
        #endregion
        #endregion

        #region SpecialKey
        void spkey(Keys key, Keys modifier)
        {
        }
        #endregion
        #endregion
    }

    #region class : InputBoxTag
    internal class InputBoxTag
    {
        public PropertyInfo? prop { get; set; }
        public InputBoxInfo? attr { get; set; }
    }
    #endregion
}
