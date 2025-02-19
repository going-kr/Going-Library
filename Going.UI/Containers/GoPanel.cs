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
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public class GoPanel : GoContainer
    {
        #region Properties
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public GoDirectionHV IconDirection { get; set; }
        public float IconGap { get; set; } = 5;

        public string Text { get; set; } = "Panel";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string PanelColor { get; set; } = "Base2";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public bool BackgroundDraw { get; set; } = true;
        public bool BorderOnly { get; set; } = false;
 
        public float TitleHeight { get; set; } = 40;

        public List<GoButtonInfo> Buttons { get; set; } = [];
        public float? ButtonWidth { get; set; }
        #endregion

        #region Event
        public event EventHandler<ButtonClickEventArgs>? ButtonClicked;
        #endregion

        #region Constructor
        public GoPanel ()
        {
            Selectable = true;
        }
        #endregion

        #region Override
        #region OnContainerDraw
        protected override void OnContainerDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cPanel = thm.ToColor(PanelColor);
            var cBorder = cPanel.BrightnessTransmit(thm.BorderBrightness); 
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtTitle = rts["Title"];
            var rtButtons = rts["Buttons"];
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
            Util.DrawTextIcon(canvas, Text, FontName, FontSize, IconString, IconSize, IconDirection, IconGap, rtTitleText, cText, GoContentAlignment.MiddleLeft);

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

            base.OnLayout();
        }
        #endregion
        #region OnMouseDown
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
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
            base.OnMouseDown(x, y, button);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
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
            base.OnMouseUp(x, y, button);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(float x, float y)
        {
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
            base.OnMouseMove(x, y);
        }
        #endregion
        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();

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
