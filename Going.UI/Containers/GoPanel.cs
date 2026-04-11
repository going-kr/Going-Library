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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    /// <summary>
    /// 제목 영역과 콘텐츠 영역으로 구성된 기본 패널 컨테이너입니다. 제목에 텍스트, 아이콘, 버튼을 표시할 수 있습니다.
    /// </summary>
    public class GoPanel : GoContainer
    {
        #region Properties
        /// <summary>
        /// 제목 영역에 표시할 아이콘 문자열을 가져오거나 설정합니다.
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
        /// 제목 텍스트를 가져오거나 설정합니다.
        /// </summary>
        [GoMultiLineProperty(PCategory.Control, 3)] public string Text { get; set; } = "Panel";
        /// <summary>
        /// 글꼴 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 4)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 글꼴 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public float FontSize { get; set; } = 12;

        /// <summary>
        /// 텍스트 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public string TextColor { get; set; } = "Fore";
        /// <summary>
        /// 패널 배경 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public string PanelColor { get; set; } = "Base2";
        /// <summary>
        /// 모서리 라운드 타입을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public GoRoundType Round { get; set; } = GoRoundType.All;

        /// <summary>
        /// 배경을 그릴지 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 10)] public bool BackgroundDraw { get; set; } = true;
        /// <summary>
        /// 테두리만 그릴지 여부를 가져오거나 설정합니다. true이면 배경을 투명하게 하고 테두리만 표시합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public bool BorderOnly { get; set; } = false;

        /// <summary>
        /// 제목 영역의 높이를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 12)] public float TitleHeight { get; set; } = 40;

        /// <summary>
        /// 제목 영역에 표시할 버튼 항목 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 13)] public List<GoButtonItem> Buttons { get; set; } = [];
        /// <summary>
        /// 버튼 영역의 전체 너비를 가져오거나 설정합니다. null이면 버튼이 표시되지 않습니다.
        /// </summary>
        [GoProperty(PCategory.Control, 14)] public float? ButtonWidth { get; set; }

        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
        /// <summary>
        /// 자식 컨트롤이 배치되는 패널 영역을 가져옵니다.
        /// </summary>
        [JsonIgnore] public override SKRect PanelBounds => Areas()["Panel"];
        #endregion

        #region Event
        /// <summary>
        /// 제목 영역의 버튼이 클릭되었을 때 발생하는 이벤트입니다.
        /// </summary>
        public event EventHandler<ButtonClickEventArgs>? ButtonClicked;
        #endregion

        #region Constructor
        /// <summary>
        /// 자식 컨트롤 목록을 사용하여 <see cref="GoPanel"/>의 새 인스턴스를 초기화합니다. JSON 역직렬화에 사용됩니다.
        /// </summary>
        /// <param name="childrens">자식 컨트롤 목록</param>
        [JsonConstructor]
        public GoPanel(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary>
        /// <see cref="GoPanel"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoPanel() { Selectable = false; }
        #endregion

        #region Override
        #region OnDraw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var cPanel = thm.ToColor(PanelColor);
            var cBorder = cPanel.BrightnessTransmit(thm.BorderBrightness); 
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtTitle = rts["Title"];
            var rtButtons = rts["Buttons"];
            var rtPanel = rts["Panel"];
            var rtTitleText = Util.FromRect(rtTitle, new GoPadding(10, 0, 0, 0));

            if (BackgroundDraw)
            {
                Util.DrawBox(canvas, rtBox, BorderOnly ? SKColors.Transparent : cPanel, cPanel, Round, thm.Corner);

                using var p = new SKPaint { IsAntialias = false };
                using var pe = SKPathEffect.CreateDash([3, 3], 2);
                p.PathEffect = pe;
                p.Color = cBorder;
                canvas.DrawLine(rtTitle.Left + 10, rtTitle.Bottom, rtTitle.Right - 10, rtTitle.Bottom, p);
            }
            Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtTitleText, cText, GoContentAlignment.MiddleLeft);

            if (Buttons.Count > 0 && ButtonWidth.HasValue)
            {
                var rtsBtn = Util.Columns(rtButtons, Buttons.Select(x => x.Size).ToArray());
                for (int i = 0; i < Buttons.Count; i++)
                {
                    var btn = Buttons[i];
                    var rt = rtsBtn[i];
                    var cBtn = cText;
                    if (!string.IsNullOrWhiteSpace(btn.Name))
                    {
                        if (btn.Hover)
                        {
                            var wh = Math.Min(IconSize * 2, Math.Min(rt.Width, rt.Height));
                            Util.DrawBox(canvas, MathTool.MakeRectangle(rt, new SKSize(wh, wh)), SKColors.Transparent, Util.FromArgb(Convert.ToByte(thm.Alpha), cBtn), GoRoundType.All, thm.Corner);
                        }
                        if (btn.Down) { cBtn = cText.BrightnessTransmit(btn.Down ? thm.DownBrightness : 0); rt.Offset(0, 1); }
                        Util.DrawIcon(canvas, btn.IconString, IconSize, rt, cBtn);
                    }
                }
            }

            base.OnDraw(canvas, thm);
        }
        #endregion
        #region OnLayout
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
        #region OnMouseDown
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            base.OnMouseDown(x, y, button);

            if (Buttons.Count > 0 && ButtonWidth.HasValue)
            {
                var rtsBtn = Util.Columns(Areas()["Buttons"], Buttons.Select(x => x.Size).ToArray());
                for (int i = 0; i < Buttons.Count; i++)
                {
                    var btn = Buttons[i];
                    var rt = rtsBtn[i];
                    
                    if (!string.IsNullOrWhiteSpace(btn.Name))
                    {
                        if (CollisionTool.Check(rt, x, y)) btn.Down = true;
                    }
                }
            }
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            base.OnMouseUp(x, y, button);

            if (Buttons.Count > 0 && ButtonWidth.HasValue)
            {
                var rtsBtn = Util.Columns(Areas()["Buttons"], Buttons.Select(x => x.Size).ToArray());
                for (int i = 0; i < Buttons.Count; i++)
                {
                    var btn = Buttons[i];
                    var rt = rtsBtn[i];

                    if (!string.IsNullOrWhiteSpace(btn.Name) && btn.Down)
                    {
                        if (CollisionTool.Check(rt, x, y)) ButtonClicked?.Invoke(this, new ButtonClickEventArgs(btn));
                    }

                    btn.Down = false;
                }
            }
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);

            if (Buttons.Count > 0 && ButtonWidth.HasValue)
            {
                var rtsBtn = Util.Columns(Areas()["Buttons"], Buttons.Select(x => x.Size).ToArray());
                for (int i = 0; i < Buttons.Count; i++)
                {
                    var btn = Buttons[i];
                    var rt = rtsBtn[i];
                    
                    if (!string.IsNullOrWhiteSpace(btn.Name))
                    {
                        btn.Hover = CollisionTool.Check(rt, x, y);
                    }
                }
            }
        }
        #endregion
        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            // Grid, Rows(세로), Columns(가로)
            var rts =  Util.Rows(dic["Content"], [$"{TitleHeight}px", "100%"]);
            dic["Title"] = rts[0];
            dic["Panel"] = rts[1];
            dic["Buttons"] = new SKRect(rts[0].Right - 10 - (ButtonWidth ?? 0), rts[0].Top, rts[0].Right - 10, rts[0].Bottom);

            return dic;
        }
        #endregion
        #endregion
    }

}
