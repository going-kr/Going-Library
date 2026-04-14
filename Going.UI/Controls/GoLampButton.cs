using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    /// <summary>
    /// 램프(표시등)가 포함된 버튼 컨트롤. On/Off 상태를 램프 색상으로 표시합니다.
    /// </summary>
    public class GoLampButton : GoControl
    {
        #region Properties
        /// <summary>버튼에 표시할 텍스트</summary>
        [GoMultiLineProperty(PCategory.Control, 0)] public string Text { get; set; } = "button";
        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 1)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일</summary>
        [GoProperty(PCategory.Control, 2)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 3)] public float FontSize { get; set; } = 12;

        /// <summary>텍스트 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 4)] public string TextColor { get; set; } = "Fore";
        /// <summary>버튼 배경 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 5)] public string ButtonColor { get; set; } = "Base3";
        /// <summary>테두리 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 6)] public string BorderColor { get; set; } = "Base3";
        /// <summary>켜짐 상태 램프 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 7)] public string OnColor { get; set; } = "Good";
        /// <summary>꺼짐 상태 램프 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 8)] public string OffColor { get; set; } = "Base2";
        /// <summary>모서리 둥글기 유형</summary>
        [GoProperty(PCategory.Control, 9)] public GoRoundType Round { get; set; } = GoRoundType.All;
        /// <summary>테두리 두께</summary>
        [GoProperty(PCategory.Control, 10)] public float BorderWidth { get; set; } = 1F;
        /// <summary>버튼 채우기 스타일</summary>
        [GoProperty(PCategory.Control, 11)] public GoButtonFillStyle FillStyle { get; set; } = GoButtonFillStyle.Flat;

        private bool bOnOff = false;
        /// <summary>램프의 On/Off 상태. 값이 변경되면 <see cref="OnOffChanged"/> 이벤트가 발생합니다.</summary>
        [GoProperty(PCategory.Control, 12)]
        public bool OnOff
        {
            get => bOnOff; set
            {
                if (bOnOff != value)
                {
                    bOnOff = value;
                    OnOffChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>램프 크기 (픽셀)</summary>
        [GoProperty(PCategory.Control, 13)] public int LampSize { get; set; } = 24;
        /// <summary>램프와 텍스트 사이 간격</summary>
        [GoProperty(PCategory.Control, 14)] public int Gap { get; set; } = 10;

        /// <summary>자동 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 15)] public GoAutoFontSize AutoFontSize { get; set; } = GoAutoFontSize.NotUsed;

        #endregion

        #region Event
        /// <summary>버튼이 클릭되었을 때 발생하는 이벤트</summary>
        public event EventHandler? ButtonClicked;
        /// <summary>On/Off 상태가 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? OnOffChanged;
        #endregion

        #region Constructor
        /// <summary>GoLampButton 클래스의 새 인스턴스를 초기화합니다.</summary>
        public GoLampButton()
        {
            Selectable = true;
        }
        #endregion

        #region Member Variable
        private bool bDown = false;
        private bool bHover = false;
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var cBtn = thm.ToColor(ButtonColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var cBor = thm.ToColor(BorderColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var cOff = thm.ToColor(OffColor);
            var cOn = thm.ToColor(OnColor);
            var cLmp = OnOff ? cOn : cOff;
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtBox = rts["Box"];
            var rtText = rts["Text"];

            Util.DrawButton(canvas, thm, rtContent, cBtn.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0),
                                                    cBor.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0),
                                                    Round, thm.Corner, true, BorderWidth, FillStyle, bDown);

            if (bDown) { rtBox.Offset(0, 1); rtText.Offset(0, 1); }

#if true
            /*
            var rtb = rtBox;
            var rtl = rtBox; rtl.Inflate(-1.5F, -1.5F);
            using var p = new SKPaint { IsAntialias = true };
            using var sh = SKShader.CreateLinearGradient(new SKPoint(rtb.Left, rtb.Top), new SKPoint(rtb.Left, rtb.Bottom), [cOff.BrightnessTransmit(-0.5F), cBtn.BrightnessTransmit(0.3F)], SKShaderTileMode.Clamp);
            p.IsStroke = false;
            p.Shader = sh;
            canvas.DrawOval(rtb, p);
            Util.DrawLamp(canvas, rtl, cOn, cOff, OnOff, false);
            p.Shader = null;
            */

            var rtb = rtBox;
            var rtl = rtBox; rtl.Inflate(-1.5F, -1.5F);
            using var p = new SKPaint { IsAntialias = true };
            p.IsStroke = false;
            p.Color = cBtn.BrightnessTransmit(-0.5F);
            canvas.DrawOval(rtb, p);
            Util.DrawLamp(canvas, thm, rtl, cOn, cOff, OnOff, false);
#else
            var rtb = rtBox; rtb.Inflate(1.5F, 1.5F);
            var cLT = cLmp.BrightnessTransmit(0.1F);
            var cRB = cLmp.BrightnessTransmit(-0.1F);
            using var p = new SKPaint { IsAntialias = true };
            using var sh = SKShader.CreateLinearGradient(new SKPoint(rtb.Left, rtb.Top), new SKPoint(rtb.Right, rtb.Bottom), [cLT, cLT, cRB, cRB], [0, 0.5F, 0.5F, 1], SKShaderTileMode.Clamp);
            p.IsStroke = false;
            p.Color = cOff.BrightnessTransmit(-0.5F);
            p.Shader = sh;
            canvas.DrawOval(rtb, p);

            rtb.Inflate(-1.5F, -1.5F);
            var cLT2 = cLmp.BrightnessTransmit(-0.3F);
            var cRB2 = cLmp.BrightnessTransmit(0.3F);
            using var sh2 = SKShader.CreateLinearGradient(new SKPoint(rtb.Left, rtb.Top), new SKPoint(rtb.Right, rtb.Bottom), [cLT2, cRB2], SKShaderTileMode.Clamp);
            p.IsStroke = true;
            p.StrokeWidth = 3;
            p.Shader = sh2;
            canvas.DrawOval(rtb, p);


#endif

            var fsz = Util.FontSize(AutoFontSize, rtBox.Height) ?? FontSize;

            Util.DrawText(canvas, Text, FontName, FontStyle, fsz, rtText, cText, GoContentAlignment.MiddleCenter);


            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (CollisionTool.Check(rtBox, x, y)) bDown = true;

            base.OnMouseDown(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (bDown)
            {
                bDown = false;
                if (CollisionTool.Check(rtBox, x, y)) ButtonClicked?.Invoke(this, EventArgs.Empty);
            }

            base.OnMouseUp(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            bHover = CollisionTool.Check(rtBox, x, y);

            base.OnMouseMove(x, y);
        }

        /// <inheritdoc/>
        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = rts["Content"];
            var (rtBox, rtText) = Util.TextIconBounds(Text, FontName, FontStyle, FontSize, new SKSize(LampSize, LampSize), GoDirectionHV.Horizon, Gap, rtContent, GoContentAlignment.MiddleCenter);

            rts["Box"] = rtBox;
            rts["Text"] = rtText;

            return rts;
        }
        #endregion
    }
}
