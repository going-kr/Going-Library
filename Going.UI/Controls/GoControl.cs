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
using System.Text.Json.Serialization;

namespace Going.UI.Controls
{
    public class PCategory
    {
        public const string ID = "ID";
        public const string Basic = "Basic";
        public const string Bounds = "Bounds";
        public const string Control = "Control";

        public static List<string> Names = [ID, Basic, Bounds, Control];
        public static int Index(string category) => Names.IndexOf(category);
    }

    public class GoPropertyAttribute(string category, int order) : Attribute
    {
        public string Category { get; set; } = category;
        public int Order { get; set; } = order;
    }

    public class GoSizePropertyAttribute(string category, int order) : GoPropertyAttribute(category, order) { }
    public class GoSizesPropertyAttribute(string category, int order) : GoPropertyAttribute(category, order) { }
    public class GoImagePropertyAttribute(string category, int order) : GoPropertyAttribute(category, order) { }

    public class GoControl : IGoControl
    {
        #region Properties
        public static int LongClickTime { get; set; } = 2000;

        public Guid Id { get; init; } = Guid.NewGuid();
        [GoProperty(PCategory.Basic, 0)] public string? Name { get; set; }
        [GoProperty(PCategory.Basic, 1)] public virtual bool Visible { get; set; } = true;
        [GoProperty(PCategory.Basic, 2)] public virtual bool Enabled { get; set; } = true;
        public bool Selectable { get; protected set; } = false;
        [JsonIgnore] public object? Tag { get; set; }

        [JsonIgnore] public float ScreenX => Parent != null && Parent is GoControl pc ? pc.ScreenX + Parent.PanelBounds.Left + X : X;
        [JsonIgnore] public float ScreenY => Parent != null && Parent is GoControl pc ? pc.ScreenY + Parent.PanelBounds.Top + Y : Y;

        [GoProperty(PCategory.Bounds, 0)] public SKRect Bounds { get => bounds; set => bounds = value; }
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
        [GoProperty(PCategory.Bounds, 3), JsonIgnore] public float Width { get => bounds.Width; set => bounds.Right = value + bounds.Left; }
        [GoProperty(PCategory.Bounds, 4), JsonIgnore] public float Height { get => bounds.Height; set => bounds.Bottom = value + bounds.Top; }
        [GoProperty(PCategory.Bounds, 5), JsonIgnore] public float Left { get => bounds.Left; set => bounds.Left = value; }
        [GoProperty(PCategory.Bounds, 6), JsonIgnore] public float Top { get => bounds.Top; set => bounds.Top = value; }
        [GoProperty(PCategory.Bounds, 7), JsonIgnore] public float Right { get => bounds.Right; set => bounds.Right = value; }
        [GoProperty(PCategory.Bounds, 8), JsonIgnore] public float Bottom { get => bounds.Bottom; set => bounds.Bottom = value; }
        [GoProperty(PCategory.Bounds, 9)] public bool Fill { get; set; } = false;
        [GoProperty(PCategory.Bounds, 10)] public GoPadding Margin { get; set; } = new(3, 3, 3, 3);

        [JsonIgnore] public bool FirstRender { get; internal set; } = true;
        [JsonIgnore] public IGoContainer? Parent { get; internal set; }
        [JsonIgnore] public GoDesign? Design { get; internal set; }

        [JsonIgnore] internal bool _MouseDown_ => bDown;

        protected Action? Invalidate;
        #endregion

        #region Event
        public event EventHandler<GoMouseClickEventArgs>? MouseClicked;
        public event EventHandler<GoMouseClickEventArgs>? MouseLongClicked;
        public event EventHandler<GoMouseClickEventArgs>? MouseDown;
        public event EventHandler<GoMouseClickEventArgs>? MouseUp;
        public event EventHandler<GoMouseClickEventArgs>? MouseDoubleClicked;
        public event EventHandler<GoMouseEventArgs>? MouseMove;
        public event EventHandler<GoMouseEventArgs>? MouseWheel;
        public event EventHandler<SKCanvas>? Drawn;

        public event EventHandler<GoDragEventArgs>? DragDrop;
        #endregion

        #region Member Variable
        private SKRect bounds = new SKRect(0, 0, 70, 30);
        private float dx, dy;
        private bool bDown = false;
        private DateTime downTime;
        #endregion

        #region Method
        #region virtual
        protected virtual void OnInit(GoDesign? design) { }
        protected virtual void OnDraw(SKCanvas canvas) { Drawn?.Invoke(this, canvas); }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnMouseDown(float x, float y, GoMouseButton button) { MouseDown?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        protected virtual void OnMouseUp(float x, float y, GoMouseButton button) { MouseUp?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        protected virtual void OnMouseClick(float x, float y, GoMouseButton button) { MouseClicked?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        protected virtual void OnMouseLongClick(float x, float y, GoMouseButton button) { MouseLongClicked?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        protected virtual void OnMouseDoubleClick(float x, float y, GoMouseButton button) { MouseDoubleClicked?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        protected virtual void OnMouseMove(float x, float y) { MouseMove?.Invoke(this, new GoMouseEventArgs(x, y)); }
        protected virtual void OnMouseWheel(float x, float y, float delta) { MouseWheel?.Invoke(this, new GoMouseWheelEventArgs(x, y, delta)); }
        protected virtual void OnMouseEnter() { }
        protected virtual void OnMouseLeave() { }
        protected virtual void OnKeyDown(bool Shift, bool Control, bool Alt, GoKeys key) { }
        protected virtual void OnKeyUp(bool Shift, bool Control, bool Alt, GoKeys key) { }
        #endregion

        #region Fire
        // 여기서 Parent에서 Design객체를 처음 만들어서 여기서 다른 객체에서 사용됨
        public void FireInit(GoDesign? design)
        {
            Design = design;
            OnInit(design);
        }

        public void FireDraw(SKCanvas canvas)
        {
            if (Visible)
            {
                using var p = new SKPaint { IsAntialias = true, Color = SKColors.Black.WithAlpha(Convert.ToByte(Enabled ? 255 : 255 - GoTheme.DisableAlpha)) };
                var sp = canvas.SaveLayer(p);

                OnDraw(canvas);

                canvas.RestoreToCount(sp);
            }
        }

        public void FireShow() { OnShow(); }
        public void FireHide() { OnHide(); }

        public void FireUpdate() { OnUpdate(); }
        public void FireMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            if (CollisionTool.Check(rtContent, x, y))
            {
                dx = x; dy = y;
                bDown = true;
                downTime = DateTime.Now;
                OnMouseDown(x, y, button);

                Task.Run(async () =>
                {
                    downTime = DateTime.Now;
                    while (bDown && (DateTime.Now - downTime).TotalMilliseconds < LongClickTime) await Task.Delay(100);

                    if ((DateTime.Now - downTime).TotalMilliseconds >= LongClickTime && CollisionTool.Check(rtContent, x, y))
                        OnMouseLongClick(x, y, button);
                });

                if (Selectable) Design?.Select(this);
            }
        }

        public void FireMouseUp(float x, float y, GoMouseButton button)
        {
            if (bDown)
            {
                OnMouseUp(x, y, button);
                bDown = false;

                var dist = Math.Abs(MathTool.GetDistance(new SKPoint(dx, dy), new SKPoint(x, y)));
                // 3픽셀 이내에 있을 때만 클릭으로 인정(터치가)
                // 그래서 감압식은 찍은 압력에 따라 좌표가 바뀌기 때문에 3픽셀을 늘이면 동작한다.
                if (CollisionTool.Check(Util.FromRect(0, 0, Width, Height), x, y) && dist < 3) OnMouseClick(x, y, button);
            }
        }

        public void FireMouseDoubleClick(float x, float y, GoMouseButton button) { OnMouseDoubleClick(x, y, button); }
        public void FireMouseMove(float x, float y) { OnMouseMove(x, y); }
        public void FireMouseWheel(float x, float y, float delta) { OnMouseWheel(x, y, delta); }
        public void FireKeyDown(bool Shift, bool Control, bool Alt, GoKeys key) { OnKeyDown(Shift, Control, Alt, key); }
        public void FireKeyUp(bool Shift, bool Control, bool Alt, GoKeys key) { OnKeyUp(Shift, Control, Alt, key); }
        #endregion

        #region Areas
        public virtual Dictionary<string, SKRect> Areas() => new() { { "Content", Util.FromRect(0, 0, Width - 1, Height - 1) } };
        #endregion

        public void SetInvalidate(Action? method) => Invalidate = method;

        internal void InvokeDragDrop(float x, float y, object item) => DragDrop?.Invoke(this, new GoDragEventArgs(x, y, item));
        internal void Leave() => OnMouseLeave();
        #endregion
    }
}
