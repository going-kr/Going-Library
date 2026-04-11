using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    /// <summary>
    /// 이미지 캔버스에서 사용되는 슬라이더 컨트롤입니다. 커서 이미지를 드래그하여 값을 조절할 수 있습니다.
    /// </summary>
    public class IcSlider : GoControl
    {
        #region Properties
        /// <summary>
        /// 글꼴 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 0)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 글꼴 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 12;
        /// <summary>
        /// 텍스트 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public string TextColor { get; set; } = "Fore";

        /// <summary>
        /// 비활성(배경) 상태 바 이미지 리소스 이름을 가져오거나 설정합니다. null이면 부모의 Off 이미지를 사용합니다.
        /// </summary>
        [GoImageProperty(PCategory.Control, 4)] public string? OffBarImage { get; set; }
        /// <summary>
        /// 활성(채움) 상태 바 이미지 리소스 이름을 가져오거나 설정합니다. null이면 부모의 On 이미지를 사용합니다.
        /// </summary>
        [GoImageProperty(PCategory.Control, 5)] public string? OnBarImage { get; set; }
        /// <summary>
        /// 비활성 상태의 커서 이미지 리소스 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoImageProperty(PCategory.Control, 6)] public string? OffCursorImage { get; set; }
        /// <summary>
        /// 활성(누름) 상태의 커서 이미지 리소스 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoImageProperty(PCategory.Control, 7)] public string? OnCursorImage { get; set; }

        /// <summary>
        /// 값 표시 서식 문자열을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public string FormatString { get; set; } = "0";
        /// <summary>
        /// 최솟값을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public double Minimum { get; set; } = 0D;
        /// <summary>
        /// 최댓값을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 10)] public double Maximum { get; set; } = 100D;
        /// <summary>
        /// 현재 값을 가져오거나 설정합니다. 값이 변경되면 <see cref="ValueChanged"/> 이벤트가 발생합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)]
        public double Value
        {
            get => nValue;
            set
            {
                if (nValue != value)
                {
                    nValue = MathTool.Constrain(value, Minimum, Maximum);
                    if (Tick.HasValue) nValue = Math.Round(nValue / Tick.Value) * Tick.Value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 눈금 간격을 가져오거나 설정합니다. 설정 시 값이 해당 간격으로 스냅됩니다.
        /// </summary>
        [GoProperty(PCategory.Control, 12)] public double? Tick { get; set; }

        /// <summary>
        /// 현재 값 텍스트를 표시할지 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 13)] public bool DrawText { get; set; } = true;
        #endregion

        #region Member Variable
        private double nValue = 0;
        private bool bCurDown = false;
        #endregion

        #region Event
        /// <summary>
        /// <see cref="Value"/> 값이 변경되었을 때 발생하는 이벤트입니다.
        /// </summary>
        public event EventHandler? ValueChanged;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtBox = rts["Content"];
            var cText = thm.ToColor(TextColor);

            if (Design != null && Parent != null)
            {
                if (OffBarImage == null && OnBarImage == null)
                {
                    #region 부모 이미지 공유 모드
                    SKImage? offB = null, onB = null;
                    if (Parent is IcContainer con) { offB = Design.GetImage(con.OffImage)?.FirstOrDefault(); onB = Design.GetImage(con.OnImage)?.FirstOrDefault(); }
                    else if (Parent is IcPage page) { offB = Design.GetImage(page.OffImage)?.FirstOrDefault(); onB = Design.GetImage(page.OnImage)?.FirstOrDefault(); }

                    if (offB != null && onB != null)
                    {
                        var sx = (double)onB.Width / Parent.Bounds.Width;
                        var sy = (double)onB.Height / Parent.Bounds.Height;

                        var vbBS = Util.FromRect(Convert.ToInt32(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy));
                        var vbBD = Util.FromRect(0, 0, Width, Height);

                        var w = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, Width));
                        var vbFS = Util.FromRect(Convert.ToInt32(Left * sx), Convert.ToInt32(Top * sy), Convert.ToSingle(w * sx), Convert.ToSingle(Height * sy));
                        var vbFD = Util.FromRect(0, 0, w, Height);

                        canvas.DrawImage(offB, vbBS, vbBD, Util.Sampling);
                        if (Value > Minimum) canvas.DrawImage(onB, vbFS, vbFD, Util.Sampling);

                        var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                        var onC = Design.GetImage(OnCursorImage).FirstOrDefault();

                        if (offC != null && onC != null)
                        {
                            var rtCur = cursorRect(offC);
                            canvas.DrawImage(bCurDown ? onC : offC, rtCur, Util.Sampling);
                            if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, rtCur, cText);
                        }
                        else
                        {
                            if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, vbBD, cText);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 독립 이미지 모드
                    var offB = Design.GetImage(OffBarImage).FirstOrDefault();
                    var onB = Design.GetImage(OnBarImage).FirstOrDefault();
                    var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                    var onC = Design.GetImage(OnCursorImage).FirstOrDefault();

                    if (offB != null && onB != null && offC != null && onC != null)
                    {
                        bounds(offB, offC, (_, rtBar, rtCur, rtFill) =>
                        {
                            canvas.DrawImage(offB, rtBar, Util.Sampling);
                            if (Value > Minimum) canvas.DrawImage(onB, Util.FromRect(0, 0, rtFill.Width, rtFill.Height), rtFill, Util.Sampling);
                            canvas.DrawImage(bCurDown ? onC : offC, rtCur, Util.Sampling);
                            if (DrawText) Util.DrawText(canvas, Value.ToString(FormatString ?? "0"), FontName, FontStyle, FontSize, rtCur, cText);
                        });
                    }
                    #endregion
                }
            }

            base.OnDraw(canvas, thm);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            if (Design != null && Parent != null)
            {
                if (OffBarImage == null && OnBarImage == null)
                {
                    var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                    var onC = Design.GetImage(OnCursorImage).FirstOrDefault();
                    if (offC != null && onC != null)
                    {
                        if (CollisionTool.Check(cursorRect(offC), x, y)) bCurDown = true;
                    }
                }
                else
                {
                    var offB = Design.GetImage(OffBarImage).FirstOrDefault();
                    var onB = Design.GetImage(OnBarImage).FirstOrDefault();
                    var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                    var onC = Design.GetImage(OnCursorImage).FirstOrDefault();

                    if (offB != null && onB != null && offC != null && onC != null)
                    {
                        bounds(offB, offC, (rtBox, rtBar, rtCur, rtFill) =>
                        {
                            if (CollisionTool.Check(rtCur, x, y)) bCurDown = true;
                        });
                    }
                }
            }
            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            if (bCurDown)
            {
                if (Design != null && Parent != null)
                {
                    if (OffBarImage == null && OnBarImage == null)
                    {
                        var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                        var onC = Design.GetImage(OnCursorImage).FirstOrDefault();
                        if (offC != null && onC != null)
                        {
                            Value = MathTool.Map(MathTool.Constrain(x, 0, Width), 0, Width, Minimum, Maximum);
                        }
                    }
                    else
                    {
                        var offB = Design.GetImage(OffBarImage).FirstOrDefault();
                        var onB = Design.GetImage(OnBarImage).FirstOrDefault();
                        var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                        var onC = Design.GetImage(OnCursorImage).FirstOrDefault();

                        if (offB != null && onB != null && offC != null && onC != null)
                        {
                            bounds(offB, offC, (rtBox, rtBar, rtCur, rtFill) =>
                            {
                                Value = MathTool.Map(MathTool.Constrain(x, rtBar.Left, rtBar.Right), rtBar.Left, rtBar.Right, Minimum, Maximum);
                            });
                        }
                    }
                }

                bCurDown = false;
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            if (bCurDown)
            {
                if (Design != null && Parent != null)
                {
                    if (OffBarImage == null && OnBarImage == null)
                    {
                        var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                        var onC = Design.GetImage(OnCursorImage).FirstOrDefault();
                        if (offC != null && onC != null)
                        {
                            Value = MathTool.Map(MathTool.Constrain(x, 0, Width), 0, Width, Minimum, Maximum);
                        }
                    }
                    else
                    {
                        var offB = Design.GetImage(OffBarImage).FirstOrDefault();
                        var onB = Design.GetImage(OnBarImage).FirstOrDefault();
                        var offC = Design.GetImage(OffCursorImage).FirstOrDefault();
                        var onC = Design.GetImage(OnCursorImage).FirstOrDefault();

                        if (offB != null && onB != null && offC != null && onC != null)
                        {
                            bounds(offB, offC, (rtBox, rtBar, rtCur, rtFill) =>
                            {
                                Value = MathTool.Map(MathTool.Constrain(x, rtBar.Left, rtBar.Right), rtBar.Left, rtBar.Right, Minimum, Maximum);
                            });
                        }
                    }
                }
            }
            base.OnMouseMove(x, y);
        }

        #endregion

        #region Method
        #region bounds
        void bounds(SKImage? bmBar, SKImage bmCur, Action<SKRect, SKRect, SKRect, SKRect> act)
        {
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var rtBar = bmBar != null ? MathTool.MakeRectangle(rtBox, new SKSize(bmBar.Width, bmBar.Height)) : rtBox;

            var w = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, rtBar.Width));
            var rtFill = Util.FromRect(rtBar.Left, rtBar.Top, w, rtBar.Height);

            var x = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, rtBar.Left, rtBar.Right));
            var y = rtBar.MidY;
            var rtCur = MathTool.MakeRectangle(new SKPoint(x, y), bmCur.Width / 2F, bmCur.Height / 2F);
            act(rtBox, rtBar, rtCur, rtFill);
        }
        #endregion

        #region cursorRect
        SKRect cursorRect(SKImage bmCur)
        {
            var cx = Convert.ToSingle(MathTool.Map(Value, Minimum, Maximum, 0, Width));
            var cy = Height / 2F;
            return MathTool.MakeRectangle(new SKPoint(cx, cy), bmCur.Width / 2F, bmCur.Height / 2F);
        }
        #endregion
        #endregion
    }
}
