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
    /// 액체를 저장하고 수위를 시각적으로 표시하는 원통형 탱크 컴포넌트입니다.
    /// <see cref="Value"/>/<see cref="Minimum"/>/<see cref="Maximum"/>으로 수위를 설정하며,
    /// 수위에 따라 액체 채움 애니메이션과 파동 효과가 표현됩니다.
    /// <see cref="Value"/>가 0보다 크고 <see cref="UseOutlet"/>이 <c>true</c>일 때 하류로 흐름이 발생하여 연결된 배관에 액체를 공급합니다.
    ///
    /// <para><b>주요 사용 시나리오</b></para>
    /// <list type="bullet">
    ///   <item>배관 시스템에서 액체 저장 탱크로 사용합니다.</item>
    ///   <item>상류 펌프로부터 Inlet을 통해 유체를 공급받고, Outlet을 통해 하류 배관으로 유체를 배출합니다.</item>
    ///   <item>교반기(Mixer)를 활성화하여 탱크 내 유체 혼합을 시각적으로 표현할 수 있습니다.</item>
    ///   <item><see cref="FillColor"/>로 지정한 액체 색상이 하류 배관에 전파되어, 배관 내 유체 색상이 탱크의 액체 색상과 동일하게 표시됩니다.</item>
    /// </list>
    ///
    /// <para><b>프로퍼티 설정 가이드</b></para>
    /// <list type="bullet">
    ///   <item><see cref="Value"/> - 현재 수위 값. <see cref="Minimum"/>~<see cref="Maximum"/> 범위 내에서 설정합니다.</item>
    ///   <item><see cref="Minimum"/>/<see cref="Maximum"/> - 수위 범위. 기본값은 0~100입니다.</item>
    ///   <item><see cref="Format"/> - 수위 값의 표시 형식 문자열. 기본값은 "0"입니다.</item>
    ///   <item><see cref="UseInlet"/> - <c>true</c>이면 상단 입구(Inlet) 포트를 활성화합니다. 상류 배관과 연결할 수 있습니다.</item>
    ///   <item><see cref="UseOutlet"/> - <c>true</c>이면 하단 출구(Outlet) 포트를 활성화합니다. <see cref="Value"/> &gt; 0일 때 하류로 흐름이 발생합니다.</item>
    ///   <item><see cref="InletPosition"/> - Inlet 포트의 수평 위치를 TopLeft/TopCenter/TopRight 중 선택합니다.</item>
    ///   <item><see cref="OutletPosition"/> - Outlet 포트의 수평 위치를 BottomLeft/BottomCenter/BottomRight 중 선택합니다.</item>
    ///   <item><see cref="UseMixer"/> - <c>true</c>이면 탱크 내부에 교반기를 표시합니다.</item>
    ///   <item><see cref="MixerOnOff"/> - <c>true</c>이면 교반기가 회전 애니메이션과 함께 가동됩니다. 거품 효과도 함께 표시됩니다.</item>
    ///   <item><see cref="FillColor"/> - 탱크 내 액체의 색상. 이 색상이 하류 배관의 유체 색상으로 전파됩니다.</item>
    ///   <item><see cref="EmptyColor"/> - 탱크가 비어 있을 때의 배경 색상입니다.</item>
    /// </list>
    ///
    /// <para><b>포트 구성</b></para>
    /// <list type="table">
    ///   <listheader><term>포트 이름</term><description>타입 및 설명</description></listheader>
    ///   <item><term>Inlet (입구)</term><description><see cref="PortType.Input"/> - 상단에 위치. <see cref="UseInlet"/>이 <c>true</c>일 때만 활성. 방향은 위쪽(T)입니다.</description></item>
    ///   <item><term>Outlet (출구)</term><description><see cref="PortType.Output"/> - 하단에 위치. <see cref="UseOutlet"/>이 <c>true</c>일 때만 활성. 방향은 아래쪽(B)입니다.</description></item>
    /// </list>
    /// <para>이 컴포넌트는 <see cref="IRotatable"/>을 구현하지 않으므로 회전을 지원하지 않습니다.
    /// 대신 <see cref="InletPosition"/>과 <see cref="OutletPosition"/>으로 포트의 수평 위치를 조절합니다.</para>
    ///
    /// <para><b>흐름 발생 조건</b></para>
    /// <para><see cref="IsFlow"/>는 <c>Value &gt; 0 &amp;&amp; UseOutlet</c>일 때 <c>true</c>입니다.
    /// 즉, 탱크에 액체가 있고 출구가 활성화된 경우에만 하류 배관으로 흐름이 전파됩니다.</para>
    /// </summary>
    /// <remarks>
    /// 사용 예시:
    /// <code>
    /// // 원통형 탱크 생성
    /// var tank = new FsCylinderTank
    /// {
    ///     Left = 100, Top = 50, Width = 120, Height = 200,
    ///     Minimum = 0, Maximum = 100,
    ///     FillColor = "DeepSkyBlue",
    ///     UseInlet = true,
    ///     UseOutlet = true,
    ///     InletPosition = TopPortPosition.TopCenter,
    ///     OutletPosition = BottomPortPosition.BottomCenter
    /// };
    /// panel.Controls.Add(tank);
    ///
    /// // 수위 설정 (50%)
    /// tank.Value = 50;
    ///
    /// // 교반기 활성화 및 가동
    /// tank.UseMixer = true;
    /// tank.MixerOnOff = true;
    ///
    /// // 입구 위치를 왼쪽 상단으로 변경
    /// tank.InletPosition = TopPortPosition.TopLeft;
    ///
    /// // 출구 위치를 오른쪽 하단으로 변경
    /// tank.OutletPosition = BottomPortPosition.BottomRight;
    /// </code>
    /// </remarks>
    public class FsCylinderTank : FsFlowObject
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
        /// 출구(Outlet) 포트 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 13)] public bool UseOutlet { get; set; } = true;

        /// <summary>
        /// 출구(Outlet) 포트의 위치를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 14)] public BottomPortPosition OutletPosition { get => outletpos; set { outletpos = value; SetPortPosition(); } }

        /// <summary>
        /// 믹서 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 15)] public bool UseMixer { get; set; }

        /// <summary>
        /// 믹서 가동 상태를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 16)] public bool MixerOnOff { get; set; }

        /// <summary>
        /// 파이프 크기를 가져옵니다. 부모 패널에서 상속됩니다.
        /// </summary>
        [JsonIgnore] public float PipeSize => Parent is FsFlowSystemPanel fs ? fs.PipeSize : 30;

        /// <summary>
        /// 프레임 크기를 가져옵니다. 부모 패널에서 상속됩니다.
        /// </summary>
        [JsonIgnore] public float FrameSize => Parent is FsFlowSystemPanel fs ? fs.FrameSize : 5;

        /// <summary>
        /// 흐름 여부를 가져옵니다. 수위가 0보다 크고 출구가 활성화된 경우 true입니다.
        /// </summary>
        [JsonIgnore] public override bool IsFlow => Value > 0 && UseOutlet;
        #endregion

        #region Member Variable
        SKPath pathIn = new SKPath();
        SKPath pathOut = new SKPath();
        SKPath pathValue = new SKPath();
        SKPath pathMixer = new SKPath();

        DateTime dt = DateTime.Now;
        TopPortPosition inletpos = TopPortPosition.TopCenter;
        BottomPortPosition outletpos = BottomPortPosition.BottomCenter;
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
        /// 원통형 탱크를 캔버스에 그립니다.
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

            PathTool.CylinderFrame(pathOut, pathIn, rtOut, FrameSize, PipeSize);
            PathTool.LiquidWave(pathValue, rtIn, Value, Minimum, Maximum, MixerOnOff);
            using var p = new SKPaint { IsAntialias = true };
            #endregion

            #region Frame
            p.IsStroke = false;
            p.Color = cFrame;
            canvas.DrawPath(pathOut, p);
            #endregion

            #region Empty
            p.IsStroke = false;
            p.Color = cEmpty;
            canvas.DrawPath(pathIn, p);

            if (UseInlet) canvas.DrawRect(Util.FromRect(inlet.Position.X - Left - PipeSize / 2f + FrameSize, inlet.Position.Y - Top - 1, PipeSize - FrameSize * 2, FrameSize + 2), p);
            if (UseOutlet) canvas.DrawRect(Util.FromRect(outlet.Position.X - Left - PipeSize / 2f + FrameSize, outlet.Position.Y - Top - FrameSize - 1, PipeSize - FrameSize * 2, FrameSize + 2), p);
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

                if (UseOutlet)
                {
                    p.IsStroke = false;
                    canvas.DrawRect(Util.FromRect(outlet.Position.X - Left - PipeSize / 2f + FrameSize, outlet.Position.Y - Top - FrameSize - 1, PipeSize - FrameSize * 2, FrameSize + 2), p);

                    p.IsStroke = true;
                    p.StrokeWidth = 1;
                    canvas.DrawRect(Util.FromRect(outlet.Position.X - Left - PipeSize / 2f + FrameSize, outlet.Position.Y - Top - FrameSize - 1, PipeSize - FrameSize * 2, FrameSize + 2), p);
                }
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
            if (UseInlet && UseOutlet) return [inlet, outlet];
            else if (UseInlet) return [inlet];
            else if (UseOutlet) return [outlet];
            else return [];
        }

        /// <summary>
        /// 이름으로 포트를 조회합니다.
        /// </summary>
        /// <param name="name">포트 이름</param>
        /// <returns>해당 포트 또는 null</returns>
        public override ConnectPort? GetPort(string? name)
        {
            if (inlet.Name == name && UseInlet) return inlet;
            else if (outlet.Name == name && UseOutlet) return outlet;
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

            switch (OutletPosition)
            {
                case BottomPortPosition.BottomLeft: outlet.Position = new SKPoint((brt.Left + 10) + (ps / 2), brt.Bottom); break;
                case BottomPortPosition.BottomCenter: outlet.Position = new SKPoint(brt.MidX, brt.Bottom); break;
                case BottomPortPosition.BottomRight: outlet.Position = new SKPoint((brt.Right - 10) - (ps / 2), brt.Bottom); break;
            }
        }
        #endregion
        #endregion
    }
}
