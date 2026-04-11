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
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    /// <summary>
    /// 제목 텍스트와 테두리로 자식 컨트롤을 그룹화하는 그룹 박스 컨테이너입니다.
    /// </summary>
    public class GoGroupBox : GoContainer
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
        /// 테두리 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public string BorderColor { get; set; } = "Base3";
        /// <summary>
        /// 모서리 라운드 타입을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public GoRoundType Round { get; set; } = GoRoundType.All;

        /// <summary>
        /// 제목 영역에 표시할 버튼 항목 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 10)] public List<GoButtonItem> Buttons { get; set; } = [];
        /// <summary>
        /// 버튼 영역의 전체 너비를 가져오거나 설정합니다. null이면 버튼이 표시되지 않습니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public float? ButtonWidth { get; set; }
        /// <summary>
        /// 테두리 두께를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 12)] public float BorderWidth { get; set; } = 1;

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
        /// 자식 컨트롤 목록을 사용하여 <see cref="GoGroupBox"/>의 새 인스턴스를 초기화합니다. JSON 역직렬화에 사용됩니다.
        /// </summary>
        /// <param name="childrens">자식 컨트롤 목록</param>
        [JsonConstructor]
        public GoGroupBox(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary>
        /// <see cref="GoGroupBox"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoGroupBox() { Selectable = false; }
        #endregion

        #region Override
        #region OnDraw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var cBorder = thm.ToColor(BorderColor);
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtTitle = rts["Title"];
            var rtButtons = rts["Buttons"];
            var rtBorder = rts["Border"];
            var sz = Util.MeasureTextIcon(Text, FontName, FontStyle, FontSize, IconString, IconSize, GoDirectionHV.Horizon, IconGap);
            var rtTitleText = Util.FromRect(rtTitle.Left + 10, rtTitle.Top, sz.Width + 20, rtTitle.Height);

            Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtTitleText, cText, GoContentAlignment.MiddleCenter);
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipRect(rtTitleText, SKClipOperation.Difference);
                canvas.ClipRect(rtButtons, SKClipOperation.Difference);
                //Util.DrawBox(canvas, rtBorder, SKColors.Transparent, cBorder, Round, thm.Corner);

                rtBorder.Right -= 1;
                using var p = new SKPaint { IsAntialias = true };
                p.IsStroke = true;
                p.Color = cBorder;
                p.StrokeWidth = BorderWidth;
                rtBorder.Inflate(-BorderWidth / 2, -BorderWidth / 2);
                var rtr = new SKRoundRect(rtBorder, thm.Corner);
                Util.SetRound(rtr, Round, thm.Corner);
                canvas.DrawRoundRect(rtr, p);
            }

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

            var th = FontSize + 10;
            var rts = Util.Rows(dic["Content"], [$"{th}px", "100%"]);
            dic["Title"] = rts[0];
            dic["Panel"] = rts[1];
            dic["Border"] = Util.FromRect(rts[1].Left, rts[1].Top - th / 2, rts[1].Width, rts[1].Height + th / 2);
            dic["Buttons"] = new SKRect(rts[0].Right - 10 - (ButtonWidth ?? 0), rts[0].Top, rts[0].Right - 10, rts[0].Bottom);

            return dic;
        }
        #endregion
        #endregion
    }
}
