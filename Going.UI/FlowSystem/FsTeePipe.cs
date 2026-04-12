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
    /// T자형 파이프 컴포넌트. 3방향 양방향 포트를 제공하여 배관을 T자로 분기시킨다.
    /// <para>FsFlowSystemPanel의 자식으로 배치되며, 3개의 Bidirectional 포트(Port1~Port3)로 T자형 배관 분기가 가능하다.</para>
    /// <para>IsFlow는 항상 true로, 연결된 배관에 흐름을 무조건 통과시킨다.</para>
    ///
    /// <para><b>주요 사용 시나리오:</b></para>
    /// <list type="bullet">
    ///   <item>배관을 T자형으로 분기하거나 합류시키는 지점에 배치한다.</item>
    ///   <item>서로 다른 유체가 합류하면 MixedColor로 표시된다.</item>
    ///   <item>Rotate 프로퍼티로 T자의 방향을 조절한다.</item>
    /// </list>
    ///
    /// <para><b>프로퍼티 설정 가이드:</b></para>
    /// <list type="bullet">
    ///   <item><see cref="Rotate"/>: 회전 각도(Deg0/Deg90/Deg180/Deg270). 포트의 방향과 위치가 자동으로 재배치된다.</item>
    ///   <item><see cref="PipeSize"/>/<see cref="FrameSize"/>: 부모 FsFlowSystemPanel에서 자동 상속된다.</item>
    /// </list>
    ///
    /// <para><b>포트 구성 (Deg0 기준):</b></para>
    /// <para>Deg0에서의 형태: 수평 직선(Port1-Port3)에서 아래로 분기(Port2). ┬ 형태.</para>
    /// <list type="table">
    ///   <listheader><term>포트 이름</term><description>방향 / 타입</description></listheader>
    ///   <item><term>Port1</term><description>L (왼쪽) / Bidirectional</description></item>
    ///   <item><term>Port2</term><description>B (아래쪽) / Bidirectional - 분기 포트</description></item>
    ///   <item><term>Port3</term><description>R (오른쪽) / Bidirectional</description></item>
    /// </list>
    ///
    /// <para><b>회전 시 포트 방향 변화:</b></para>
    /// <list type="table">
    ///   <listheader><term>회전</term><description>Port1 → Port2(분기) → Port3 / 형태</description></listheader>
    ///   <item><term>Deg0</term><description>L → B → R / ┬ (아래로 분기)</description></item>
    ///   <item><term>Deg90</term><description>T → L → B / ├ (왼쪽으로 분기)</description></item>
    ///   <item><term>Deg180</term><description>R → T → L / ┴ (위로 분기)</description></item>
    ///   <item><term>Deg270</term><description>B → R → T / ┤ (오른쪽으로 분기)</description></item>
    /// </list>
    /// <para>회전은 시계 방향으로 90도씩 증가하며, Port2가 항상 분기 방향이 된다.</para>
    /// </summary>
    /// <remarks>
    /// 사용 예:
    /// <code>
    /// var panel = new FsFlowSystemPanel();
    /// var tee = new FsTeePipe
    /// {
    ///     X = 200, Y = 200,
    ///     Width = 60, Height = 60,
    ///     Rotate = ObjectRotate.Deg0  // ┬ 형태
    /// };
    /// panel.Childrens.Add(tee);
    ///
    /// // 왼쪽에서 들어오는 배관 → Port1(L)
    /// panel.Connections.Add(new FlowConnection
    /// {
    ///     StartControlId = leftPipe.Id, StartPortName = "Outlet",
    ///     EndControlId = tee.Id, EndPortName = "Port1"
    /// });
    /// // Port3(R) → 오른쪽으로 나가는 배관
    /// panel.Connections.Add(new FlowConnection
    /// {
    ///     StartControlId = tee.Id, StartPortName = "Port3",
    ///     EndControlId = rightPipe.Id, EndPortName = "Inlet"
    /// });
    /// // Port2(B) → 아래로 분기하는 배관
    /// panel.Connections.Add(new FlowConnection
    /// {
    ///     StartControlId = tee.Id, StartPortName = "Port2",
    ///     EndControlId = bottomPipe.Id, EndPortName = "Inlet"
    /// });
    /// panel.RefreshCache();
    /// </code>
    /// </remarks>
    public class FsTeePipe : FsFlowObject, IRotatable
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
        /// 흐름 여부를 가져옵니다. T자 파이프는 항상 흐름 상태입니다.
        /// </summary>
        [JsonIgnore] public override bool IsFlow => true;
        #endregion

        #region Member Variable
        ObjectRotate rotate = ObjectRotate.Deg0;

        ConnectPort port1 = new ConnectPort { Name = "Port1", Position = new SKPoint(0, 0), Type = PortType.Bidirectional, Direction = PortDirection.L };
        ConnectPort port2 = new ConnectPort { Name = "Port2", Position = new SKPoint(0, 0), Type = PortType.Bidirectional, Direction = PortDirection.B };
        ConnectPort port3 = new ConnectPort { Name = "Port3", Position = new SKPoint(0, 0), Type = PortType.Bidirectional, Direction = PortDirection.R };
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
        /// T자형 파이프를 그립니다.
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

                var cp = new SKPoint(cx, cy);
                switch (rotate)
                {
                    case ObjectRotate.Deg0:
                        {
                            var p1 = new SKPoint(l, cy);
                            var p2 = new SKPoint(r, cy);
                            var p3 = new SKPoint(cx, b);
                            Drawline(canvas, thm, p1, p2, p3, cp, cFrame, cEmpty, p);
                        }
                        break;
                    case ObjectRotate.Deg90:
                        {
                            var p1 = new SKPoint(cx, t);
                            var p2 = new SKPoint(cx, b);
                            var p3 = new SKPoint(l, cy);
                            Drawline(canvas, thm, p1, p2, p3, cp, cFrame, cEmpty, p);
                        }
                        break;
                    case ObjectRotate.Deg180:
                        {
                            var p1 = new SKPoint(r, cy);
                            var p2 = new SKPoint(l, cy);
                            var p3 = new SKPoint(cx, t);
                            Drawline(canvas, thm, p1, p2, p3, cp, cFrame, cEmpty, p);
                        }
                        break;
                    case ObjectRotate.Deg270:
                        {
                            var p1 = new SKPoint(cx, b);
                            var p2 = new SKPoint(cx, t);
                            var p3 = new SKPoint(r, cy);
                            Drawline(canvas, thm, p1, p2, p3, cp, cFrame, cEmpty, p);
                        }
                        break;
                }
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
        /// <returns>3개의 연결 포트</returns>
        public override IEnumerable<ConnectPort> IoPorts() => [port1, port2, port3];

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
                    port2.Direction = PortDirection.B; port2.Position = new SKPoint(cx, b);
                    port3.Direction = PortDirection.R; port3.Position = new SKPoint(r, cy);
                    break;
                case ObjectRotate.Deg90:
                    port1.Direction = PortDirection.T; port1.Position = new SKPoint(cx, t);
                    port2.Direction = PortDirection.L; port2.Position = new SKPoint(l, cy);
                    port3.Direction = PortDirection.B; port3.Position = new SKPoint(cx, b);
                    break;
                case ObjectRotate.Deg180:
                    port1.Direction = PortDirection.R; port1.Position = new SKPoint(r, cy);
                    port2.Direction = PortDirection.T; port2.Position = new SKPoint(cx, t);
                    port3.Direction = PortDirection.L; port3.Position = new SKPoint(l, cy);
                    break;
                case ObjectRotate.Deg270:
                    port1.Direction = PortDirection.B; port1.Position = new SKPoint(cx, b);
                    port2.Direction = PortDirection.R; port2.Position = new SKPoint(r, cy);
                    port3.Direction = PortDirection.T; port3.Position = new SKPoint(cx, t);
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
                p.MaskFilter = mf;
                canvas.DrawLine(p1, p2, p);
                canvas.DrawLine(p3, p4, p);
                p.MaskFilter = null;
            }
        }
        #endregion
    }
}
