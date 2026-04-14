using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Design
{
    #region GoDropDownWindow
    /// <summary>
    /// 드롭다운 윈도우 컨트롤. 컨트롤 아래에 팝업으로 표시되는 윈도우를 제공합니다.
    /// </summary>
    public class GoDropDownWindow : GoContainer
    {
        /// <summary>
        /// 윈도우 배경색을 가져오거나 설정합니다.
        /// </summary>
        public string WindowColor { get; set; } = "Window";
        /// <summary>
        /// 윈도우 테두리 색상을 가져오거나 설정합니다.
        /// </summary>
        public string BorderColor { get; set; } = "WindowBorder";
        /// <summary>
        /// 윈도우 모서리 둥글기 타입을 가져오거나 설정합니다.
        /// </summary>
        public GoRoundType Round { get; set; } = GoRoundType.All;

        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        /// <summary>
        /// <see cref="GoDropDownWindow"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        [JsonConstructor]
        public GoDropDownWindow(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary>
        /// <see cref="GoDropDownWindow"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoDropDownWindow() { Visible = false; }

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtWnd = rts["Content"];
            rtWnd = Util.Int(rtWnd);
            rtWnd.Offset(0.5F, 0.5F);
            var rtrWnd = new SKRoundRect(rtWnd, thm.Corner);
            Util.SetRound(rtrWnd, Round, thm.Corner);
            var cBack = thm.ToColor(WindowColor);
            var cBorder = thm.ToColor(BorderColor);

            using var p = new SKPaint { IsAntialias = true };

            if (cBack != SKColors.Transparent)
            {
                using var imgf = SKImageFilter.CreateDropShadow(3, 3, 3, 3, Util.FromArgb(thm.ShadowAlpha, SKColors.Black));
                p.ImageFilter = imgf;
                p.IsStroke = false;
                p.Color = cBack;
                canvas.DrawRoundRect(rtrWnd, p);
                p.ImageFilter = null;

            }

            if (cBorder != SKColors.Transparent)
            {
                p.IsStroke = true;
                p.StrokeWidth = 1F;
                p.Color = cBorder;
                canvas.DrawRoundRect(rtrWnd, p);
            }

            base.OnDraw(canvas, thm);

        }


        /// <summary>
        /// 드롭다운 윈도우를 표시합니다.
        /// </summary>
        public virtual void Show() => GoDesign.ActiveDesign?.ShowDropDownWindow(this);
        /// <summary>
        /// 드롭다운 윈도우를 숨깁니다.
        /// </summary>
        public virtual void Hide() => GoDesign.ActiveDesign?.HideDropDownWindow(this);
    }
    #endregion

    #region GoComboBoxDropDownWindow
    /// <summary>
    /// 콤보박스용 드롭다운 윈도우. 리스트박스를 포함하여 항목 선택 기능을 제공합니다.
    /// </summary>
    public class GoComboBoxDropDownWindow : GoDropDownWindow
    {
        private GoListBox lb;
        private Action<GoListItem?>? feedback;

        /// <summary>
        /// <see cref="GoComboBoxDropDownWindow"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoComboBoxDropDownWindow()
        {
            lb = new GoListBox { Margin = new GoPadding(0), Dock = GoDockStyle.Fill, BackgroundDraw = false };
            Childrens.Add(lb);

            lb.ItemClicked += (o, s) =>
            {
                feedback?.Invoke(s.Item);
                Hide();
            };
        }

        /// <summary>
        /// 콤보박스 드롭다운을 지정한 위치와 항목 목록으로 표시합니다.
        /// </summary>
        /// <param name="screenBounds">드롭다운이 표시될 기준 영역</param>
        /// <param name="fontName">폰트 이름</param>
        /// <param name="fontStyle">폰트 스타일</param>
        /// <param name="fontSize">폰트 크기</param>
        /// <param name="itemHeight">항목 높이</param>
        /// <param name="maximumViewCount">최대 표시 항목 수</param>
        /// <param name="items">선택 가능한 항목 목록</param>
        /// <param name="selectedItem">현재 선택된 항목</param>
        /// <param name="result">선택 결과 콜백</param>
        public void Show(SKRect screenBounds, string fontName, GoFontStyle fontStyle, float fontSize, float itemHeight, int maximumViewCount,
                         List<GoListItem> items, GoListItem? selectedItem,
                         Action<GoListItem?> result)
        {
            feedback = result;

            lb.FontName = fontName;
            lb.FontSize = fontSize;
            lb.FontStyle = fontStyle;
            lb.IconSize = fontSize + 2;
            lb.IconGap = 3;
            lb.ItemHeight = itemHeight;

            var designH = GoDesign.ActiveDesign?.Height ?? 0;
            var rtValue = screenBounds;
            var iw = items.Count > 0 ? items.Select(x => Util.MeasureTextIcon(x.Text, lb.FontName, lb.FontStyle, lb.FontSize, x.IconString, lb.IconSize, GoDirectionHV.Horizon, lb.IconGap)).Max(x => x.Width) : 0F;
            var ih = itemHeight * Math.Min(maximumViewCount, items.Count) + 1;
            var w = Math.Max(iw, rtValue.Width);
            var bounds = Util.FromRect(rtValue.Left, rtValue.Bottom + 1, w, ih);
            if (bounds.Bottom > designH) bounds = Util.FromRect(rtValue.Left, rtValue.Top - ih - 1, Math.Max(iw, rtValue.Width), ih);

            Bounds = bounds;

            lb.Items.Clear();
            lb.SelectedItems.Clear();
            lb.Items.AddRange(items);
            if (selectedItem != null)
            {
                lb.SelectedItems.Add(selectedItem);
                lb.ScrollPosition = lb.Items.IndexOf(selectedItem) * itemHeight;
            }

            Show();
        }
    }
    #endregion

    #region GoColorDropDownWindow
    /// <summary>
    /// 색상 선택용 드롭다운 윈도우. 색상 선택기와 확인/취소 버튼을 포함합니다.
    /// </summary>
    public class GoColorDropDownWindow : GoDropDownWindow
    {
        private GoColorSelector cs;
        private GoTableLayoutPanel tbl;
        private GoButton btnOK, btnCancel;
        private Action<SKColor?>? feedback;

        /// <summary>
        /// <see cref="GoColorDropDownWindow"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoColorDropDownWindow()
        {
            tbl = new GoTableLayoutPanel { Margin = new GoPadding(5), Dock = GoDockStyle.Fill, };
            tbl.Columns = ["100%", "80px", "80px"];
            tbl.Rows = ["100%", "40px"];
            Childrens.Add(tbl);

            cs = new GoColorSelector { Dock = GoDockStyle.Fill, Margin = new GoPadding(5) };
            tbl.Childrens.Add(cs, 0, 0, 3, 1);

            btnOK = new GoButton { Dock = GoDockStyle.Fill, Margin = new GoPadding(5), Text = "선택" };
            btnCancel = new GoButton { Dock = GoDockStyle.Fill, Margin = new GoPadding(5), Text = "취소" };
            tbl.Childrens.Add(btnOK, 1, 1);
            tbl.Childrens.Add(btnCancel, 2, 1);

            btnOK.ButtonClicked += (o, s) =>
            {
                feedback?.Invoke(cs.Value);
                Hide();
            };

            btnCancel.ButtonClicked += (o, s) =>
            {
                feedback?.Invoke(null);
                Hide();
            };
        }

        /// <summary>
        /// 색상 선택 드롭다운을 지정한 위치와 초기 색상으로 표시합니다.
        /// </summary>
        /// <param name="screenBounds">드롭다운이 표시될 기준 영역</param>
        /// <param name="fontName">폰트 이름</param>
        /// <param name="fontStyle">폰트 스타일</param>
        /// <param name="fontSize">폰트 크기</param>
        /// <param name="value">초기 색상 값</param>
        /// <param name="result">선택 결과 콜백. 취소 시 null이 전달됩니다.</param>
        public void Show(SKRect screenBounds, string fontName, GoFontStyle fontStyle, float fontSize, SKColor value, Action<SKColor?> result)
        {
            feedback = result;

            btnOK.FontName = btnCancel.FontName = cs.FontName = fontName;
            btnOK.FontSize = btnCancel.FontSize = cs.FontSize = fontSize;
            btnOK.FontStyle = btnCancel.FontStyle = cs.FontStyle = fontStyle;
            cs.Value = value;

            var designH = GoDesign.ActiveDesign?.Height ?? 0;
            var rtValue = screenBounds;
            var w = MathTool.Constrain(rtValue.Width, 280, 360);
            var h = w + 40;

            var bounds = Util.FromRect(rtValue.Left, rtValue.Bottom + 1, w, h);
            if (bounds.Bottom > designH) bounds = Util.FromRect(rtValue.Left, rtValue.Top - h - 1, Math.Max(w, rtValue.Width), h);
            Bounds = bounds;

            Show();
        }
    }
    #endregion

    #region GoDateTimeDropDownWindow
    /// <summary>
    /// 날짜/시간 선택용 드롭다운 윈도우. 달력과 시/분/초 입력을 포함합니다.
    /// </summary>
    public class GoDateTimeDropDownWindow : GoDropDownWindow
    {
        private GoTableLayoutPanel tbl;
        private GoCalendar cal;
        private GoButton btnOK, btnCancel;
        private GoInputNumber<int> inH, inM, inS;
        private Action<DateTime?>? feedback;
        private GoDateTimeKind style = GoDateTimeKind.DateTime;

        /// <summary>
        /// <see cref="GoDateTimeDropDownWindow"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoDateTimeDropDownWindow()
        {
            tbl = new GoTableLayoutPanel { Margin = new GoPadding(5), Dock = GoDockStyle.Fill, };
            Childrens.Add(tbl);

            cal = new GoCalendar { Dock = GoDockStyle.Fill, Margin = new GoPadding(5), BackgroundDraw = false, MultiSelect = false };
            inH = new GoInputNumber<int> { Dock = GoDockStyle.Fill, Margin = new GoPadding(5), Minimum = 0, Maximum = 23, UnitSize = 30, Unit = "시" };
            inM = new GoInputNumber<int> { Dock = GoDockStyle.Fill, Margin = new GoPadding(5), Minimum = 0, Maximum = 59, UnitSize = 30, Unit = "분" };
            inS = new GoInputNumber<int> { Dock = GoDockStyle.Fill, Margin = new GoPadding(5), Minimum = 0, Maximum = 59, UnitSize = 30, Unit = "초" };
            btnOK = new GoButton { Dock = GoDockStyle.Fill, Margin = new GoPadding(5), Text = "선택" };
            btnCancel = new GoButton { Dock = GoDockStyle.Fill, Margin = new GoPadding(5), Text = "취소" };

            btnOK.ButtonClicked += (o, s) =>
            {
                if (cal.SelectedDays.Count > 0)
                {
                    var dt = cal.SelectedDays.First().Date + new TimeSpan(inH.Value, inM.Value, inS.Value);

                    feedback?.Invoke(dt);
                    Hide();
                }
            };

            btnCancel.ButtonClicked += (o, s) =>
            {
                feedback?.Invoke(null);
                Hide();
            };
        }

        /// <summary>
        /// 날짜/시간 선택 드롭다운을 지정한 위치와 초기 값으로 표시합니다.
        /// </summary>
        /// <param name="screenBounds">드롭다운이 표시될 기준 영역</param>
        /// <param name="fontName">폰트 이름</param>
        /// <param name="fontStyle">폰트 스타일</param>
        /// <param name="fontSize">폰트 크기</param>
        /// <param name="value">초기 날짜/시간 값</param>
        /// <param name="style">날짜/시간 선택 종류 (DateTime, Date, Time)</param>
        /// <param name="result">선택 결과 콜백. 취소 시 null이 전달됩니다.</param>
        public void Show(SKRect screenBounds, string fontName, GoFontStyle fontStyle, float fontSize, DateTime value, GoDateTimeKind style, Action<DateTime?> result)
        {
            feedback = result;
            this.style = style;

            tbl.Childrens.Clear();

            var designH = GoDesign.ActiveDesign?.Height ?? 0;
            var rtValue = screenBounds;
            var w = MathTool.Constrain(rtValue.Width, 240, 300);
            var h = w + 40 + 40;
            if (style == GoDateTimeKind.DateTime)
            {
                h = w + 40 + 40;

                tbl.Columns = ["33.34%", "33.33%", "33.33%"];
                tbl.Rows = ["100%", "40px", "40px"];

                tbl.Childrens.Add(cal, 0, 0, 3, 1);
                tbl.Childrens.Add(inH, 0, 1);
                tbl.Childrens.Add(inM, 1, 1);
                tbl.Childrens.Add(inS, 2, 1);
                tbl.Childrens.Add(btnOK, 1, 2);
                tbl.Childrens.Add(btnCancel, 2, 2);

            }
            else if (style == GoDateTimeKind.Date)
            {
                h = w + 40;

                tbl.Columns = ["33.34%", "33.33%", "33.33%"];
                tbl.Rows = ["100%", "40px"];

                tbl.Childrens.Add(cal, 0, 0, 3, 1);
                tbl.Childrens.Add(btnOK, 1, 1);
                tbl.Childrens.Add(btnCancel, 2, 1);
            }
            else if (style == GoDateTimeKind.Time)
            {
                h = 80 + 10;

                tbl.Columns = ["33.34%", "33.33%", "33.33%"];
                tbl.Rows = ["40px", "40px"];

                tbl.Childrens.Add(inH, 0, 0);
                tbl.Childrens.Add(inM, 1, 0);
                tbl.Childrens.Add(inS, 2, 0);
                tbl.Childrens.Add(btnOK, 1, 1);
                tbl.Childrens.Add(btnCancel, 2, 1);
            }

            #region bounds
            var bounds = Util.FromRect(rtValue.Left, rtValue.Bottom + 1, Math.Max(w, rtValue.Width), h);
            if (bounds.Bottom > designH) bounds = Util.FromRect(rtValue.Left, rtValue.Top - h - 1, Math.Max(w, rtValue.Width), h);
            if(bounds.Top < 0) bounds = Util.FromRect(rtValue.Left, rtValue.MidY - (h/2F), Math.Max(w, rtValue.Width), h);
            Bounds = bounds;
            #endregion

            #region set
            IGoControl[] cls = [cal, inH, inM, inS, btnOK, btnCancel];
            foreach (var v in cls)
            {
                if (v is GoLabel c1) { c1.FontName = fontName; c1.FontSize = fontSize; c1.FontStyle = fontStyle; }
                else if (v is GoInputNumber<int> c2) { c2.FontName = fontName; c2.FontSize = fontSize; c2.FontStyle = fontStyle; }
                else if (v is GoCalendar c3) { c3.FontName = fontName; c3.FontSize = fontSize; c3.FontStyle = fontStyle; }
                else if (v is GoButton c4) { c4.FontName = fontName; c4.FontSize = fontSize; c4.FontStyle = fontStyle; }
            }

            cal.SelectedDays.Clear();
            cal.SelectedDays.Add(value.Date);
            inH.Value = style != GoDateTimeKind.Date ? value.Hour : 0;
            inM.Value = style != GoDateTimeKind.Date ? value.Minute : 0;
            inS.Value = style != GoDateTimeKind.Date ? value.Second : 0;
            #endregion

            Show();
        }
    }
    #endregion
}
