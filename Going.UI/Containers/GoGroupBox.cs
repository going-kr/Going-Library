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
    public class GoGroupBox : GoContainer
    {
        #region Properties
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public float IconGap { get; set; } = 5;

        public string Text { get; set; } = "Panel";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string BorderColor { get; set; } = "Base3";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public List<GoButtonItem> Buttons { get; set; } = [];
        public float? ButtonWidth { get; set; }

        [JsonInclude]
        public override List<IGoControl> Childrens { get; } = [];
        #endregion

        #region Event
        public event EventHandler<ButtonClickEventArgs>? ButtonClicked;
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoGroupBox(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoGroupBox() { Selectable = true; }
        #endregion

        #region Override
        #region OnContainerDraw
        protected override void OnContainerDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cBorder = thm.ToColor(BorderColor);
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtTitle = rts["Title"];
            var rtButtons = rts["Buttons"];
            var rtBorder = rts["Border"];
            var sz = Util.MeasureTextIcon(Text, FontName, FontSize, IconString, IconSize, GoDirectionHV.Horizon, IconGap);
            var rtTitleText = Util.FromRect(rtTitle.Left + 10, rtTitle.Top, sz.Width + 20, rtTitle.Height);

            Util.DrawTextIcon(canvas, Text, FontName, FontSize, IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtTitleText, cText, GoContentAlignment.MiddleCenter);
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipRect(rtTitleText, SKClipOperation.Difference);
                canvas.ClipRect(rtButtons, SKClipOperation.Difference);
                Util.DrawBox(canvas, rtBorder, SKColors.Transparent, cBorder, Round, thm.Corner);
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

            base.OnContainerDraw(canvas);
        }
        #endregion
        #region OnLayout
        protected override void OnLayout()
        {
            var rts = Areas();
            var rtPanel = rts["Panel"];
            foreach (var c in Childrens)
            {
                if (c.Fill)
                {
                    c.Bounds = Util.FromRect(rtPanel, c.Margin);
                }
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
