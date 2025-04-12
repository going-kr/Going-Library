using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Forms.Controls;
using Going.UI.Forms.ImageCanvas;
using Going.UI.ImageCanvas;
using Going.UI.Json;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using Going.UIEditor.Datas;
using Going.UIEditor.Utils;
using GuiLabs.Undo;
using OpenTK.Graphics.ES20;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Windows.Media.Protection.PlayReady;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using GoButton = Going.UI.Controls.GoButton;
using GoControl = Going.UI.Controls.GoControl;
using Timer = System.Windows.Forms.Timer;

namespace Going.UIEditor.Windows
{
    public partial class EditorWindow : xWindow
    {
        #region Const
        const int ANC_SZ = 4;
        #endregion

        #region Properties
        public object Target { get; private set; }

        public bool CanUndo => actmgr.CanUndo;
        public bool CanRedo => actmgr.CanRedo;
        #endregion

        #region Member Variable
        private Timer tmr;
        ActionManager actmgr;
        List<IGoControl> sels = new List<IGoControl>();

        SKPoint? ptDown, ptMove;
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
            actmgr = new ActionManager();
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
            var p = Program.CurrentProject;
            p?.Design.Init();
            base.OnShown(e);
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
                prj.Design.UseTitleBar = prj.Design.UseLeftSideBar = prj.Design.UseRightSideBar = prj.Design.UseFooter = true;
                SKRect? rt = GetBounds();
                using var p = new SKPaint { };
                using var pe = SKPathEffect.CreateDash([2, 2], 2);

                #region BG
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
                        var crt = rt.Value; crt.Inflate(1, 1);
                        canvas.ClipRect(crt);
                        canvas.Translate(rt.Value.Left, rt.Value.Top);

                        canvas.Clear(thm.Back);

                        prj.Design.SetSize(Convert.ToInt32(rt.Value.Width), Convert.ToInt32(rt.Value.Height));

                        if (Target is GoDesign design2) prj.Design.DesignTimeDraw(canvas);
                        else if (Target is GoPage page2) prj.Design.DesignTimeDraw(canvas, page2);
                        else if (Target is GoWindow wnd2) wnd2.FireDraw(canvas);

                        #region Selected Control
                        foreach (var c in sels)
                        {
                            if (c is GoControl vc)
                            {

                                using (new SKAutoCanvasRestore(canvas))
                                {
                                    #region rt
                                    var vrt = Util.FromRect(vc.ScreenX, vc.ScreenY, vc.Width, vc.Height);
                                    if (vc.Parent is GoScrollablePanel sc)
                                    {
                                        vrt.Offset(-sc.ViewPosition.X, -sc.ViewPosition.Y);
                                        canvas.ClipRect(Util.FromRect(sc.ScreenX, sc.ScreenY, sc.Width, sc.Height));
                                    }
                                    #endregion
                                    #region box
                                    p.IsStroke = true;
                                    p.Color = SKColors.Red;
                                    p.StrokeWidth = 1;
                                    canvas.DrawRect(vrt, p);
                                    #endregion
                                    #region anchor
                                    {
                                        var ev = PointToClient(MousePosition);
                                        int mx = Convert.ToInt32(ev.X - rt.Value.Left);
                                        int my = Convert.ToInt32(ev.Y - rt.Value.Top);

                                        var ancs = Util2.GetAnchors(vc, vrt);

                                        if (CollisionTool.Check(vrt, mx, my) || ancs.Any(x => CollisionTool.Check(MathTool.MakeRectangle(x.Position, ANC_SZ * 2), new SKPoint(mx, my))))
                                        {
                                            p.IsAntialias = true;
                                            foreach (var anc in ancs)
                                            {
                                                if (anc.Name != "move")
                                                {
                                                    p.IsStroke = false;
                                                    p.Color = SKColors.White;
                                                    canvas.DrawCircle(anc.Position, ANC_SZ, p);

                                                    p.IsStroke = true;
                                                    p.StrokeWidth = 1;
                                                    p.Color = SKColors.Black;
                                                    canvas.DrawCircle(anc.Position, ANC_SZ, p);
                                                }
                                                else
                                                {
                                                    var ic = "fa-arrows-up-down-left-right";
                                                    var irt = MathTool.MakeRectangle(anc.Position, 12);
                                                    Util.DrawIcon(canvas, ic, 9, irt, SKColors.White, SKColors.Black, 2);
                                                }
                                            }
                                            p.IsAntialias = false;
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        #endregion
                    }
                }

                using(new SKAutoCanvasRestore(canvas))
                {
                    if (rt != null)
                    {
                        var crt = rt.Value; crt.Inflate(1, 1);
                        canvas.Translate(rt.Value.Left, rt.Value.Top);

                        #region Selected Drag
                        if (ptDown.HasValue && ptMove.HasValue)
                        {
                            var drt = MathTool.MakeRectangle(ptDown.Value, ptMove.Value);

                            p.IsStroke = true;
                            p.Color = SKColors.Gray;
                            p.StrokeWidth = 1;
                            p.PathEffect = pe;
                            canvas.DrawRect(drt, p);
                            p.PathEffect = null;
                        }
                        #endregion
                    }
                }
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

                    ptDown = new SKPoint(x, y);
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

                    if (ptDown.HasValue) ptMove = new SKPoint(x, y);
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

                    var (con, cx, cy) = container(x, y);
                    if (con != null)
                    {
                        if (ptDown.HasValue)
                        {
                            #region search
                            List<IGoControl>? vsels = null;
                            if (Math.Abs(MathTool.GetDistance(new SKPoint(x, y), ptDown.Value)) > 3)
                            {
                                var vp = containerpos(con, ptDown.Value.X, ptDown.Value.Y);
                                var pc = prj.Design.ControlStack(con, vp.X, vp.Y).LastOrDefault();
                                var drt = MathTool.MakeRectangle(new SKPoint(x, y), ptDown.Value);
                                drt.Offset(cx - x, cy - y);
                                vsels = prj.Design.ControlStack(con, drt).Where(x => x.Parent == pc).ToList();
                            }
                            else
                            {
                                var ls = prj.Design.ControlStack(con, cx, cy);
                                var vsel = ls.LastOrDefault();
                                if (vsel != null) vsels = [vsel];
                            }
                            #endregion
                            #region select
                            if (vsels != null)
                            {
                                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                                {
                                    foreach (var v in vsels)
                                        if (sels.Contains(v)) sels.Remove(v);
                                        else sels.Add(v);
                                }
                                else if ((ModifierKeys & Keys.Control) == Keys.Control)
                                {
                                    foreach (var v in vsels)
                                        if (sels.Contains(v)) sels.Remove(v);
                                        else sels.Add(v);
                                }
                                else
                                {
                                    sels.Clear();
                                    foreach (var v in vsels) sels.Add(v);
                                }
                            }
                            #endregion
                        }
                    }
                }

                ptDown = ptMove = null;
            }
            base.OnMouseUp(e);
        }

        #region MouseWheel
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

                    var (con, cx, cy) = container(x, y);
                    if (con != null)
                    {
                        var ls = prj.Design.ControlStack(con, cx, cy);
                        var vc = ls.LastOrDefault();

                        #region Wheel
                        if (vc is GoScrollablePanel sc)
                        {
                            var vx = x - sc.ScreenX;
                            var vy = y - sc.ScreenY;
                            sc.FireMouseWheel(vx, vy, delta);
                        }
                        #endregion
                    }
                }
            }
            base.OnMouseWheel(e);
        }
        #endregion
        #endregion

        #region Key
        #region ProcessCmdKey
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (Focused)
            {
                if (msg.Msg == 0x100 || msg.Msg == 0x104)
                {
                    switch (keyData)
                    {
                        case Keys.Control | Keys.C: Copy(); break;
                        case Keys.Control | Keys.X: Cut(); break;
                        case Keys.Control | Keys.V: Paste(); break;

                        case Keys.Control | Keys.Z: Undo(); break;
                        case Keys.Control | Keys.Y: Redo(); break;

                        case Keys.Delete: Delete(); break;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
        #endregion

        #region Drag
        #region OnDragOver
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
        #endregion
        #region OnDragDrop
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            var prj = Program.CurrentProject;
            this.Activate();
            if (prj != null)
            {
                var rt = GetBounds();
                var pt = PointToClient(new Point(drgevent.X, drgevent.Y));
                var v = drgevent.Data?.GetData(typeof(GoToolItem)) as GoToolItem;
                if (v != null && rt.HasValue && CollisionTool.Check(rt.Value, pt.X, pt.Y) && v.Tag is Type tp)
                {
                    int x = Convert.ToInt32(pt.X - rt.Value.Left);
                    int y = Convert.ToInt32(pt.Y - rt.Value.Top);

                    if (Target is GoDesign design2)
                    {
                        var (rtL, rtT, rtR, rtB, rtF, rtFR) = design2.DesignTimeBounds();

                        if (design2.UseLeftSideBar && CollisionTool.Check(rtL, x, y))
                            mousepos(design2.LeftSideBar, x, y, (con, vx, vy) => DT_Drop(con, vx, vy, tp));
                        else if (design2.UseRightSideBar && CollisionTool.Check(rtR, x, y))
                            mousepos(design2.RightSideBar, x, y, (con, vx, vy) => DT_Drop(con, vx, vy, tp));
                        else if (design2.UseTitleBar && CollisionTool.Check(rtT, x, y))
                            mousepos(design2.TitleBar, x, y, (con, vx, vy) => DT_Drop(con, vx, vy, tp));
                        else if (design2.UseFooter && CollisionTool.Check(rtB, x, y))
                            mousepos(design2.Footer, x, y, (con, vx, vy) => DT_Drop(con, vx, vy, tp));
                    }
                    else if (Target is GoPage page2)
                        mousepos(page2, x, y, (con, vx, vy) => DT_Drop(con, vx, vy, tp));
                    else if (Target is GoWindow wnd2)
                        mousepos(wnd2, x, y, (con, vx, vy) => DT_Drop(con, vx, vy, tp));
                }
                base.OnDragDrop(drgevent);
            }
            Invalidate();
        }
        #endregion
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

        #region Action
        public void TransAction(Action act)
        {
            if (actmgr != null) using (Transaction.Create(actmgr)) act();
        }

        public void EditObject(object obj, PropertyInfo Info, object? oldval, object? newval)
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                actmgr.RecordAction(new EditAction(obj, Info, oldval, newval));
                p.Edit = true;
            }
        }

        public void AddControl(IGoContainer container, GoControl control)
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                actmgr.RecordAction(new ControlAddAction(container, control));
                sels.Clear();
                sels.Add(control);
                p.Edit = true;
            }
        }

        public void DeleteControl(GoControl control)
        {
            sels.Clear();

            var p = Program.CurrentProject;
            if (p != null && control.Parent != null)
            {
                actmgr.RecordAction(new ControlDeleteAction(control.Parent, control));
                p.Edit = true;
            }
        }
        #endregion

        #region Copy / Cut / Paste / Escape
        void Copy()
        {
            var s = JsonSerializer.Serialize(sels, GoJsonConverter.Options);
            Clipboard.SetData("going-control", s);
        }
        void Cut()
        {
            Copy();
            Delete();
        }
        
        void Paste()
        {
            var s = Clipboard.GetData("going-control") as string;
            if (s != null)
            {
                var r = Regex.Replace(s, @"""Id""\s*:\s*""[^""]+""", match => $@"""Id"": ""{Guid.NewGuid()}""");
                var lso = JsonSerializer.Deserialize<List<IGoControl>>(s, GoJsonConverter.Options);
                var ls = JsonSerializer.Deserialize<List<IGoControl>>(r, GoJsonConverter.Options);

                var (con, _, _) = container(0, 0);
                var vcon = sels.FirstOrDefault() as IGoContainer ?? con;
                if (vcon != null && ls != null)
                {
                    foreach (var c in ls)
                    {
                        if (vcon.Childrens is GoTableLayoutControlCollection tcc) { }
                        else if (vcon.Childrens is GoGridLayoutControlCollection gcc) { }
                        else if (vcon.Childrens is List<IGoControl> vls) vls.Add(c);
                    }
                    sels = ls;
                }
            }
        }
        #endregion

        #region Undo / Redo
        public void Undo()
        {
            var p = Program.CurrentProject;
            if (p != null && CanUndo) { actmgr?.Undo(); p.Edit = true; }
        }
        public void Redo()
        {
            var p = Program.CurrentProject;
            if (p != null && CanRedo) { actmgr?.Redo(); p.Edit = true; }
        }
        #endregion

        #region Delete 
        void Delete()
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                TransAction(() =>
                {
                    var ls = sels.ToList();
                    sels.Clear();

                    foreach (var v in ls)
                        if (v is GoControl c)
                            DeleteControl(c);
                });
            }
        }
        #endregion

        #region DesignTime
        #region Drop
        void DT_Drop(IGoContainer con, int x, int y, Type tp)
        {
            var v = Activator.CreateInstance(tp);
            if (v is GoControl vc)
            {
                vc.Left = x; vc.Top = y; vc.Width = 80; vc.Height = 40;
                TransAction(() => { AddControl(con, vc); });
            }
        }
        #endregion
        #endregion
              
        #region Tool
        #region mousepos
        void mousepos(IGoContainer con, int x, int y, Action<IGoContainer, int, int> act)
        {
            if (con is GoControl c)
            {
                var vx = Convert.ToInt32(x - c.Left - con.PanelBounds.Left);
                var vy = Convert.ToInt32(y - c.Top - con.PanelBounds.Top);
                act(con, vx, vy);
            }
            else
            {
                var vx = Convert.ToInt32(x - con.PanelBounds.Left);
                var vy = Convert.ToInt32(y - con.PanelBounds.Top);
                act(con, vx, vy);
            }
        }
        #endregion

        #region container
        (IGoContainer? con, int cx, int cy)  container(int x, int y)
        {
            IGoContainer? con = null;

            if (Target is GoDesign design2)
            {
                var (rtL, rtT, rtR, rtB, rtF, rtFR) = design2.DesignTimeBounds();

                if (design2.UseLeftSideBar && CollisionTool.Check(rtL, x, y)) con = design2.LeftSideBar;
                else if (design2.UseRightSideBar && CollisionTool.Check(rtR, x, y)) con = design2.RightSideBar;
                else if (design2.UseTitleBar && CollisionTool.Check(rtT, x, y)) con = design2.TitleBar;
                else if (design2.UseFooter && CollisionTool.Check(rtB, x, y)) con = design2.Footer;
            }
            else if (Target is GoPage page2) con = page2;
            else if (Target is GoWindow wnd2) con = wnd2;

            if (con != null)
            {
                var p = containerpos(con, x, y);
                return (con, Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
            }
            else return (con, x, y);
        }
        #endregion
        #region containerpos
        SKPoint containerpos(IGoContainer con, float x, float y)
        {
            var cx = x - (con is GoControl cc1 ? cc1.Left : 0) - con.PanelBounds.Left + con.ViewPosition.X;
            var cy = y - (con is GoControl cc2 ? cc2.Top : 0) - con.PanelBounds.Top + con.ViewPosition.Y;
            return new SKPoint(cx, cy);
        }
        #endregion
        #endregion
        #endregion
    }

    #region EditAction
    public class EditAction : GuiLabs.Undo.AbstractAction
    {
        object targetItem;
        PropertyInfo pi;
        object? newval;
        object? oldval;

        public EditAction(object targetItem, PropertyInfo pi, object? oldval, object? newval)
        {
            this.targetItem = targetItem;
            this.pi = pi;
            this.newval = newval;
            this.oldval = oldval;
        }

        protected override void ExecuteCore() => pi.SetValue(targetItem, newval);
        protected override void UnExecuteCore() => pi.SetValue(targetItem, oldval);
    }
    #endregion
    #region ControlAddAction 
    public class ControlAddAction : AbstractAction
    {
        IGoContainer container;
        GoControl control;
        int col, row, colspan, rowspan;

        public ControlAddAction(IGoContainer container, GoControl control)
        {
            this.container = container;
            this.control = control;
        }

        public ControlAddAction(IGoContainer container, GoControl control, int col, int row, int colspan, int rowspan)
        {
            this.container = container;
            this.control = control;
            this.col = col;
            this.row = row;
            this.colspan = colspan;
            this.rowspan = rowspan;
        }

        public ControlAddAction(IGoContainer container, GoControl control, int col, int row)
        {
            this.container = container;
            this.control = control;
            this.col = col;
            this.row = row;
        }

        protected override void ExecuteCore()
        {
            if (container.Childrens is List<IGoControl> ls) ls.Add(control);
            else if (container.Childrens is GoGridLayoutControlCollection gls) gls.Add(control, col, row);
            else if (container.Childrens is GoTableLayoutControlCollection tls) tls.Add(control, col, row, colspan, rowspan);
        }
        
        protected override void UnExecuteCore()
        {
            if (container.Childrens is List<IGoControl> ls) ls.Remove(control);
            else if (container.Childrens is GoGridLayoutControlCollection gls) gls.Remove(control);
            else if (container.Childrens is GoTableLayoutControlCollection tls) tls.Remove(control);
        }
    }
    #endregion
    #region ControlDeleteAction
    public class ControlDeleteAction : AbstractAction
    {
        IGoContainer container;
        GoControl control;
        int col, row, colspan, rowspan;

        public ControlDeleteAction(IGoContainer container, GoControl control)
        {
            this.container = container;
            this.control = control;

            if (container.Childrens is GoGridLayoutControlCollection gls && gls.Contains(control))
            {
                var v = gls[control];
                if (v != null) { col = v.Column; row = v.Row; }
            }
            else if (container.Childrens is GoTableLayoutControlCollection tls && tls.Contains(control))
            {
                var v = tls[control];
                if (v != null) { col = v.Column; row = v.Row; colspan = v.ColSpan; rowspan = v.RowSpan; }
            }
        }

        protected override void ExecuteCore()
        {
            if (container.Childrens is List<IGoControl> ls) ls.Remove(control);
            else if (container.Childrens is GoGridLayoutControlCollection gls) gls.Remove(control);
            else if (container.Childrens is GoTableLayoutControlCollection tls) tls.Remove(control);
        }
        protected override void UnExecuteCore()
        {
            if (container.Childrens is List<IGoControl> ls) ls.Add(control);
            else if (container.Childrens is GoGridLayoutControlCollection gls) gls.Add(control, col, row);
            else if (container.Childrens is GoTableLayoutControlCollection tls) tls.Add(control, col, row, colspan, rowspan);
        }
    }
    #endregion
    #region Anchor
    public class Anchor
    {
        public GoControl Control { get; set; }
        public string Name { get; set; }
        public SKPoint Position { get; set; }
    }
    #endregion

}
