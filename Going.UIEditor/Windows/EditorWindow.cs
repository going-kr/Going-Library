using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Forms.Controls;
using Going.UI.Forms.ImageCanvas;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using Going.UIEditor.Datas;
using OpenTK.Graphics.ES20;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Media.Protection.PlayReady;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Windows.Forms.Timer;

namespace Going.UIEditor.Windows
{
    public partial class EditorWindow : xWindow
    {
        #region Properties
        public object Target { get; private set; }
        #endregion

        #region Member Variable
        private Timer tmr;
        #endregion

        #region Constructor
        public EditorWindow(object editorTarget)
        {
            InitializeComponent();

            #region set
            Target = editorTarget;
            AllowDrop = true;
            if (editorTarget is GoDesign) { Title = "Mater"; TitleIconString = "fa-pager"; }
            else if (editorTarget is GoPage page) { Title = page.Name ?? ""; TitleIconString = "fa-file-image"; }
            else if (editorTarget is GoWindow wnd) { Title = wnd.Name ?? ""; TitleIconString = "fa-window-maximize"; }
            #endregion

            #region new
            tmr = new Timer { Interval = 10, Enabled = true };
            #endregion

            #region Event
            tmr.Tick += (o, s) =>
            {
                if (IsActivated)
                    Invalidate();
            };
            #endregion
        }
        #endregion

        #region Override
        #region Shown
        protected override void OnShown(EventArgs e)
        {
            Program.CurrentProject?.Design.Init();
            base.OnShown(e);
        }
        #endregion

        #region Activate
        protected override void OnActivated(EventArgs e)
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                if (Target is GoDesign design2) { }
                else if (Target is GoPage page2) p.Design.SetPage(page2);
                else if (Target is GoWindow wnd2) { }
                Invalidate();
            }
            base.OnActivated(e);
        }
        #endregion

        #region Draw
        public override void OnContentDraw(ContentDrawEventArgs e)
        {
            var prj = Program.CurrentProject;
            var thm = GoTheme.Current;
            var canvas = e.Canvas;
            if (prj != null)
            {
                SKRect? rt = GetBounds();
                using var p = new SKPaint { };

                #region Fill
                if (rt.HasValue)
                {
                    using var imgf = SKImageFilter.CreateDropShadow(2, 2, 2, 2, Util.FromArgb(128, SKColors.Black));
                    p.ImageFilter = imgf;
                    p.IsStroke = false;
                    p.Color = thm.Back;
                    canvas.DrawRect(rt.Value, p);
                    p.ImageFilter = null;
                }
                #endregion

                using (new SKAutoCanvasRestore(canvas))
                {
                    if (rt != null)
                    {
                        canvas.ClipRect(rt.Value);
                        canvas.Translate(rt.Value.Left, rt.Value.Top);

                        canvas.Clear(thm.Back);

                        prj.Design.SetSize(Convert.ToInt32(rt.Value.Width), Convert.ToInt32(rt.Value.Height));

                        if (Target is GoDesign design2) DrawMaster(canvas, prj, design2);
                        else if (Target is GoPage page2) DrawPage(canvas, prj, page2);
                        else if (Target is GoWindow wnd2) DrawWindow(canvas, prj, wnd2);
                    }
                }

                #region Border
                if (rt.HasValue)
                {
                    p.IsStroke = true;
                    p.Color = SKColors.Black;
                    p.StrokeWidth = 1;
                    canvas.DrawRect(rt.Value, p);
                }
                #endregion
            }

            base.OnContentDraw(e);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(MouseEventArgs e)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var rt = GetBounds();
                if (rt.HasValue)
                {
                    int x = Convert.ToInt32(e.X - rt.Value.Left);
                    int y = Convert.ToInt32(e.Y - rt.Value.Top);
                    if (Target is GoDesign design2) MouseDownMaster(design2, x, y);
                    else if (Target is GoPage page2) MouseDownPage(page2, x, y);
                    else if (Target is GoWindow wnd2) MouseDownWindow(wnd2, x, y);
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var rt = GetBounds();
                if (rt.HasValue)
                {
                    int x = Convert.ToInt32(e.X - rt.Value.Left);
                    int y = Convert.ToInt32(e.Y - rt.Value.Top);
                    if (Target is GoDesign design2) MouseMoveMaster(design2, x, y);
                    else if (Target is GoPage page2) MouseMovePage(page2, x, y);
                    else if (Target is GoWindow wnd2) MouseMoveWindow(wnd2, x, y);
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var rt = GetBounds();
                if (rt.HasValue)
                {
                    int x = Convert.ToInt32(e.X - rt.Value.Left);
                    int y = Convert.ToInt32(e.Y - rt.Value.Top);
                    if (Target is GoDesign design2) MouseUpMaster(design2, x, y);
                    else if (Target is GoPage page2) MouseUpPage(page2, x, y);
                    else if (Target is GoWindow wnd2) MouseUpWindow(wnd2, x, y);
                }
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var rt = GetBounds();
                if (rt.HasValue)
                {
                    int x = Convert.ToInt32(e.X - rt.Value.Left);
                    int y = Convert.ToInt32(e.Y - rt.Value.Top);
                    float delta = e.Delta / 120F;
                    if (Target is GoDesign design2) MouseWheelMaster(design2, x, y, delta);
                    else if (Target is GoPage page2) MouseWheelPage(page2, x, y, delta);
                    else if (Target is GoWindow wnd2) MouseWheelWindow(wnd2, x, y, delta);
                }
            }
            base.OnMouseWheel(e);
        }
        #endregion

        #region Drag
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var rt = GetBounds();
                var pt = PointToClient(new Point(drgevent.X, drgevent.Y));
                var v = drgevent.Data?.GetData(typeof(GoToolItem)) as GoToolItem;
                if (v != null && rt.HasValue && CollisionTool.Check(rt.Value, pt.X, pt.Y))
                    drgevent.Effect = DragDropEffects.All;
                else
                    drgevent.Effect = DragDropEffects.None;
            }

            base.OnDragOver(drgevent);
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var rt = GetBounds();
                var pt = PointToClient(new Point(drgevent.X, drgevent.Y));
                var v = drgevent.Data?.GetData(typeof(GoToolItem)) as GoToolItem;
                if (v != null && rt.HasValue && CollisionTool.Check(rt.Value, pt.X, pt.Y) && v.Tag is Type tp)
                {
                    int x = Convert.ToInt32(pt.X - rt.Value.Left);
                    int y = Convert.ToInt32(pt.Y - rt.Value.Top);

                    if (Target is GoDesign design2) DropMaster(design2, x, y, tp);
                    else if (Target is GoPage page2) DropPage(page2, x, y, tp);
                    else if (Target is GoWindow wnd2) DropWindow(wnd2, x, y, tp);
                }
                base.OnDragDrop(drgevent);
            }
            Invalidate();
        }
        #endregion
        #endregion

        #region Method
        #region GetBounds
        SKRect? GetBounds()
        {
            SKRect? rt = null;

            var prj = Program.CurrentProject;

            if (prj != null)
            {
                if (Target is GoDesign design)
                    rt = MathTool.MakeRectangle(Util.FromRect(0, 0, Width, Height), new SkiaSharp.SKSize(prj.Width, prj.Height));
                else if (Target is GoPage page)
                    rt = MathTool.MakeRectangle(Util.FromRect(0, 0, Width, Height), new SkiaSharp.SKSize(prj.Width, prj.Height));
                else if (Target is GoWindow wnd)
                    rt = MathTool.MakeRectangle(Util.FromRect(0, 0, Width, Height), new SkiaSharp.SKSize(wnd.Width, wnd.Height));
            }

            return rt;
        }
        #endregion

        #region Master
        void DrawMaster(SKCanvas canvas, Project prj, GoDesign design) { }
        void DropMaster(GoDesign design, int x, int y, Type tp) { }
        void MouseDownMaster(GoDesign design, int x, int y) { }
        void MouseUpMaster(GoDesign design, int x, int y) { }
        void MouseMoveMaster(GoDesign design, int x, int y) { }
        void MouseWheelMaster(GoDesign design, int x, int y, float delta) { }
        #endregion

        #region Page
        void DrawPage(SKCanvas canvas, Project prj, GoPage page)
        {
            if (IsActivated) prj.Design.SetPage(page);
            prj.Design.Draw(canvas);
        }
        void DropPage(GoPage page, int x, int y, Type tp)
        {
            var v = Activator.CreateInstance(tp);
            if (v is IGoControl vc)
            {
                vc.Left = x; vc.Top = y; vc.Width = 80; vc.Height = 40;
                page.Childrens.Add(vc);
            }
        }
        void MouseDownPage(GoPage page, int x, int y) { }
        void MouseUpPage(GoPage page, int x, int y) { }
        void MouseMovePage(GoPage page, int x, int y) { }
        void MouseWheelPage(GoPage page, int x, int y, float delta) { }
        #endregion

        #region Window
        void DrawWindow(SKCanvas canvas, Project prj, GoWindow wnd) { }
        void DropWindow(GoWindow wnd, int x, int y, Type tp) { }
        void MouseDownWindow(GoWindow wnd, int x, int y) { }
        void MouseUpWindow(GoWindow wnd, int x, int y) { }
        void MouseMoveWindow(GoWindow wnd, int x, int y) { }
        void MouseWheelWindow(GoWindow wnd, int x, int y, float delta) { }
        #endregion
        #endregion
    }
}
