using Going.UI.Containers;
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
using System.Text.Json.Serialization;
using Timer = System.Timers.Timer;

namespace Going.UI.FlowSystem
{
    /// <summary>
    /// 배관 네트워크의 전체 관리자 역할을 하는 FlowSystem의 핵심 패널 컨트롤.
    /// <para>FsFlowObject 파생 컴포넌트들을 <see cref="Childrens"/>에 배치하고,
    /// <see cref="Connections"/>에 <see cref="FlowConnection"/>을 정의하여 배관 연결 네트워크를 구성한다.</para>
    ///
    /// <para><b>주요 사용 시나리오:</b></para>
    /// <list type="bullet">
    ///   <item>공정 배관 다이어그램(P&amp;ID) 시각화 및 실시간 흐름 모니터링</item>
    ///   <item>펌프, 밸브, 탱크, 파이프 분기를 포함한 유체 배관 네트워크 구성</item>
    ///   <item>유체 흐름 애니메이션을 통한 운전 상태 시각적 확인</item>
    /// </list>
    ///
    /// <para><b>프로퍼티 설정 가이드:</b></para>
    /// <list type="bullet">
    ///   <item><see cref="BaseWidth"/>/<see cref="BaseHeight"/>: 패널의 논리적 크기를 설정한다. null이면 실제 컨트롤 크기를 사용하며, 값을 지정하면 자동 스케일링된다.</item>
    ///   <item><see cref="PanelAlignment"/>: 스케일링 시 콘텐츠 정렬 위치를 지정한다.</item>
    ///   <item><see cref="PipeSize"/>: 배관의 굵기(픽셀)를 설정한다. 기본값 30.</item>
    ///   <item><see cref="FrameSize"/>: 배관 테두리 두께를 설정한다. 기본값 5.</item>
    ///   <item><see cref="EmptyColor"/>: 유체가 흐르지 않는 빈 배관의 색상. 기본값 "Base1".</item>
    ///   <item><see cref="MixedColor"/>: 서로 다른 유체가 혼합될 때 표시되는 색상. 기본값 "Magenta".</item>
    ///   <item><see cref="FrameColor"/>: 배관 외곽선 색상. 기본값 "Base4".</item>
    /// </list>
    ///
    /// <para><b>FlowConnection 사용법:</b></para>
    /// <para>FlowConnection은 두 컴포넌트의 포트를 연결하는 배관을 정의한다.</para>
    /// <list type="bullet">
    ///   <item><c>StartControlId</c>/<c>EndControlId</c>: 연결할 컴포넌트의 Id (Guid).</item>
    ///   <item><c>StartPortName</c>/<c>EndPortName</c>: 각 컴포넌트에서 연결할 포트 이름 (예: "Inlet", "Outlet", "Port1").</item>
    ///   <item><c>Nodes</c>: 중간 경유점 목록. PipeNode의 Direction(L/R/T/B)과 Position(픽셀 좌표)으로 배관 경로를 꺾는다.</item>
    /// </list>
    ///
    /// <para><b>RefreshCache() 호출 시점:</b></para>
    /// <para>다음 상황에서 반드시 <see cref="RefreshCache"/>를 호출해야 한다:</para>
    /// <list type="bullet">
    ///   <item>Childrens에 컴포넌트를 추가/제거한 후</item>
    ///   <item>Connections에 연결을 추가/제거한 후</item>
    ///   <item>컴포넌트의 Id가 변경된 후</item>
    /// </list>
    /// <para>OnInit 시 자동으로 호출되므로, 초기 구성 후에는 런타임 변경 시에만 수동 호출이 필요하다.</para>
    ///
    /// <para><b>흐름 전파 알고리즘:</b></para>
    /// <para>FlowProcess는 매 Draw 사이클마다 실행되며, 다음 순서로 동작한다:</para>
    /// <list type="number">
    ///   <item>모든 Connection의 흐름 상태를 초기화한다.</item>
    ///   <item>시작점(FsCylinderTank의 UseOutlet, FsSiloTank, FsPump의 Outlet)에서 출발한다.</item>
    ///   <item>각 시작점의 IsFlow 상태와 유체 색상을 가지고 재귀적으로 연결된 포트를 따라 전파한다.</item>
    ///   <item>전파 경로상의 모든 컴포넌트가 IsFlow=true여야 흐름이 계속 전달된다.</item>
    ///   <item>FsTeePipe/FsCrossPipe에서 분기 시, 입력된 유체 색상이 다르면 MixedColor로 표시된다.</item>
    /// </list>
    ///
    /// <para><b>버블 애니메이션:</b></para>
    /// <para>내부 10ms 간격 타이머가 각 FlowConnection의 Tick()을 호출하여 버블 애니메이션을 구동한다.
    /// IsFlow가 true인 연결에서 300ms마다 새 버블이 생성되며, 배관 경로를 따라 이동하는 흰색 반투명 원으로 시각화된다.</para>
    /// </summary>
    /// <remarks>
    /// 사용 예:
    /// <code>
    /// // 1. 패널 생성
    /// var panel = new FsFlowSystemPanel
    /// {
    ///     BaseWidth = 800,
    ///     BaseHeight = 600,
    ///     PipeSize = 30,
    ///     FrameSize = 5
    /// };
    ///
    /// // 2. 컴포넌트 배치
    /// var tank = new FsCylinderTank { X = 50, Y = 100, UseOutlet = true };
    /// var pump = new FsPump { X = 250, Y = 100, OnOff = true };
    /// var tee = new FsTeePipe { X = 450, Y = 100 };
    /// panel.Childrens.Add(tank);
    /// panel.Childrens.Add(pump);
    /// panel.Childrens.Add(tee);
    ///
    /// // 3. 배관 연결 정의
    /// panel.Connections.Add(new FlowConnection
    /// {
    ///     StartControlId = tank.Id, StartPortName = "Outlet",
    ///     EndControlId = pump.Id, EndPortName = "Inlet"
    /// });
    /// panel.Connections.Add(new FlowConnection
    /// {
    ///     StartControlId = pump.Id, StartPortName = "Outlet",
    ///     EndControlId = tee.Id, EndPortName = "Port1",
    ///     Nodes = { new PipeNode { Direction = PortDirection.R, Position = 400 } }
    /// });
    ///
    /// // 4. 캐시 갱신
    /// panel.RefreshCache();
    /// </code>
    /// </remarks>
    public class FsFlowSystemPanel : GoContainer
    {
        #region Properties
        /// <summary>
        /// 자식 컨트롤 컬렉션을 가져옵니다.
        /// </summary>
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        /// <summary>
        /// 기본 너비를 가져오거나 설정합니다. null이면 실제 너비를 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public int? BaseWidth { get; set; }

        /// <summary>
        /// 기본 높이를 가져오거나 설정합니다. null이면 실제 높이를 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public int? BaseHeight { get; set; }

        /// <summary>
        /// 패널 내 콘텐츠 정렬 방식을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public GoContentAlignment PanelAlignment { get; set; } = GoContentAlignment.MiddleCenter;

        /// <summary>
        /// 파이프의 두께를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public float PipeSize { get; set; } = 30;

        /// <summary>
        /// 파이프 테두리의 두께를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public float FrameSize { get; set; } = 5;

        /// <summary>
        /// 비어 있는 파이프의 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public string EmptyColor { get; set; } = "Base1";

        /// <summary>
        /// 혼합 유체의 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public string MixedColor { get; set; } = "Magenta";

        /// <summary>
        /// 파이프 테두리의 색상을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public string FrameColor { get; set; } = "Base4";

        /// <summary>
        /// 플로우 연결 목록을 가져오거나 설정합니다.
        /// </summary>
        public List<FlowConnection> Connections { get; set; } = [];

        /// <summary>
        /// 자식 컨트롤이 배치되는 패널 영역을 가져옵니다.
        /// 디자인 모드에서는 Editor 영역, 런타임에서는 Scale 영역을 반환합니다.
        /// </summary>
        [JsonIgnore] public override SKRect PanelBounds => Design?.DesignMode ?? false ? Areas()["Editor"] : Areas()["Scale"];
        #endregion

        #region Member Variable
        SKPath path = new SKPath();

        Dictionary<Guid, FsFlowObject> ctrls = [];
        Dictionary<string, FlowConnection> connsPort = [];
        Dictionary<Guid, List<FlowConnection>> connsCtrl = [];
        IEnumerable<ConnectPort> startPorts = [];

        Timer tmr;
        #endregion

        #region Constructor
        /// <summary>
        /// JSON 역직렬화를 위한 생성자입니다.
        /// </summary>
        /// <param name="childrens">자식 컨트롤 목록</param>
        [JsonConstructor]
        public FsFlowSystemPanel(List<IGoControl> childrens) : this() => Childrens = childrens;

        /// <summary>
        /// FsFlowSystemPanel의 새 인스턴스를 초기화합니다.
        /// </summary>
        public FsFlowSystemPanel()
        {
            tmr = new Timer { Enabled = true, Interval = 10 };
            tmr.Elapsed += (o, s) =>
            {
                try
                {
                    foreach (var c in Connections.ToArray()) c.Tick();
                }
                catch { }
            };
        }
        #endregion

        #region Override
        #region Init / Dispose
        /// <inheritdoc/>
        protected override void OnInit(GoDesign? design)
        {
            RefreshCache();

            base.OnInit(design);
        }

        /// <inheritdoc/>
        protected override void OnDispose()
        {
            tmr.Enabled = false;

            path.Dispose();
            tmr.Dispose();
            base.OnDispose();
        }
        #endregion

        #region Update / Draw
        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];
            var rtEditor = rts["Editor"];

            if (Design?.DesignMode ?? false)
            {
                var rt = rtEditor;
                rt.Inflate(-0.5F, -0.5F);
                rt.Offset(0.5F, 0.5F);
                using var pe = SKPathEffect.CreateDash([1, 2], 2);
                using var p = new SKPaint { };
                p.IsAntialias = false;
                p.IsStroke = true; p.StrokeWidth = 1; p.Color = thm.Base3;
                p.PathEffect = pe;
                canvas.DrawRect(rt, p);

                using (new SKAutoCanvasRestore(canvas))
                {
                    Draw(canvas, thm);

                    base.OnDraw(canvas, thm);
                }
            }
            else
            {
                float widthRatio = rtContent.Width / rtScale.Width;
                float heightRatio = rtContent.Height / rtScale.Height;
                float scale = Math.Min(widthRatio, heightRatio);

                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.Scale(scale);

                    Draw(canvas, thm);

                    base.OnDraw(canvas, thm);
                }
            }
        }
        #endregion

        #region Areas
        /// <summary>
        /// 패널의 영역 딕셔너리를 반환합니다.
        /// Content, Scale, Editor 영역을 포함합니다.
        /// </summary>
        /// <returns>영역 이름과 사각형의 딕셔너리</returns>
        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var (iw, ih) = GetScaledSize();

            var rtContent = rts["Content"];

            float widthRatio = rtContent.Width / iw;
            float heightRatio = rtContent.Height / ih;
            float scale = Math.Min(widthRatio, heightRatio);
            var w = Width / scale;
            var h = Height / scale;

            rts["Scale"] = MathTool.MakeRectangle(Util.FromRect(0, 0, w, h), new SKSize(iw, ih), PanelAlignment);
            rts["Editor"] = MathTool.MakeRectangle(rtContent, new SKSize(BaseWidth ?? Width, BaseHeight ?? Height), GoContentAlignment.MiddleCenter);

            return rts;
        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            if (Design?.DesignMode ?? false)
            {
            }
            else
            {
                x /= scale;
                y /= scale;

                base.OnMouseDown(x + ViewPosition.X, y + ViewPosition.Y, button);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            if (Design?.DesignMode ?? false)
            {
            }
            else
            {
                x /= scale;
                y /= scale;
                base.OnMouseMove(x + ViewPosition.X, y + ViewPosition.Y);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            if (Design?.DesignMode ?? false)
            {
            }
            else
            {
                x /= scale;
                y /= scale;
                base.OnMouseUp(x + ViewPosition.X, y + ViewPosition.Y, button);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            if (Design?.DesignMode ?? false)
            {
            }
            else
            {
                x /= scale;
                y /= scale;

                base.OnMouseClick(x + ViewPosition.X, y + ViewPosition.Y, button);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            if (Design?.DesignMode ?? false)
            {
            }
            else
            {
                x /= scale;
                y /= scale;

                base.OnMouseDoubleClick(x + ViewPosition.X, y + ViewPosition.Y, button);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            if (Design?.DesignMode ?? false)
            {
            }
            else
            {
                x /= scale;
                y /= scale;

                base.OnMouseLongClick(x + ViewPosition.X, y + ViewPosition.Y, button);
            }
        }
        #endregion
        #endregion

        #region Method
        #region Draw
        void Draw(SKCanvas canvas, GoTheme thm)
        {
            foreach (var v in Childrens) v.FireUpdate();
            foreach (var v in Connections) v.Update(this);

            FlowProcess(thm);

            var cEmpty = thm.ToColor(EmptyColor);
            var cMix = thm.ToColor(MixedColor);
            var cFrame = thm.ToColor(FrameColor);
            using var p = new SKPaint { IsAntialias = true };

            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.Translate(PanelBounds.Left, PanelBounds.Top);

                foreach (var c in Connections) c.Draw(canvas, thm, path, cFrame, cEmpty, PipeSize, FrameSize);
            }
        }
        #endregion

        #region Tools
        #region RefreshCache
        /// <summary>
        /// 내부 캐시를 갱신합니다.
        /// 자식 컨트롤 및 연결 정보를 재구성하고 시작 포트 목록을 재계산합니다.
        /// </summary>
        public void RefreshCache()
        {
            #region ctrls
            ctrls = Childrens.Where(x => x is FsFlowObject).ToDictionary(x => x.Id, y => (FsFlowObject)y);
            #endregion

            #region conns
            Dictionary<string, FlowConnection> conns_p = [];
            Dictionary<Guid, List<FlowConnection>> conns_c = [];
            foreach (var c in Connections)
            {
                #region conns_p
                var ks = $"{c.StartControlId}:{c.StartPortName}";
                var ke = $"{c.EndControlId}:{c.EndPortName}";
                if (!conns_p.ContainsKey(ks)) conns_p.Add(ks, c);
                if (!conns_p.ContainsKey(ke)) conns_p.Add(ke, c);
                #endregion
                #region conns_c
                if (c.StartControlId.HasValue)
                {
                    if (!conns_c.ContainsKey(c.StartControlId.Value)) conns_c.Add(c.StartControlId.Value, []);
                    conns_c[c.StartControlId.Value].Add(c);
                }

                if (c.EndControlId.HasValue)
                {
                    if (!conns_c.ContainsKey(c.EndControlId.Value)) conns_c.Add(c.EndControlId.Value, []);
                    conns_c[c.EndControlId.Value].Add(c);
                }
                #endregion
            }
            connsPort = conns_p;
            connsCtrl = conns_c;
            #endregion

            #region startPorts
            var lsc = ctrls.Values.Where(x => (x is FsCylinderTank ct && ct.UseOutlet) ||
                                          (x is FsSiloTank st) ||
                                          (x is FsPump pp && !pp.UseInlet)).ToList();


            startPorts = lsc.Select(x => x.GetPort("Outlet")).Where(x => x != null).Select(x => x!);
            #endregion
        }
        #endregion

        #region GetControl
        /// <summary>
        /// 지정된 ID의 플로우 객체를 가져옵니다.
        /// </summary>
        /// <param name="id">컨트롤 ID (null 허용)</param>
        /// <returns>해당 ID의 플로우 객체, 없으면 null</returns>
        public FsFlowObject? GetControl(Guid? id)
        {
            if (id.HasValue && ctrls.TryGetValue(id.Value, out var c)) return c;
            else return null;
        }
        #endregion

        #region GetConnection
        /// <summary>
        /// 지정된 컨트롤 ID와 포트 이름에 해당하는 연결을 가져옵니다.
        /// </summary>
        /// <param name="controlid">컨트롤 ID</param>
        /// <param name="portName">포트 이름</param>
        /// <returns>해당 연결, 없으면 null</returns>
        public FlowConnection? GetConnection(Guid controlid, string portName) =>
            connsPort.TryGetValue($"{controlid}:{portName}", out var c) ? c : null;
        #endregion

        #region GetConnections
        /// <summary>
        /// 지정된 컨트롤 ID에 연결된 모든 연결을 가져옵니다.
        /// </summary>
        /// <param name="controlid">컨트롤 ID</param>
        /// <returns>해당 컨트롤의 연결 목록</returns>
        public IEnumerable<FlowConnection> GetConnections(Guid controlid) =>
            connsCtrl.TryGetValue(controlid, out var ls) ? ls : [];
        #endregion

        #region HasConnection
        /// <summary>
        /// 지정된 컨트롤 ID와 포트 이름에 연결이 존재하는지 확인합니다.
        /// </summary>
        /// <param name="controlid">컨트롤 ID</param>
        /// <param name="portName">포트 이름</param>
        /// <returns>연결이 존재하면 true</returns>
        public bool HasConnection(Guid controlid, string portName) => connsPort.ContainsKey($"{controlid}:{portName}");
        #endregion

        #region GetScaledSize
        (float width, float height) GetScaledSize()
        {
            if (!BaseWidth.HasValue && !BaseHeight.HasValue)
                return (Width, Height);

            if (BaseWidth.HasValue && BaseHeight.HasValue)
            {
                return (BaseWidth.Value, BaseHeight.Value);
            }

            float aspectRatio = (float)Width / Height;

            if (BaseWidth.HasValue)
            {
                float scaledWidth = BaseWidth.Value;
                float scaledHeight = scaledWidth / aspectRatio;
                return (scaledWidth, scaledHeight);
            }
            else
            {
                float scaledHeight = BaseHeight!.Value;
                float scaledWidth = scaledHeight * aspectRatio;
                return (scaledWidth, scaledHeight);
            }
        }
        #endregion

        #region Flow
        void FlowProcess(GoTheme thm)
        {
            var cMix = thm.ToColor(MixedColor);

            #region Init
            foreach (var v in Connections)
            {
                v.IsFlow = false;
                v.LiquidColor = null;
                if (v.StartPort != null) v.StartPort.State = PortFlow.None;
                if (v.EndPort != null) v.EndPort.State = PortFlow.None;
            }
            #endregion

            foreach (var v in startPorts)
            {
                if (v != null)
                {
                    SKColor cLiquid = SKColors.DeepSkyBlue;
                    if (v.Control is FsSiloTank t1) cLiquid = thm.ToColor(t1.FillColor);
                    else if (v.Control is FsCylinderTank t2) cLiquid = thm.ToColor(t2.FillColor);
                    else if (v.Control is FsPump t3) cLiquid = thm.ToColor("DeepSkyBlue");

                    var pi = new PropagationInfo
                    {
                        IsFlow = v.Control?.IsFlow ?? false,
                        LiquidColor = cLiquid,
                        MixedColor = cMix,
                    };
                    propagation(v, pi);
                }
            }
        }

        void propagation(ConnectPort start, PropagationInfo pi)
        {
            if (start.Control != null)
            {
                var conn = GetConnection(start.Control.Id, start.Name);
                if (conn != null)
                {
                    #region end pick
                    ConnectPort? end = null;
                    if (conn.StartPort == start && conn.EndPort != null) end = conn.EndPort;
                    else if (conn.EndPort == start && conn.StartPort != null) end = conn.StartPort;
                    #endregion

                    #region Set
                    start.State = PortFlow.Output;

                    pi.IsFlow &= start.Control.IsFlow;

                    conn.IsFlow |= pi.IsFlow;
                    if (pi.IsFlow)
                    {
                        if (start.Control is FsTeePipe || start.Control is FsCrossPipe)
                        {
                            var ps = start.Control.IoPorts();
                            var cs = ps.Select(x => GetConnection(start.Control.Id, x.Name)?.LiquidColor).Where(x => x.HasValue)
                                       .Select(x => x!.Value)
                                       .Distinct().ToList();

                            if (cs.Count() == 1) conn.LiquidColor = cs.First();
                            else if (cs.Count() > 1) conn.LiquidColor = pi.MixedColor;
                        }
                        else conn.LiquidColor = pi.LiquidColor;
                    }
                    #endregion

                    if (end != null && end.Control != null && pi.IsFlow && end.Control.IsFlow && (end.Type == PortType.Input || end.Type == PortType.Bidirectional))
                    {
                        var nexts = GetConnections(end.Control.Id);
                        foreach (var nd in nexts.Where(x => x.StartPort != end && x.EndPort != end))
                        {
                            if (end.Control is FsTeePipe || end.Control is FsCrossPipe)
                            {
                                end.State = PortFlow.Input;

                                bool sb = nd.StartPort != null && (nd.StartPort.State == PortFlow.None || nd.StartPort.State == PortFlow.Output);
                                bool eb = nd.EndPort != null && (nd.EndPort.State == PortFlow.None || nd.EndPort.State == PortFlow.Output);

                                if (nd.StartControlId == end.Control.Id && nd.StartPort != null && eb) propagation(nd.StartPort, pi);
                                else if (nd.EndControlId == end.Control.Id && nd.EndPort != null && eb) propagation(nd.EndPort, pi);
                            }
                            else if (end.Control is FsCylinderTank || end.Control is FsSiloTank) { }
                            else
                            {
                                end.State = PortFlow.Input;

                                if (nd.StartControlId == end.Control.Id && nd.StartPort != null) propagation(nd.StartPort, pi);
                                else if (nd.EndControlId == end.Control.Id && nd.EndPort != null) propagation(nd.EndPort, pi);
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #endregion
        #endregion
    }

    #region FlowConnection
    /// <summary>
    /// 두 플로우 객체 간의 파이프 연결을 나타내는 클래스입니다.
    /// 연결 경로, 유체 흐름 시각화, 버블 애니메이션 등을 관리합니다.
    /// </summary>
    public class FlowConnection
    {
        #region Properties
        /// <summary>
        /// 연결의 고유 식별자를 가져오거나 설정합니다.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 시작 컨트롤의 ID를 가져오거나 설정합니다.
        /// </summary>
        public Guid? StartControlId { get; set; }

        /// <summary>
        /// 끝 컨트롤의 ID를 가져오거나 설정합니다.
        /// </summary>
        public Guid? EndControlId { get; set; }

        /// <summary>
        /// 시작 포트의 이름을 가져오거나 설정합니다.
        /// </summary>
        public string? StartPortName { get; set; }

        /// <summary>
        /// 끝 포트의 이름을 가져오거나 설정합니다.
        /// </summary>
        public string? EndPortName { get; set; }

        /// <summary>
        /// 중간 파이프 노드 목록을 가져오거나 설정합니다.
        /// </summary>
        public List<PipeNode> Nodes { get; set; } = [];

        /// <summary>
        /// 시작 포트를 가져오거나 설정합니다.
        /// </summary>
        [JsonIgnore] internal ConnectPort? StartPort { get; set; }

        /// <summary>
        /// 끝 포트를 가져오거나 설정합니다.
        /// </summary>
        [JsonIgnore] internal ConnectPort? EndPort { get; set; }

        /// <summary>
        /// 계산된 연결 경로 점 목록을 가져오거나 설정합니다.
        /// </summary>
        [JsonIgnore] public List<SKPoint> Points { get; set; } = [];

        /// <summary>
        /// 스무딩 처리된 그리기용 점 목록을 가져오거나 설정합니다.
        /// </summary>
        [JsonIgnore] public List<SKPoint> DrawingPoints { get; set; } = [];

        /// <summary>
        /// 유체가 흐르고 있는지 여부를 가져오거나 설정합니다.
        /// </summary>
        [JsonIgnore] internal bool IsFlow { get; set; } = false;

        /// <summary>
        /// 유체의 색상을 가져오거나 설정합니다.
        /// </summary>
        [JsonIgnore] internal SKColor? LiquidColor { get; set; }
        #endregion

        #region Member Variable
        float pl, pt, pr, pb;
        double dist = 0;

        List<Bubble> bubbles = [];
        DateTime tm;
        #endregion

        #region Method
        #region Update
        /// <summary>
        /// 연결 경로를 업데이트합니다. 패널 위치가 변경되었거나 디자인 모드일 때 경로를 재계산합니다.
        /// </summary>
        /// <param name="pnl">소속 패널</param>
        public void Update(FsFlowSystemPanel pnl)
        {
            var changed = false;
            changed |= pl != pnl.Left || pt != pnl.Top || pr != pnl.Right || pb != pnl.Bottom;

            if (changed || (pnl.Design?.DesignMode ?? false))
            {
                var cs = pnl.GetControl(StartControlId);
                var ce = pnl.GetControl(EndControlId);
                StartPort = cs?.GetPort(StartPortName); if (StartPort != null) StartPort.Control = cs;
                EndPort = ce?.GetPort(EndPortName); if (EndPort != null) EndPort.Control = ce;

                if (StartPort != null && EndPort != null)
                {
                    Points = PipeTool.Lines(StartPort, Nodes, EndPort);
                    DrawingPoints = PipeTool.Smooth(StartPort, EndPort, Points);
                    dist = PipeTool.TotalDistance(DrawingPoints);
                }
            }

            pl = pnl.Left; pt = pnl.Top; pr = pnl.Right; pb = pnl.Bottom;
        }
        #endregion

        #region Collision
        /// <summary>
        /// 지정된 좌표가 이 연결 경로와 충돌하는지 확인합니다.
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <param name="pipeSize">파이프 크기</param>
        /// <returns>충돌하면 true</returns>
        public bool Collision(float x, float y, float pipeSize)
        {
            bool ret = false;
            for (int i = 0; i < Points.Count - 1; i++)
                ret |= CollisionTool.CheckLine(Points[i], Points[i + 1], new SKPoint(x, y), pipeSize);

            return ret;
        }
        #endregion

        #region Draw
        /// <summary>
        /// 연결 파이프를 캔버스에 그립니다.
        /// </summary>
        /// <param name="canvas">그리기 대상 캔버스</param>
        /// <param name="thm">현재 테마</param>
        /// <param name="path">재사용할 SKPath 인스턴스</param>
        /// <param name="cFrame">테두리 색상</param>
        /// <param name="cEmpty">비어있는 파이프 색상</param>
        /// <param name="pipeSize">파이프 크기</param>
        /// <param name="frameSize">테두리 크기</param>
        public void Draw(SKCanvas canvas, GoTheme thm, SKPath path, SKColor cFrame, SKColor cEmpty, float pipeSize, float frameSize)
        {
            if (DrawingPoints.Count > 0)
            {
                path.Reset();

                path.AddPoly([.. DrawingPoints], false);

                using var p = new SKPaint { IsAntialias = true };

                p.IsStroke = true;
                p.StrokeWidth = pipeSize;
                p.StrokeJoin = SKStrokeJoin.Bevel;
                p.Color = cFrame;
                canvas.DrawPath(path, p);

                p.StrokeWidth = pipeSize - (frameSize * 2);
                p.Color = IsFlow && LiquidColor.HasValue ? LiquidColor.Value : cEmpty;
                canvas.DrawPath(path, p);

                if (IsFlow)
                {
                    try
                    {
                        var ls = bubbles.ToArray();
                        foreach (var bubble in ls)
                        {
                            var r = bubble.Position / bubble.Total;
                            if (EndPort != null && EndPort.State == PortFlow.Output) r = 1.0 - r;
                            var pt = PipeTool.Location(DrawingPoints, r);

                            p.IsStroke = false;
                            p.Color = SKColors.White.WithAlpha(120);
                            canvas.DrawCircle(pt, 5, p);
                        }
                    }
                    catch { }
                }
            }
        }
        #endregion

        #region Tick
        /// <summary>
        /// 버블 애니메이션을 한 프레임 진행합니다. 타이머에서 주기적으로 호출됩니다.
        /// </summary>
        public void Tick()
        {
            if (IsFlow)
            {
                var now = DateTime.Now;
                if ((now - tm).TotalMilliseconds >= 300)
                {
                    tm = now;
                    bubbles.Add(new Bubble { Total = dist });
                }

                for (int i = bubbles.Count - 1; i >= 0; i--)
                {
                    bubbles[i].Position += 5;
                    if (bubbles[i].Complete) bubbles.RemoveAt(i);
                }
            }
        }
        #endregion

        #region GetAnchors
        /// <summary>
        /// 이 연결의 앵커 목록을 반환합니다.
        /// </summary>
        /// <returns>플로우 앵커 목록</returns>
        public List<FlowAnchor> GetAnchors()
        {
            List<FlowAnchor> ret = [];

            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                ret.Add(new FlowAnchor(this, i, i + 1));
            }

            return ret;
        }
        #endregion
        #endregion
    }
    #endregion

    #region FlowAnchor
    /// <summary>
    /// 플로우 연결의 앵커 포인트를 나타내는 클래스입니다.
    /// 두 파이프 노드 사이의 중간 지점을 제공합니다.
    /// </summary>
    /// <param name="conn">소속 연결</param>
    /// <param name="sidx">시작 노드 인덱스</param>
    /// <param name="eidx">끝 노드 인덱스</param>
    public class FlowAnchor(FlowConnection conn, int sidx, int eidx)
    {
        /// <summary>
        /// 시작 노드의 인덱스를 가져옵니다.
        /// </summary>
        public int Index => sidx;

        /// <summary>
        /// 시작 파이프 노드를 가져옵니다.
        /// </summary>
        public PipeNode Start => conn.Nodes[sidx];

        /// <summary>
        /// 끝 파이프 노드를 가져옵니다.
        /// </summary>
        public PipeNode End => conn.Nodes[eidx];

        /// <summary>
        /// 앵커의 중간 위치를 가져옵니다.
        /// </summary>
        public SKPoint Position => MathTool.CenterPoint(conn.Points[sidx + 1], conn.Points[eidx + 1]);

        /// <summary>
        /// 앵커의 방향(수직/수평)을 가져옵니다.
        /// </summary>
        public GoDirectionHV Direction => Start.Direction == PortDirection.T || Start.Direction == PortDirection.B ? GoDirectionHV.Vertical : GoDirectionHV.Horizon;
    }
    #endregion

    #region Bubble
    /// <summary>
    /// 파이프 내 흐름 시각화를 위한 버블 클래스입니다.
    /// </summary>
    class Bubble
    {
        /// <summary>
        /// 버블의 현재 위치를 가져오거나 설정합니다.
        /// </summary>
        public double Position { get; set; }

        /// <summary>
        /// 파이프의 전체 길이를 가져오거나 설정합니다.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// 버블이 파이프 끝에 도달했는지 여부를 가져옵니다.
        /// </summary>
        public bool Complete => Total <= Position;
    }
    #endregion

    #region PropagationInfo
    /// <summary>
    /// 유체 흐름 전파 정보를 담는 내부 클래스입니다.
    /// </summary>
    internal class PropagationInfo
    {
        /// <summary>
        /// 유체의 색상을 가져오거나 설정합니다.
        /// </summary>
        internal SKColor LiquidColor { get; set; }

        /// <summary>
        /// 혼합 유체의 색상을 가져오거나 설정합니다.
        /// </summary>
        internal SKColor MixedColor { get; set; }

        /// <summary>
        /// 흐름 여부를 가져오거나 설정합니다.
        /// </summary>
        internal bool IsFlow { get; set; } = false;
    }
    #endregion
}
