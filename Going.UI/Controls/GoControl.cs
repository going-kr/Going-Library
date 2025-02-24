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
    public class GoControl : IGoControl
    {
        #region Properties
        public static int LongClickTime { get; set; } = 2000;
        
        public Guid Id { get; init; } = Guid.NewGuid();
        public string? Name { get; set; }
        public SKRect Bounds { get => bounds; set => bounds = value; }
        public GoPadding Margin { get; set; } = new(3, 3, 3, 3);
        public bool Fill { get; set; } = false;
        public virtual bool Visible { get; set; } = true;
        public virtual bool Enabled { get; set; } = true;
        public bool Selectable { get; protected set; } = false;

        [JsonIgnore] public float X { get => bounds.Left; set => bounds.Left = value; }
        [JsonIgnore] public float Y { get => bounds.Top; set => bounds.Top = value; }
        [JsonIgnore] public float Left { get => bounds.Left; set => bounds.Left = value; }
        [JsonIgnore] public float Top { get => bounds.Top; set => bounds.Top = value; }
        [JsonIgnore] public float Right { get => bounds.Right; set => bounds.Right = value; }
        [JsonIgnore] public float Bottom { get => bounds.Bottom; set => bounds.Bottom = value; }
        [JsonIgnore] public float Width { get => bounds.Width; set => bounds.Right = value + bounds.Left; }
        [JsonIgnore] public float Height { get => bounds.Height; set => bounds.Bottom = value + bounds.Top; }
        [JsonIgnore] public float ScreenX => Parent != null && Parent is GoControl pc ? pc.ScreenX + X : X;
        [JsonIgnore] public float ScreenY => Parent != null && Parent is GoControl pc ? pc.ScreenY + Y : Y;

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
        protected virtual void OnUpdate() {  }
        protected virtual void OnMouseDown(float x, float y, GoMouseButton button) { MouseDown?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        protected virtual void OnMouseUp(float x, float y, GoMouseButton button) { MouseUp?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        protected virtual void OnMouseClick(float x, float y, GoMouseButton button) { MouseClicked?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        protected virtual void OnMouseLongClick(float x, float y, GoMouseButton button) { MouseLongClicked?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        protected virtual void OnMouseDoubleClick(float x, float y, GoMouseButton button) { MouseDoubleClicked?.Invoke(this, new GoMouseClickEventArgs(x, y, button)); }
        protected virtual void OnMouseMove(float x, float y) { MouseMove?.Invoke(this, new GoMouseEventArgs(x, y)); }
        protected virtual void OnMouseWheel(float x, float y, float delta) { MouseWheel?.Invoke(this, new GoMouseWheelEventArgs(x, y, delta)); }
        protected virtual void OnKeyDown(bool Shift, bool Control, bool Alt, GoKeys key) {  }
        protected virtual void OnKeyUp(bool Shift, bool Control, bool Alt, GoKeys key) {  }
        #endregion

        #region Fire
        public void FireInit(GoDesign? design)
        {
            this.Design = design;
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
                    while (bDown && (DateTime.Now - downTime).TotalMilliseconds < LongClickTime) await Task.Delay(100);
                    downTime = DateTime.Now;

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
        #endregion
    }
}
