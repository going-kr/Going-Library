using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Forms.Controls;
using Going.UI.Json;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using Going.UIEditor.Utils;
using GuiLabs.Undo;
using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.ES20;
using SkiaSharp;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Cursor = System.Windows.Forms.Cursor;
using GoControl = Going.UI.Controls.GoControl;
using Keys = System.Windows.Forms.Keys;
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
        List<IGoControl> sels { get; } = [];

        Anchor? dragAnchor;

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
                prj.Design.DesignMode = true;
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
                        #region base
                        var crt = rt.Value; crt.Inflate(1, 1);
                        canvas.ClipRect(crt);
                        canvas.Translate(rt.Value.Left, rt.Value.Top);
                        canvas.Clear(thm.Back);
                        #endregion

                        #region draw
                        prj.Design.SetSize(Convert.ToInt32(rt.Value.Width), Convert.ToInt32(rt.Value.Height));

                        if (Target is GoDesign design2) prj.Design.DrawPage(canvas, null);
                        else if (Target is GoPage page2) prj.Design.DrawPage(canvas, page2);
                        else if (Target is GoWindow wnd2) wnd2.FireDraw(canvas);
                        #endregion
                    }
                }

                using (new SKAutoCanvasRestore(canvas))
                {
                    if (rt != null)
                    {
                        var crt = rt.Value; crt.Inflate(1, 1);
                        canvas.Translate(rt.Value.Left, rt.Value.Top);

                        if (dragAnchor != null)
                        {
                            #region drag anchor
                            if (ptDown.HasValue && ptMove.HasValue)
                            {
                                var gx = ptMove.Value.X - ptDown.Value.X;
                                var gy = ptMove.Value.Y - ptDown.Value.Y;

                                foreach (var c in sels.Where(x => x.Parent == dragAnchor.Control.Parent || dragAnchor.Name != "move"))
                                {
                                    if (!isTopLevelContainer(c))
                                    {
                                        var nrt = calcbox(dragAnchor.Name, Util.FromRect(c.ScreenX, c.ScreenY, c.Width, c.Height), gx, gy);
                                        var (vx, vy) = containerviewpos(c.Parent);
                                        nrt.Offset(-vx, -vy);

                                        p.IsStroke = true;
                                        p.Color = SKColors.Red;
                                        p.StrokeWidth = 1;
                                        p.PathEffect = pe;
                                        canvas.DrawRect(nrt, p);
                                        p.PathEffect = null;
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region selected control
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

                            foreach (var c in sels)
                            {
                                if (c is GoControl vc)
                                {
                                    using (new SKAutoCanvasRestore(canvas))
                                    {
                                        #region rt
                                        var vrt = Util.FromRect(vc.ScreenX, vc.ScreenY, vc.Width, vc.Height);
                                        var (vx, vy) = containerviewpos(vc.Parent);
                                        vrt.Offset(-vx, -vy);
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
                                            int x = Convert.ToInt32(ev.X - rt.Value.Left);
                                            int y = Convert.ToInt32(ev.Y - rt.Value.Top);

                                            var ancs = Util2.GetAnchors(vc, vrt);

                                            if (CollisionTool.Check(vrt, x, y) || ancs.Any(a => CollisionTool.Check(MathTool.MakeRectangle(a.Position, ANC_SZ * 2), new SKPoint(x, y))))
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
                }
            }

            base.OnContentDraw(e);
        }
        #endregion

        #region Mouse
        #region OnMouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var rt = GetBounds();
                if (rt.HasValue)
                {
                    #region mouse pos
                    int x = Convert.ToInt32(e.X - rt.Value.Left);
                    int y = Convert.ToInt32(e.Y - rt.Value.Top);
                    ptDown = new SKPoint(x, y);
                    #endregion

                    #region anchor search
                    Anchor? a = selanchor(x, y);
                    if (a != null) dragAnchor = a;
                    #endregion
                }
            }
            base.OnMouseDown(e);
        }
        #endregion
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var rt = GetBounds();
                if (rt.HasValue)
                {
                    #region mouse pos
                    int x = Convert.ToInt32(e.X - rt.Value.Left);
                    int y = Convert.ToInt32(e.Y - rt.Value.Top);
                    if (ptDown.HasValue) ptMove = new SKPoint(x, y);
                    #endregion

                    #region anchor cursor
                    var cur = Cursors.Default;
                    if (dragAnchor != null)
                    {
                        #region check target container 
                        (IGoContainer con, int x, int y, Dictionary<IGoControl, SKPoint> gps)? target = null;
                        if (dragAnchor.Name == "move")
                        {
                            var (con, cx, cy) = target_container(x, y, dragAnchor.Control);
                            if (con != null) target = (con, cx, cy, sels.ToDictionary(x => x, y => new SKPoint(dragAnchor.Control.ScreenX - y.ScreenX, dragAnchor.Control.ScreenY - y.ScreenY)));
                        }
                        #endregion

                        if (target.HasValue)
                        {
                            if (target.Value.con is GoTableLayoutPanel tpnl)
                            {
                                #region table layout
                                cur = Cursors.No;

                                if (sels.Count == 1)
                                {
                                    #region move
                                    if (dragAnchor.Name == "move" && tableCollision(tpnl, target.Value.x, target.Value.y, dragAnchor.Control, out var tidx) && tidx.Control == null)
                                        cur = cursor(dragAnchor.Name);
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                #region other
                                cur = cursor(dragAnchor.Name);
                                #endregion
                            }
                        }
                        else cur = Cursors.No;
                    }
                    else
                    {
                        Anchor? a = selanchor(x, y);
                        if (a != null) cur = cursor(a.Name);
                    }
                    Cursor = cur;
                    #endregion
                }
            }
            base.OnMouseMove(e);
        }
        #endregion
        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var rt = GetBounds();
                if (rt.HasValue)
                {
                    #region mouse pos
                    int x = Convert.ToInt32(e.X - rt.Value.Left);
                    int y = Convert.ToInt32(e.Y - rt.Value.Top);
                    #endregion

                    if (dragAnchor != null)
                    {
                        #region anchor drag
                        if (ptDown.HasValue && ptMove.HasValue)
                        {
                            var gx = ptMove.Value.X - ptDown.Value.X;
                            var gy = ptMove.Value.Y - ptDown.Value.Y;

                            #region check target container 
                            (IGoContainer con, int x, int y, Dictionary<IGoControl, SKPoint> gps)? target = null;
                            if (dragAnchor.Name == "move")
                            {
                                var (con, cx, cy) = target_container(x, y, dragAnchor.Control);
                                if (con != null) target = (con, cx, cy, sels.ToDictionary(x => x, y => new SKPoint(dragAnchor.Control.ScreenX - y.ScreenX, dragAnchor.Control.ScreenY - y.ScreenY)));
                            }
                            #endregion

                            if (target.HasValue)
                            {
                                if (target.Value.con is GoTableLayoutPanel tpnl)
                                {
                                    #region table layout
                                    if (sels.Count == 1)
                                    {
                                        #region move
                                        if (dragAnchor.Name == "move" && tableCollision(tpnl, target.Value.x, target.Value.y, dragAnchor.Control, out var tidx) && tidx.Control == null)
                                        {
                                            DeleteControl(dragAnchor.Control);
                                            AddControl(tpnl, dragAnchor.Control, tidx.Col, tidx.Row, tidx.Colspan, tidx.Rowspan);
                                        }
                                        #endregion
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region other
                                    TransAction(() =>
                                    {
                                        var pi = typeof(IGoControl).GetProperty("Bounds");
                                        if (pi != null)
                                        {
                                            foreach (var c in sels.Where(x => (x.Parent == dragAnchor.Control.Parent) || dragAnchor.Name != "move"))
                                            {
                                                if (!isTopLevelContainer(c))
                                                {
                                                    if (dragAnchor.Name == "move" && target.HasValue && c.Parent != target.Value.con)
                                                    {
                                                        #region parent 변경 시
                                                        DeleteControl(c);
                                                        AddControl(target.Value.con, c);
                                                        var ort = c.Bounds;
                                                        var nrt = Util.FromRect(target.Value.x - target.Value.gps[c].X - c.Width / 2, target.Value.y - target.Value.gps[c].Y - c.Height / 2, c.Width, c.Height);
                                                        EditObject(c, pi, ort, nrt);
                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        #region parent 동일 시
                                                        var ort = c.Bounds;
                                                        var nrt = calcbox(dragAnchor.Name, c.Bounds, gx, gy);
                                                        EditObject(c, pi, ort, nrt);
                                                        #endregion
                                                    }
                                                }
                                            }

                                            SelectedControl([.. sels], [.. sels]);
                                        }
                                    });
                                    #endregion
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region control select
                        var (con, cx, cy) = container(x, y);
                        if (con != null && ptDown.HasValue)
                        {
                            #region search
                            List<IGoControl>? vsels = null;
                            if (Math.Abs(MathTool.GetDistance(new SKPoint(x, y), ptDown.Value)) > 3)
                            {
                                var pc = prj.Design.ControlStack(con, cx, cy).LastOrDefault();
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
                        #endregion
                    }
                }

                #region release
                dragAnchor = null;
                ptDown = ptMove = null;
                #endregion
            }
            base.OnMouseUp(e);
        }
        #endregion
        #region OnMouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var rt = GetBounds();
                if (rt.HasValue)
                {
                    #region mousepos
                    int x = Convert.ToInt32(e.X - rt.Value.Left);
                    int y = Convert.ToInt32(e.Y - rt.Value.Top);
                    float delta = e.Delta / 120F;
                    #endregion

                    #region wheel
                    var (con, cx, cy) = container(x, y);
                    if (con != null)
                    {
                        var ls = prj.Design.ControlStack(con, cx, cy);
                        var vc = ls.LastOrDefault();
                        if (vc is GoScrollablePanel sc)
                        {
                            var vx = x - sc.ScreenX;
                            var vy = y - sc.ScreenY;
                            sc.FireMouseWheel(vx, vy, delta);
                        }
                    }
                    #endregion
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

                var ef = DragDropEffects.None;
                if (v != null && rt.HasValue && CollisionTool.Check(rt.Value, pt.X, pt.Y))
                {
                    int x = Convert.ToInt32(pt.X - rt.Value.Left);
                    int y = Convert.ToInt32(pt.Y - rt.Value.Top);

                    var (con, cx, cy) = target_container(x, y);
                    if (con != null)
                    {
                        if (con is GoTableLayoutPanel tpnl)
                            ef = tableIndex(tpnl, cx, cy, out var idx) && idx.Control == null ? DragDropEffects.All : DragDropEffects.None;
                        else
                            ef = DragDropEffects.All;
                    }
                }
                drgevent.Effect = ef;
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

                    var (con, cx, cy) = target_container(x, y);
                    if (con != null)
                    {
                        var nc = Activator.CreateInstance(tp);
                        if (nc is GoControl vc)
                        {
                            vc.FireInit(prj.Design);
                            #region default value
                            vc.Left = cx; vc.Top = cy; vc.Width = 80; vc.Height = 40;
                            if (vc is GoTableLayoutPanel tpnl)
                            {
                                tpnl.Rows = ["50%", "50%"];
                                tpnl.Columns = ["50%", "50%"];
                            }
                            #endregion

                            TransAction(() =>
                            {
                                if (con is GoTableLayoutPanel tpnl && tableIndex(tpnl, cx, cy, out var tidx) && tidx.Control == null)
                                {
                                    AddControl(tpnl, vc, tidx.Col, tidx.Row, 1, 1);
                                    SelectedControl([.. sels], [vc]);
                                }
                                else
                                {
                                    AddControl(con, vc);
                                    SelectedControl([.. sels], [vc]);
                                }

                            });
                        }
                    }

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
            if (actmgr != null)
                using (var trans = Transaction.Create(actmgr))
                {
                    act();
                }
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

        public void AddControl(IGoContainer container, IGoControl control)
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                actmgr.RecordAction(new ControlAddAction(container, control));
                p.Edit = true;
            }
        }

        public void AddControl(GoTableLayoutPanel container, IGoControl control, int col, int row, int colspan, int rowspan)
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                actmgr.RecordAction(new ControlAddAction(container, control, col, row, colspan, rowspan));
                p.Edit = true;
            }
        }

        public void AddControl(GoGridLayoutPanel container, IGoControl control, int col, int row)
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                actmgr.RecordAction(new ControlAddAction(container, control, col, row));
                p.Edit = true;
            }
        }

        public void DeleteControl(IGoControl control)
        {
            var p = Program.CurrentProject;
            if (p != null && control.Parent != null && !isTopLevelContainer(control))
            {
                actmgr.RecordAction(new ControlDeleteAction(control.Parent, control));
                p.Edit = true;
            }
        }

        public void SelectedControl(IEnumerable<IGoControl> olds, IEnumerable<IGoControl> news)
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                actmgr.RecordAction(new SelectedAction(sels, news, olds));
                p.Edit = true;
            }
        }
        #endregion

        #region Edit
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
                    TransAction(() =>
                    {
                        foreach (var c in ls)
                        {
                            if (c is GoControl vc)
                            {
                                if (vcon.Childrens is GoTableLayoutControlCollection tcc) { }
                                else if (vcon.Childrens is GoGridLayoutControlCollection gcc) { }
                                else if (vcon.Childrens is List<IGoControl> vls)
                                {
                                    var rt = c.Bounds; rt.Offset(10, 10); c.Bounds = rt;
                                    AddControl(vcon, c);
                                }
                            }
                        }

                        SelectedControl([.. sels], ls);
                    });
                }
            }
        }
        #endregion

        #region Undo / Redo
        public void Undo()
        {
            var p = Program.CurrentProject;
            if (p != null && CanUndo)
            {
                actmgr?.Undo();
                p.Edit = true;
            }
        }
        public void Redo()
        {
            var p = Program.CurrentProject;
            if (p != null && CanRedo)
            {
                actmgr?.Redo(); p.Edit = true;
            }
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
                    foreach (var v in sels.ToArray())
                        if (v is GoControl c)
                            DeleteControl(c);

                    SelectedControl([.. sels], []);
                });
            }
        }
        #endregion
        #endregion

        #region Tool
        #region container
        (IGoContainer? con, int cx, int cy) container(int x, int y)
        {
            IGoContainer? con = null;

            if (Target is GoDesign design2)
            {
                var (rtL, rtT, rtR, rtB, rtF, rtFR) = design2.LayoutBounds();

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
                return (con, Convert.ToInt32(p.x), Convert.ToInt32(p.y));
            }
            else return (con, x, y);
        }
        #endregion

        #region containerpos
        (int x, int y) containerpos(IGoContainer con, float x, float y)
        {
            var (vx, vy) = containerviewpos(con);
            var cx = x - (con is GoControl cc1 ? cc1.ScreenX : 0) - con.PanelBounds.Left + vx;
            var cy = y - (con is GoControl cc2 ? cc2.ScreenY : 0) - con.PanelBounds.Top + vy;
            return (Convert.ToInt32(cx), Convert.ToInt32(cy));
        }
        #endregion

        #region containerviewpos
        (float x, float y) containerviewpos(IGoContainer? con)
        {
            var vx = 0F;
            var vy = 0F;

            var vcon = con;
            while (vcon != null)
            {
                vx += vcon.ViewPosition.X;
                vy += vcon.ViewPosition.Y;

                if (vcon is IGoControl c) vcon = c.Parent;
            }

            return (vx, vy);
        }
        #endregion

        #region cursor
        Cursor cursor(string? anchorName)
        {
            var cur = Cursors.Default;
            if (anchorName != null)
                switch (anchorName)
                {
                    case "l": cur = Cursors.SizeWE; break;
                    case "r": cur = Cursors.SizeWE; break;
                    case "t": cur = Cursors.SizeNS; break;
                    case "b": cur = Cursors.SizeNS; break;

                    case "lt": cur = Cursors.SizeNWSE; break;
                    case "rt": cur = Cursors.SizeNESW; break;
                    case "lb": cur = Cursors.SizeNESW; break;
                    case "rb": cur = Cursors.SizeNWSE; break;

                    case "move": cur = Cursors.SizeAll; break;
                }
            return cur;
        }
        #endregion

        #region selanchor
        Anchor? selanchor(int x, int y)
        {
            Anchor? a = null;
            foreach (var c in sels)
            {
                #region rt
                var vrt = Util.FromRect(c.ScreenX, c.ScreenY, c.Width, c.Height);
                var (vx, vy) = containerviewpos(c.Parent);
                vrt.Offset(-vx, -vy);
                #endregion

                if (c is GoControl vc)
                {
                    var ancs = Util2.GetAnchors(vc, vrt);
                    a ??= ancs.FirstOrDefault(a => CollisionTool.Check(MathTool.MakeRectangle(a.Position, ANC_SZ * 2), x, y));
                }
            }
            return a;
        }
        #endregion

        #region isTopLevelContainer
        bool isTopLevelContainer(IGoControl c) => c is GoPage || c is GoWindow || c is GoTitleBar || c is GoSideBar || c is GoFooter;
        #endregion

        #region calcbox
        SKRect calcbox(string anchorName, SKRect rt, float gx, float gy)
        {
            var nrt = rt;
            switch (anchorName)
            {
                case "move": nrt.Offset(gx, gy); break;
                case "l": nrt.Left += gx; break;
                case "r": nrt.Right += gx; break;
                case "t": nrt.Top += gy; break;
                case "b": nrt.Bottom += gy; break;

                case "lt": nrt.Left += gx; nrt.Top += gy; break;
                case "rt": nrt.Right += gx; nrt.Top += gy; break;
                case "lb": nrt.Left += gx; nrt.Bottom += gy; break;
                case "rb": nrt.Right += gx; nrt.Bottom += gy; break;
            }
            return nrt;
        }
        #endregion

        #region tableIndex
        bool tableIndex(GoTableLayoutPanel tpnl, int x, int y, out TableIndex idx)
        {
            idx = new TableIndex { Col = -1, Row = -1, Colspan = 1, Rowspan = 1, Control = null };

            var ret = false;
            var rt = tpnl.Areas()["Content"];
            var rts = Util.Grid(rt, [.. tpnl.Columns], [.. tpnl.Rows]);
            for (int ir = 0; ir < tpnl.Rows.Count; ir++)
                for (int ic = 0; ic < tpnl.Columns.Count; ic++)
                    if (CollisionTool.Check(rts[ir, ic], x, y))
                    {
                        idx.Col = ic;
                        idx.Row = ir;
                        idx.Control = tpnl.Childrens[ic, ir];
                        ret = true;
                    }

            return ret;
        }
        
        bool tableCollision(GoTableLayoutPanel tpnl, int x, int y, IGoControl c, out TableIndex idx)
        {
            idx = new TableIndex { Col = -1, Row = -1, Colspan = 1, Rowspan = 1, Control = null };

            var ri = tpnl.Childrens[c];
            if (ri != null) { idx.Colspan = ri.ColSpan; idx.Rowspan = ri.RowSpan; }

            var ret = false;
            var rt = tpnl.Areas()["Content"];
            var rts = Util.Grid(rt, [.. tpnl.Columns], [.. tpnl.Rows]);
            for (int ir = 0; ir < tpnl.Rows.Count; ir++)
                for (int ic = 0; ic < tpnl.Columns.Count; ic++)
                    if (CollisionTool.Check(rts[ir, ic], x, y))
                    {
                        var srt = Util.Merge(rts, ic, ir, idx.Colspan, idx.Rowspan); srt.Inflate(-1, -1);
                        var vidx = tpnl.Childrens.Indexes.FirstOrDefault(ti =>
                        {
                            var drt = Util.Merge(rts, ti.Value.Column, ti.Value.Row, ti.Value.ColSpan, ti.Value.RowSpan);
                            drt.Inflate(-1, -1);
                            return CollisionTool.Check(drt, srt);
                        }).Value;
                        var vc = vidx != null && tpnl.Childrens[vidx.Column, vidx.Row] != c ? tpnl.Childrens[vidx.Column, vidx.Row] : null;

                        idx.Col = ic;
                        idx.Row = ir;
                        idx.Control = vc;
                        ret = vc == null;
                    }

            return ret;
        }
        #endregion

        #region target_container
        (IGoContainer? con, int cx, int cy) target_container(int x, int y, IGoControl? dragControl = null)
        {
            var prj = Program.CurrentProject;
            var (con, cx, cy) = container(x, y);
            if (con != null && prj != null)
            {
                if (dragControl != null)
                {
                    var cls = prj.Design.ControlStack(con, cx, cy);
                    var idx = cls.IndexOf(dragControl);
                    if (idx >= 0) cls = cls.GetRange(0, idx);
                    var pc = cls.LastOrDefault();
                    if (pc is IGoContainer vcon)
                    {
                        con = vcon;
                        (cx, cy) = containerpos(con, x, y);
                    }
                    else
                    {
                        con = null;
                    }
                }
                else
                {
                    var pc = prj.Design.ControlStack(con, cx, cy).LastOrDefault();
                    if (pc is IGoContainer vcon)
                    {
                        con = vcon;
                        (cx, cy) = containerpos(con, x, y);
                    }
                }
            }

            return (con, cx, cy);
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

        protected override void ExecuteCore()
        {
            pi.SetValue(targetItem, newval);
            Debug.WriteLine($"do : mvoe");
        }

        protected override void UnExecuteCore()
        {
            pi.SetValue(targetItem, oldval);
            Debug.WriteLine($"undo : mvoe");
        }
    }
    #endregion
    #region ControlAddAction 
    public class ControlAddAction : AbstractAction
    {
        IGoContainer container;
        IGoControl control;
        int col, row, colspan, rowspan;

        public ControlAddAction(IGoContainer container, IGoControl control)
        {
            this.container = container;
            this.control = control;
        }

        public ControlAddAction(IGoContainer container, IGoControl control, int col, int row, int colspan, int rowspan)
        {
            this.container = container;
            this.control = control;
            this.col = col;
            this.row = row;
            this.colspan = colspan;
            this.rowspan = rowspan;
        }

        public ControlAddAction(IGoContainer container, IGoControl control, int col, int row)
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
        IGoControl control;
        int col, row, colspan, rowspan;

        public ControlDeleteAction(IGoContainer container, IGoControl control)
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
    #region SelectedAction
    public class SelectedAction : AbstractAction
    {
        List<IGoControl> sels;
        IEnumerable<IGoControl> news, olds;

        public SelectedAction(List<IGoControl> sels, IEnumerable<IGoControl> news, IEnumerable<IGoControl> olds)
        {
            this.sels = sels;
            this.olds = olds;
            this.news = news;
        }

        protected override void ExecuteCore()
        {
            sels.Clear();
            sels.AddRange(news);
        }
        protected override void UnExecuteCore()
        {
            sels.Clear();
            sels.AddRange(olds);
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
    #region TalbeInde
    class TableIndex
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public int Colspan { get; set; }
        public int Rowspan { get; set; }
        public IGoControl? Control { get; set; }
    }
    #endregion

}
