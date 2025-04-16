using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
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

        IGoControl? downControl;
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
                                dragAnchorProc(dragAnchor, ptDown.Value, ptMove.Value,
                                #region tpnl
                                (vcon, vc, tidx) =>
                                {
                                    if (tidx != null && vcon is GoTableLayoutPanel tpnl)
                                    {
                                        var trt = Util.FromRect(tpnl.ScreenX, tpnl.ScreenY, tpnl.Width - 1, tpnl.Height - 1);
                                        var rts = Util.Grid(trt, [.. tpnl.Columns], [.. tpnl.Rows]);
                                        var vrt = Util.Merge(rts, tidx.Col, tidx.Row, tidx.Colspan, tidx.Rowspan);

                                        p.IsStroke = true;
                                        p.Color = SKColors.Red;
                                        p.StrokeWidth = 1;
                                        p.PathEffect = pe;
                                        canvas.DrawRect(vrt, p);
                                        p.PathEffect = null;
                                    }
                                },
                                #endregion
                                #region gpnl
                                (vcon, vc, gidx) =>
                                {
                                    if (gidx != null && vcon is GoGridLayoutPanel gpnl)
                                    {
                                        var rts = gpnl.GridBounds(Util.FromRect(gpnl.ScreenX, gpnl.ScreenY, gpnl.Width - 1, gpnl.Height - 1));
                                        var vrt = gpnl.CellBounds(rts, gidx.Col, gidx.Row);
                                        if (vrt.HasValue)
                                        {
                                            p.IsStroke = true;
                                            p.Color = SKColors.Red;
                                            p.StrokeWidth = 1;
                                            p.PathEffect = pe;
                                            canvas.DrawRect(vrt.Value, p);
                                            p.PathEffect = null;
                                        }
                                    }
                                },
                                #endregion
                                #region Other
                                (vcon, vc, srt, nrt) =>
                                {
                                    p.IsStroke = true;
                                    p.Color = SKColors.Red;
                                    p.StrokeWidth = 1;
                                    p.PathEffect = pe;
                                    canvas.DrawRect(srt, p);
                                    p.PathEffect = null;
                                }
                                #endregion
                                );
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
                                    #region rt
                                    var vrt = Util.FromRect(vc.ScreenX, vc.ScreenY, vc.Width, vc.Height);
                                    var (vx, vy) = containerviewpos(vc.Parent);
                                    vrt.Offset(-vx, -vy);
                                    #endregion

                                    using (new SKAutoCanvasRestore(canvas))
                                    {
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

                                            var ancs = Util2.GetAnchors(vc, vrt).Where(a => c.Parent is GoGridLayoutPanel ? a.Name == "move" : true); 

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

                                #region swpnl
                                if (c is GoSwitchPanel sw)
                                {
                                    #region rt
                                    var vrt = Util.FromRect(sw.ScreenX, sw.ScreenY, sw.Width, sw.Height);
                                    var (vx, vy) = containerviewpos(sw.Parent);
                                    vrt.Offset(-vx, -vy);
                                    #endregion

                                    var (rtA, rtT, rtP, rtN) = swpnl_toolbounds(sw, vrt);
                                    int idx = sw.SelectedPage != null ? sw.Pages.IndexOf(sw.SelectedPage) : -1;
                                    var s = sw.SelectedPage != null ? sw.SelectedPage.Name : $"Page {idx}";

                                    Util.DrawText(canvas, s, "나눔고딕", GoFontStyle.Normal, 12, rtT, thm.Base5);
                                    Util.DrawIcon(canvas, "fa-angle-left", 12, rtP, idx - 1 >= 0 ? thm.Base5 : thm.Base3);
                                    Util.DrawIcon(canvas, "fa-angle-right", 12, rtN, idx + 1 < sw.Pages.Count ? thm.Base5 : thm.Base3);
                                    #region box
                                    p.IsStroke = true;
                                    p.Color = SKColors.Red;
                                    p.StrokeWidth = 1;
                                    p.PathEffect = pe;
                                    canvas.DrawRect(rtA, p);
                                    p.PathEffect = null;
                                    #endregion
                                }
                                #endregion
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

                    #region spcial container (spnl/tab/swpnl) mouse event 
                    if (a == null)
                    {
                        var tc = target_control(x, y);
                        if (tc is GoScrollablePanel spnl)
                        {
                            #region scroll
                            var srts = spnl.Areas();
                            var rtV = srts["ScrollV"];
                            var rtH = srts["ScrollH"];

                            var sx = x - spnl.ScreenX;
                            var sy = y - spnl.ScreenY;
                            if (CollisionTool.Check(rtV, sx, sy) || CollisionTool.Check(rtH, sx, sy))
                            {
                                ptDown = null;
                                downControl = spnl;
                                downControl.FireMouseDown(sx, sy, ToGoMouseButton(e.Button));
                            }
                            #endregion
                        }
                        else if(tc is GoTabControl tab)
                        {
                            #region tab
                            var sx = x - tab.ScreenX;
                            var sy = y - tab.ScreenY;
                            if (CollisionTool.Check(tab.Areas()["Nav"], sx, sy))
                            {
                                ptDown = null;
                                downControl = tab;
                                downControl.FireMouseDown(sx, sy, ToGoMouseButton(e.Button));
                            }
                            #endregion
                        }
                        else 
                        {
                            #region sw
                            var tc2 = target_control(x, y + 20);
                            if (tc2 is GoSwitchPanel sw && sels.Contains(sw))
                            {
                                #region swpnl
                                var (rtA, rtT, rtP, rtN) = swpnl_toolbounds(sw, Util.FromRect(0, 0, sw.Width, sw.Height));
                                int idx = sw.SelectedPage != null ? sw.Pages.IndexOf(sw.SelectedPage) : -1;
                                #endregion

                                var sx = x - sw.ScreenX;
                                var sy = y - sw.ScreenY;

                                if (CollisionTool.Check(rtA, sx, sy))
                                {
                                    ptDown = null;

                                    if (CollisionTool.Check(rtP, sx, sy) && idx - 1 >= 0)
                                        sw.SetPage(sw.Pages[idx - 1].Name);
                                    else if (CollisionTool.Check(rtN, sx, sy) && idx + 1 < sw.Pages.Count)
                                        sw.SetPage(sw.Pages[idx + 1].Name);
                                }
                            }
                            #endregion
                        }
                    }
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
                        if (ptDown.HasValue && ptMove.HasValue)
                        {
                            dragAnchorProc(dragAnchor, ptDown.Value, ptMove.Value,
                            #region tpnl
                            (vcon, vc, tidx) => {
                                cur = tidx != null ? cursor(dragAnchor.Name) : Cursors.No;
                            },
                            #endregion
                            #region gpnl
                            (vcon, vc, gidx) =>
                            {
                                cur = gidx != null ? cursor(dragAnchor.Name) : Cursors.No;
                            },
                            #endregion
                            #region Other    
                            (vcon, vc, srt, nrt) => { cur = cursor(dragAnchor.Name); }
                            #endregion
                            );
                        }
                    }
                    else
                    {
                        Anchor? a = selanchor(x, y);
                        if (a != null) cur = cursor(a.Name);
                    }
                    Cursor = cur;
                    #endregion

                    #region spcial container (spnl/tab/swpnl) mouse event 
                    if (dragAnchor == null)
                    {
                        if (downControl is GoScrollablePanel spnl) spnl.FireMouseMove(x - spnl.ScreenX, y - spnl.ScreenY);
                    }
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
                            var pi = typeof(IGoControl).GetProperty("Bounds");
                            if (pi != null)
                            {
                                TransAction(() =>
                                {
                                    dragAnchorProc(dragAnchor, ptDown.Value, ptMove.Value,
                                    #region tpnl
                                    (vcon, vc, tidx) =>
                                    {
                                        if(tidx != null && vcon is GoTableLayoutPanel tpnl)
                                        {
                                            DeleteControl(dragAnchor.Control);
                                            AddControl(tpnl, dragAnchor.Control, tidx.Col, tidx.Row, tidx.Colspan, tidx.Rowspan);
                                        }
                                    },
                                    #endregion
                                    #region gpnl
                                    (vcon, vc, gidx) =>
                                    {
                                        if (gidx != null && vcon is GoGridLayoutPanel gpnl)
                                        {
                                            DeleteControl(dragAnchor.Control);
                                            AddControl(gpnl, dragAnchor.Control, gidx.Col, gidx.Row);
                                        }
                                    },
                                    #endregion
                                    #region other
                                    (vcon, vc, srt, nrt) =>
                                    {
                                        if (dragAnchor.Name == "move" && vc.Parent != vcon)
                                        {
                                            DeleteControl(vc);
                                            AddControl(vcon, vc);
                                            var ort = vc.Bounds;
                                            EditObject(vc, pi, ort, nrt);
                                        }
                                        else
                                        {
                                            var ort = vc.Bounds;
                                            EditObject(vc, pi, ort, nrt);
                                        }
                                    }
                                    #endregion
                                    );
                                });
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        if (ptDown.HasValue)
                        {
                            #region search
                            List<IGoControl>? vsels = null;
                            if (Math.Abs(MathTool.GetDistance(new SKPoint(x, y), ptDown.Value)) > 3)
                                vsels = target_control(ptDown.Value, new SKPoint(x, y));
                            else
                                vsels = target_control(x, y) is IGoControl c ? [c] : [];
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

                        #region spcial container (spnl/tab/swpnl) mouse event 
                        if (downControl is GoScrollablePanel spnl) spnl.FireMouseUp(x - spnl.ScreenX, y - spnl.ScreenY, ToGoMouseButton(e.Button));
                        else if(downControl is GoTabControl tab) tab.FireMouseUp(x - tab.ScreenX, y - tab.ScreenY, ToGoMouseButton(e.Button));
                        #endregion
                    }
                }

                #region release
                var clears1 = sels.Where(x => x.Parent is GoTabControl tab && !tab.Childrens.Contains(x)).ToArray();
                var clears2 = sels.Where(x => x.Parent is GoSwitchPanel swpnl && !swpnl.Childrens.Contains(x)).ToArray();
                foreach (var c in clears1) sels.Remove(c);
                foreach (var c in clears2) sels.Remove(c);

                downControl = null;
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
                    var cls = target_controlstack(x, y); 
                    if (cls.LastOrDefault(x => x is GoScrollablePanel) is GoScrollablePanel sc)
                    {
                        var vx = x - sc.ScreenX;
                        var vy = y - sc.ScreenY;
                        sc.FireMouseWheel(vx, vy, delta);
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
                            ef = tableIndex(tpnl, cx, cy, out var idx) ? DragDropEffects.All : DragDropEffects.None;
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
                                tpnl.Rows = ["20%", "20%", "20%", "20%", "20%"];
                                tpnl.Columns = ["20%", "20%", "20%", "20%", "20%"];
                            }
                            else if(vc is GoGridLayoutPanel gpnl)
                            {
                                gpnl.AddRow("25%", ["25%", "25%", "25%", "25%"]);
                                gpnl.AddRow("25%", ["33.3%", "33.4%", "33.3%"]);
                                gpnl.AddRow("25%", ["50%", "50%"]);
                                gpnl.AddRow("25%", ["100%"]);
                            }
                            else if(vc is GoTabControl tab)
                            {
                                tab.TabPages.Add(new GoTabPage { Text = "tab1" });
                                tab.TabPages.Add(new GoTabPage { Text = "tab2" });
                            }
                            else if (vc is GoSwitchPanel swpnl)
                            {
                                swpnl.Pages.Add(new GoSubPage { Name = "pnl1" });
                                swpnl.Pages.Add(new GoSubPage { Name = "pnl2" });
                            }
                            #endregion

                            TransAction(() =>
                            {
                                if (con is GoTableLayoutPanel tpnl && tableIndex(tpnl, cx, cy, out var tidx))
                                {
                                    AddControl(tpnl, vc, tidx.Col, tidx.Row, 1, 1);
                                    SelectedControl([.. sels], [vc]);
                                }
                                else if (con is GoGridLayoutPanel gpnl && gridIndex(gpnl, cx, cy, out var gidx))
                                {
                                    AddControl(gpnl, vc, gidx.Col, gidx.Row);
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

        #region target_control
        IGoControl? target_control(int x, int y)
        {
            IGoControl? ret = null;
            var prj = Program.CurrentProject;
            var (con, cx, cy) = container(x, y);
            if (con != null && prj != null)
            {
                ret = prj.Design.ControlStack(con, cx, cy).LastOrDefault();
            }
            return ret;
        }

        List<IGoControl> target_control(SKPoint ptDown, SKPoint ptUp)
        {
            //int x = Convert.ToInt32(ptUp.X), y = Convert.ToInt32(ptUp.Y);
            int x = Convert.ToInt32(ptDown.X), y = Convert.ToInt32(ptDown.Y);
            List<IGoControl> ret = [];
            var prj = Program.CurrentProject;
            var (con, cx, cy) = target_container(x, y);
            if (con != null && prj != null)
            {
                var drt = MathTool.MakeRectangle(ptUp, ptDown);
                drt.Offset(cx - x, cy - y);

                //var pc = prj.Design.ControlStack(con, cx, cy).LastOrDefault();
                ret = prj.Design.ControlStack(con, drt).Where(x => x.Parent == con).ToList();
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
                    //var pc = cls.LastOrDefault();
                    var pc = cls.LastOrDefault(vv => vv is IGoContainer);
                    if (pc is IGoContainer vcon)
                    {
                        con = vcon;
                        (cx, cy) = containerpos(con, x, y);
                    }
                }
                else
                {
                    //var pc = prj.Design.ControlStack(con, cx, cy).LastOrDefault();
                    var pc = prj.Design.ControlStack(con, cx, cy).LastOrDefault(vv => vv is IGoContainer);
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
        #region target_controlstack
        List<IGoControl> target_controlstack(int x, int y)
        {
            List<IGoControl> ret = [];
            var prj = Program.CurrentProject;
            var (con, cx, cy) = container(x, y);
            if (con != null && prj != null)
            {
                ret = prj.Design.ControlStack(con, cx, cy);
            }
            return ret;
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
                    var ancs = Util2.GetAnchors(vc, vrt).Where(a => c.Parent is GoGridLayoutPanel ? a.Name == "move" : true);
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
            idx = new TableIndex { Col = -1, Row = -1, Colspan = 1, Rowspan = 1 };

            var ret = false;
            var rt = tpnl.Areas()["Content"];
            var rts = Util.Grid(rt, [.. tpnl.Columns], [.. tpnl.Rows]);
            for (int ir = 0; ir < tpnl.Rows.Count; ir++)
                for (int ic = 0; ic < tpnl.Columns.Count; ic++)
                    if (CollisionTool.Check(rts[ir, ic], x, y) && tpnl.Childrens[ic, ir] == null)
                    {
                        idx.Col = ic;
                        idx.Row = ir;
                        ret = true;
                    }

            return ret;
        }

        bool tableIndex(GoTableLayoutPanel tpnl, SKPoint ptDown, SKPoint ptUp, Anchor anc, out List<TableIndex> idxs)
        {
            idxs = [];

            var gx = ptUp.X - ptDown.X;
            var gy = ptUp.Y - ptDown.Y;
            var crt = calcbox(anc.Name, anc.Control.Bounds, gx, gy);

            var rt = tpnl.Areas()["Content"];
            var rts = Util.Grid(rt, [.. tpnl.Columns], [.. tpnl.Rows]);
            for (int ir = 0; ir < tpnl.Rows.Count; ir++)
                for (int ic = 0; ic < tpnl.Columns.Count; ic++)
                    if (CollisionTool.Check(rts[ir, ic], crt))
                    {
                        idxs.Add(new TableIndex { Col = ic, Row = ir });
                    }

            return idxs.Count > 0;
        }
        #endregion

        #region gridindex
        bool gridIndex(GoGridLayoutPanel gpnl, int x, int y, out GridIndex idx)
        {
            idx = new GridIndex { Col = -1, Row = -1 };

            var ret = false;
            var rts = gpnl.GridBounds();
            for (int ir = 0; ir < gpnl.Rows.Count; ir++)
                for (int ic = 0; ic < gpnl.Rows[ir].Columns.Count; ic++)
                    if (gpnl.CellBounds(rts, ic, ir) is SKRect vrt && CollisionTool.Check(vrt, x, y) && gpnl.Childrens[ic, ir] == null)
                    {
                        idx.Col = ic;
                        idx.Row = ir;
                        ret = true;
                    }

            return ret;
        }
        #endregion

        #region dragAnchorProc
        void dragAnchorProc(Anchor anc, SKPoint ptDown, SKPoint ptMove,
            Action <IGoContainer, IGoControl, TableIndex?> tbl,
            Action<IGoContainer, IGoControl, GridIndex?> grid,
            Action<IGoContainer, IGoControl, SKRect, SKRect> other)
        {
            var x = Convert.ToInt32(ptMove.X);
            var y = Convert.ToInt32(ptMove.Y);
            var c = anc.Control;
            var (con, cx, cy) = target_container(x, y, c);

            if (con is GoTableLayoutPanel tpnl)
            {
                TableIndex? tidx = null;
                #region tpnl
                if (sels.Count == 1)
                {
                    var rt = tpnl.Areas()["Content"];
                    var rts = Util.Grid(rt, [.. tpnl.Columns], [.. tpnl.Rows]);

                    var ri = tpnl.Childrens[c];

                    if (anc.Name == "move")
                    {
                        if (tableIndex(tpnl, cx, cy, out var tidx2))
                        {
                            var l = tidx2.Col;
                            var t = tidx2.Row;
                            var r = ((ri?.ColSpan ?? tidx2.Colspan) - 1) + l;
                            var b = ((ri?.RowSpan ?? tidx2.Rowspan) - 1) + t;
                            if (l >= 0 && t >= 0 && r < tpnl.Columns.Count && b < tpnl.Rows.Count)
                                tidx = new TableIndex { Col = tidx2.Col, Row = tidx2.Row, Colspan = ri?.ColSpan ?? tidx2.Colspan, Rowspan = ri?.RowSpan ?? tidx2.Rowspan };
                        }
                    }
                    else
                    {
                        if (tableIndex(tpnl, ptDown, ptMove, anc, out var tidxs) && !tidxs.Any(x => tpnl.Childrens[x.Col, x.Row] != null && tpnl.Childrens[x.Col, x.Row] != c))
                        {
                            var l = tidxs.Min(x => x.Col);
                            var t = tidxs.Min(x => x.Row);
                            var r = tidxs.Max(x => x.Col);
                            var b = tidxs.Max(x => x.Row);

                            if (l >= 0 && t >= 0 && r < tpnl.Columns.Count && b < tpnl.Rows.Count)
                                tidx = new TableIndex { Col = l, Row = t, Colspan = r - l + 1, Rowspan = b - t + 1 };
                        }
                    }
                }
                #endregion
                tbl(con, c, tidx);
            }
            else if(con is GoGridLayoutPanel gpnl)
            {
                GridIndex? gidx = null;
                #region gpnl
                if (sels.Count == 1)
                {
                    var rts = gpnl.GridBounds();
                    if (anc.Name == "move")
                    {
                        if (gridIndex(gpnl, cx, cy, out var gidx2))
                        {
                            var l = gidx2.Col;
                            var t = gidx2.Row;
                            gidx = new GridIndex { Col = gidx2.Col, Row = gidx2.Row, };
                        }
                    }
                }
                #endregion
                grid(con, c, gidx);
            }
            else if(con != null)
            {
                #region other
                var gx = ptMove.X - ptDown.X;
                var gy = ptMove.Y - ptDown.Y;

                var gps = sels.ToDictionary(x => x, y => new SKPoint(anc.Control.ScreenX - y.ScreenX, anc.Control.ScreenY - y.ScreenY));
                //nrt = Util.FromRect(cx - gps[c].X - c.Width / 2, cy - gps[c].Y - c.Height / 2, c.Width, c.Height);

                foreach (var vc in sels.Where(x => x.Parent == anc.Control.Parent || anc.Name != "move"))
                {
                    if (!isTopLevelContainer(vc))
                    {
                        var srt = calcbox(anc.Name, vc.Bounds, gx, gy);
                        var (vx, vy) = containerviewpos(vc.Parent);
                        srt.Offset(-vx, -vy);
                        srt.Offset(vc.ScreenX - vc.Left, vc.ScreenY - vc.Top);

                        var (tx, ty) = containerpos(con, srt.Left, srt.Top);
                        var nrt = Util.FromRect(tx, ty, srt.Width, srt.Height);


                        other(con, vc, srt, nrt);
                    }
                }
                #endregion
            }

        }
        #endregion

        #region swpnl_toolbounds
        (SKRect rtA, SKRect rtT, SKRect rtP, SKRect rtN) swpnl_toolbounds(GoSwitchPanel sw, SKRect rt)
        {
            int idx = sw.SelectedPage != null ? sw.Pages.IndexOf(sw.SelectedPage) : -1;
            var s = sw.SelectedPage != null ? sw.SelectedPage.Name : $"Page {idx}";
            var sz = Util.MeasureText(s, "나눔고딕", GoFontStyle.Normal, 12);

            var H = 20;
            var rtA = Util.FromRect(rt.Left, rt.Top - H, sz.Width + 20 + H + H, H);
            var rtT = Util.FromRect(rt.Left, rt.Top - H-1, sz.Width + 20, H);
            var rtP = Util.FromRect(rtT.Right, rtT.Top, H, H);
            var rtN = Util.FromRect(rtP.Right, rtP.Top, H, H);

            return (rtA, rtT, rtP, rtN);
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
    #region TalbeIndex
    class TableIndex
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public int Colspan { get; set; }
        public int Rowspan { get; set; }
    }
    #endregion
    #region GridIndex
    class GridIndex
    {
        public int Col { get; set; }
        public int Row { get; set; }
    }
    #endregion
}
