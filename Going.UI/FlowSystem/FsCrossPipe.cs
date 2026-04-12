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
    /// 십자형(+) 파이프 컴포넌트. 4방향 양방향 포트를 제공하여 배관을 교차 분기시킨다.
    /// <para>FsFlowSystemPanel의 자식으로 배치되며, 4개의 Bidirectional 포트(Port1~Port4)로 최대 4방향 배관 연결이 가능하다.</para>
    /// <para>IsFlow는 항상 true로, 연결된 배관에 흐름을 무조건 통과시킨다.</para>
    ///
    /// <para><b>주요 사용 시나리오:</b></para>
    /// <list type="bullet">
    ///   <item>배관이 십자로 교차하는 지점에 배치하여 4방향 분기를 구성한다.</item>
    ///   <item>서로 다른 유체가 합류하면 MixedColor로 표시된다.</item>
    /// </list>
    ///
    /// <para><b>프로퍼티 설정 가이드:</b></para>
    /// <list type="bullet">
    ///   <item><see cref="Rotate"/>: 회전 각도(Deg0/Deg90/Deg180/Deg270). 포트의 방향과 위치가 자동으로 재배치된다.</item>
    ///   <item><see cref="PipeSize"/>/<see cref="FrameSize"/>: 부모 FsFlowSystemPanel에서 자동 상속된다.</item>
    /// </list>
    ///
    /// <para><b>포트 구성 (Deg0 기준):</b></para>
    /// <list type="table">
    ///   <listheader><term>포트 이름</term><description>방향 / 타입</description></listheader>
    ///   <item><term>Port1</term><description>L (왼쪽) / Bidirectional</description></item>
    ///   <item><term>Port2</term><description>T (위쪽) / Bidirectional</description></item>
    ///   <item><term>Port3</term><description>R (오른쪽) / Bidirectional</description></item>
    ///   <item><term>Port4</term><description>B (아래쪽) / Bidirectional</description></item>
    /// </list>
    ///
    /// <para><b>회전 시 포트 방향 변화:</b></para>
    /// <list type="table">
    ///   <listheader><term>회전</term><description>Port1 → Port2 → Port3 → Port4</description></listheader>
    ///   <item><term>Deg0</term><description>L → T → R → B</description></item>
    ///   <item><term>Deg90</term><description>T → R → B → L</description></item>
    ///   <item><term>Deg180</term><description>R → B → L → T</description></item>
    ///   <item><term>Deg270</term><description>B → L → T → R</description></item>
    /// </list>
    /// <para>회전은 시계 방향으로 90도씩 증가하며, 모든 포트가 동일하게 회전한다.</para>
    /// </summary>
    /// <remarks>
    /// 사용 예:
    /// <code>
    /// var panel = new FsFlowSystemPanel();
    /// var cross = new FsCrossPipe
    /// {
    ///     X = 200, Y = 200,
    ///     Width = 60, Height = 60,
    ///     Rotate = ObjectRotate.Deg0
    /// };
    /// panel.Childrens.Add(cross);
    ///
    /// // 왼쪽 파이프와 Port1(L) 연결
    /// panel.Connections.Add(new FlowConnection
    /// {
    ///     StartControlId = leftPipe.Id, StartPortName = "Outlet",
    ///     EndControlId = cross.Id, EndPortName = "Port1"
    /// });
    /// // 오른쪽 파이프와 Port3(R) 연결
    /// panel.Connections.Add(new FlowConnection
    /// {
    ///     StartControlId = cross.Id, StartPortName = "Port3",
    ///     EndControlId = rightPipe.Id, EndPortName = "Inlet"
    /// });
    /// panel.RefreshCache();
    /// </code>
    /// </remarks>
    public class FsCrossPipe : FsFlowObject, IRotatable
    {
        #region Properties
        /// <summary>
        /// 객체의 회전 각도를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)]
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
        /// 파이프 크기를 가져옵니다. 부모 패널에서 상속됩니다.
        /// </summary>
        [JsonIgnore] public float PipeSize => Parent is FsFlowSystemPanel fs ? fs.PipeSize : 30;

        /// <summary>
        /// 프레임 크기를 가져옵니다. 부모 패널에서 상속됩니다.
        /// </summary>
        [JsonIgnore] public float FrameSize => Parent is FsFlowSystemPanel fs ? fs.FrameSize : 5;

        /// <summary>
        /// 흐름 여부를 가져옵니다. 십자 파이프는 항상 흐름 상태입니다.
        /// </summary>
        [JsonIgnore] public override bool IsFlow => true;
        #endregion

        #region Member Variable
        ObjectRotate rotate = ObjectRotate.Deg0;

        ConnectPort port1 = new ConnectPort { Name = "Port1", Position = new SKPoint(0, 0), Type = PortType.Bidirectional, Direction = PortDirection.L };
        ConnectPort port2 = new ConnectPort { Name = "Port2", Position = new SKPoint(0, 0), Type = PortType.Bidirectional, Direction = PortDirection.T };
        ConnectPort port3 = new ConnectPort { Name = "Port3", Position = new SKPoint(0, 0), Type = PortType.Bidirectional, Direction = PortDirection.R };
        ConnectPort port4 = new ConnectPort { Name = "Port4", Position = new SKPoint(0, 0), Type = PortType.Bidirectional, Direction = PortDirection.B };
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
        /// 십자형 파이프를 그립니다.
        /// </summary>
        /// <param name="canvas">SkiaSharp 캔버스</param>
        /// <param name="thm">테마</param>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            SetPortPosition();

            if (Parent is FsFlowSystemPanel fs)
            {
                #region var
                using var p = new SKPaint { IsAntialias = true };

                var cEmpty = thm.ToColor(fs.EmptyColor);
                var cFrame = thm.ToColor(fs.FrameColor);
                #endregion

                var rt = Areas()["Content"];
                var l = rt.Left - 1;
                var t = rt.Top - 1;
                var r = rt.Right + 1;
                var b = rt.Bottom + 1;
                var cx = rt.MidX;
                var cy = rt.MidY;

                var p1 = new SKPoint(l, cy);
                var p2 = new SKPoint(r, cy);
                var p3 = new SKPoint(cx, t);
                var p4 = new SKPoint(cx, b);

                Drawline(canvas, thm, p1, p2, p3, p4, cFrame, cEmpty, p);
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

        #region IoPort
        /// <summary>
        /// 모든 입출력 포트를 반환합니다.
        /// </summary>
        /// <returns>4개의 연결 포트</returns>
        public override IEnumerable<ConnectPort> IoPorts() => [port1, port2, port3, port4];

        /// <summary>
        /// 지정된 이름의 포트를 반환합니다.
        /// </summary>
        /// <param name="name">포트 이름</param>
        /// <returns>해당 포트 또는 null</returns>
        public override ConnectPort? GetPort(string? name)
        {
            if (port1.Name == name) return port1;
            else if (port2.Name == name) return port2;
            else if (port3.Name == name) return port3;
            else if (port4.Name == name) return port4;
            else return null;
        }
        #endregion

        #region PortPosition
        void SetPortPosition()
        {
            var brt = Bounds;
            var l = brt.Left;
            var r = brt.Right;
            var t = brt.Top;
            var b = brt.Bottom;
            float cx = brt.MidX;
            float cy = brt.MidY;

            switch (rotate)
            {
                case ObjectRotate.Deg0:
                    port1.Direction = PortDirection.L; port1.Position = new SKPoint(l, cy);
                    port2.Direction = PortDirection.T; port2.Position = new SKPoint(cx, t);
                    port3.Direction = PortDirection.R; port3.Position = new SKPoint(r, cy);
                    port4.Direction = PortDirection.B; port4.Position = new SKPoint(cx, b);
                    break;
                case ObjectRotate.Deg90:
                    port1.Direction = PortDirection.T; port1.Position = new SKPoint(cx, t);
                    port2.Direction = PortDirection.R; port2.Position = new SKPoint(r, cy);
                    port3.Direction = PortDirection.B; port3.Position = new SKPoint(cx, b);
                    port4.Direction = PortDirection.L; port4.Position = new SKPoint(l, cy);
                    break;
                case ObjectRotate.Deg180:
                    port1.Direction = PortDirection.R; port1.Position = new SKPoint(r, cy);
                    port2.Direction = PortDirection.B; port2.Position = new SKPoint(cx, b);
                    port3.Direction = PortDirection.L; port3.Position = new SKPoint(l, cy);
                    port4.Direction = PortDirection.T; port4.Position = new SKPoint(cx, t);
                    break;
                case ObjectRotate.Deg270:
                    port1.Direction = PortDirection.B; port1.Position = new SKPoint(cx, b);
                    port2.Direction = PortDirection.L; port2.Position = new SKPoint(l, cy);
                    port3.Direction = PortDirection.T; port3.Position = new SKPoint(cx, t);
                    port4.Direction = PortDirection.R; port4.Position = new SKPoint(r, cy);
                    break;
            }
        }
        #endregion
        #endregion

        #region Method
        void Drawline(SKCanvas canvas, GoTheme thm, SKPoint p1, SKPoint p2, SKPoint p3, SKPoint p4, SKColor cFrame, SKColor cEmpty, SKPaint p)
        {
            if (Parent is FsFlowSystemPanel fs)
            {
                p.IsStroke = true;
                p.StrokeWidth = PipeSize;
                p.StrokeJoin = SKStrokeJoin.Bevel;
                p.Color = cFrame;
                canvas.DrawLine(p1, p2, p);
                canvas.DrawLine(p3, p4, p);

                using var mf = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, FrameSize);
                p.StrokeWidth = PipeSize - (FrameSize * 4);
                p.Color = cFrame.BrightnessTransmit(0.5F);
                canvas.DrawLine(p1, p2, p);
                canvas.DrawLine(p3, p4, p);
            }
        }
        #endregion
    }
}
