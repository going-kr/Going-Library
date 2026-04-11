using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    /// <summary>
    /// 스텝 표시 컨트롤. 단계별 진행 상태를 시각적으로 표시합니다.
    /// </summary>
    public class GoStep : GoControl
    {
        #region Properties
        /// <summary>
        /// 이전 버튼의 아이콘 문자열을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public string? PrevIconString { get; set; } = "fa-chevron-left";
        /// <summary>
        /// 다음 버튼의 아이콘 문자열을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public string? NextIconString { get; set; } = "fa-chevron-right";

        /// <summary>
        /// 버튼 색상의 테마 색상 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public string ButtonColor { get; set; } = "Base3";
        /// <summary>
        /// 비선택 스텝의 색상 테마 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public string StepColor { get; set; } = "Base2";
        /// <summary>
        /// 선택된 스텝의 색상 테마 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public string SelectColor { get; set; } = "Select";
        /// <summary>
        /// 스텝 표시를 원형으로 할지 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public bool IsCircle { get; set; } = false;
        /// <summary>
        /// 이전/다음 버튼 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public bool UseButton { get; set; } = true;

        /// <summary>
        /// 전체 스텝 수를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public int StepCount { get; set; } = 7;
        /// <summary>
        /// 현재 선택된 스텝 인덱스를 가져오거나 설정합니다. 값이 변경되면 <see cref="StepChanged"/> 이벤트가 발생합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)]
        public int Step
        {
            get => nStep;
            set
            {
                var v = Convert.ToInt32(MathTool.Constrain(value, 0, StepCount - 1));
                if (nStep != v)
                {
                    nStep = v;
                    StepChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Member Variable
        private int nStep = 0;
        private bool bDownP = false, bDownN = false;
        private bool bHoverP = false, bHoverN = false;
        #endregion

        #region Event
        /// <summary>
        /// <see cref="Step"/> 값이 변경되었을 때 발생합니다.
        /// </summary>
        public event EventHandler? StepChanged;
        #endregion

        #region Constructor
        public GoStep()
        {
            Selectable = true;
        }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cBtn = thm.ToColor(ButtonColor);
            var cStep = thm.ToColor(StepColor);
            var cSel = thm.ToColor(SelectColor);
            var rts = Areas();
            var rtBox = rts["Box"];
            var rtPrev = rts["Prev"];
            var rtNext = rts["Next"];

            if (UseButton)
            {
                var isz = rtBox.Height / 2;
                var cP = cBtn.BrightnessTransmit(bDownP ? thm.DownBrightness : 0);
                var cN = cBtn.BrightnessTransmit(bDownN ? thm.DownBrightness : 0);
                if (bDownP) rtPrev.Offset(0, 1);
                if (bDownN) rtNext.Offset(0, 1);
                Util.DrawIcon(canvas, PrevIconString, isz, rtPrev, cP.BrightnessTransmit(bHoverP ? thm.HoverFillBrightness : 0), bHoverP ? cP.BrightnessTransmit(thm.HoverBorderBrightness) : SKColors.Transparent);
                Util.DrawIcon(canvas, NextIconString, isz, rtNext, cN.BrightnessTransmit(bHoverN ? thm.HoverFillBrightness : 0), bHoverN ? cN.BrightnessTransmit(thm.HoverBorderBrightness) : SKColors.Transparent);
            }

            using var p = new SKPaint { IsAntialias = true };
            stepLoop(rtBox, (i, rt) =>
            {
                var vrt = rt;

                var rnd = GoRoundType.All;
                if(IsCircle)
                {
                    var wh = Math.Min(rt.Width, rt.Height);
                    vrt = MathTool.MakeRectangle(rt, new SKSize(wh, wh));
                    rnd = GoRoundType.Ellipse;
                }

                if (i != Step)
                    Util.DrawBox(canvas, vrt, cStep, rnd, thm.Corner);
                else
                    Util.DrawBox(canvas, vrt, cSel, cSel.BrightnessTransmit(thm.HoverBorderBrightness), rnd, thm.Corner);

            });

            base.OnDraw(canvas, thm);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            if (UseButton)
            {
                var rtPrev = rts["Prev"];
                var rtNext = rts["Next"];
                if (CollisionTool.Check(rtPrev, x, y)) bDownP = true;
                if (CollisionTool.Check(rtNext, x, y)) bDownN = true;
            }
            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();

            if (UseButton)
            {
                var rtPrev = rts["Prev"];
                var rtNext = rts["Next"];
                bHoverP = CollisionTool.Check(rtPrev, x, y);
                bHoverN = CollisionTool.Check(rtNext, x, y);
            }

            base.OnMouseMove(x, y);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtPrev = rts["Prev"];
            var rtNext = rts["Next"];

            if (UseButton)
            {
                if (bDownP)
                {
                    bDownP = false;
                    if (CollisionTool.Check(rtPrev, x, y)) { Step = MathTool.Constrain(Step - 1, 0, StepCount - 1); }
                }

                if (bDownN)
                {
                    bDownN = false;
                    if (CollisionTool.Check(rtNext, x, y)) { Step = MathTool.Constrain(Step + 1, 0, StepCount - 1); }
                }
            }
            base.OnMouseUp(x, y, button);
        }
        #endregion

        #region Areas
        /// <inheritdoc/>
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];

            var bw = UseButton ? 40 : 0;
            var rts = Util.Columns(rtContent, [$"{bw}px", "100%", $"{bw}px"]);
            dic["Prev"] = rts[0];
            dic["Box"] = rts[1];
            dic["Next"] = rts[2];
            return dic;
        }
        #endregion
        #endregion

        #region Method
        void stepLoop(SKRect rtBox, Action<int, SKRect> loop)
        {
            var ix = rtBox.Left;
            var tk = (rtBox.Width - (10 * (StepCount - 1))) / StepCount;

            for (int i = 0; i < StepCount; i++)
            {
                var rt = Util.FromRect(ix, rtBox.Top, tk, rtBox.Height);
                loop(i, rt);
                ix += (tk + 10);
            }
        }
        #endregion
    }
}
