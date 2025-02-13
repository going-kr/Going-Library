using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Utils;
using SkiaSharp;

namespace Going.UI.Control.Controls
{
    public class GoControl
    {
        #region Properties
        public string Name { get; set; }

        public SKRect Bounds { get => bounds; set => bounds = value; }
        public float X { get => bounds.Left; set => bounds.Left = value; }
        public float Y { get => bounds.Top; set => bounds.Top = value; }
        public float Left { get => bounds.Left; set => bounds.Left = value; }
        public float Top { get => bounds.Top; set => bounds.Top = value; }
        public float Right { get => bounds.Right; set => bounds.Right = value; }
        public float Bottom { get => bounds.Bottom; set => bounds.Bottom = value; }
        public float Width { get => bounds.Width; set => bounds.Right = value + bounds.Left; }
        public float Height { get => bounds.Height; set => bounds.Bottom = value + bounds.Top; }

        public GoPadding Margin { get; set; } = new(3, 3, 3, 3);
        public bool Fill { get; set; } = false;
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

        public void Draw(SKCanvas canvas) { OnDraw(canvas); }
        public void Update() { OnUpdate(); }
        public void MouseDown(float x, float y, GoMouseButton button) { OnMouseDown(x, y, button); }
        public void MouseUp(float x, float y, GoMouseButton button) { OnMouseUp(x, y, button); }
        public void MouseDoubleClick(float x, float y, GoMouseButton button) { OnMouseDoubleClick(x, y, button); }
        public void MouseMove(float x, float y) { OnMouseMove(x, y); }
        public void MouseWheel(float x, float y, float delta) { OnMouseWheel(x, y, delta); }

        public virtual Dictionary<string, SKRect> Areas()
        {
            return new Dictionary<string, SKRect>
            {
                { "Content", Util.FromRect(0, 0, Width - 1, Height - 1) }
            };
        }
        #endregion
    }
}
