using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System.Text.Json.Serialization;

namespace Going.UI.Controls
{
    public class GoControl : IGoControl
    {
        #region Properties
        public string Name { get; set; }
        public SKRect Bounds { get => bounds; set => bounds = value; }
        public GoPadding Margin { get; set; } = new(3, 3, 3, 3);
        public bool Fill { get; set; } = false;
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;

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
        #endregion

        #region Member Variable
        private SKRect bounds = new SKRect(0, 0, 70, 30);
        #endregion

        #region Method
        protected virtual void OnDraw(SKCanvas canvas) { }
        protected virtual void OnUpdate() { }
        protected virtual void OnMouseDown(float x, float y, GoMouseButton button) { }
        protected virtual void OnMouseUp(float x, float y, GoMouseButton button) { }
        protected virtual void OnMouseDoubleClick(float x, float y, GoMouseButton button) { }
        protected virtual void OnMouseMove(float x, float y) { }
        protected virtual void OnMouseWheel(float x, float y, float delta) { }

        public void Draw(SKCanvas canvas)
        {
            if (Visible)
            {
                using var p = new SKPaint { IsAntialias = true, Color = SKColors.Black.WithAlpha(Convert.ToByte(Enabled ? 255 : 255 - GoTheme.DisableAlpha)) };
                var sp = canvas.SaveLayer(p);

                OnDraw(canvas);

                canvas.RestoreToCount(sp);
            }
        }

        public void Update() { OnUpdate(); }
        public void MouseDown(float x, float y, GoMouseButton button) { OnMouseDown(x, y, button); }
        public void MouseUp(float x, float y, GoMouseButton button) { OnMouseUp(x, y, button); }
        public void MouseDoubleClick(float x, float y, GoMouseButton button) { OnMouseDoubleClick(x, y, button); }
        public void MouseMove(float x, float y) { OnMouseMove(x, y); }
        public void MouseWheel(float x, float y, float delta) { OnMouseWheel(x, y, delta); }

        public virtual Dictionary<string, SKRect> Areas() => new() { { "Content", Util.FromRect(0, 0, Width - 1, Height - 1) } };
        #endregion
    }
}
