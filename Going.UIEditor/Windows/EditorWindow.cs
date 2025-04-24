#define Space
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
using Microsoft.VisualBasic;
using SkiaSharp;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using WeifenLuo.WinFormsUI.Docking;
using Windows.ApplicationModel.Appointments;
using Windows.Media.Protection.PlayReady;
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
        const int MAG_GP = 5;
        const int MAG_INTERVAL = 10;
        #endregion

        #region Properties
        public object Target { get; private set; }

        public bool CanUndo => actmgr.CanUndo;
        public bool CanRedo => actmgr.CanRedo;

        public int SelectedItemCount => sels.Count;
        #endregion

        #region Member Variable
        private Timer tmr;
        ActionManager actmgr;
        //List<IGoControl> sels { get; } = [];
        List<object> sels { get; } = [];
        bool selDesign = false;
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
            if (editorTarget is GoDesign) { Title = "Master"; TitleIconString = "fa-pager"; }
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
                var prj = Program.CurrentProject;
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

        #region GotFocus
        protected override void OnGotFocus(EventArgs e)
        {
            if (DockPanel.Contents.FirstOrDefault(x => x is PropertiesWindow) is PropertiesWindow pw)
            {
                if (pw.SelectedEditor != this)
                    pw.SelectObjects(this, sels);
            }
            base.OnGotFocus(e);
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
                            #region magnet
                            if (ptMove.HasValue && ptDown.HasValue)
                            {
                                var c = dragAnchor.Control;
                                var gx = ptMove.Value.X - ptDown.Value.X;
                                var gy = ptMove.Value.Y - ptDown.Value.Y;
                                var srt = calcbox(dragAnchor.Name, c.Bounds, gx, gy);
                                var (vx, vy) = containerviewpos(c.Parent);
                                srt.Offset(-vx, -vy);
                                srt.Offset(c.ScreenX - c.Left, c.ScreenY - c.Top);

                                mag_proc(dragAnchor, srt, false, (mx) => gx += mx.DistR, (my) => gy += my.DistR);
                                mag_proc(dragAnchor, srt, true, 
                                (mx) =>
                                {
                                    p.IsStroke = true;
                                    p.Color = SKColors.Red;
                                    p.StrokeWidth = 1;

                                    var ort = calcbox(dragAnchor.Name, c.Bounds, gx, gy);
                                    var (vx, vy) = containerviewpos(c.Parent);
                                    ort.Offset(-vx, -vy);
                                    ort.Offset(c.ScreenX - c.Left, c.ScreenY - c.Top);

                                    var trt = new SKRect(mx.PairXs[0], mx.PairYs[0], mx.PairXs[2], mx.PairYs[2]);

                                    if (mx.GapMode == "n")
                                    {
                                        var y1 = Math.Min(ort.Top, mx.PairYs.Min());
                                        var y2 = Math.Max(ort.Bottom, mx.PairYs.Max());
                                        if (mx.MagX.HasValue) canvas.DrawLine(mx.MagX.Value, y1, mx.MagX.Value, y2, p);
                                    }
                                    else if(mx.GapMode == "s")
                                    {
                                        var tort = trt; tort.Inflate(-MAG_INTERVAL, -MAG_INTERVAL);

                                        float overlapT = Math.Max(ort.Top, trt.Top);
                                        float overlapB = Math.Min(ort.Bottom, trt.Bottom);
                                        var dy = MathTool.Center(overlapT, overlapB);

                                        if (mx.MagX.HasValue) canvas.DrawLine(mx.MagX.Value, dy, mx.MagNameX?.Contains('l') ?? false ? tort.Left : tort.Right, dy, p);
                                    }
                                    else if (mx.GapMode == "p")
                                    {
                                        var tort = trt; tort.Inflate(MAG_INTERVAL, MAG_INTERVAL);

                                        float overlapT = Math.Max(ort.Top, trt.Top);
                                        float overlapB = Math.Min(ort.Bottom, trt.Bottom);
                                        var dy = MathTool.Center(overlapT, overlapB);

                                        if (mx.MagX.HasValue) canvas.DrawLine(mx.MagX.Value, dy, mx.MagNameX?.Contains('l') ?? false ? tort.Left : tort.Right, dy, p);
                                    }
                                }, 
                                (my) =>
                                {
                                    p.IsStroke = true;
                                    p.Color = SKColors.Red;
                                    p.StrokeWidth = 1;

                                    var ort = calcbox(dragAnchor.Name, c.Bounds, gx, gy);
                                    var (vx, vy) = containerviewpos(c.Parent);
                                    ort.Offset(-vx, -vy);
                                    ort.Offset(c.ScreenX - c.Left, c.ScreenY - c.Top);

                                    var trt = new SKRect(my.PairXs[0], my.PairYs[0], my.PairXs[2], my.PairYs[2]);

                                    if (my.GapMode == "n")
                                    {
                                        var x1 = Math.Min(ort.Left, my.PairXs.Min());
                                        var x2 = Math.Max(ort.Right, my.PairXs.Max());
                                        if (my.MagY.HasValue) canvas.DrawLine(x1, my.MagY.Value, x2, my.MagY.Value, p);
                                    }
                                    else if(my.GapMode == "s")
                                    {
                                        var tort = trt; tort.Inflate(-MAG_INTERVAL, -MAG_INTERVAL);

                                        float overlapL = Math.Max(ort.Left, trt.Left);
                                        float overlapR = Math.Min(ort.Right, trt.Right);
                                        var dx = MathTool.Center(overlapL, overlapR);

                                        if (my.MagY.HasValue) canvas.DrawLine(dx, my.MagY.Value, dx, my.MagNameY?.Contains('t') ?? false ? tort.Top : tort.Bottom, p);
                                    }
                                    else if (my.GapMode == "p")
                                    {
                                        var tort = trt; tort.Inflate(MAG_INTERVAL, MAG_INTERVAL);

                                        float overlapL = Math.Max(ort.Left, trt.Left);
                                        float overlapR = Math.Min(ort.Right, trt.Right);
                                        var dx = MathTool.Center(overlapL, overlapR);

                                        if (my.MagY.HasValue) canvas.DrawLine(dx, my.MagY.Value, dx, my.MagNameY?.Contains('t') ?? false ? tort.Top : tort.Bottom, p);
                                    }
                                });
                            }
                            #endregion
                      
                            #region drag anchor
                            if (ptDown.HasValue && ptMove.HasValue)
                            {
                                dragAnchorProc(dragAnchor, ptDown.Value, ptMove.Value,
                                #region tpnl
                                (vcon, vc, tidx) =>
                                {
                                    if (tidx != null && vcon is GoTableLayoutPanel tpnl)
                                    {
                                        var (vx, vy) = containerviewpos(tpnl.Parent);
                                        var trt = Util.FromRect(tpnl.ScreenX - vx, tpnl.ScreenY - vy, tpnl.Width - 1, tpnl.Height - 1);
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
                                        var (vx, vy) = containerviewpos(gpnl.Parent);
                                        var rts = gpnl.GridBounds(Util.FromRect(gpnl.ScreenX - vx, gpnl.ScreenY - vy, gpnl.Width - 1, gpnl.Height - 1));
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
                                #region other
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

                            foreach (var c in sels.Select(x => x as IGoControl).Where(x => x != null))
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

                            #region selected design
                            if (Target is GoDesign design2 && sels.FirstOrDefault() is GoDesign ds)
                            {
                                p.IsStroke = true;
                                p.Color = SKColors.Red;
                                p.StrokeWidth = 1;
                                canvas.DrawRect(Util.FromRect(0, 0, prj.Width, prj.Height), p);
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
                        var tc2 = target_control(x, y + 20);
                        if (tc2 is GoSwitchPanel sw && sels.Contains(sw))
                        {
                            #region swpnl
                            var (rtA, rtT, rtP, rtN) = swpnl_toolbounds(sw, Util.FromRect(0, 0, sw.Width, sw.Height));
                            int idx = sw.SelectedPage != null ? sw.Pages.IndexOf(sw.SelectedPage) : -1;
                            #endregion

                            var (sx, sy) = containerpos(sw.Parent!, x, y);
                            sx -= Convert.ToInt32(sw.Left);
                            sy -= Convert.ToInt32(sw.Top);

                            if (CollisionTool.Check(rtA, sx, sy))
                            {
                                ptDown = null;

                                if (CollisionTool.Check(rtP, sx, sy) && idx - 1 >= 0)
                                    sw.SetPage(sw.Pages[idx - 1].Name);
                                else if (CollisionTool.Check(rtN, sx, sy) && idx + 1 < sw.Pages.Count)
                                    sw.SetPage(sw.Pages[idx + 1].Name);
                            }
                        }
                        else
                        {
                            var tc = target_control(x, y);
                            if (tc is GoScrollablePanel spnl)
                            {
                                #region scroll
                                var srts = spnl.Areas();
                                var rtV = srts["ScrollV"];
                                var rtH = srts["ScrollH"];

                                var (sx, sy) = containerpos(spnl.Parent!, x, y);
                                sx -= Convert.ToInt32(spnl.Left);
                                sy -= Convert.ToInt32(spnl.Top);

                                if (CollisionTool.Check(rtV, sx, sy) || CollisionTool.Check(rtH, sx, sy))
                                {
                                    ptDown = null;
                                    downControl = spnl;
                                    downControl.FireMouseDown(sx, sy, ToGoMouseButton(e.Button));
                                }
                                #endregion
                            }
                            else if (tc is GoTabControl tab)
                            {
                                #region tab
                                var (sx, sy) = containerpos(tab.Parent!, x, y);
                                sx -= Convert.ToInt32(tab.Left);
                                sy -= Convert.ToInt32(tab.Top);

                                if (CollisionTool.Check(tab.Areas()["Nav"], sx, sy))
                                {
                                    ptDown = null;
                                    downControl = tab;
                                    downControl.FireMouseDown(sx, sy, ToGoMouseButton(e.Button));
                                }
                                #endregion
                            }
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
                        if (downControl is GoScrollablePanel spnl)
                        {
                            var (sx, sy) = containerpos(spnl.Parent!, x, y);
                            sx -= Convert.ToInt32(spnl.Left);
                            sy -= Convert.ToInt32(spnl.Top);
                            spnl.FireMouseMove(sx, sy);
                        }
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
                bool changed = false;
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
                            List<object>? vsels = null;
                            if (Math.Abs(MathTool.GetDistance(new SKPoint(x, y), ptDown.Value)) > 3)
                                vsels = target_control(ptDown.Value, new SKPoint(x, y)).Cast<object>().ToList();
                            else
                            {
                                List<object> msels = target_control(x, y) is IGoControl c ? [c] : [];
                                if ((msels == null || msels?.Count == 0) && Target is GoDesign ds)
                                    vsels = [ds];
                                else
                                    vsels = msels;
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

                                changed = true;
                            }
                            #endregion
                        }

                        #region spcial container (spnl/tab/swpnl) mouse event 

                        if (downControl is GoScrollablePanel spnl)
                        {
                            var (sx, sy) = containerpos(spnl.Parent!, x, y);
                            sx -= Convert.ToInt32(spnl.Left);
                            sy -= Convert.ToInt32(spnl.Top);
                            spnl.FireMouseUp(sx, sy, ToGoMouseButton(e.Button));
                        }
                        else if (downControl is GoTabControl tab)
                        {
                            var (sx, sy) = containerpos(tab.Parent!, x, y);
                            sx -= Convert.ToInt32(tab.Left);
                            sy -= Convert.ToInt32(tab.Top);
                            tab.FireMouseUp(sx, sy, ToGoMouseButton(e.Button));
                        }
                        #endregion
                    }
                }

                #region release
                var clears1 = sels.Where(x => x is IGoControl sc && sc.Parent is GoTabControl tab && !tab.Childrens.Contains(sc)).ToArray();
                var clears2 = sels.Where(x => x is IGoControl sc && sc.Parent is GoSwitchPanel swpnl && !swpnl.Childrens.Contains(sc)).ToArray();
                foreach (var c in clears1) sels.Remove(c);
                foreach (var c in clears2) sels.Remove(c);

                changed |= clears1.Count() > 0 || clears2.Count() > 0;
                if (changed && DockPanel.Contents.FirstOrDefault(x => x is PropertiesWindow) is PropertiesWindow pw)
                {
                    pw.SelectObjects(this, sels);
                }

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
                        var (vx, vy) = containerviewpos(sc.Parent);
                        var sx = x - sc.ScreenX + vx;
                        var sy = y - sc.ScreenY + vy;
                        if (CollisionTool.Check(Util.FromRect(0, 0, sc.Width, sc.Height), sx, sy))
                            sc.FireMouseWheel(sx, sy, delta);
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

                        case Keys.Control | Keys.A: SelectAll(); break;

                        case Keys.Delete: Delete(); break;

                        case Keys.Left: ControlMove(-1, 0); break;
                        case Keys.Right: ControlMove(1, 0); break;
                        case Keys.Up: ControlMove(0, -1); break;
                        case Keys.Down: ControlMove(0, 1); break;

                        case Keys.Shift | Keys.Left: ControlMove(-10, 0); break;
                        case Keys.Shift | Keys.Right: ControlMove(10, 0); break;
                        case Keys.Shift | Keys.Up: ControlMove(0, -10); break;
                        case Keys.Shift | Keys.Down: ControlMove(0, 10); break;
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

        public void SelectedControl(IEnumerable<object> olds, IEnumerable<object> news)
        {
            var p = Program.CurrentProject;
            if (p != null)
            {
                actmgr.RecordAction(new SelectedAction(this, DockPanel, sels, news, olds));
                p.Edit = true;
            }
        }
        #endregion

        #region Edit
        #region Copy / Cut / Paste / Escape
        public void Copy()
        {
            var vsels = sels.Where(x => x is not GoPage && x is not GoTitleBar && x is not GoSideBar && x is not GoFooter && x is not GoDesign).Select(x => x as IGoControl).Where(x => x != null);
            var s = JsonSerializer.Serialize(vsels, GoJsonConverter.Options);
            Clipboard.SetData("going-control", s);
        }
        public void Cut()
        {
            Copy();
            Delete();
        }

        public void Paste()
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

                if (DockPanel.Contents.FirstOrDefault(x => x is PropertiesWindow) is PropertiesWindow pw) pw.RefreshGrid();
            }
        }
        public void Redo()
        {
            var p = Program.CurrentProject;
            if (p != null && CanRedo)
            {
                actmgr?.Redo(); p.Edit = true;

                if (DockPanel.Contents.FirstOrDefault(x => x is PropertiesWindow) is PropertiesWindow pw) pw.RefreshGrid();
            }
        }
        #endregion

        #region SelectAll
        public void SelectAll()
        {
            var p = Program.CurrentProject;
            if (p != null )
            {
                var alls = search_control(container()).Where(xc => xc.Parent is IGoControl vcon ? CollisionTool.Check(xc.Bounds, Util.FromRect(xc.Parent.ViewPosition.X, xc.Parent.ViewPosition.Y, vcon.Width, vcon.Height)) : false);
                sels.Clear();
                sels.AddRange(alls);
                Invalidate();
            }
        }
        #endregion

        #region ControlMove
        void ControlMove(float gx, float gy)
        {
            var pi = typeof(IGoControl).GetProperty("Bounds");
            if (pi != null)
            {
                TransAction(() =>
                {
                    foreach (var c in sels.Select(x => x as IGoControl).Where(x => x != null && x.Parent is not GoTableLayoutPanel && x.Parent is not GoGridLayoutPanel))
                    {
                        var ort = c.Bounds;
                        var nrt = c.Bounds; nrt.Offset(gx, gy);
                        EditObject(c, pi, ort, nrt);
                    }
                });
            }
        }
        #endregion

        #region Delete 
        public void Delete()
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
        List<IGoContainer> container()
        {
            List<IGoContainer> con = [];

            if (Target is GoDesign design2)
            {
                var (rtL, rtT, rtR, rtB, rtF, rtFR) = design2.LayoutBounds();

                if (design2.UseLeftSideBar) con.Add(design2.LeftSideBar);
                else if (design2.UseRightSideBar) con.Add(design2.RightSideBar);
                else if (design2.UseTitleBar) con.Add(design2.TitleBar);
                else if (design2.UseFooter) con.Add(design2.Footer);
            }
            else if (Target is GoPage page2) con.Add(page2);
            else if (Target is GoWindow wnd2) con.Add(wnd2);

            return con;
        }

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

        /*
        (int x, int y) containerpos_wo(IGoContainer con, float x, float y)
        {
            var (vx, vy) = containerviewpos(con is GoScrollablePanel spnl ? spnl.Parent : con);
            var cx = x - (con is GoControl cc1 ? cc1.ScreenX : 0) - con.PanelBounds.Left + vx;
            var cy = y - (con is GoControl cc2 ? cc2.ScreenY : 0) - con.PanelBounds.Top + vy;
            return (Convert.ToInt32(cx), Convert.ToInt32(cy));
        }
        */
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
            foreach (var c in sels.Select(x => x as IGoControl).Where(x => x != null))
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

            var (con, cx, cy) = target_container(Convert.ToInt32(ptMove.X), Convert.ToInt32(ptMove.Y), c);
            if (anc.Name != "move")
            {
                con = c.Parent;
                (cx, cy) = containerpos(con!, x, y);
            }
            
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

                #region magnet
                {
                    var srt = calcbox(anc.Name, c.Bounds, gx, gy);
                    var (vx, vy) = containerviewpos(c.Parent);
                    srt.Offset(-vx, -vy);
                    srt.Offset(c.ScreenX - c.Left, c.ScreenY - c.Top);

                    mag_proc(anc, srt, false, (mx) => gx += mx.DistR, (my) => gy += my.DistR);
                }
                #endregion

                foreach (var vc in sels.Select(x => x as IGoControl).Where(x => x != null && (x.Parent == anc.Control.Parent || anc.Name != "move")))
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

        #region search_control
        List<IGoControl> search_control(IEnumerable<IGoContainer> containers)
        {
            List<IGoControl> ret = [];

            if (containers != null)
            {
                foreach (var container in containers)
                    if (container is IGoControl vc)
                    {
                        ret.Add(vc);
                        foreach (var c in container.Childrens)
                            if (c is IGoContainer con)
                                ret.AddRange(search_control([con]));
                            else
                                ret.Add(c);
                    }
            }

            return ret;
        }
        #endregion

        #region mag_controlpos
        (float[] xs, float[] ys) mag_controlpos(IGoControl c, float inflate = 0F)
        {
            var (vx, vy) = containerviewpos(c.Parent);
            var xg = c.ScreenX - c.Left - vx;
            var yg = c.ScreenY - c.Top - vy;
            float[] ys = [c.Bounds.Top + yg - inflate, c.Bounds.MidY + yg, c.Bounds.Bottom + yg + inflate];
            float[] xs = [c.Bounds.Left + xg - inflate, c.Bounds.MidX + xg, c.Bounds.Right + xg + inflate];

            return (xs, ys);
        }

        (float[] xs, float[] ys) mag_controlpos(IGoContainer con, float inflate = 0F)
        {
            var c = (IGoControl)con;
            var (vx, vy) = containerviewpos(c.Parent);
            var xg = c.ScreenX - c.Left - vx;
            var yg = c.ScreenY - c.Top - vy;

            var vrt = con.PanelBounds;
            vrt.Offset(c.Left, c.Top);

            float[] ys = [vrt.Top + yg - inflate, vrt.MidY, vrt.Bottom + yg + inflate];
            float[] xs = [vrt.Left + xg - inflate, vrt.MidX + xg, vrt.Right + xg + inflate];

            return (xs, ys);
        }

        (float[] xs, float[] ys) mag_controlpos(SKRect srt)
        {
            float[] ys = [srt.Top, srt.MidY, srt.Bottom];
            float[] xs = [srt.Left, srt.MidX, srt.Right];

            return (xs, ys);
        }
        #endregion
        #region mag_check
        bool mag_check(IGoControl tc, float[] xs, float[] ys, Anchor anc)
        {
            var (txs, tys) = mag_controlpos(tc);

            #region var
            float mx = xs[1], my = ys[1];
            switch (anc.Name)
            {
                case "move": mx = xs[1]; my = ys[1]; break;
                case "l": mx = xs[0]; my = ys[1]; break;
                case "t": mx = xs[1]; my = ys[0]; break;
                case "r": mx = xs[2]; my = ys[1]; break;
                case "b": mx = xs[1]; my = ys[2]; break;
                case "lt": mx = xs[0]; my = ys[0]; break;
                case "rt": mx = xs[2]; my = ys[0]; break;
                case "lb": mx = xs[0]; my = ys[2]; break;
                case "rb": mx = xs[2]; my = ys[2]; break;
            }
            var vcon = anc.Name == "move" ? target_container((int)mx, (int)my, anc.Control).con : anc.Control.Parent;
            #endregion

            var rtO = new SKRect(xs[0], ys[0], xs[2], ys[2]);
            var rtT = new SKRect(txs[0], tys[0], txs[2], tys[2]);

            
            foreach (var x in xs)
                foreach (var tx in vcon == tc.Parent && CollisionTool.CheckVertical(rtO, rtT) && !CollisionTool.Check(rtO , rtT) ? [txs[0] - MAG_INTERVAL, txs[0], txs[1], txs[2], txs[2] + MAG_INTERVAL] : txs)
                    if (Math.Abs(tx - x) <= MAG_GP) return true;

            foreach (var y in ys)
                foreach (var ty in vcon == tc.Parent && CollisionTool.CheckHorizon(rtO, rtT) && !CollisionTool.Check(rtO, rtT) ? [tys[0] - MAG_INTERVAL, tys[0], tys[1], tys[2], tys[2] + MAG_INTERVAL] : tys)
                    if (Math.Abs(ty - y) <= MAG_GP)  return true;

            return false;
        }
        #endregion
        #region mag_points
        List<Mag> mag_points(List<IGoControl> tcs, float[] xs, float[] ys, Anchor anc)
        {
            List<Mag> ret = [];

            #region var
            float mx = xs[1], my = ys[1];
            switch (anc.Name)
            {
                case "move": mx = xs[1]; my = ys[1]; break;
                case "l": mx = xs[0]; my = ys[1]; break;
                case "t": mx = xs[1]; my = ys[0]; break;
                case "r": mx = xs[2]; my = ys[1]; break;
                case "b": mx = xs[1]; my = ys[2]; break;
                case "lt": mx = xs[0]; my = ys[0]; break;
                case "rt": mx = xs[2]; my = ys[0]; break;
                case "lb": mx = xs[0]; my = ys[2]; break;
                case "rb": mx = xs[2]; my = ys[2]; break;
            }
            var vcon = anc.Name == "move" ? target_container((int)mx, (int)my, anc.Control).con : anc.Control.Parent;
            #endregion

            #region parent
            {
                if (vcon != null && vcon is IGoControl tc && vcon is not GoGridLayoutPanel && vcon is not GoTableLayoutPanel)
                {
                    var (txs, tys) = mag_controlpos(vcon, -MAG_INTERVAL);

                    for (int io = 0; io < xs.Length; io++)
                        for (int it = 0; it < txs.Length; it++)
                            if (Math.Abs(txs[it] - xs[io]) <= MAG_GP && ((anc.Name == "move" && io != 1 && it != 1) || (anc.Name.Contains('l') && io == 0) || (anc.Name.Contains('r') && io == 2)))
                                ret.Add(new Mag
                                {
                                    Target = tc,
                                    Dist = Math.Abs(txs[it] - xs[io]),
                                    DistR = txs[it] - xs[io],
                                    OriginX = xs[io],
                                    OriginNameX = io == 0 ? "l" : (io == 1 ? "c" : "r"),
                                    MagX = txs[it],
                                    MagNameX = it == 0 ? "gl" : (it == 1 ? "gc" : "gr"),
                                    PairXs = txs,
                                    PairYs = tys,
                                    GapMode = "p",
                                });

                    for (int io = 0; io < ys.Length; io++)
                        for (int it = 0; it < tys.Length; it++)
                            if (Math.Abs(tys[it] - ys[io]) <= MAG_GP && ((anc.Name == "move" && io != 1 && it != 1) || (anc.Name.Contains('t') && io == 0) || (anc.Name.Contains('b') && io == 2)))
                                ret.Add(new Mag
                                {
                                    Target = tc,
                                    Dist = Math.Abs(tys[it] - ys[io]),
                                    DistR = tys[it] - ys[io],
                                    OriginY = ys[io],
                                    OriginNameY = io == 0 ? "t" : (io == 1 ? "c" : "b"),
                                    MagY = tys[it],
                                    MagNameY = it == 0 ? "gt" : (it == 1 ? "gc" : "gb"),
                                    PairXs = txs,
                                    PairYs = tys,
                                    GapMode = "p",
                                });
                }
            }
            #endregion

            #region all control
            foreach (var tc in tcs)
            {
                #region Space
                if(tc.Parent == vcon)
                {
                    var (txs, tys) = mag_controlpos(tc, MAG_INTERVAL);

                    var rtO = new SKRect(xs[0], ys[0], xs[2], ys[2]);
                    var rtT = new SKRect(txs[0], tys[0], txs[2], tys[2]);

                    for (int io = 0; io < xs.Length; io++)
                        for (int it = 0; it < txs.Length; it++)
                            if (Math.Abs(txs[it] - xs[io]) <= MAG_GP && ((anc.Name == "move" && io != 1 && it != 1) || (anc.Name.Contains('l') && io == 0) || (anc.Name.Contains('r') && io == 2)) && CollisionTool.CheckVertical(rtO, rtT) )
                                ret.Add(new Mag
                                {
                                    Target = tc,
                                    Dist = Math.Abs(txs[it] - xs[io]),
                                    DistR = txs[it] - xs[io],
                                    OriginX = xs[io],
                                    OriginNameX = io == 0 ? "l" : (io == 1 ? "c" : "r"),
                                    MagX = txs[it],
                                    MagNameX = it == 0 ? "gl" : (it == 1 ? "gc" : "gr"),
                                    PairXs = txs,
                                    PairYs = tys,
                                    GapMode = "s",
                                });

                    for (int io = 0; io < ys.Length; io++)
                        for (int it = 0; it < tys.Length; it++)
                            if (Math.Abs(tys[it] - ys[io]) <= MAG_GP && ((anc.Name == "move" && io != 1 && it != 1) || (anc.Name.Contains('t') && io == 0) || (anc.Name.Contains('b') && io == 2)) && CollisionTool.CheckHorizon(rtO, rtT) )
                                ret.Add(new Mag
                                {
                                    Target = tc,
                                    Dist = Math.Abs(tys[it] - ys[io]),
                                    DistR = tys[it] - ys[io],
                                    OriginY = ys[io],
                                    OriginNameY = io == 0 ? "t" : (io == 1 ? "c" : "b"),
                                    MagY = tys[it],
                                    MagNameY = it == 0 ? "gt" : (it == 1 ? "gc" : "gb"),
                                    PairXs = txs,
                                    PairYs = tys,
                                    GapMode= "s",
                                });
                }
                #endregion

                #region Control
                {
                    var (txs, tys) = mag_controlpos(tc);

                    for (int io = 0; io < xs.Length; io++)
                        for (int it = 0; it < txs.Length; it++)
                            if (Math.Abs(txs[it] - xs[io]) <= MAG_GP && (anc.Name == "move" || (anc.Name.Contains('l') && io == 0) || (anc.Name.Contains('r') && io == 2)))
                                ret.Add(new Mag
                                {
                                    Target = tc,
                                    Dist = Math.Abs(txs[it] - xs[io]),
                                    DistR = txs[it] - xs[io],
                                    OriginX = xs[io],
                                    OriginNameX = io == 0 ? "l" : (io == 1 ? "c" : "r"),
                                    MagX = txs[it],
                                    MagNameX = it == 0 ? "l" : (it == 1 ? "c" : "r"),
                                    PairXs = txs,
                                    PairYs = tys,
                                    GapMode = "n",
                                });

                    for (int io = 0; io < ys.Length; io++)
                        for (int it = 0; it < tys.Length; it++)
                            if (Math.Abs(tys[it] - ys[io]) <= MAG_GP && (anc.Name == "move" || (anc.Name.Contains('t') && io == 0) || (anc.Name.Contains('b') && io == 2)))
                                ret.Add(new Mag
                                {
                                    Target = tc,
                                    Dist = Math.Abs(tys[it] - ys[io]),
                                    DistR = tys[it] - ys[io],
                                    OriginY = ys[io],
                                    OriginNameY = io == 0 ? "t" : (io == 1 ? "c" : "b"),
                                    MagY = tys[it],
                                    MagNameY = it == 0 ? "t" : (it == 1 ? "c" : "b"),
                                    PairXs = txs,
                                    PairYs = tys,
                                    GapMode = "n",
                                });
                }
                #endregion
            }
            #endregion

            return ret;
        }
        #endregion
        #region mag_proc
        void mag_proc(Anchor anc, SKRect srt, bool loop, Action<Mag> routineX, Action<Mag> routineY)
        {
            var c = anc.Control;
            var alls = search_control(container()).Where(xc => (xc.Parent is IGoControl vcon ? CollisionTool.Check(xc.Bounds, Util.FromRect(xc.Parent.ViewPosition.X, xc.Parent.ViewPosition.Y, vcon.Width, vcon.Height)) : false) && !sels.Contains(xc)).ToList();

            if (c is IGoContainer con)
            {
                foreach (var vc in alls.ToArray())
                    if (parent_check(vc, con)  ) alls.Remove(vc);


            }

            var (xs, ys) = mag_controlpos(srt);
            var tls = alls.Where(tc => tc != c && mag_check(tc, xs, ys, anc)).ToList();
            var pts = mag_points(tls, xs, ys, anc);

            var lsx = pts.Where(x => x.MagX != null);
            var lsy = pts.Where(x => x.MagY != null);
            var lkx = lsx.ToLookup(x => x.Dist);
            var lky = lsy.ToLookup(x => x.Dist);
            var kx = lsx.OrderBy(x => x.Dist).FirstOrDefault();
            var ky = lsy.OrderBy(y => y.Dist).FirstOrDefault();

            if (kx != null && lkx.Contains(kx.Dist))
            {
                var xls = lkx[kx.Dist].ToList();
                var fx = xls.FirstOrDefault();
                if (fx != null)
                {
                    if (loop)
                        foreach (var mx in xls.Where(vv => vv.Dist == fx.Dist && vv.DistR == fx.DistR)) routineX(mx);
                    else 
                        routineX(fx);
                }
            }

            if (ky != null && lky.Contains(ky.Dist))
            {
                var yls = lky[ky.Dist].ToList();
                var fy = yls.FirstOrDefault();
                if (fy != null)
                {
                    if (loop)
                        foreach (var my in yls.Where(vv => vv.Dist == fy.Dist && vv.DistR == fy.DistR)) routineY(my);
                    else
                        routineY(fy);
                    
                }
            }
        }
        #endregion

        #region parent_check
        bool parent_check(IGoControl c, IGoContainer check_parent)
        {
            if (c.Parent == check_parent) return true;
            else
            {
                if (c.Parent != null && c.Parent is IGoControl vc) return parent_check(vc, check_parent);
                else return false;
            }
        }
        #endregion
        #endregion
        #endregion
    }

    #region classes
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
        EditorWindow wnd;
        DockPanel dockPanel;
        List<object> sels;
        IEnumerable<object> news, olds;

        public SelectedAction(EditorWindow wnd, DockPanel dockPanel, List<object> sels, IEnumerable<object> news, IEnumerable<object> olds)
        {
            this.wnd = wnd;
            this.dockPanel = dockPanel;
            this.sels = sels;
            this.olds = olds;
            this.news = news;
        }

        protected override void ExecuteCore()
        {
            sels.Clear();
            sels.AddRange(news);
            if (dockPanel.Contents.FirstOrDefault(x => x is PropertiesWindow) is PropertiesWindow pw) pw.SelectObjects(wnd, sels);
        }
        protected override void UnExecuteCore()
        {
            sels.Clear();
            sels.AddRange(olds);
            if (dockPanel.Contents.FirstOrDefault(x => x is PropertiesWindow) is PropertiesWindow pw) pw.SelectObjects(wnd, sels);
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
    #region Mag
    class Mag
    {
        public IGoControl? Target { get; set; }

        public string? MagNameX { get; set; }
        public string? MagNameY { get; set; }
        public float? MagX { get; set; }
        public float? MagY { get; set; }

        public string? OriginNameX { get; set; }
        public string? OriginNameY { get; set; }
        public float? OriginX { get; set; }
        public float? OriginY { get; set; }

        public float[] PairXs { get; set; }
        public float[] PairYs { get; set; }

        public float Dist { get; set; }
        public float DistR { get; set; }
        public string GapMode { get; set; }
    }
    #endregion
    #endregion
}
