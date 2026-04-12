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
    /// 배관 흐름의 개폐를 제어하는 글로브 밸브 컴포넌트입니다.
    /// <see cref="OnOff"/>가 <c>true</c>(열림)이면 유체가 통과하고, <c>false</c>(닫힘)이면 흐름이 차단됩니다.
    /// 상태 변경 시 200ms 동안 핸들 개폐 애니메이션이 재생되며, 중앙의 상태 램프가 개방/폐쇄 색상으로 표시됩니다.
    ///
    /// <para><b>주요 사용 시나리오</b></para>
    /// <list type="bullet">
    ///   <item>배관 경로 중간에 배치하여 유체 흐름을 개폐 제어합니다.</item>
    ///   <item>펌프와 탱크 사이, 또는 탱크와 탱크 사이에서 흐름 차단/허용 용도로 사용합니다.</item>
    ///   <item>양방향(Bidirectional) 포트이므로 흐름 방향에 관계없이 배치할 수 있습니다.</item>
    /// </list>
    ///
    /// <para><b>프로퍼티 설정 가이드</b></para>
    /// <list type="bullet">
    ///   <item><see cref="OnOff"/> - <c>true</c>: 밸브 열림(흐름 통과), <c>false</c>: 밸브 닫힘(흐름 차단). 상태 변경 시 200ms 개폐 애니메이션이 자동 재생됩니다.</item>
    ///   <item><see cref="Rotate"/> - 밸브 전체를 0/90/180/270도 회전시킵니다. 포트 위치와 방향도 함께 변경됩니다.</item>
    ///   <item><see cref="OnColor"/> - 밸브 열림 상태의 램프 색상입니다. 기본값은 "Lime"입니다.</item>
    ///   <item><see cref="OffColor"/> - 밸브 닫힘 상태의 램프 색상입니다. 기본값은 "Red"입니다.</item>
    ///   <item><see cref="FrameColor"/> - 밸브 본체 및 핸들의 프레임 색상입니다.</item>
    /// </list>
    ///
    /// <para><b>포트 구성</b></para>
    /// <list type="table">
    ///   <listheader><term>포트 이름</term><description>타입 및 설명</description></listheader>
    ///   <item><term>Port1</term><description><see cref="PortType.Bidirectional"/> - 양방향 포트. 기본 위치는 왼쪽(L)입니다.</description></item>
    ///   <item><term>Port2</term><description><see cref="PortType.Bidirectional"/> - 양방향 포트. 기본 위치는 오른쪽(R)입니다.</description></item>
    /// </list>
    /// <para>두 포트 모두 양방향이므로, 어느 쪽에서든 유체가 흘러들어올 수 있으며 반대쪽으로 전달됩니다.</para>
    ///
    /// <para><b>회전에 따른 포트 위치 변화</b></para>
    /// <list type="table">
    ///   <listheader><term>Rotate</term><description>Port1 위치 / Port2 위치</description></listheader>
    ///   <item><term>Deg0</term><description>Port1=왼쪽(L) / Port2=오른쪽(R)</description></item>
    ///   <item><term>Deg90</term><description>Port1=위쪽(T) / Port2=아래쪽(B)</description></item>
    ///   <item><term>Deg180</term><description>Port1=오른쪽(R) / Port2=왼쪽(L)</description></item>
    ///   <item><term>Deg270</term><description>Port1=아래쪽(B) / Port2=위쪽(T)</description></item>
    /// </list>
    ///
    /// <para><b>개폐 애니메이션</b></para>
    /// <para><see cref="OnOff"/> 상태가 변경되면 200ms 동안 핸들이 접혔다 펼쳐지는 애니메이션이 재생됩니다.
    /// 중앙의 상태 램프는 열림 시 <see cref="OnColor"/>, 닫힘 시 <see cref="OffColor"/>로 표시됩니다.</para>
    /// </summary>
    /// <remarks>
    /// 사용 예시:
    /// <code>
    /// // 밸브 생성 및 배치
    /// var valve = new FsValve
    /// {
    ///     Left = 300, Top = 100, Width = 60, Height = 60,
    ///     OnColor = "Lime",
    ///     OffColor = "Red"
    /// };
    /// panel.Controls.Add(valve);
    ///
    /// // 밸브 열기 (흐름 허용)
    /// valve.OnOff = true;
    ///
    /// // 밸브 닫기 (흐름 차단)
    /// valve.OnOff = false;
    ///
    /// // 세로 방향 배관에 맞게 90도 회전
    /// valve.Rotate = ObjectRotate.Deg90;
    /// </code>
    /// </remarks>
    public class FsValve : FsFlowObject, IRotatable
    {
        #region Properties
        /// <summary>
        /// 파이프 크기를 가져옵니다. 부모 패널에서 상속됩니다.
        /// </summary>
        [JsonIgnore] public float PipeSize => Parent is FsFlowSystemPanel fs ? fs.PipeSize : 30;

        /// <summary>
        /// 프레임 크기를 가져옵니다. 부모 패널에서 상속됩니다.
        /// </summary>
        [JsonIgnore] public float FrameSize => Parent is FsFlowSystemPanel fs ? fs.FrameSize : 5;

        /// <summary>
        /// 개방 시 표시 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public string OnColor { get; set; } = "Lime";

        /// <summary>
        /// 폐쇄 시 표시 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public string OffColor { get; set; } = "Red";

        /// <summary>
        /// 프레임 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public string FrameColor { get; set; } = "Base4";

        /// <summary>
        /// 밸브의 개방/폐쇄 상태를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)]
        public bool OnOff
        {
            get => bOnOff;
            set
            {
                if (bOnOff != value)
                {
                    bOnOff = value;
                    changeTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 객체의 회전 각도를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)]
        public ObjectRotate Rotate
        {
            get => rotate;
            set
            {
                if (rotate != value)
                {
                    rotate = value;
                    SetPortPosition();
                }
            }
        }

        /// <summary>
        /// 흐름 여부를 가져옵니다. OnOff 상태에 따라 결정됩니다.
        /// </summary>
        [JsonIgnore] public override bool IsFlow => OnOff;
        #endregion

        #region Member Variable
        bool bOnOff = false;
        ObjectRotate rotate = ObjectRotate.Deg0;
        DateTime changeTime = DateTime.Now - TimeSpan.FromSeconds(1);
        SKPath path = new SKPath();
        SKPath pathH = new SKPath();

        ConnectPort port1 = new ConnectPort { Name = "Port1", Type = PortType.Bidirectional, Direction = PortDirection.L };
        ConnectPort port2 = new ConnectPort { Name = "Port2", Type = PortType.Bidirectional, Direction = PortDirection.R };
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
        /// 밸브를 그립니다. 본체, 핸들 애니메이션, 상태 램프를 포함합니다.
        /// </summary>
        /// <param name="canvas">SkiaSharp 캔버스</param>
        /// <param name="thm">테마</param>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            SetPortPosition();

            #region var
            using var p = new SKPaint { IsAntialias = true };

            var cOn = thm.ToColor(OnColor);
            var cOff = thm.ToColor(OffColor);
            var cFrame = thm.ToColor(FrameColor);

            var hs = Math.Min((DateTime.Now - changeTime).TotalMilliseconds, 200);
            var handleSize = Convert.ToSingle(MathTool.Map(Math.Abs(hs - 100), 0, 100, 10, PipeSize + 10));

            var ro = 0;
            switch (Rotate)
            {
                case ObjectRotate.Deg0: ro = 0; break;
                case ObjectRotate.Deg90: ro = 90; break;
                case ObjectRotate.Deg180: ro = 180; break;
                case ObjectRotate.Deg270: ro = 270; break;
            }
            #endregion

            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.Translate(Width / 2, Height / 2);
                canvas.RotateDegrees(ro);

                var rt = Util.FromRect(-Width / 2, -Height / 2, Width, Height);

                PathTool.GlobeValve(path, rt, PipeSize, FrameSize, handleSize);

                #region Body
                p.IsStroke = false;
                p.Color = cFrame;
                canvas.DrawPath(path, p);
                #endregion

                #region Handle
                if ((DateTime.Now - changeTime).TotalMilliseconds < 200)
                {
                    var radius = (PipeSize + 10) / 2f;
                    var handleWidth = handleSize;
                    var handleHeight = 10f;
                    var vrt = MathTool.MakeRectangle(new SKPoint(0, -(radius + handleHeight)), handleWidth / 2F, handleHeight / 2F);
                    var lightColor = cFrame.BrightnessTransmit(thm.GradientLightBrightness * 1.25F);
                    var darkColor = cFrame.BrightnessTransmit(thm.GradientDarkBrightness * 1.25F);

                    using var shader = SKShader.CreateLinearGradient(
                                        new SKPoint(vrt.Left, vrt.Top), new SKPoint(vrt.Right, vrt.Top),
                                        hs - 100 < 0 ? [lightColor, darkColor] : [darkColor, lightColor], [0.0f, 1.0f], SKShaderTileMode.Clamp);

                    p.IsStroke = false;
                    p.Shader = shader;
                    canvas.DrawRoundRect(vrt, 4, 4, p);
                    p.Shader = null;
                }
                #endregion
            }

            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.Translate(Width / 2, Height / 2);

                #region Lamp
                var cx = 0F;
                var cy = 0F;
                var r = PipeSize / 2F;
                var baseColor = OnOff ? cOn : cOff;
                using (var lampPaint = new SKPaint { IsAntialias = true })
                {
                    lampPaint.Shader = SKShader.CreateRadialGradient(
                        new SKPoint(cx - r * 0.25F, cy - r * 0.25F),
                        r,
                        new SKColor[] { baseColor.BrightnessTransmit(0.4f), baseColor.BrightnessTransmit(-0.2f) },
                        null,
                        SKShaderTileMode.Clamp);
                    canvas.DrawCircle(cx, cy, r, lampPaint);

                    lampPaint.Shader = null;
                    lampPaint.Color = SKColors.White.WithAlpha(160);
                    canvas.DrawCircle(cx - (r * 0.25f), cy - (r * 0.25f), r * 0.2f, lampPaint);
                }
                #endregion
            }

            base.OnDraw(canvas, thm);
        }
        #endregion

        #region Areas
        /// <summary>
        /// 컨트롤 영역을 반환합니다.
        /// </summary>
        /// <returns>영역 딕셔너리</returns>
        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var rtContent = Util.FromRect(0, 0, Width, Height);

            rts["Content"] = rtContent;
            return rts;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// 리소스를 해제합니다.
        /// </summary>
        protected override void OnDispose()
        {
            path.Dispose();
            pathH.Dispose();
            base.OnDispose();
        }
        #endregion

        #region IoPort
        /// <summary>
        /// 모든 입출력 포트를 반환합니다.
        /// </summary>
        /// <returns>2개의 양방향 연결 포트</returns>
        public override IEnumerable<ConnectPort> IoPorts() => [port1, port2];

        /// <summary>
        /// 지정된 이름의 포트를 반환합니다.
        /// </summary>
        /// <param name="name">포트 이름</param>
        /// <returns>해당 포트 또는 null</returns>
        public override ConnectPort? GetPort(string? name)
        {
            if (port1.Name == name) return port1;
            if (port2.Name == name) return port2;
            return null;
        }
        #endregion

        #region PortPosition
        void SetPortPosition()
        {
            var brt = Bounds;
            float ps = PipeSize;
            float gap = ps / 4f;

            // 밸브 중심에서 포트까지의 실제 거리
            float bodyWidth = gap + ps;

            float cx = brt.MidX;
            float cy = brt.MidY;

            // Deg0 기준 논리적 좌표
            SKPoint p1 = new SKPoint(cx - bodyWidth, cy);
            SKPoint p2 = new SKPoint(cx + bodyWidth, cy);

            // Rotate에 따른 실제 좌표 계산
            float angle = Rotate switch
            {
                ObjectRotate.Deg90 => 90,
                ObjectRotate.Deg180 => 180,
                ObjectRotate.Deg270 => 270,
                _ => 0
            };

            if (angle != 0)
            {
                var center = new SKPoint(cx, cy);
                port1.Position = RotatePoint(p1, center, angle);
                port2.Position = RotatePoint(p2, center, angle);
                port1.Direction = RotateDirection(PortDirection.L, Rotate);
                port2.Direction = RotateDirection(PortDirection.R, Rotate);
            }
            else
            {
                port1.Position = p1;
                port2.Position = p2;
                port1.Direction = PortDirection.L;
                port2.Direction = PortDirection.R;
            }
        }

        /// <summary>
        /// 좌표를 지정된 중심점 기준으로 회전합니다.
        /// </summary>
        private SKPoint RotatePoint(SKPoint point, SKPoint center, float angle)
        {
            float rad = angle * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            float dx = point.X - center.X;
            float dy = point.Y - center.Y;

            return new SKPoint(cos * dx - sin * dy + center.X, sin * dx + cos * dy + center.Y);
        }

        /// <summary>
        /// 포트 방향을 회전 각도에 따라 변환합니다.
        /// </summary>
        private PortDirection RotateDirection(PortDirection baseDir, ObjectRotate rotate)
        {
            int step = (int)rotate;
            var dir = baseDir;
            for (int i = 0; i < step; i++)
            {
                dir = dir switch
                {
                    PortDirection.L => PortDirection.T,
                    PortDirection.T => PortDirection.R,
                    PortDirection.R => PortDirection.B,
                    PortDirection.B => PortDirection.L,
                    _ => dir
                };
            }
            return dir;
        }
        #endregion
        #endregion
    }
}
