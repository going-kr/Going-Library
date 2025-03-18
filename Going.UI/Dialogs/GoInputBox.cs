using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Dialogs
{
    public class GoInputBox : GoWindow
    {
        #region Const
        float minw = 200;
        float minh = 140;
        #endregion

        #region Properties
        public string OkText { get => btnOk.Text; set => btnOk.Text = value; }
        public string CancelText { get => btnCancel.Text; set => btnCancel.Text = value; }

        public int ItemHeight { get; set; } = 40;
        public int ItemTitleWidth { get; set; } = 80;
        public int ItemValueWidth { get; set; } = 150;
        #endregion

        #region Member Varaible
        GoTableLayoutPanel tpnl;
        GoTableLayoutPanel tpnl2;
        GoButton btnOk, btnCancel;

        string type = "";
        Type? numberType;
        Type? tmplType;
        Action<string?>? stringcallback;
        Action<object?>? numbercallback;
        Action<bool?>? boolcallback;
        Action<object?>? tmplcallback;
        #endregion

        #region Constructor
        public GoInputBox()
        {
            IconString = "fa-pen-to-square";
            IconSize = 18;
            IconGap = 10;

            tpnl = new GoTableLayoutPanel { Fill = true, Margin = new GoPadding(10), Columns = ["34%", "33%", "33%"], Rows = ["100%", "40px"] };
            tpnl2 = new GoTableLayoutPanel { Fill = true, Margin = new GoPadding(0), };
            btnOk = new GoButton { Fill = true, Text = "확인" };
            btnCancel = new GoButton { Fill = true, Text = "취소" };

            tpnl.Childrens.Add(tpnl2, 0, 0, 3, 1);
            tpnl.Childrens.Add(btnOk, 1, 1);
            tpnl.Childrens.Add(btnCancel, 2, 1);
            Childrens.Add(tpnl);

            btnOk.ButtonClicked += (o, s) =>
            {
                Close();

                if (type == "String") stringcallback?.Invoke(tpnl2.Childrens[0] is GoInputString c ? c.Value : null);
                else if (type == "Number")
                {
                    if (numberType == typeof(byte)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<byte> c ? c.Value : null);
                    else if (numberType == typeof(sbyte)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<sbyte> c ? c.Value : null);
                    else if (numberType == typeof(ushort)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<ushort> c ? c.Value : null);
                    else if (numberType == typeof(short)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<short> c ? c.Value : null);
                    else if (numberType == typeof(uint)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<uint> c ? c.Value : null);
                    else if (numberType == typeof(int)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<int> c ? c.Value : null);
                    else if (numberType == typeof(ulong)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<ulong> c ? c.Value : null);
                    else if (numberType == typeof(long)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<long> c ? c.Value : null);
                    else if (numberType == typeof(float)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<float> c ? c.Value : null);
                    else if (numberType == typeof(double)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<double> c ? c.Value : null);
                    else if (numberType == typeof(decimal)) numbercallback?.Invoke(tpnl2.Childrens[0] is GoInputNumber<decimal> c ? c.Value : null);
                    else numbercallback?.Invoke(null);
                }
                else if (type == "Bool") boolcallback?.Invoke(tpnl2.Childrens[0] is GoInputBoolean c ? c.Value : null);
                else if (type == "Template" && tmplType != null)
                {
                    var v = Activator.CreateInstance(tmplType);
                    #region Value
                    foreach (var c in tpnl2.Childrens.Cast<GoControl>())
                    {
                        if (c != null && c.Tag is InputBoxTag tag)
                        {
                            var p = tag.prop;
                            var info = tag.attr;
                            if (p != null )
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
                    tmplcallback?.Invoke(v);
                }
            };

            btnCancel.ButtonClicked += (o, s) =>
            {
                Close();

                if (type == "String") stringcallback?.Invoke(null);
                else if (type == "Number") numbercallback?.Invoke(null);
                else if (type == "Bool") boolcallback?.Invoke(null);
            };
        }
        #endregion

        #region Method
        #region Show[Class]
        public void Showinputbox<T>(string title, int columnCount, T? value, Dictionary<string, InputBoxInfo> infos, Action<T?> result) where T : class
        {
            this.Text = title;
            type = "Template";
            tmplType = typeof(T);
            tmplcallback = (ret) =>
            {
                if (ret is T v) result(v);
                else result(null);
            };

            #region var
            var ps = typeof(T).GetProperties();
            var props = ps.Where(x => CheckProp(x, infos != null && infos.TryGetValue(x.Name, out InputBoxInfo? value) ? value : null)).ToList();
            var rowCount = Convert.ToInt32(Math.Ceiling((double)props.Count / (double)columnCount));
            var csz = 100F / columnCount;
            var rsz = 100F / rowCount;
            #endregion
            #region Size
            var w = Math.Max(10 + (columnCount * (ItemTitleWidth + ItemValueWidth + 10)) + 10, minw);
            var h = Math.Max(TitleHeight + 10 + (rowCount * (ItemHeight + 6)) + 10 + 40 + 10, minh);
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
                    tpnl2.Childrens.Add(c, col, row);
                    c.Items.AddRange(Enum.GetValues(v.PropertyType).Cast<object>().Select(x => new GoListItem() { Text = x.ToString(), Tag = x }));

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

            Show(w, h);
        }
        #endregion
        #region Show[Type]
        public void ShowString(string title, Action<string?> result) => ShowString(title, null, result);
        public void ShowString(string title, string? value, Action<string?> result)
        {
            this.Text = title;
            type = "String";
            stringcallback = result;

            var w = Math.Max(Width, minw);
            var h = Math.Max(Height, minh);

            tpnl2.Columns = ["100%"];
            tpnl2.Rows = ["100%"];
            tpnl2.Childrens.Clear();
            tpnl2.Childrens.Add(new GoInputString { Fill = true, Value = value ?? "" }, 0, 0);

            Show(w, h);
        }

        public void ShowNumber<T>(string title, Action<T?> result) where T : struct => ShowNumber<T>(title, null, null, null, result);
        public void ShowNumber<T>(string title, T? value, Action<T?> result) where T : struct => ShowNumber<T>(title, value, null, null, result);
        public void ShowNumber<T>(string title, T? min, T? max, Action<T?> result) where T : struct => ShowNumber<T>(title, null, min, max, result);
        public void ShowNumber<T>(string title, T? value, T? min, T? max, Action<T?> result) where T : struct
        {
            this.Text = title;
            type = "Number";
            numberType = typeof(T);
            numbercallback = (ret) =>
            {
                if (ret is T v) result(v);
                else result(null);
            };

            var w = Math.Max(Width, minw);
            var h = Math.Max(Height, minh);

            tpnl2.Columns = ["100%"];
            tpnl2.Rows = ["100%"];
            tpnl2.Childrens.Clear();
            tpnl2.Childrens.Add(new GoInputNumber<T> { Fill = true, Value = value ?? default, Minimum = min, Maximum = max }, 0, 0);

            Show(w, h);
        }

        public void ShowBool(string title, Action<bool?> result) => ShowBool(title, false, "True", "False", result);
        public void ShowBool(string title, bool value, Action<bool?> result) => ShowBool(title, value, "True", "False", result);
        public void ShowBool(string title, string onText, string offText, Action<bool?> result) => ShowBool(title, false, onText, offText, result);
        public void ShowBool(string title, bool value, string onText, string offText, Action<bool?> result)
        {
            this.Text = title;
            type = "Bool";
            boolcallback = result;

            var w = Math.Max(Width, minw);
            var h = Math.Max(Height, minh);

            tpnl2.Columns = ["100%"];
            tpnl2.Rows = ["100%"];
            tpnl2.Childrens.Clear();
            tpnl2.Childrens.Add(new GoInputBoolean { Fill = true, Value = value, OnText = onText, OffText = offText }, 0, 0);

            Show(w, h);
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
        #endregion
    }

    #region attr : InputBoxIgnore
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class InputBoxIgnoreAttribute : Attribute { }
    #endregion
    #region class : InputBoxTag
    internal class InputBoxTag
    {
        public PropertyInfo? prop { get; set; }
        public InputBoxInfo? attr { get; set; }
    }
    #endregion
    #region class : InputBoxInfo
    public class InputBoxInfo
    {
        public decimal? Minimum { get; set; } = null;
        public decimal? Maximum { get; set; } = null;

        public string? Title { get; set; }
        public string? FormatString { get; set; }
        public string? OnText { get; set; }
        public string? OffText { get; set; }
        public List<GoListItem>? Items { get; set; }
    }
    #endregion
}
