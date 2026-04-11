using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Dialogs
{
    /// <summary>
    /// 선택 다이얼로그. 체크박스, 라디오 버튼, 콤보박스 방식으로 항목을 선택할 수 있는 윈도우를 제공합니다.
    /// </summary>
    public class GoSelectorBox : GoWindow
    {
        #region Properties
        /// <summary>
        /// 확인 버튼의 텍스트를 가져오거나 설정합니다.
        /// </summary>
        public string OkText { get => btnOk.Text; set => btnOk.Text = value; }
        /// <summary>
        /// 취소 버튼의 텍스트를 가져오거나 설정합니다.
        /// </summary>
        public string CancelText { get => btnCancel.Text; set => btnCancel.Text = value; }
        #endregion

        #region Member Varaible
        GoTableLayoutPanel tpnl;
        GoTableLayoutPanel tpnl2;
        GoButton btnOk, btnCancel;

        string type = "";
        Action<List<GoListItem>?>? chkcallback;
        Action<GoListItem?>? callback;
        #endregion

        #region Constructor
        public GoSelectorBox()
        {
            IconString = "fa-check";
            IconSize = 18;
            IconGap = 10;

            tpnl = new GoTableLayoutPanel { Dock = GoDockStyle.Fill, Margin = new GoPadding(10), Columns = ["34%", "33%", "33%"], Rows = ["100%", "40px"] };
            tpnl2 = new GoTableLayoutPanel { Dock = GoDockStyle.Fill, Margin = new GoPadding(0), };
            btnOk = new GoButton { Dock = GoDockStyle.Fill, Text = "확인" };
            btnCancel = new GoButton { Dock = GoDockStyle.Fill, Text = "취소" };

            tpnl.Childrens.Add(tpnl2, 0, 0, 3, 1);
            tpnl.Childrens.Add(btnOk, 1, 1);
            tpnl.Childrens.Add(btnCancel, 2, 1);
            Childrens.Add(tpnl);

            btnOk.ButtonClicked += (o, s) =>
            {
                Close();

                if (type == "Check") chkcallback?.Invoke(tpnl2.Childrens.Cast<GoCheckBox>().Where(x => x.Checked && x.Tag is GoListItem).Select(x => (GoListItem)x.Tag).ToList());
                else if (type == "Radio") callback?.Invoke(tpnl2.Childrens.Cast<GoRadioBox>().FirstOrDefault(x => x.Checked && x.Tag is GoListItem)?.Tag as GoListItem);
                else if (type == "Combo") callback?.Invoke(tpnl2.Childrens[0] is GoInputCombo c && c.SelectedIndex >= 0 ? c.Items[c.SelectedIndex] : null);
            };
            
            btnCancel.ButtonClicked += (o, s) =>
            {
                Close();

                if (type == "Check") chkcallback?.Invoke(null);
                else callback?.Invoke(null);
            };
        }
        #endregion

        #region Method
        #region ShowCheck
        /// <summary>
        /// 체크박스 방식의 다중 선택 다이얼로그를 표시합니다.
        /// </summary>
        /// <param name="title">다이얼로그 제목</param>
        /// <param name="columnCount">레이아웃 열 수</param>
        /// <param name="items">선택 가능한 항목 목록</param>
        /// <param name="selectedItems">초기 선택된 항목 목록</param>
        /// <param name="result">선택 결과 콜백. 취소 시 null이 전달됩니다.</param>
        public void ShowCheck(string title, int columnCount, List<GoListItem> items, List<GoListItem>? selectedItems, Action<List<GoListItem>?> result)
        {
            this.Text = title;
            this.type = "Check";
            this.chkcallback = result;

            var isz = items.Count > 0 ? items.Max(x => Util.MeasureTextIcon(x.Text, FontName, FontStyle, FontSize, x.IconString, FontSize + 2, Enums.GoDirectionHV.Horizon, 5).Width) : 0;

            var rowCount = Convert.ToInt32(Math.Ceiling((double)items.Count / (double)columnCount));
            var csz = 100F / columnCount;
            var rsz = 100F / rowCount;

            var lsc = new List<string>(); for (int i = 0; i < columnCount; i++) lsc.Add($"{csz}%");
            var lsr = new List<string>(); for (int i = 0; i < rowCount; i++) lsr.Add($"{rsz}%");

            tpnl2.Childrens.Clear();
            tpnl2.Columns = lsc;
            tpnl2.Rows = lsr;

            int ic = 0, ir = 0;
            foreach (var v in items)
            {
                tpnl2.Childrens.Add(new GoCheckBox { Dock = GoDockStyle.Fill, Text = v.Text ?? "", Tag = v, ContentAlignment = GoContentAlignment.MiddleLeft, BoxSize = 20, Checked = selectedItems?.Contains(v) ?? false }, ic, ir);

                ic++;
                if (ic == columnCount) { ic = 0; ir++; }
            }

            var w = Math.Max(((isz + 40) * columnCount) + 40, 200);
            var h = Math.Max((40 * rowCount) + 100, 120);

            Show(w, h);
        }

        /// <summary>
        /// 열거형 타입의 체크박스 방식 다중 선택 다이얼로그를 표시합니다.
        /// </summary>
        /// <typeparam name="T">열거형 타입</typeparam>
        /// <param name="title">다이얼로그 제목</param>
        /// <param name="columnCount">레이아웃 열 수</param>
        /// <param name="selectedItems">초기 선택된 항목 목록</param>
        /// <param name="result">선택 결과 콜백. 취소 시 null이 전달됩니다.</param>
        /// <param name="textCnverter">열거형 값을 텍스트로 변환하는 함수</param>
        public void ShowCheck<T>(string title, int columnCount, List<T>? selectedItems, Action<List<T>?> result, Func<T, string>? textCnverter = null) where T : struct, Enum
        {
            List<GoListItem> items = Enum.GetValues<T>().Select(x => new GoListItem { Text = textCnverter != null ? textCnverter(x) : x.ToString(), Tag = x }).ToList();
            List<GoListItem>? sel = selectedItems != null ? items.Where(x => x.Tag is T v && selectedItems.Contains(v)).ToList() : null;
            ShowCheck(title, columnCount, items, sel, (vret) =>
            {
                if (vret != null)
                {
                    var ls = new List<T>();
                    foreach (var v in vret) if (v.Tag is T r) ls.Add(r);
                    result(ls);
                }
                else result(null);
            });
        }
        #endregion
        #region ShowRadio
        /// <summary>
        /// 라디오 버튼 방식의 단일 선택 다이얼로그를 표시합니다.
        /// </summary>
        /// <param name="title">다이얼로그 제목</param>
        /// <param name="columnCount">레이아웃 열 수</param>
        /// <param name="items">선택 가능한 항목 목록</param>
        /// <param name="selectedItem">초기 선택된 항목</param>
        /// <param name="result">선택 결과 콜백. 취소 시 null이 전달됩니다.</param>
        public void ShowRadio(string title, int columnCount, List<GoListItem> items, GoListItem? selectedItem, Action<GoListItem?> result)
        {
            this.Text = title;
            this.type = "Radio";
            this.callback = result;

            var isz = items.Count > 0 ? items.Max(x => Util.MeasureTextIcon(x.Text, FontName, FontStyle, FontSize, x.IconString, FontSize + 2, Enums.GoDirectionHV.Horizon, 5).Width) : 0;

            var rowCount = Convert.ToInt32(Math.Ceiling((double)items.Count / (double)columnCount));
            var csz = 100F / columnCount;
            var rsz = 100F / rowCount;

            var lsc = new List<string>(); for (int i = 0; i < columnCount; i++) lsc.Add($"{csz}%");
            var lsr = new List<string>(); for (int i = 0; i < rowCount; i++) lsr.Add($"{rsz}%");

            tpnl2.Childrens.Clear();
            tpnl2.Columns = lsc;
            tpnl2.Rows = lsr;

            int ic = 0, ir = 0;
            foreach (var v in items)
            {
                tpnl2.Childrens.Add(new GoRadioBox { Dock = GoDockStyle.Fill, Text = v.Text ?? "", Tag = v, ContentAlignment = GoContentAlignment.MiddleLeft, BoxSize = 20, Checked = selectedItem == v }, ic, ir);

                ic++;
                if (ic == columnCount) { ic = 0; ir++; }
            }

            var w = Math.Max(((isz + 40) * columnCount) + 40, 200);
            var h = Math.Max((40 * rowCount) + 100, 120);

            Show(w, h);
        }

        /// <summary>
        /// 열거형 타입의 라디오 버튼 방식 단일 선택 다이얼로그를 표시합니다.
        /// </summary>
        /// <typeparam name="T">열거형 타입</typeparam>
        /// <param name="title">다이얼로그 제목</param>
        /// <param name="columnCount">레이아웃 열 수</param>
        /// <param name="selectedItem">초기 선택된 항목</param>
        /// <param name="result">선택 결과 콜백. 취소 시 null이 전달됩니다.</param>
        /// <param name="textCnverter">열거형 값을 텍스트로 변환하는 함수</param>
        public void ShowRadio<T>(string title, int columnCount, T? selectedItem, Action<T?> result, Func<T, string>? textCnverter = null) where T : struct, Enum
        {
            var items = Enum.GetValues<T>().Select(x => new GoListItem { Text = textCnverter != null ? textCnverter(x) : x.ToString(), Tag = x }).ToList();
            var sel = selectedItem != null ? items.FirstOrDefault(x => selectedItem.Value.Equals(x.Tag)) : null;
            ShowRadio(title, columnCount, items, sel, (ret) =>
            {
                if (ret?.Tag is T v) result(v);
                else result(null);
            });
        }
        #endregion
        #region ShowCombo
        /// <summary>
        /// 콤보박스 방식의 단일 선택 다이얼로그를 표시합니다.
        /// </summary>
        /// <param name="title">다이얼로그 제목</param>
        /// <param name="items">선택 가능한 항목 목록</param>
        /// <param name="selectedItem">초기 선택된 항목</param>
        /// <param name="result">선택 결과 콜백. 취소 시 null이 전달됩니다.</param>
        public void ShowCombo(string title, List<GoListItem> items, GoListItem? selectedItem, Action<GoListItem?> result)
        {
            this.Text = title;
            this.type = "Combo";
            this.callback = result;

            tpnl2.Childrens.Clear();
            tpnl2.Columns = ["100%"];
            tpnl2.Rows = ["100%"];

            var isz = items.Count > 0 ? items.Max(x => Util.MeasureTextIcon(x.Text, FontName, FontStyle, FontSize, x.IconString, FontSize + 2, Enums.GoDirectionHV.Horizon, 5).Width) : 0;

            var cmb = new GoInputCombo { Dock = GoDockStyle.Fill, Items = items, SelectedIndex = selectedItem != null ? items.IndexOf(selectedItem) : -1 };
            tpnl2.Childrens.Add(cmb, 0, 0);
           
            var w = Math.Max(isz + 80, 200);
            var h = Math.Max(40 + 100, 120);

            Show(w, h);
        }

        /// <summary>
        /// 열거형 타입의 콤보박스 방식 단일 선택 다이얼로그를 표시합니다.
        /// </summary>
        /// <typeparam name="T">열거형 타입</typeparam>
        /// <param name="title">다이얼로그 제목</param>
        /// <param name="selectedItem">초기 선택된 항목</param>
        /// <param name="result">선택 결과 콜백. 취소 시 null이 전달됩니다.</param>
        /// <param name="textCnverter">열거형 값을 텍스트로 변환하는 함수</param>
        public void ShowCombo<T>(string title, T? selectedItem, Action<T?> result, Func<T, string>? textCnverter = null) where T : struct, Enum
        {
            var items = Enum.GetValues<T>().Select(x => new GoListItem { Text = textCnverter != null ? textCnverter(x) : x.ToString(), Tag = x }).ToList();
            var sel = selectedItem != null ? items.FirstOrDefault(x => selectedItem.Value.Equals(x.Tag)) : null;
            ShowCombo(title, items, sel, (ret) =>
            {
                if (ret?.Tag is T v) result(v);
                else result(null);
            });
        }
        #endregion
        #endregion
    }
}
