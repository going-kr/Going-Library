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
    /// 유체를 토출하여 배관 흐름의 시작점 역할을 하는 펌프 컴포넌트입니다.
    /// 원형 본체 내부에 임펠러(날개차)를 가지며, <see cref="OnOff"/>가 <c>true</c>일 때 임펠러가 회전 애니메이션과 함께 흐름을 발생시킵니다.
    ///
    /// <para><b>주요 사용 시나리오</b></para>
    /// <list type="bullet">
    ///   <item>배관 시스템에서 유체의 흐름을 시작시키는 동력원으로 사용합니다.</item>
    ///   <item>상류 탱크로부터 유체를 흡입하여 하류 배관으로 토출하는 구성에 적합합니다.</item>
    ///   <item><see cref="Reverse"/>를 사용하여 흐름 방향을 반전시킬 수 있습니다.</item>
    /// </list>
    ///
    /// <para><b>프로퍼티 설정 가이드</b></para>
    /// <list type="bullet">
    ///   <item><see cref="OnOff"/> - <c>true</c>로 설정하면 펌프가 가동되어 흐름이 발생하고, 임펠러 회전 애니메이션이 재생됩니다.</item>
    ///   <item><see cref="UseInlet"/> - <c>true</c>로 설정하면 흡입구(Inlet) 포트가 활성화되어 상류 배관과 연결할 수 있습니다. 기본값은 <c>false</c>입니다.</item>
    ///   <item><see cref="Reverse"/> - <c>true</c>로 설정하면 토출 방향이 반전됩니다. Outlet과 Inlet의 위치가 좌우 반전됩니다.</item>
    ///   <item><see cref="Rotate"/> - 펌프 전체를 0/90/180/270도 회전시킵니다. 포트 위치와 방향도 함께 변경됩니다.</item>
    ///   <item><see cref="OnColor"/>/<see cref="OffColor"/> - 가동/정지 상태의 표시 색상을 지정합니다.</item>
    ///   <item><see cref="EmptyColor"/> - 유체가 공급되지 않을 때의 본체 색상입니다.</item>
    /// </list>
    ///
    /// <para><b>포트 구성</b></para>
    /// <list type="table">
    ///   <listheader><term>포트 이름</term><description>타입 및 설명</description></listheader>
    ///   <item><term>Outlet (토출구)</term><description><see cref="PortType.Output"/> - 항상 활성. 유체가 토출되는 출구 포트입니다.</description></item>
    ///   <item><term>Inlet (흡입구)</term><description><see cref="PortType.Input"/> - <see cref="UseInlet"/>이 <c>true</c>일 때만 활성. 상류로부터 유체를 흡입하는 입구 포트입니다.</description></item>
    /// </list>
    ///
    /// <para><b>회전에 따른 포트 위치 변화 (Reverse=false 기준)</b></para>
    /// <list type="table">
    ///   <listheader><term>Rotate</term><description>Outlet 위치 / Inlet 위치</description></listheader>
    ///   <item><term>Deg0</term><description>Outlet=오른쪽(R) / Inlet=왼쪽(L)</description></item>
    ///   <item><term>Deg90</term><description>Outlet=아래쪽(B) / Inlet=위쪽(T)</description></item>
    ///   <item><term>Deg180</term><description>Outlet=왼쪽(L) / Inlet=오른쪽(R)</description></item>
    ///   <item><term>Deg270</term><description>Outlet=위쪽(T) / Inlet=아래쪽(B)</description></item>
    /// </list>
    /// <para><see cref="Reverse"/>가 <c>true</c>이면 Outlet과 Inlet의 위치가 반전됩니다.</para>
    ///
    /// <para><b>임펠러 애니메이션</b></para>
    /// <para><see cref="OnOff"/>가 <c>true</c>일 때 내부 임펠러가 연속적으로 회전하는 애니메이션이 재생됩니다.
    /// 정지 시 임펠러는 고정 상태로 표시되며, 색상이 약간 어두워집니다.</para>
    /// </summary>
    /// <remarks>
    /// 사용 예시:
    /// <code>
    /// // 기본 펌프 생성 (토출구만 사용)
    /// var pump = new FsPump
    /// {
    ///     Left = 200, Top = 100, Width = 80, Height = 80,
    ///     OnColor = "Lime",
    ///     OffColor = "Red"
    /// };
    /// panel.Controls.Add(pump);
    ///
    /// // 펌프 가동
    /// pump.OnOff = true;
    ///
    /// // 흡입구를 활성화하여 상류 탱크와 연결
    /// pump.UseInlet = true;
    ///
    /// // 90도 회전하여 세로 방향 배치
    /// pump.Rotate = ObjectRotate.Deg90;
    ///
    /// // 흐름 방향 반전
    /// pump.Reverse = true;
    /// </code>
    /// </remarks>
    public class FsPump : FsFlowObject, IRotatable
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
        /// 가동 시 표시 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public string OnColor { get; set; } = "Lime";

        /// <summary>
        /// 정지 시 표시 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public string OffColor { get; set; } = "Red";

        /// <summary>
        /// 비어있을 때의 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public string EmptyColor { get; set; } = "Base1";

        /// <summary>
        /// 프레임 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public string FrameColor { get; set; } = "Base4";

        /// <summary>
        /// 흡입구 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public bool UseInlet { get; set; } = false;

        /// <summary>
        /// 펌프의 가동/정지 상태를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)]
        public bool OnOff
        {
            get => bOnOff;
            set
            {
                if (bOnOff != value) bOnOff = value;
            }
        }

        /// <summary>
        /// 객체의 회전 각도를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)]
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
        /// 역방향 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public bool Reverse { get; set; } = false;

        /// <summary>
        /// 흐름 여부를 가져옵니다. OnOff 상태에 따라 결정됩니다.
        /// </summary>
        [JsonIgnore] public override bool IsFlow => OnOff;
        #endregion

        #region Member Variable
        SKPath path = new SKPath();
        SKPath pathFan = new SKPath();

        bool bOnOff = false;
        ObjectRotate rotate = ObjectRotate.Deg0;
        DateTime dt = DateTime.Now;
        ConnectPort portOutlet = new ConnectPort { Name = "Outlet", Type = PortType.Output, Direction = PortDirection.T };
        ConnectPort portInlet = new ConnectPort { Name = "Inlet", Type = PortType.Input, Direction = PortDirection.B };
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
        /// 펌프를 그립니다. 본체, 방향 표시, 임펠러를 포함합니다.
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
            var cEmpty = thm.ToColor(EmptyColor);
            var cFrame = thm.ToColor(FrameColor);
            var cFan = OnOff ? cFrame : cFrame.BrightnessTransmit(-0.25F);
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
                var cx = 0F;
                var cy = 0F;
                var radius = PipeSize * 1.5f;
                var radius_fan = ((PipeSize * 3) - (FrameSize * 2)) / 2F;
                var rofan = OnOff ? MathTool.Map(DateTime.Now.Millisecond, 0, 1000, 0, 360) : 0;

                PathTool.CPump(path, rt, PipeSize, FrameSize, UseInlet, Reverse);
                PathTool.Impeller(pathFan, new SKPoint(0, 0), radius_fan - FrameSize, 6);

                #region Frame
                p.IsStroke = false;
                p.Color = cFrame;
                canvas.DrawPath(path, p);
                #endregion

                #region Body
                SKColor cLiquid = cEmpty;
                bool isFlow = false;
                if (Parent is FsFlowSystemPanel fs && fs.GetConnection(Id, portInlet.Name) is FlowConnection conn)
                {
                    isFlow = conn.IsFlow;
                    if (conn.LiquidColor.HasValue) cLiquid = conn.LiquidColor.Value.BrightnessTransmit(-0.6F);
                }

                p.IsStroke = false;
                p.Color = IsFlow ? cLiquid : cEmpty;
                canvas.DrawCircle(cx, cy, radius_fan, p);
                #endregion

                #region Direction
                {
                    var dir = Reverse ? -1f : 1f;
                    var pOutlet = new SKPoint(cx + ((radius + FrameSize) * dir), cy - radius + (PipeSize / 2f));
                    var pInlet = new SKPoint(cx - ((radius + FrameSize) * dir), cy + radius - (PipeSize / 2f));

                    var c = IsFlow ? cLiquid : cEmpty;

                    if (UseInlet)
                    {
                        var vrt = MathTool.MakeRectangle(pInlet, PipeSize, PipeSize);
                        vrt.Offset((PipeSize / 4F) * dir, 1);
                        Util.DrawIcon(canvas, Reverse ? "fa-caret-left" : "fa-caret-right", PipeSize / 2F, vrt, c);
                    }

                    {
                        var vrt = MathTool.MakeRectangle(pOutlet, PipeSize, PipeSize);
                        vrt.Offset(-(PipeSize / 3F) * dir, -1);
                        Util.DrawIcon(canvas, Reverse ? "fa-caret-left" : "fa-caret-right", PipeSize / 2F, vrt, c);
                    }
                }
                #endregion

                #region Fan
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.RotateDegrees(Convert.ToSingle(rofan));
                    p.IsStroke = false;
                    p.Color = cFan;
                    canvas.DrawPath(pathFan, p);
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
            pathFan.Dispose();
            base.OnDispose();
        }
        #endregion

        #region IoPort
        /// <summary>
        /// 모든 입출력 포트를 반환합니다.
        /// </summary>
        /// <returns>토출구 및 흡입구(사용 시) 포트</returns>
        public override IEnumerable<ConnectPort> IoPorts()
        {
            if (UseInlet)
                return [portOutlet, portInlet];
            else
                return [portOutlet];
        }

        /// <summary>
        /// 지정된 이름의 포트를 반환합니다.
        /// </summary>
        /// <param name="name">포트 이름</param>
        /// <returns>해당 포트 또는 null</returns>
        public override ConnectPort? GetPort(string? name)
        {
            if (portOutlet.Name == name) return portOutlet;
            if (portInlet.Name == name && UseInlet) return portInlet;
            return null;
        }
        #endregion

        #region PortPosition
        void SetPortPosition()
        {
            var brt = Bounds;
            float ps = PipeSize;
            float fs = FrameSize;
            float radius = ps * 1.5f;

            float cx = brt.MidX;
            float cy = brt.MidY;

            // Reverse 방향 계산
            float dir = Reverse ? -1f : 1f;

            // Deg0 기준 논리적 좌표
            // Outlet: 상단 토출구 끝단 중앙
            SKPoint pOutlet = new SKPoint(
                cx + ((radius + fs) * dir),
                cy - radius + (ps / 2f)
            );

            // Inlet: 하단 흡입구 끝단 중앙
            SKPoint pInlet = new SKPoint(
                cx - ((radius + fs) * dir),
                cy + radius - (ps / 2f)
            );

            // Deg0 기준 방향
            PortDirection outletDir = Reverse ? PortDirection.L : PortDirection.R;
            PortDirection inletDir = Reverse ? PortDirection.R : PortDirection.L;

            // Rotate에 따른 실제 좌표 및 방향 계산
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
                portOutlet.Position = RotatePoint(pOutlet, center, angle);
                portOutlet.Direction = RotateDirection(outletDir, Rotate);

                if (UseInlet)
                {
                    portInlet.Position = RotatePoint(pInlet, center, angle);
                    portInlet.Direction = RotateDirection(inletDir, Rotate);
                }
            }
            else
            {
                portOutlet.Position = pOutlet;
                portOutlet.Direction = outletDir;

                if (UseInlet)
                {
                    portInlet.Position = pInlet;
                    portInlet.Direction = inletDir;
                }
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

            return new SKPoint(
                cos * dx - sin * dy + center.X,
                sin * dx + cos * dy + center.Y
            );
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
