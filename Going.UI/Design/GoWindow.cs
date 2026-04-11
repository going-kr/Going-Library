using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Going.UI.Design
{
    /// <summary>
    /// 윈도우 컨트롤. 타이틀바와 닫기 버튼이 포함된 모달 윈도우를 제공합니다.
    /// </summary>
    public class GoWindow : GoContainer
    {
        #region Properties
        /// <summary>
        /// 타이틀바에 표시할 아이콘 문자열을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        /// <summary>
        /// 아이콘 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        /// <summary>
        /// 아이콘과 텍스트 사이의 간격을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public float IconGap { get; set; } = 5;

        /// <summary>
        /// 윈도우 타이틀 텍스트를 가져오거나 설정합니다.
        /// </summary>
        [GoMultiLineProperty(PCategory.Control, 3)] public string Text { get; set; } = "Window";
        /// <summary>
        /// 폰트 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 4)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 폰트 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>
        /// 폰트 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public float FontSize { get; set; } = 12;

        /// <summary>
        /// 텍스트 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// 윈도우 배경색을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public string WindowColor { get; set; } = "Back";
        /// <summary>
        /// 윈도우 테두리 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public string BorderColor { get; set; } = "Base2";
        /// <summary>
        /// 윈도우 모서리 둥글기 타입을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 10)] public GoRoundType Round { get; set; } = GoRoundType.All;
        /// <summary>
        /// 타이틀바 높이를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public float TitleHeight { get; set; } = 40;

        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        /// <summary>
        /// 패널 영역의 바운드를 가져옵니다.
        /// </summary>
        [JsonIgnore] public override SKRect PanelBounds => Areas()["Panel"];
        #endregion

        #region Member Variable
        bool bCloseHover = false;
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoWindow(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        public GoWindow() { }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            #region var
            var rts = Areas();
            var rtWnd = rts["Content"]; rtWnd = Util.Int(rtWnd); rtWnd.Offset(0.5F, 0.5F);
            var rtrWnd = new SKRoundRect(rtWnd, thm.Corner); Util.SetRound(rtrWnd, Round, thm.Corner);
            var rtTitle = rts["Title"];
            var rtClose = rts["Close"];
            var rtPanel = rts["Panel"];
            var rtTitleText = Util.FromRect(rtTitle, new GoPadding(10, 0, 0, 0));

            var cText = thm.ToColor(TextColor);
            var cBack = thm.ToColor(WindowColor);
            var cBorder = thm.ToColor(BorderColor);

            using var p = new SKPaint { IsAntialias = true };
            #endregion

            #region Fill
            if (cBack != SKColors.Transparent)
            {
                using var imgf = SKImageFilter.CreateDropShadow(3, 3, 3, 3, Util.FromArgb(thm.ShadowAlpha, SKColors.Black));
                p.ImageFilter = imgf;
                p.IsStroke = false;
                p.Color = cBack;
                canvas.DrawRoundRect(rtrWnd, p);
                p.ImageFilter = null;

            }
            #endregion
            #region Border
            if (cBorder != SKColors.Transparent)
            {
                p.IsStroke = true;
                p.StrokeWidth = 1F;
                p.Color = cBorder;
                canvas.DrawRoundRect(rtrWnd, p);
            }
            #endregion
            #region Title
            var x = (Design?.MousePosition.X ?? 0) - Left;
            var y = (Design?.MousePosition.Y ?? 0) - Top;
            var bCloseHover = CollisionTool.Check(rtClose, x, y);
            var cClose = !bCloseHover ? thm.Fore : SKColors.Red;

            Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtTitleText, cText, GoContentAlignment.MiddleLeft);
            Util.DrawIcon(canvas, "fa-xmark", TitleHeight / 2, rtClose, cClose);
            #endregion
            #region Sep
            using var pe = SKPathEffect.CreateDash([3, 3], 2);
            p.PathEffect = pe;
            p.Color = cBorder;
            var sy = Convert.ToInt32(rtTitle.Bottom) + 0.5F;
            canvas.DrawLine(rtTitle.Left + 10, sy, rtTitle.Right - 10, sy, p);
            #endregion

            base.OnDraw(canvas, thm);
        }
        #endregion

        #region Mouse
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtClose = rts["Close"];

            if (CollisionTool.Check(rtClose, x, y))
            {
                OnCloseButtonClick();
            }

            base.OnMouseClick(x, y, button);
        }
        #endregion

        #region Layout
        protected override void OnLayout()
        {
            var rts = Areas();
            var rtPanel = rts["Panel"];

            var nofills = Childrens.Where(c => c.Dock != GoDockStyle.Fill).ToList();
            var fills = Childrens.Where(c => c.Dock == GoDockStyle.Fill).ToList();
            var bnds = Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height); //Util.FromRect(rtPanel);
            foreach (var control in nofills) bnds = ApplyDocking(control, bnds);
            foreach (var c in fills)
            {
                c.Left = bnds.Left + c.Margin.Left;
                c.Top = bnds.Top + c.Margin.Top;
                c.Right = bnds.Right - c.Margin.Right;
                c.Bottom = bnds.Bottom - c.Margin.Bottom;
            }

            //base.OnLayout();
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rts = Util.Rows(dic["Content"], [$"{TitleHeight}px", "100%"]);
            dic["Title"] = rts[0];
            dic["Panel"] = rts[1];
            dic["Close"] = new SKRect(rts[0].Right - TitleHeight, rts[0].Top, rts[0].Right, rts[0].Bottom);
            return dic;
        }
        #endregion
        #endregion

        #region Method
        protected virtual void OnCloseButtonClick() => Close();
        protected virtual void OnClosing(GoCancelableEventArgs e) { }

        /// <summary>
        /// 윈도우를 표시합니다.
        /// </summary>
        public void Show()
        {
            (Design ?? GoDesign.ActiveDesign)?.ShowWindow(this);
        }

        /// <summary>
        /// 지정한 크기로 윈도우를 표시합니다.
        /// </summary>
        /// <param name="width">윈도우 너비</param>
        /// <param name="height">윈도우 높이</param>
        public void Show(float width, float height)
        {
            var design = Design ?? GoDesign.ActiveDesign;
            if (design != null)
            {
                Bounds = MathTool.MakeRectangle(Util.FromRect(0, 0, design.Width, design.Height), new SKSize(width, height));
                Show();
            }
        }

        /// <summary>
        /// 윈도우를 닫습니다. OnClosing에서 취소하지 않으면 숨겨집니다.
        /// </summary>
        public void Close()
        {
            var e = new GoCancelableEventArgs();
            OnClosing(e);
            if (!e.Cancel) Design?.HideWindow(this);
        }
        #endregion
    }
}
