using Going.UI.Bindings;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Going.UI.Controls
{
    /// <summary>
    /// 프로퍼티 카테고리 상수를 정의하는 클래스
    /// </summary>
    public class PCategory
    {
        /// <summary>ID 카테고리</summary>
        public const string ID = "ID";
        /// <summary>기본 카테고리</summary>
        public const string Basic = "Basic";
        /// <summary>위치/크기 카테고리</summary>
        public const string Bounds = "Bounds";
        /// <summary>컨트롤 카테고리</summary>
        public const string Control = "Control";

        /// <summary>카테고리 이름 목록</summary>
        public static List<string> Names = [ID, Basic, Bounds, Control];
        /// <summary>
        /// 카테고리 이름으로 인덱스를 반환합니다.
        /// </summary>
        /// <param name="category">카테고리 이름</param>
        /// <returns>카테고리 인덱스</returns>
        public static int Index(string category) => Names.IndexOf(category);
    }

    /// <summary>
    /// 디자인 편집기에서 프로퍼티의 카테고리와 정렬 순서를 지정하는 어트리뷰트
    /// </summary>
    /// <param name="category">프로퍼티 카테고리</param>
    /// <param name="order">카테고리 내 정렬 순서</param>
    public class GoPropertyAttribute(string category, int order) : Attribute
    {
        /// <summary>프로퍼티 카테고리</summary>
        public string Category { get; set; } = category;
        /// <summary>카테고리 내 정렬 순서</summary>
        public int Order { get; set; } = order;
    }

    /// <summary>
    /// 크기 프로퍼티를 나타내는 어트리뷰트
    /// </summary>
    /// <param name="category">프로퍼티 카테고리</param>
    /// <param name="order">카테고리 내 정렬 순서</param>
    public class GoSizePropertyAttribute(string category, int order) : GoPropertyAttribute(category, order) { }
    /// <summary>
    /// 복수 크기 프로퍼티를 나타내는 어트리뷰트
    /// </summary>
    /// <param name="category">프로퍼티 카테고리</param>
    /// <param name="order">카테고리 내 정렬 순서</param>
    public class GoSizesPropertyAttribute(string category, int order) : GoPropertyAttribute(category, order) { }
    /// <summary>
    /// 이미지 프로퍼티를 나타내는 어트리뷰트
    /// </summary>
    /// <param name="category">프로퍼티 카테고리</param>
    /// <param name="order">카테고리 내 정렬 순서</param>
    public class GoImagePropertyAttribute(string category, int order) : GoPropertyAttribute(category, order) { }
    /// <summary>
    /// 글꼴 이름 프로퍼티를 나타내는 어트리뷰트
    /// </summary>
    /// <param name="category">프로퍼티 카테고리</param>
    /// <param name="order">카테고리 내 정렬 순서</param>
    public class GoFontNamePropertyAttribute(string category, int order) : GoPropertyAttribute(category, order) { }
    /// <summary>
    /// 여러 줄 텍스트 프로퍼티를 나타내는 어트리뷰트
    /// </summary>
    /// <param name="category">프로퍼티 카테고리</param>
    /// <param name="order">카테고리 내 정렬 순서</param>
    public class GoMultiLinePropertyAttribute(string category, int order) : GoPropertyAttribute(category, order) { }

    /// <summary>
    /// 모든 Going.UI 컨트롤의 기본 클래스. 위치, 크기, 마우스/키보드 이벤트 처리 등 공통 기능을 제공합니다.
    /// </summary>
    public class GoControl : IGoControl, IDisposable
    {
        #region Properties
        /// <summary>전역 롱클릭 판정 시간 (밀리초)</summary>
        public static int GlobalLongClickTime { get; set; } = 1000;

        /// <summary>컨트롤의 고유 식별자</summary>
        public Guid Id { get; init; } = Guid.NewGuid();
        /// <summary>컨트롤 이름</summary>
        [GoProperty(PCategory.Basic, 0)] public string? Name { get; set; }
        /// <summary>컨트롤 표시 여부</summary>
        [GoProperty(PCategory.Basic, 1)] public virtual bool Visible { get; set; } = true;
        /// <summary>컨트롤 활성화 여부</summary>
        [GoProperty(PCategory.Basic, 2)] public virtual bool Enabled { get; set; } = true;
        /// <summary>롱클릭 사용 여부</summary>
        [GoProperty(PCategory.Basic, 3)] public bool UseLongClick { get; set; } = false;
        /// <summary>롱클릭 판정 시간 (밀리초). null이면 <see cref="GlobalLongClickTime"/>을 사용합니다.</summary>
        [GoProperty(PCategory.Basic, 4)] public int? LongClickTime { get; set; } = null;

        /// <summary>디자인 편집기에서 선택 가능 여부</summary>
        public bool Selectable { get; protected set; } = false;
        /// <summary>사용자 정의 데이터를 저장하는 태그</summary>
        [JsonIgnore] public object? Tag { get; set; }
        /// <summary>화면 기준 X 좌표</summary>
        [JsonIgnore] public float ScreenX => Parent != null && Parent is GoControl pc ? pc.ScreenX + Parent.PanelBounds.Left + X : X;
        /// <summary>화면 기준 Y 좌표</summary>
        [JsonIgnore] public float ScreenY => Parent != null && Parent is GoControl pc ? pc.ScreenY + Parent.PanelBounds.Top + Y : Y;

        /// <summary>컨트롤의 경계 영역</summary>
        [GoProperty(PCategory.Bounds, 0)] public SKRect Bounds { get => bounds; set => bounds = value; }
        /// <summary>컨트롤의 X 좌표 (왼쪽 위치)</summary>
        [GoProperty(PCategory.Bounds, 1), JsonIgnore]
        public float X
        {
            get => bounds.Left;
            set
            {
                var gx = value - bounds.Left;
                var rt = bounds;
                rt.Offset(gx, 0);
                bounds = rt;
            }
        }

        /// <summary>컨트롤의 Y 좌표 (위쪽 위치)</summary>
        [GoProperty(PCategory.Bounds, 2), JsonIgnore]
        public float Y
        {
            get => bounds.Top;
            set
            {
                var gy = value - bounds.Top;
                var rt = bounds;
                rt.Offset(0, gy);
                bounds = rt;
            }
        }

        /// <summary>컨트롤 너비</summary>
        [GoProperty(PCategory.Bounds, 3), JsonIgnore] public float Width { get => bounds.Width; set => bounds.Right = value + bounds.Left; }
        /// <summary>컨트롤 높이</summary>
        [GoProperty(PCategory.Bounds, 4), JsonIgnore] public float Height { get => bounds.Height; set => bounds.Bottom = value + bounds.Top; }
        /// <summary>컨트롤 왼쪽 좌표</summary>
        [GoProperty(PCategory.Bounds, 5), JsonIgnore] public float Left { get => bounds.Left; set => bounds.Left = value; }
        /// <summary>컨트롤 위쪽 좌표</summary>
        [GoProperty(PCategory.Bounds, 6), JsonIgnore] public float Top { get => bounds.Top; set => bounds.Top = value; }
        /// <summary>컨트롤 오른쪽 좌표</summary>
        [GoProperty(PCategory.Bounds, 7), JsonIgnore] public float Right { get => bounds.Right; set => bounds.Right = value; }
        /// <summary>컨트롤 아래쪽 좌표</summary>
        [GoProperty(PCategory.Bounds, 8), JsonIgnore] public float Bottom { get => bounds.Bottom; set => bounds.Bottom = value; }
        /// <summary>컨트롤의 도킹 스타일</summary>
        [GoProperty(PCategory.Bounds, 9)] public GoDockStyle Dock { get; set; } = GoDockStyle.None;
        /// <summary>컨트롤의 외부 여백</summary>
        [GoProperty(PCategory.Bounds, 10)] public GoPadding Margin { get; set; } = new(4, 4, 4, 4);

        /// <summary>첫 번째 렌더링 여부</summary>
        [JsonIgnore] public bool FirstRender { get; internal set; } = true;
        /// <summary>컨트롤 화면 표시 상태</summary>
        [JsonIgnore] public bool View { get; internal set; } = true;
        /// <summary>부모 컨테이너</summary>
        [JsonIgnore] public IGoContainer? Parent { get; internal set; }
        /// <summary>디자인 편집기 객체</summary>
        [JsonIgnore] public GoDesign? Design { get; internal set; }

        [JsonIgnore] internal bool _MouseDown_ => bDown;

        private Action? actInv;
        #endregion

        #region Event
        /// <summary>마우스 클릭 시 발생하는 이벤트</summary>
        public event EventHandler<GoMouseClickEventArgs>? MouseClicked;
        /// <summary>마우스 롱클릭 시 발생하는 이벤트</summary>
        public event EventHandler<GoMouseClickEventArgs>? MouseLongClicked;
        /// <summary>롱클릭이 취소되었을 때 발생하는 이벤트</summary>
        public event EventHandler<GoMouseClickEventArgs>? MouseLongClickCanceled;
        /// <summary>마우스 버튼을 눌렀을 때 발생하는 이벤트</summary>
        public event EventHandler<GoMouseClickEventArgs>? MouseDown;
        /// <summary>마우스 버튼을 놓았을 때 발생하는 이벤트</summary>
        public event EventHandler<GoMouseClickEventArgs>? MouseUp;
        /// <summary>마우스 더블클릭 시 발생하는 이벤트</summary>
        public event EventHandler<GoMouseClickEventArgs>? MouseDoubleClicked;
        /// <summary>마우스가 이동할 때 발생하는 이벤트</summary>
        public event EventHandler<GoMouseEventArgs>? MouseMove;
        /// <summary>마우스 휠 스크롤 시 발생하는 이벤트</summary>
        public event EventHandler<GoMouseWheelEventArgs>? MouseWheel;
        /// <summary>컨트롤이 그려진 후 발생하는 이벤트</summary>
        public event EventHandler<GoDrawnEventArgs>? Drawn;

        /// <summary>드래그 앤 드롭 시 발생하는 이벤트</summary>
        public event EventHandler<GoDragEventArgs>? DragDrop;
        #endregion

        #region Member Variable
        private SKRect bounds = new SKRect(0, 0, 70, 30);
        private float dx, dy, mx, my;
        private bool bDown = false;
        private DateTime downTime;
        private List<GoBinding>? bindings;
        private bool disposed;

        // gudx 선언적 바인딩: 역직렬화 시 {path} 식을 보관, WireBindings(root) 때 실제 바인딩으로 전환
        internal Dictionary<string, string>? PendingBindings { get; private set; }
        internal void AddPendingBinding(string propName, string expr)
            => (PendingBindings ??= new Dictionary<string, string>(StringComparer.Ordinal))[propName] = expr;
        #endregion

        #region Method
        #region virtual
        /// <summary>
        /// 사용자가 컨트롤을 조작 중일 때 true를 반환하여 binding 펌프를 정지시킵니다.
        /// 슬라이더 드래그, 입력창 편집 등의 일시 정지 신호로 사용됩니다.
        /// </summary>
        protected internal virtual bool IsBindingSuppressed => false;
        /// <summary>컨트롤이 초기화될 때 호출됩니다. 파생 클래스에서 재정의하여 초기화 로직을 구현합니다.</summary>
        /// <param name="design">디자인 편집기 객체</param>
        protected virtual void OnInit(GoDesign? design) { }
        /// <summary>컨트롤을 그립니다. 파생 클래스에서 재정의하여 그리기 로직을 구현합니다.</summary>
        /// <param name="canvas">SkiaSharp 캔버스</param>
        /// <param name="thm">테마</param>
        protected virtual void OnDraw(SKCanvas canvas, GoTheme thm) { Drawn?.Invoke(this, new GoDrawnEventArgs(canvas, thm)); }

        /// <summary>
        /// 컨테이너가 이 컨트롤을 그릴 때 Bounds로 클리핑할지 여부. 기본 true.
        /// false면 GUI.Draw가 ClipRect를 생략 — 그림자/글로우/회전이 Bounds 밖으로 나가도 잘리지 않습니다.
        /// (GsShape가 Clip 속성으로 구동)
        /// </summary>
        protected internal virtual bool ClipToBounds => true;
        /// <summary>컨트롤이 표시될 때 호출됩니다.</summary>
        protected virtual void OnShow() { View = true; }
        /// <summary>컨트롤이 숨겨질 때 호출됩니다.</summary>
        protected virtual void OnHide() { View = false; }
        /// <summary>컨트롤 상태를 주기적으로 업데이트할 때 호출됩니다.</summary>
        protected virtual void OnUpdate() { }
        /// <summary>마우스 버튼이 눌렸을 때 호출됩니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">눌린 마우스 버튼</param>
        protected virtual void OnMouseDown(float x, float y, GoMouseButton button) { MouseDown?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        /// <summary>마우스 버튼이 놓였을 때 호출됩니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">놓은 마우스 버튼</param>
        protected virtual void OnMouseUp(float x, float y, GoMouseButton button) { MouseUp?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        /// <summary>마우스 클릭이 발생했을 때 호출됩니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">클릭한 마우스 버튼</param>
        protected virtual void OnMouseClick(float x, float y, GoMouseButton button) { MouseClicked?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        /// <summary>롱클릭이 발생했을 때 호출됩니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">클릭한 마우스 버튼</param>
        protected virtual void OnMouseLongClick(float x, float y, GoMouseButton button) { MouseLongClicked?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        /// <summary>롱클릭이 취소되었을 때 호출됩니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">클릭한 마우스 버튼</param>
        protected virtual void OnMouseLongClickCancel(float x, float y, GoMouseButton button) { MouseLongClickCanceled?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        /// <summary>마우스 더블클릭이 발생했을 때 호출됩니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">클릭한 마우스 버튼</param>
        protected virtual void OnMouseDoubleClick(float x, float y, GoMouseButton button) { MouseDoubleClicked?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        /// <summary>마우스가 이동할 때 호출됩니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        protected virtual void OnMouseMove(float x, float y) { MouseMove?.Invoke(this, new GoMouseEventArgs(x, y)); }
        /// <summary>마우스 휠이 회전할 때 호출됩니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="delta">휠 스크롤 량</param>
        protected virtual void OnMouseWheel(float x, float y, float delta) { MouseWheel?.Invoke(this, new GoMouseWheelEventArgs(x, y, delta)); }
        /// <summary>마우스가 컨트롤 영역에 진입했을 때 호출됩니다.</summary>
        protected virtual void OnMouseEnter() { }
        /// <summary>마우스가 컨트롤 영역을 벗어났을 때 호출됩니다.</summary>
        protected virtual void OnMouseLeave() { }
        /// <summary>키가 눌렸을 때 호출됩니다.</summary>
        /// <param name="Shift">Shift 키 눌림 여부</param>
        /// <param name="Control">Control 키 눌림 여부</param>
        /// <param name="Alt">Alt 키 눌림 여부</param>
        /// <param name="key">눌린 키</param>
        protected virtual void OnKeyDown(bool Shift, bool Control, bool Alt, GoKeys key) { }
        /// <summary>키가 놓였을 때 호출됩니다.</summary>
        /// <param name="Shift">Shift 키 눌림 여부</param>
        /// <param name="Control">Control 키 눌림 여부</param>
        /// <param name="Alt">Alt 키 눌림 여부</param>
        /// <param name="key">놓은 키</param>
        protected virtual void OnKeyUp(bool Shift, bool Control, bool Alt, GoKeys key) { }
        /// <summary>리소스가 해제될 때 호출됩니다.</summary>
        protected virtual void OnDispose() { }
        #endregion

        #region Fire
        /// <summary>
        /// 컨트롤을 초기화합니다. 부모 컨테이너에서 호출됩니다.
        /// </summary>
        /// <param name="design">디자인 편집기 객체</param>
        // 여기서 Parent에서 Design객체를 처음 만들어서 여기서 다른 객체에서 사용됨
        public void FireInit(GoDesign? design)
        {
            Design = design;
            OnInit(design);
        }

        /// <summary>
        /// 컨트롤을 그립니다.
        /// </summary>
        /// <param name="canvas">SkiaSharp 캔버스</param>
        /// <param name="thm">테마</param>
        public void FireDraw(SKCanvas canvas, GoTheme thm)
        {
            if (Visible)
            {
                using var p = new SKPaint { IsAntialias = true, Color = SKColors.Black.WithAlpha(Convert.ToByte(Enabled ? 255 : 255 - GoTheme.DisableAlpha)) };
                var sp = canvas.SaveLayer(p);

                OnDraw(canvas, thm);

                canvas.RestoreToCount(sp);
            }
        }

        /// <summary>컨트롤이 표시될 때 호출합니다.</summary>
        public void FireShow() { OnShow(); }
        /// <summary>컨트롤이 숨겨질 때 호출합니다.</summary>
        public void FireHide() { OnHide(); }

        /// <summary>컨트롤의 상태를 업데이트합니다.</summary>
        public void FireUpdate()
        {
            if (bindings != null) PumpBindings();
            OnUpdate();
        }

        private void PumpBindings()
        {
            var list = bindings;
            if (list == null || list.Count == 0) return;
            if (disposed) return;

            // 재진입 안전: 펌프 도중 setter가 Bind/UnbindAll을 호출하더라도
            // 이번 프레임은 일관된 스냅샷으로 진행
            var snapshot = list.ToArray();
            bool suppressed = IsBindingSuppressed;

            for (int i = 0; i < snapshot.Length; i++)
            {
                var b = snapshot[i];

                if (suppressed)
                {
                    // 조작 중 — 양방향 모두 정지. 초기화된 binding만 flush 표시
                    // (초기화 전 binding을 flush하면 컨트롤 default 값이 소스를 덮어씀)
                    if (b.SourceSet != null && b.Initialized) b.PendingFlush = true;
                    continue;
                }

                // 조작 종료 직후 한 번 flush (컨트롤 → 소스)
                if (b.PendingFlush && b.SourceSet != null)
                {
                    object? cur;
                    try { cur = b.CtrlGet(this); }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[GoBinding] control getter failed during flush for {b.CtrlProperty.Name}: {ex.Message}");
                        b.PendingFlush = false;
                        continue;
                    }

                    try
                    {
                        b.SourceSet(cur);
                        b.LastCtrlValue = cur;
                        b.LastSrcValue = cur;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[GoBinding] source setter failed during flush for {b.CtrlProperty.Name}: {ex.Message}");
                    }
                    b.PendingFlush = false;
                    // flush 후엔 이번 프레임은 더 이상 처리 안 함 (다음 프레임에 일반 흐름)
                    continue;
                }

                // 일반 흐름 — 소스 → 컨트롤
                object? newSrc;
                try { newSrc = b.SourceGet(); }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GoBinding] source getter failed for {b.CtrlProperty.Name}: {ex.Message}");
                    continue;
                }

                if (b.IsCommand && b.HasPendingCommand)
                {
                    if (object.Equals(newSrc, b.PendingCommandValue))
                    {
                        b.LastSrcValue = newSrc;
                        b.LastCtrlValue = newSrc;
                        b.HasPendingCommand = false;
                        b.PendingCommandValue = null;
                    }
                    else if (Environment.TickCount64 - b.PendingCommandTick < b.CommandTimeout)
                    {
                        continue;
                    }
                    else
                    {
                        b.HasPendingCommand = false;
                        b.PendingCommandValue = null;
                        try
                        {
                            b.CtrlSet(this, newSrc);
                            b.LastSrcValue = newSrc;
                            b.LastCtrlValue = newSrc;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[GoBinding] control setter failed after command timeout for {b.CtrlProperty.Name}: {ex.Message}");
                        }
                        continue;
                    }
                }

                if (!b.Initialized || !object.Equals(newSrc, b.LastSrcValue))
                {
                    try
                    {
                        b.CtrlSet(this, newSrc);
                        b.LastSrcValue = newSrc;
                        b.LastCtrlValue = newSrc;  // 자기 트리거 방지 (역방향 분기의 비교 기준)
                        b.Initialized = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[GoBinding] control setter failed for {b.CtrlProperty.Name}: {ex.Message}");
                    }
                }

                // 컨트롤 → 소스
                if (b.SourceSet != null)
                {
                    object? cur;
                    try { cur = b.CtrlGet(this); }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[GoBinding] control getter failed for {b.CtrlProperty.Name}: {ex.Message}");
                        continue;
                    }

                    if (!object.Equals(cur, b.LastCtrlValue))
                    {
                        try
                        {
                            b.SourceSet(cur);
                            b.LastCtrlValue = cur;
                            if (b.IsCommand)
                            {
                                b.PendingCommandValue = cur;
                                b.PendingCommandTick = Environment.TickCount64;
                                b.HasPendingCommand = true;
                            }
                            else
                            {
                                b.LastSrcValue = cur;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[GoBinding] source setter failed for {b.CtrlProperty.Name}: {ex.Message}");
                        }
                    }
                }
            }
        }

        internal void AddOrReplaceBinding(GoBinding b)
        {
            if (disposed) return;
            bindings ??= new List<GoBinding>();
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].CtrlProperty == b.CtrlProperty)
                {
                    Debug.WriteLine($"[GoBinding] rebinding existing property {b.CtrlProperty.Name} on {GetType().Name} — previous binding replaced");
                    bindings[i] = b;
                    return;
                }
            }
            bindings.Add(b);
        }

        internal void ClearBindings()
        {
            if (disposed) return;
            bindings?.Clear();
        }

        internal void RemoveBindingByProperty(System.Reflection.PropertyInfo pi)
        {
            if (disposed) return;
            if (bindings == null) return;
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].CtrlProperty == pi)
                {
                    bindings.RemoveAt(i);
                    return;
                }
            }
        }
        
        /// <summary>
        /// 마우스 버튼 누름 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">눌린 마우스 버튼</param>
        public void FireMouseDown(float x, float y, GoMouseButton button)
        {
            if (Visible)
            {
                var rts = Areas();
                var rtContent = rts["Content"];
                if (CollisionTool.Check(rtContent, x, y))
                {
                    mx = dx = x;
                    my = dy = y;
                    bDown = true;
                    downTime = DateTime.Now;
                    OnMouseDown(x, y, button);

                    if (UseLongClick)
                        Task.Run(async () =>
                        {
                            var time = LongClickTime ?? GlobalLongClickTime;

                            downTime = DateTime.Now;
                            while (bDown && (DateTime.Now - downTime).TotalMilliseconds < time) await Task.Delay(100);

                            if (bDown)
                            {
                                bDown = false;

                                if ((DateTime.Now - downTime).TotalMilliseconds >= time && CollisionTool.Check(rtContent, mx, my))
                                    OnMouseLongClick(x, y, button);
                                else
                                    OnMouseLongClickCancel(x, y, button);
                            }
                        });

                    if (Selectable) Design?.Select(this);
                }
            }
        }

        /// <summary>
        /// 마우스 버튼 놓음 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">놓은 마우스 버튼</param>
        public void FireMouseUp(float x, float y, GoMouseButton button)
        {
            if (Visible)
            {
                if (bDown)
                {
                    bDown = false;

                    OnMouseUp(x, y, button);

                    var dist = Math.Abs(MathTool.GetDistance(new SKPoint(dx, dy), new SKPoint(x, y)));
                    // 3픽셀 이내에 있을 때만 클릭으로 인정(터치가)
                    // 그래서 감압식은 찍은 압력에 따라 좌표가 바뀌기 때문에 3픽셀을 늘이면 동작한다.
                    if (CollisionTool.Check(Util.FromRect(0, 0, Width, Height), x, y) && dist < 3) OnMouseClick(x, y, button);
                }
            }
        }

        /// <summary>
        /// 마우스 더블클릭 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">클릭한 마우스 버튼</param>
        public void FireMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            if (Visible)
            {
                OnMouseDoubleClick(x, y, button);
            }
        }
        
        /// <summary>
        /// 마우스 이동 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        public void FireMouseMove(float x, float y)
        {
            if (Visible)
            {
                mx = x; my = y; OnMouseMove(x, y);
            }
        }
        
        /// <summary>
        /// 마우스 휠 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="delta">휠 스크롤 량</param>
        public void FireMouseWheel(float x, float y, float delta)
        {
            if (Visible)
            {
                OnMouseWheel(x, y, delta);
            }
        }
        
        /// <summary>
        /// 키 누름 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="Shift">Shift 키 눌림 여부</param>
        /// <param name="Control">Control 키 눌림 여부</param>
        /// <param name="Alt">Alt 키 눌림 여부</param>
        /// <param name="key">눌린 키</param>
        public void FireKeyDown(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            if (Visible)
            {
                OnKeyDown(Shift, Control, Alt, key);
            }
        }

        /// <summary>
        /// 키 놓음 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="Shift">Shift 키 눌림 여부</param>
        /// <param name="Control">Control 키 눌림 여부</param>
        /// <param name="Alt">Alt 키 눌림 여부</param>
        /// <param name="key">놓은 키</param>
        public void FireKeyUp(bool Shift, bool Control, bool Alt, GoKeys key)
        {
            if (Visible)
            {
                OnKeyUp(Shift, Control, Alt, key);
            }
        }

        /// <summary>리소스를 해제합니다.</summary>
        public void Dispose()
        {
            if (disposed) return;
            ClearBindings();    // disposed 플래그 set 전에 실행 (ClearBindings가 가드로 거르지 않도록)
            disposed = true;
            OnDispose();
        }
        #endregion

        #region Areas
        /// <summary>
        /// 컨트롤의 영역 사전을 반환합니다. 기본적으로 "Content" 영역을 포함합니다.
        /// </summary>
        /// <returns>영역 이름과 사각형으로 구성된 사전</returns>
        public virtual Dictionary<string, SKRect> Areas() => new() { { "Content", Util.FromRect(0, 0, Width - 1, Height - 1) } };
        #endregion

        #region Etc
        /// <summary>
        /// 컨트롤의 화면 표시 상태를 설정합니다.
        /// </summary>
        /// <param name="view">표시 여부</param>
        public void SetView(bool view) => View = view;
        /// <summary>
        /// 화면 갱신 콜백 메서드를 설정합니다.
        /// </summary>
        /// <param name="method">갱신 시 호출할 메서드</param>
        public void SetInvalidate(Action? method) => actInv = method;

        /// <summary>컨트롤의 화면 갱신을 요청합니다.</summary>
        protected void Invalidate()
        {
            if (actInv != null) actInv?.Invoke();
            else if (View) Design?.Invalidate();
        }
        
        internal void InvokeDragDrop(float x, float y, object item) => DragDrop?.Invoke(this, new GoDragEventArgs(x, y, item));
        internal void Leave() => OnMouseLeave();

        #endregion

        #region SetParent
        /// <summary>
        /// 부모 컨테이너를 설정합니다. 내부 사용 전용입니다.
        /// </summary>
        /// <param name="parent">부모 컨테이너</param>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void SetParent(IGoContainer? parent) => Parent = parent;
        #endregion
        #endregion
    }
}
