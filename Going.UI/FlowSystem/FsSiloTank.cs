using Going.UI.Controls;
using Going.UI.Design;
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
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.FlowSystem
{
    /// <summary>
    /// 하단이 깔때기(호퍼) 형태인 사일로 탱크 컴포넌트입니다.
    /// <see cref="FsCylinderTank"/>와 유사하게 액체 수위 표시, 주입 애니메이션, 교반기 동작을 지원하지만,
    /// 사일로 특유의 하단 깔때기 형태로 렌더링되며 출구 동작 방식이 다릅니다.
    ///
    /// <para><b>FsCylinderTank와의 주요 차이점</b></para>
    /// <list type="bullet">
    ///   <item>탱크 하단이 깔때기(호퍼) 형태로, 분체나 액체가 중력에 의해 자연 배출되는 형상을 표현합니다.</item>
    ///   <item>출구(Outlet)가 항상 하단 중앙에 고정되어 있으며, <see cref="FsCylinderTank.OutletPosition"/>처럼 위치를 변경할 수 없습니다.</item>
    ///   <item>Outlet 포트는 항상 활성화되어 있습니다 (UseOutlet 프로퍼티 없음).</item>
    ///   <item><see cref="IsFlow"/>가 <c>Value &gt; 0</c>만으로 결정됩니다. <see cref="FsCylinderTank"/>는 <c>Value &gt; 0 &amp;&amp; UseOutlet</c>이 조건인 것과 다릅니다.</item>
    /// </list>
    ///
    /// <para><b>주요 사용 시나리오</b></para>
    /// <list type="bullet">
    ///   <item>분체, 곡물, 시멘트 등 중력 배출이 필요한 저장 탱크를 표현할 때 사용합니다.</item>
    ///   <item>하단 깔때기 형태를 통해 자연 배출 구조를 시각적으로 표현합니다.</item>
    ///   <item>상류 펌프로부터 Inlet을 통해 유체를 공급받고, 하단 중앙 출구를 통해 하류로 배출합니다.</item>
    /// </list>
    ///
    /// <para><b>프로퍼티 설정 가이드</b></para>
    /// <list type="bullet">
    ///   <item><see cref="Value"/> - 현재 수위 값. <see cref="Minimum"/>~<see cref="Maximum"/> 범위 내에서 설정합니다.</item>
    ///   <item><see cref="Minimum"/>/<see cref="Maximum"/> - 수위 범위. 기본값은 0~100입니다.</item>
    ///   <item><see cref="Format"/> - 수위 값의 표시 형식 문자열. 기본값은 "0"입니다.</item>
    ///   <item><see cref="UseInlet"/> - <c>true</c>이면 상단 입구(Inlet) 포트를 활성화합니다.</item>
    ///   <item><see cref="InletPosition"/> - Inlet 포트의 수평 위치를 TopLeft/TopCenter/TopRight 중 선택합니다.</item>
    ///   <item><see cref="UseMixer"/> - <c>true</c>이면 탱크 내부에 교반기를 표시합니다.</item>
    ///   <item><see cref="MixerOnOff"/> - <c>true</c>이면 교반기가 회전 애니메이션과 함께 가동됩니다.</item>
    ///   <item><see cref="FillColor"/> - 탱크 내 액체의 색상. 하류 배관의 유체 색상으로 전파됩니다.</item>
    /// </list>
    ///
    /// <para><b>포트 구성</b></para>
    /// <list type="table">
    ///   <listheader><term>포트 이름</term><description>타입 및 설명</description></listheader>
    ///   <item><term>Inlet (입구)</term><description><see cref="PortType.Input"/> - 상단에 위치. <see cref="UseInlet"/>이 <c>true</c>일 때만 활성. 방향은 위쪽(T)입니다.</description></item>
    ///   <item><term>Outlet (출구)</term><description><see cref="PortType.Output"/> - 하단 중앙에 고정. 항상 활성. 방향은 아래쪽(B)입니다.</description></item>
    /// </list>
    ///
    /// <para><b>흐름 발생 조건</b></para>
    /// <para><see cref="IsFlow"/>는 <c>Value &gt; 0</c>일 때 <c>true</c>입니다.
    /// <see cref="FsCylinderTank"/>와 달리 UseOutlet 조건 없이, 수위가 있으면 항상 하류로 흐름이 전파됩니다.</para>
    /// </summary>
    /// <remarks>
    /// 사용 예시:
    /// <code>
    /// // 사일로 탱크 생성
    /// var silo = new FsSiloTank
    /// {
    ///     Left = 100, Top = 50, Width = 120, Height = 250,
    ///     Minimum = 0, Maximum = 100,
    ///     FillColor = "Orange",
    ///     UseInlet = true,
    ///     InletPosition = TopPortPosition.TopCenter
    /// };
    /// panel.Controls.Add(silo);
    ///
    /// // 수위 설정 (70%)
    /// silo.Value = 70;
    ///
    /// // 교반기 활성화 및 가동
    /// silo.UseMixer = true;
    /// silo.MixerOnOff = true;
    /// </code>
    /// </remarks>
    public class FsSiloTank : FsFlowObject
    {
        #region Properties
        /// <summary>
        /// 텍스트에 사용할 폰트 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 0)] public string FontName { get; set; } = "나눔고딕";

        /// <summary>
        /// 텍스트의 폰트 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;

        /// <summary>
        /// 텍스트의 폰트 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 18;

        /// <summary>
        /// 텍스트 색상을 가져오거나 설정합니다. 테마 색상 키를 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public string TextColor { get; set; } = "Fore";

        /// <summary>
        /// 액체 채움 색상을 가져오거나 설정합니다. 테마 색상 키를 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public string FillColor { get; set; } = "DeepSkyBlue";

        /// <summary>
        /// 빈 영역 색상을 가져오거나 설정합니다. 테마 색상 키를 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public string EmptyColor { get; set; } = "Base1";

        /// <summary>
        /// 프레임 색상을 가져오거나 설정합니다. 테마 색상 키를 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public string FrameColor { get; set; } = "Base4";

        /// <summary>
        /// 현재 수위 값을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public double Value { get; set; } = 0;

        /// <summary>
        /// 수위 최소값을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public double Minimum { get; set; } = 0;

        /// <summary>
        /// 수위 최대값을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 9)] public double Maximum { get; set; } = 100;

        /// <summary>
        /// 수위 값의 표시 형식을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 10)] public string Format { get; set; } = "0";

        /// <summary>
        /// 입구(Inlet) 포트 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public bool UseInlet { get; set; } = true;

        /// <summary>
        /// 입구(Inlet) 포트의 위치를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 12)] public TopPortPosition InletPosition { get => inletpos; set { inletpos = value; SetPortPosition(); } }

        /// <summary>
        /// 믹서 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 13)] public bool UseMixer { get; set; }

        /// <summary>
        /// 믹서 가동 상태를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 14)] public bool MixerOnOff { get; set; }

        /// <summary>
        /// 파이프 크기를 가져옵니다. 부모 패널에서 상속됩니다.
        /// </summary>
        [JsonIgnore] public float PipeSize => Parent is FsFlowSystemPanel fs ? fs.PipeSize : 30;

        /// <summary>
        /// 프레임 크기를 가져옵니다. 부모 패널에서 상속됩니다.
        /// </summary>
        [JsonIgnore] public float FrameSize => Parent is FsFlowSystemPanel fs ? fs.FrameSize : 5;

        /// <summary>
        /// 흐름 여부를 가져옵니다. 수위가 0보다 큰 경우 true입니다.
        /// </summary>
        [JsonIgnore] public override bool IsFlow => Value > 0;
        #endregion

        #region Member Variable
        SKPath pathIn = new SKPath();
        SKPath pathOut = new SKPath();
        SKPath pathValue = new SKPath();
        SKPath pathMixer = new SKPath();

        DateTime dt = DateTime.Now;
        TopPortPosition inletpos = TopPortPosition.TopCenter;
        ConnectPort inlet = new ConnectPort { Name = "Inlet", Position = new SKPoint(0, 0), Type = PortType.Input, Direction = PortDirection.T };
        ConnectPort outlet = new ConnectPort { Name = "Outlet", Position = new SKPoint(0, 0), Type = PortType.Output, Direction = PortDirection.B };
        #endregion

        #region Override
        #region Init
        /// <summary>
        /// 초기화 시 포트 위치를 설정합니다.
        /// </summary>
        /// <param name="design">디자인 객체</param>
        protected override void OnInit(GoDesign? design)
        {
            SetPortPosition();

            base.OnInit(design);
        }
        #endregion

        #region Update
        /// <summary>
        /// 업데이트 시 포트 위치를 갱신합니다.
        /// </summary>
        protected override void OnUpdate()
        {
            SetPortPosition();

            base.OnUpdate();
        }
        #endregion

        #region Draw
        /// <summary>
        /// 사일로 탱크를 캔버스에 그립니다.
        /// 프레임, 빈 영역, 주입 애니메이션, 액체 채움, 믹서, 텍스트 순서로 렌더링합니다.
        /// </summary>
        /// <param name="canvas">SkiaSharp 캔버스</param>
        /// <param name="thm">테마 객체</param>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            SetPortPosition();

            #region var
            var rts = Areas();
            var rtOut = rts["Out"];
            var rtIn = rts["In"];
            var rtText = rts["Text"];

            var cText = thm.ToColor(TextColor);
            var cFill = thm.ToColor(FillColor);
            var cEmpty = thm.ToColor(EmptyColor);
            var cFrame = thm.ToColor(FrameColor);

            PathTool.SiloFrame(pathOut, pathIn, rtOut, FrameSize, PipeSize);
            PathTool.LiquidWave(pathValue, rtIn, Value, Minimum, Maximum, MixerOnOff);
            using var p = new SKPaint { IsAntialias = true };
            #endregion

            #region Frame
            p.IsStroke = false;
            p.Color = cFrame;
            canvas.DrawPath(pathOut, p);

            if (UseInlet)
                canvas.DrawRect(Util.FromRect(inlet.Position.X - Left - PipeSize / 2f, inlet.Position.Y - Top, PipeSize, Height * 0.2f), p);
            #endregion

            #region Empty
            p.IsStroke = false;
            p.Color = cEmpty;
            canvas.DrawPath(pathIn, p);

            if (UseInlet)
                canvas.DrawRect(Util.FromRect(inlet.Position.X - Left - PipeSize / 2f + FrameSize, inlet.Position.Y - Top, PipeSize - FrameSize * 2, Height * 0.2f), p);
            canvas.DrawRect(Util.FromRect(outlet.Position.X - Left - PipeSize / 2f + FrameSize, outlet.Position.Y - Top - FrameSize - 1, PipeSize - FrameSize * 2, FrameSize + 2), p);
            #endregion

            #region Inject
            if (UseInlet && Parent is FsFlowSystemPanel fs && fs.GetConnection(Id, inlet.Name) is FlowConnection conn && conn.IsFlow)
            {
                var inletX = inlet.Position.X - Left;
                var inletY = inlet.Position.Y - Top;
                var half = (PipeSize - FrameSize * 2F) / 2f;
                var flowColor = conn.LiquidColor.HasValue ? conn.LiquidColor.Value : cFill;

                using (new SKAutoCanvasRestore(canvas))
                {
                    p.IsStroke = false;
                    p.Color = flowColor;

                    canvas.DrawRect(Util.FromRect(inletX - half, inletY, half * 2, Height * 0.2F), p);

                    canvas.ClipPath(pathIn);
                    canvas.DrawRect(Util.FromRect(inletX - half, inletY, half * 2, Height - FrameSize * 2), p);

                    var cf = flowColor.BrightnessTransmit(0.75F);
                    canvas.ClipRect(Util.FromRect(inletX - half, inletY, half * 2, Height - FrameSize * 2));
                    for (int i = 0; i < 5; i++)
                    {
                        float seed = (float)(((DateTime.Now - dt).TotalMilliseconds + (i * 150)) % 750) / 750f;
                        float flowHeight = Height - FrameSize * 2 - inletY;
                        float dy = inletY + (flowHeight * seed);
                        float bubbleSize = 3 + (i % 3);
                        float dx = inletX + (float)Math.Sin(seed * Math.PI * 4 + i) * (half * 0.5f);

                        byte alpha = (byte)(Math.Sin(seed * Math.PI) * 100 + 100);
                        p.Color = cf.WithAlpha(alpha);

                        canvas.DrawCircle(dx, dy, bubbleSize, p);
                    }
                }
            }
            #endregion

            #region Fill
            if (Value > 0)
            {
                using var sh = SKShader.CreateLinearGradient(
                    new SKPoint(rtOut.Left, rtOut.Top), new SKPoint(rtOut.Right, rtOut.Top),
                    [cFill.WithAlpha(220), cFill.WithAlpha(255), cFill.WithAlpha(220)],
                    [0f, 0.5f, 1f],
                    SKShaderTileMode.Clamp
                );

                p.Color = cFill;
                p.Shader = sh;
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipPath(pathIn, SKClipOperation.Intersect, true);

                    p.IsStroke = false;
                    canvas.DrawPath(pathValue, p);

                    p.IsStroke = true;
                    p.StrokeWidth = 1;
                    canvas.DrawPath(pathValue, p);
                }

                p.IsStroke = false;
                canvas.DrawRect(Util.FromRect(outlet.Position.X - Left - PipeSize / 2f + FrameSize, outlet.Position.Y - Top - FrameSize - 1, PipeSize - FrameSize * 2, FrameSize + 2), p);

                p.IsStroke = true;
                p.StrokeWidth = 1;
                canvas.DrawRect(Util.FromRect(outlet.Position.X - Left - PipeSize / 2f + FrameSize, outlet.Position.Y - Top - FrameSize - 1, PipeSize - FrameSize * 2, FrameSize + 2), p);

                p.Shader = null;
            }
            #endregion

            #region Mixer
            if (UseMixer)
            {
                #region Mixer
                var cx = Width * 0.5F;
                var cy = Height * 0.75F;
                var hf = FrameSize / 2F;
                var ani = MixerOnOff ? ((DateTime.Now - dt).TotalMilliseconds % 200F - 100F) / 100F : 0f;
                var ww = Convert.ToSingle(MathTool.Map(Math.Abs(ani), 0F, 1F, Width * 0.3F, Width * 0.003F));
                var wh = 7;
                var mixerBounds = pathMixer.Bounds;

                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.Translate(cx, cy);

                    PathTool.Mixer(pathMixer, new SKPoint(0, 0), ww, wh, FrameSize);

                    var lightColor = cFrame.BrightnessTransmit(thm.GradientLightBrightness);
                    var darkColor = cFrame.BrightnessTransmit(thm.GradientDarkBrightness);

                    if (MixerOnOff)
                    {
                        using var shader = SKShader.CreateLinearGradient(new SKPoint(-ww, mixerBounds.Top), new SKPoint(+ww, mixerBounds.Top), ani > 0 ? [lightColor, darkColor] : [darkColor, lightColor], [0.0f, 1.0f], SKShaderTileMode.Clamp);
                        p.IsStroke = false;
                        p.Shader = shader;
                        canvas.DrawPath(pathMixer, p);
                        p.Shader = null;
                    }
                    else
                    {
                        p.IsStroke = false;
                        p.Color = cFrame;
                        canvas.DrawPath(pathMixer, p);
                    }
                }

                var rt = new SKRect(cx - hf, 0, cx + hf, cy);
                p.IsStroke = false;
                p.Color = cFrame;
                canvas.DrawRect(rt, p);
                #endregion

                #region Bubbles
                if (MixerOnOff && Value > Minimum)
                {
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipPath(pathIn);
                        canvas.ClipPath(pathValue);

                        p.Shader = null;
                        p.Style = SKPaintStyle.Fill;
                        p.Color = SKColors.White.WithAlpha(160);

                        for (int i = 0; i < 6; i++)
                        {
                            float seed = (float)(((DateTime.Now - dt).TotalMilliseconds + (i * 125)) % 500) / 500f;
                            float bx = rtIn.Left + (rtIn.Width * (0.2f + i * 0.12f));
                            float by = rtIn.Bottom - (rtIn.Height * seed * Convert.ToSingle(Value / Maximum));

                            canvas.DrawCircle(bx, by, 2 + i % 3, p);
                        }
                    }
                }
                #endregion

                #region Alpha
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipPath(pathIn, SKClipOperation.Intersect, true);
                    p.Color = cFill.WithAlpha(100);
                    canvas.DrawPath(pathValue, p);
                }
                #endregion
            }
            #endregion

            #region Text
            var txt = string.IsNullOrWhiteSpace(Format) ? Value.ToString() : Value.ToString(Format);
            Util.DrawText(canvas, txt, FontName, FontStyle, FontSize, rtText, cText, cEmpty.WithAlpha(128), 10);
            #endregion

            base.OnDraw(canvas, thm);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// 리소스를 해제합니다.
        /// </summary>
        protected override void OnDispose()
        {
            pathIn.Dispose();
            pathOut.Dispose();
            pathValue.Dispose();
            pathMixer.Dispose();
            base.OnDispose();
        }
        #endregion

        #region Areas
        /// <summary>
        /// 탱크의 영역 사전을 반환합니다.
        /// Content, Out, In, Text 영역이 포함됩니다.
        /// </summary>
        /// <returns>영역 이름과 사각형의 사전</returns>
        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = Util.FromRect(0, 0, Width, Height);
            var rtOut = rtContent;
            var rtIn = rtOut; rtIn.Inflate(-FrameSize, -FrameSize);
            var rtText = Util.FromRect(rtIn.Left, rtIn.Top + (rtContent.Height * 0.2F), rtIn.Width, FontSize + 10);

            rts["Content"] = rtContent;
            rts["Out"] = rtOut;
            rts["In"] = rtIn;
            rts["Text"] = rtText;
            return rts;
        }
        #endregion

        #region IoPorts
        /// <summary>
        /// 사용 가능한 입출력 포트 목록을 반환합니다.
        /// </summary>
        /// <returns>포트 열거</returns>
        public override IEnumerable<ConnectPort> IoPorts()
        {
            if (UseInlet) return [inlet, outlet];
            else return [outlet];
        }

        /// <summary>
        /// 이름으로 포트를 조회합니다.
        /// </summary>
        /// <param name="name">포트 이름</param>
        /// <returns>해당 포트 또는 null</returns>
        public override ConnectPort? GetPort(string? name)
        {
            if (inlet.Name == name && UseInlet) return inlet;
            else if (outlet.Name == name) return outlet;
            else return null;
        }
        #endregion

        #region PortPosition
        /// <summary>
        /// 입구/출구 포트의 위치를 현재 설정에 따라 갱신합니다.
        /// </summary>
        void SetPortPosition()
        {
            var brt = Bounds;

            var ps = PipeSize;
            switch (InletPosition)
            {
                case TopPortPosition.TopLeft: inlet.Position = new SKPoint((brt.Left + 10) + (ps / 2), brt.Top); break;
                case TopPortPosition.TopCenter: inlet.Position = new SKPoint(brt.MidX, brt.Top); break;
                case TopPortPosition.TopRight: inlet.Position = new SKPoint((brt.Right - 10) - (ps / 2), brt.Top); break;
            }

            outlet.Position = new SKPoint(brt.MidX, brt.Bottom);
        }
        #endregion
        #endregion
    }
}
