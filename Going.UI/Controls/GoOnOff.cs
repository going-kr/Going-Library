using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.UI.Controls
{
    /// <summary>
    /// On/Off 슬라이딩 컨트롤. 커서를 드래그하거나 클릭하여 On/Off 상태를 전환합니다.
    /// </summary>
    public class GoOnOff : GoControl
    {
        #region Properties
        /// <summary>텍스트 표시 여부</summary>
        [GoProperty(PCategory.Control, 0)] public bool DrawText { get; set; } = true;
        /// <summary>On 상태 텍스트</summary>
        [GoProperty(PCategory.Control, 1)] public string OnText { get; set; } = "On";
        /// <summary>Off 상태 텍스트</summary>
        [GoProperty(PCategory.Control, 2)] public string OffText { get; set; } = "Off";
        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 3)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일</summary>
        [GoProperty(PCategory.Control, 4)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 5)] public float FontSize { get; set; } = 12;

        /// <summary>텍스트 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 6)] public string TextColor { get; set; } = "Fore";
        /// <summary>배경 박스 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 7)] public string BoxColor { get; set; } = "Base1";
        /// <summary>테두리 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 8)] public string BorderColor { get; set; } = "Base3";
        /// <summary>슬라이딩 커서 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 9)] public string CursorColor { get; set; } = "Base3";
        /// <summary>On 상태 색상 (테마 색상 키 또는 색상명)</summary>
        [GoProperty(PCategory.Control, 10)] public string OnColor { get; set; } = "lime";
        /// <summary>Off 상태 색상 (테마 색상 키 또는 색상명)</summary>
        [GoProperty(PCategory.Control, 11)] public string OffColor { get; set; } = "gray";
        /// <summary>커서에 아이콘 표시 여부</summary>
        [GoProperty(PCategory.Control, 12)] public bool CursorIconDraw { get; set; } = true;
        /// <summary>커서 아이콘 문자열</summary>
        [GoProperty(PCategory.Control, 13)] public string CursorIconString { get; set; } = "fa-power-off";
        /// <summary>커서 아이콘 크기. null이면 커서 높이의 절반을 사용합니다.</summary>
        [GoProperty(PCategory.Control, 14)] public float? CursorIconSize { get; set; }
        /// <summary>모서리 둥글기. null이면 컨트롤 높이를 사용합니다.</summary>
        [GoProperty(PCategory.Control, 15)] public float? Corner { get; set; }

        private bool bOnOff = false;
        /// <summary>On/Off 상태. 값이 변경되면 애니메이션과 함께 <see cref="OnOffChanged"/> 이벤트가 발생합니다.</summary>
        [GoProperty(PCategory.Control, 16)]
        public bool OnOff
        {
            get => bOnOff;
            set
            {
                if (bOnOff != value)
                {
                    ani.Stop();
                    ani.Start(Animation.Time200, value ? "ON" : "OFF", () =>
                    {
                        bOnOff = value;
                        OnOffChanged?.Invoke(this, EventArgs.Empty);
                    });
                }
            }
        }

        /// <summary>자동 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 17)] public GoAutoFontSize AutoFontSize { get; set; } = GoAutoFontSize.NotUsed;
        /// <summary>자동 커서 아이콘 크기 설정</summary>
        [GoProperty(PCategory.Control, 17)] public GoAutoFontSize AutoCursorIconSize { get; set; } = GoAutoFontSize.NotUsed;
        #endregion

        #region Member Variable
        private bool bHover = false;
        private bool bDown = false;

        private Animation ani = new Animation();
        private bool downOnOff = false;
        private SKPoint? ptDown = null;
        private SKPoint? ptMove = null;
        private SKPath path = new SKPath();
        #endregion

        #region Event
        /// <summary>On/Off 상태가 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler? OnOffChanged;
        #endregion

        #region Constructor
        /// <summary>GoOnOff 클래스의 새 인스턴스를 초기화합니다.</summary>
        public GoOnOff()
        {
            Selectable = true;
            ani.Refresh = () => Invalidate();
        }
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var cBox = thm.ToColor(BoxColor);
            var cBorder = thm.ToColor(BorderColor);
            var cCursor = thm.ToColor(CursorColor);
            var cOn = thm.ToColor(OnColor);
            var cOff = thm.ToColor(OffColor);
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtCursor = rts["Cursor"];
            var rtOn = rts["On"];
            var rtOff = rts["Off"];
            var corner = Corner ?? rtContent.Height;

            Util.DrawBox(canvas, rtContent, cBox, SKColors.Transparent, GoRoundType.All, corner);

            var fsz = Util.FontSize(AutoFontSize, rtContent.Height) ?? FontSize;
            var isz = Util.FontSize(AutoCursorIconSize, rtContent.Height) ?? CursorIconSize;

            var crt = rtContent; crt.Inflate(-2, -2);
            PathTool.Box(path, crt, GoRoundType.All, corner);
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipPath(path);
                Util.DrawBox(canvas, rtCursor, cCursor.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0),
                                               cCursor.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), GoRoundType.All, corner);

                if (DrawText)
                {
                    Util.DrawText(canvas, OnText, FontName, FontStyle, fsz, rtOn, cText);
                    Util.DrawText(canvas, OffText, FontName, FontStyle, fsz, rtOff, cText);
                }

                if (CursorIconDraw)
                    Util.DrawIcon(canvas, CursorIconString, isz ?? rtCursor.Height / 2, rtCursor, OnOff ? cOn : cOff);
            }

            Util.DrawBox(canvas, rtContent, SKColors.Transparent, cBorder, GoRoundType.All, corner);
            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        protected override void OnDispose()
        {
            path.Dispose();
            base.OnDispose();
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtCursor = rts["Cursor"];
            var rtOn = rts["On"];
            var rtOff = rts["Off"];

            if (CollisionTool.Check(rtCursor, x, y))
            {
                downOnOff = OnOff;
                ptDown = ptMove = new SKPoint(x, y);
            }
            else if (CollisionTool.Check(rtOn, x, y)) OnOff = true;
            else if (CollisionTool.Check(rtOff, x, y)) OnOff = false;

            base.OnMouseDown(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            if (ptDown.HasValue)
            {
                ptDown = ptMove = null;
                OnOff = x > rtContent.MidX;
            }

            base.OnMouseUp(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtCursor = rts["Cursor"];

            bHover = CollisionTool.Check(rtCursor, x, y);
            if (ptDown.HasValue)
            {
                ptMove = new SKPoint(x, y);
                bOnOff = x > rtContent.MidX;
            }

            base.OnMouseMove(x, y);
        }

        #region Areas
        /// <inheritdoc/>
        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = rts["Content"];
            var CursorPadding = 5;
            var rtArea = Util.FromRect(rtContent); rtArea.Inflate(-CursorPadding, -CursorPadding);

            var cw = rtArea.Width * 0.5F;
            var ow = rtArea.Width * 0.5F;

            var sx = 0F;
            if (ptDown.HasValue && ptMove.HasValue)
            {
                var sv = downOnOff ? -ow : 0;
                var ev = downOnOff ? 0 : ow;
                sx = (downOnOff ? rtArea.Left : rtArea.Left - ow) + MathTool.Constrain(ptMove.Value.X - ptDown.Value.X, sv, ev);
            }
            else
            {
                if (Animation.UseAnimation && ani.IsPlaying)
                {
                    var sv = ani.Variable == "ON" ? -ow : 0;
                    var ev = ani.Variable == "ON" ? 0 : -ow;

                    sx = rtArea.Left + ani.Value(AnimationAccel.Linear, sv, ev);
                }
                else sx = OnOff ? rtArea.Left : rtArea.Left - ow;
            }

            var rtOff = Util.FromRect(sx, rtArea.Top, ow, rtArea.Height);
            var rtCursor = Util.FromRect(rtOff.Right, rtArea.Top, cw, rtArea.Height);
            var rtOn = Util.FromRect(rtCursor.Right, rtArea.Top, ow, rtArea.Height);

            rts["On"] = rtOn;
            rts["Off"] = rtOff;
            rts["Cursor"] = rtCursor;

            return rts;
        }
        #endregion
        #endregion
    }
}
