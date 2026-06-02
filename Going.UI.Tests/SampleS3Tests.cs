using System;
using System.IO;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests;

/// <summary>s3: 네이티브 컨트롤만으로 만든 밀도 높은 2페이지 대시보드(Overview/Process). Vo 미사용.</summary>
public class SampleS3Tests
{
    private const int W = 1600, H = 1000;
    private static readonly string ArtifactDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "voobj-artifacts");
    private static string Dest =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ui-sample", "s3");

    // palette
    const string BG = "#0E1117", Pnl = "#161B22", Pnl2 = "#10141B", Alt = "#10151C", Track = "#232B36", Line = "#2A323F";
    const string Pri = "#E8EEF5", Sub = "#8B97A8", Muted = "#5E6B7D";
    const string Cyan = "#34C3FF", Blue = "#5B8DEF", Violet = "#9B6BEF", Green = "#39D98A", Amber = "#F59E0B", Red = "#EF4444";

    // ── helpers ──────────────────────────────────────────────
    static GoPanel Panel(string title, float th = 34) => new()
    {
        Text = title, TitleHeight = th, FontName = "나눔고딕", FontSize = 13, FontStyle = GoFontStyle.Bold, TextColor = Sub,
        PanelColor = Pnl, PanelColor2 = Pnl2, BorderColor = Line, Round = GoRoundType.All, TitleDivider = false, Elevation = 2, Padding = new(14),
    };
    static GoLabel L(string s, float size, string color, GoContentAlignment a = GoContentAlignment.MiddleLeft, GoFontStyle st = GoFontStyle.Normal) =>
        new() { Text = s, FontSize = size, TextColor = color, ContentAlignment = a, FontStyle = st, BackgroundDraw = false, Dock = GoDockStyle.Fill };
    static GoTableLayoutPanel Tbl(string[] cols, string[] rows) => new() { Columns = [.. cols], Rows = [.. rows], Dock = GoDockStyle.Fill };
    static GoChip Chip(string t, string? dot, string col, string chipc, string bord) =>
        new() { Text = t, DotColor = dot, TextColor = col, ChipColor = chipc, BorderColor = bord, FontSize = 12, Dock = GoDockStyle.Fill, Margin = new(3, 14, 3, 14) };
    static GoValueNumber<double> Val(string title, double v, string fmt, string unit, string accent) => new()
    {
        // 넓은 단일 값 타일: 단위를 FormatString 에 내장(우측 부유·잘림 방지), 타이틀은 TitleSize 로 노출
        Direction = GoDirectionHV.Vertical,   // 타이틀을 상단 행으로 (Horizon이면 좌측 컬럼이라 잘림)
        Title = title, TitleSize = 22, TitleFontSize = 12, TitleBoxDraw = false, TitleContentAlignment = GoContentAlignment.MiddleLeft,
        Value = v, FormatString = string.IsNullOrEmpty(unit) ? fmt : $"{fmt}' {unit}'",
        FontSize = 24, FontStyle = GoFontStyle.Bold, TextColor = accent, FillColor = Pnl, ValueColor = Pnl2, BorderColor = Line, Round = GoRoundType.All,
        Dock = GoDockStyle.Fill,
    };
    static GoProgress Bar(double v, string c1, string c2) =>
        new() { Minimum = 0, Maximum = 100, Value = v, FillColor = c1, FillColor2 = c2, EmptyColor = Track, BorderWidth = 0, Corner = 5, Dock = GoDockStyle.Fill };
    static GoLamp LampRow(string name, string state, string color) =>
        new() { Text = name, OnOff = true, OnColor = color, OffColor = Track, TextColor = "#C7D0DC", FontSize = 13, LampSize = 9, Gap = 10, ContentAlignment = GoContentAlignment.MiddleLeft, Dock = GoDockStyle.Fill };
    static GoGauge Gauge(string title, double v, string c) => new()
    {
        Title = title, TitleFontSize = 12, Value = v, Minimum = 0, Maximum = 100, Format = "0'%'", FillColor = c, EmptyColor = Track,
        BorderWidth = 0, BarSize = 16, FontSize = 26, TextColor = Pri, Dock = GoDockStyle.Fill,
    };

    static GoBoxPanel Root()
        => new() { BoxColor = BG, BorderColor = BG, Round = GoRoundType.Rect, BackgroundDraw = true, BorderWidth = 0, Padding = new(20), Dock = GoDockStyle.Fill };

    static GoNavigator Nav()
    {
        var n = new GoNavigator
        {
            Direction = GoDirectionHV.Horizon, IconDirection = GoDirectionHV.Horizon, IconSize = 13, IconGap = 6,
            MenuGap = 22, Indent = 0, FontSize = 13, FontStyle = GoFontStyle.Bold, TextColor = Sub, Dock = GoDockStyle.Fill, Margin = new(0, 8, 0, 8),
        };
        n.Menus.Add(new GoMenuItem { Text = "OVERVIEW", IconString = "fa-gauge-high", PageName = "Overview" });
        n.Menus.Add(new GoMenuItem { Text = "PROCESS", IconString = "fa-diagram-project", PageName = "Process" });
        return n;
    }

    // ── Page 1: Overview ─────────────────────────────────────
    static GoPage Overview()
    {
        var page = new GoPage { Name = "Overview", Padding = new(0) };
        var root = Root();
        var main = Tbl(["*"], ["56px", "14px", "118px", "14px", "*", "14px", "250px"]);

        // header
        var head = Tbl(["*", "300px", "230px"], ["*"]);
        head.Childrens.Add(L("OPERATIONS · LINE 7", 24, Pri, GoContentAlignment.MiddleLeft, GoFontStyle.Bold), 0, 0);
        head.Childrens.Add(Nav(), 1, 0);
        var chips = Tbl(["*", "*"], ["*"]);
        chips.Childrens.Add(Chip("LIVE", Green, Green, "#10231A", "#1E5B3F"), 0, 0);
        chips.Childrens.Add(Chip("3 ALM", Amber, Amber, "#2A2110", "#5B4620"), 1, 0);
        head.Childrens.Add(chips, 2, 0);
        main.Childrens.Add(head, 0, 0);

        // KPI row (GoValueNumber tiles)
        var kpis = Tbl(["*", "14px", "*", "14px", "*", "14px", "*", "14px", "*"], ["*"]);
        kpis.Childrens.Add(Val("THROUGHPUT", 1284, "0", "u/h", Cyan), 0, 0);
        kpis.Childrens.Add(Val("OEE", 86.4, "0.0", "%", Green), 2, 0);
        kpis.Childrens.Add(Val("ENERGY", 412, "0", "kW", Blue), 4, 0);
        kpis.Childrens.Add(Val("DEFECT", 0.42, "0.00", "%", Amber), 6, 0);
        kpis.Childrens.Add(Val("UPTIME", 99.2, "0.0", "%", Green), 8, 0);
        main.Childrens.Add(kpis, 0, 2);

        // mid: power | gauges | service health
        var mid = Tbl(["*", "14px", "440px", "14px", "340px"], ["*"]);
        var pwr = Panel("POWER & LOAD");
        var pwrg = Tbl(["*"], ["*", "16px", "20px", "8px", "20px", "8px", "20px"]);
        pwrg.Childrens.Add(new GoMeter { Title = "POWER (kW)", TitleFontSize = 11, Value = 48.5, Minimum = 0, Maximum = 100, GraduationLarge = 20, GraduationSmall = 5, Format = "0", NeedleColor = Cyan, NeedlePointColor = Red, TextColor = Cyan, RemarkColor = Muted, FontSize = 20, Dock = GoDockStyle.Fill }, 0, 0);
        pwrg.Childrens.Add(Bar(72, Cyan, Blue), 0, 2);
        pwrg.Childrens.Add(Bar(64, Green, Cyan), 0, 4);
        pwrg.Childrens.Add(Bar(55, Blue, Violet), 0, 6);
        pwr.Childrens.Add(pwrg);
        mid.Childrens.Add(pwr, 0, 0);

        var gp = Panel("EFFICIENCY");
        var gg = Tbl(["*", "12px", "*"], ["*"]);
        gg.Childrens.Add(Gauge("효율", 86, Cyan), 0, 0);
        gg.Childrens.Add(Gauge("품질", 94, Green), 2, 0);
        gp.Childrens.Add(gg);
        mid.Childrens.Add(gp, 2, 0);

        var svc = Panel("SERVICE HEALTH");
        var sg = Tbl(["*"], ["*", "*", "*", "*", "*", "*"]);
        (string n, string c)[] svcs = { ("API Gateway", Green), ("Database", Green), ("Cache", Amber), ("Storage", Green), ("Scheduler", Green), ("Vision AI", Red) };
        for (int i = 0; i < svcs.Length; i++)
        {
            var r = Tbl(["*", "72px"], ["*"]);
            r.Childrens.Add(LampRow(svcs[i].n, "", svcs[i].c), 0, 0);
            r.Childrens.Add(L(svcs[i].c == Green ? "RUN" : svcs[i].c == Amber ? "WARN" : "FAULT", 11, svcs[i].c, GoContentAlignment.MiddleRight, GoFontStyle.Bold), 1, 0);
            sg.Childrens.Add(r, 0, i);
        }
        svc.Childrens.Add(sg);
        mid.Childrens.Add(svc, 4, 0);
        main.Childrens.Add(mid, 0, 4);

        // bottom: trend | event log
        var bottom = Tbl(["*", "14px", "380px"], ["*"]);
        var trend = Panel("HOURLY THROUGHPUT");
        float[] hs = { 45, 68, 52, 80, 61, 90, 73, 58, 84, 66, 77, 49, 88, 62 };
        var bcols = new System.Collections.Generic.List<string>();
        for (int i = 0; i < hs.Length; i++) { bcols.Add("*"); if (i < hs.Length - 1) bcols.Add("8px"); }
        var bars = Tbl([.. bcols], ["*"]);
        for (int i = 0; i < hs.Length; i++)
            // BottomToTop: FillColor=아래, FillColor2=위 → 위를 밝은 cyan으로 (s1과 동일 방향)
            bars.Childrens.Add(new GoProgress { Direction = ProgressDirection.BottomToTop, Minimum = 0, Maximum = 100, Value = hs[i], FillColor = hs[i] >= 88 ? Green : Blue, FillColor2 = Cyan, EmptyColor = Pnl2, BorderWidth = 0, Corner = 4, Dock = GoDockStyle.Fill }, i * 2, 0);
        trend.Childrens.Add(bars);
        bottom.Childrens.Add(trend, 0, 0);

        var logp = Panel("EVENT LOG");
        var list = new GoListBox { BoxColor = Pnl2, BorderColor = Line, FontSize = 12, TextColor = "#C7D0DC", Dock = GoDockStyle.Fill };
        foreach (var (t, ic) in new[] { ("12:04 ST-09 cycle complete", "fa-circle-check"), ("12:03 Vision AI fault", "fa-circle-exclamation"), ("12:01 Cache latency warning", "fa-triangle-exclamation"), ("11:58 Batch #4471 started", "fa-play"), ("11:55 OEE target reached", "fa-bullseye"), ("11:52 Operator login: kim", "fa-user") })
            list.Items.Add(new GoListItem { Text = t, IconString = ic });
        logp.Childrens.Add(list);
        bottom.Childrens.Add(logp, 2, 0);
        main.Childrens.Add(bottom, 0, 6);

        root.Childrens.Add(main);
        page.Childrens.Add(root);
        return page;
    }

    // ── Page 2: Process ──────────────────────────────────────
    static GoPage Process()
    {
        var page = new GoPage { Name = "Process", Padding = new(0) };
        var root = Root();
        var main = Tbl(["*"], ["56px", "14px", "120px", "14px", "*", "14px", "210px"]);

        var head = Tbl(["*", "300px", "230px"], ["*"]);
        head.Childrens.Add(L("PROCESS · BATCH #4471", 24, Pri, GoContentAlignment.MiddleLeft, GoFontStyle.Bold), 0, 0);
        head.Childrens.Add(Nav(), 1, 0);
        var chips = Tbl(["*", "*"], ["*"]);
        chips.Childrens.Add(Chip("RUN", Green, Green, "#10231A", "#1E5B3F"), 0, 0);
        chips.Childrens.Add(Chip("AUTO", Cyan, Cyan, "#10222B", "#1E4B5B"), 1, 0);
        head.Childrens.Add(chips, 2, 0);
        main.Childrens.Add(head, 0, 0);

        // step bar
        var stepP = Panel("SEQUENCE", 0);
        var step = new GoStepBar
        {
            Mode = GoStepBarMode.Story, DotColor = "#1B2530", ActiveColor = Cyan, LineColor = Line,
            ActiveTextColor = Cyan, InactiveTextColor = Muted, FontSize = 13, DotSize = 44, IconSize = 18, LabelGap = 8, LineWidth = 3, Dock = GoDockStyle.Fill,
        };
        foreach (var (t, ic) in new[] { ("Charge", "fa-fill"), ("Mix", "fa-arrows-rotate"), ("React", "fa-fire"), ("Cool", "fa-snowflake"), ("Discharge", "fa-arrow-up-from-bracket") })
            step.Steps.Add(new GoStepItem { Text = t, IconString = ic });
        step.Step = 3;   // Steps 추가 후 설정 (setter가 0으로 클램프되는 것 방지)
        stepP.Childrens.Add(step);
        main.Childrens.Add(stepP, 0, 2);

        // mid: live values | targets & command
        var mid = Tbl(["*", "14px", "430px"], ["*"]);
        var live = Panel("LIVE VALUES");
        var lv = Tbl(["*", "12px", "*", "12px", "*"], ["*", "12px", "*"]);
        lv.Childrens.Add(Val("TEMP", 182.4, "0.0", "℃", Amber), 0, 0);
        lv.Childrens.Add(Val("PRESS", 12.6, "0.0", "bar", Cyan), 2, 0);
        lv.Childrens.Add(Val("FLOW", 48.2, "0.0", "L/m", Green), 4, 0);
        lv.Childrens.Add(Val("LEVEL", 73, "0", "%", Blue), 0, 2);
        lv.Childrens.Add(Val("RPM", 1450, "0", "rpm", Violet), 2, 2);
        lv.Childrens.Add(new GoMeter { Title = "VIBRATION", TitleFontSize = 11, Value = 2.4, Minimum = 0, Maximum = 10, GraduationLarge = 2, GraduationSmall = 1, Format = "0.0", NeedleColor = Green, NeedlePointColor = Red, TextColor = Green, RemarkColor = Muted, FontSize = 16, Dock = GoDockStyle.Fill }, 4, 2);
        live.Childrens.Add(lv);
        mid.Childrens.Add(live, 0, 0);

        var cmd = Panel("TARGET & COMMAND");
        var cg = Tbl(["*"], ["66px", "14px", "42px", "14px", "42px", "16px", "54px", "*"]);
        var tg = Tbl(["*", "12px", "*"], ["*"]);
        tg.Childrens.Add(Val("TARGET T", 185, "0", "℃", Sub), 0, 0);
        tg.Childrens.Add(Val("TARGET P", 13, "0", "bar", Sub), 2, 0);
        cg.Childrens.Add(tg, 0, 0);
        var modes = new GoButtons { Mode = GoButtonsMode.Radio, FontSize = 12, TextColor = Sub, ButtonColor = Alt, BorderColor = Line, SelectedButtonColor = "#10222B", SelectedBorderColor = Cyan, Round = GoRoundType.All, Dock = GoDockStyle.Fill };
        modes.Buttons.Add(new GoButtonsItem { Name = "a", Text = "AUTO", Selected = true });
        modes.Buttons.Add(new GoButtonsItem { Name = "m", Text = "MANUAL" });
        modes.Buttons.Add(new GoButtonsItem { Name = "s", Text = "SIM" });
        cg.Childrens.Add(modes, 0, 2);
        var oo = Tbl(["*", "12px", "*"], ["*"]);
        oo.Childrens.Add(new GoOnOff { OnText = "PUMP", OffText = "PUMP", DrawText = true, OnOff = true, OnColor = Cyan, OffColor = Track, BoxColor = Alt, BorderColor = Line, CursorColor = "#C7D0DC", FontSize = 12, TextColor = Pri, Dock = GoDockStyle.Fill }, 0, 0);
        oo.Childrens.Add(new GoOnOff { OnText = "HEATER", OffText = "HEATER", DrawText = true, OnOff = true, OnColor = Amber, OffColor = Track, BoxColor = Alt, BorderColor = Line, CursorColor = "#C7D0DC", FontSize = 12, TextColor = Pri, Dock = GoDockStyle.Fill }, 2, 0);
        cg.Childrens.Add(oo, 0, 4);
        var btns = Tbl(["*", "10px", "*", "10px", "*"], ["*"]);
        btns.Childrens.Add(new GoButton { Text = "START", FontStyle = GoFontStyle.Bold, FontSize = 13, TextColor = "#08110C", ButtonColor = Green, BorderColor = Green, Round = GoRoundType.All, Dock = GoDockStyle.Fill }, 0, 0);
        btns.Childrens.Add(new GoButton { Text = "HOLD", FontStyle = GoFontStyle.Bold, FontSize = 13, TextColor = "#1A1408", ButtonColor = Amber, BorderColor = Amber, Round = GoRoundType.All, Dock = GoDockStyle.Fill }, 2, 0);
        btns.Childrens.Add(new GoButton { Text = "STOP", FontStyle = GoFontStyle.Bold, FontSize = 13, TextColor = "#FFFFFF", ButtonColor = Red, BorderColor = Red, Round = GoRoundType.All, Dock = GoDockStyle.Fill }, 4, 0);
        cg.Childrens.Add(btns, 0, 6);
        cmd.Childrens.Add(cg);
        mid.Childrens.Add(cmd, 2, 0);
        main.Childrens.Add(mid, 0, 4);

        // bottom: interlocks | alarms
        var bottom = Tbl(["*", "14px", "360px"], ["*"]);
        var ilk = Panel("INTERLOCKS");
        var ig = Tbl(["*", "10px", "*", "10px", "*", "10px", "*"], ["*", "10px", "*"]);
        (string n, string c)[] ilks = { ("Door", Green), ("E-Stop", Green), ("Overtemp", Green), ("Overpress", Amber), ("Flow Lo", Green), ("Level Hi", Green), ("Vacuum", Green), ("Comm", Green) };
        for (int i = 0; i < 8; i++)
        {
            var lampBox = new GoBoxPanel { BoxColor = Alt, BorderColor = Line, Round = GoRoundType.All, Padding = new(8) };
            lampBox.Childrens.Add(new GoLamp { Text = ilks[i].n, OnOff = true, OnColor = ilks[i].c, OffColor = Track, TextColor = "#C7D0DC", FontSize = 12, LampSize = 10, Gap = 8, ContentAlignment = GoContentAlignment.MiddleLeft, Dock = GoDockStyle.Fill });
            ig.Childrens.Add(lampBox, (i % 4) * 2, (i / 4) * 2);
        }
        ilk.Childrens.Add(ig);
        bottom.Childrens.Add(ilk, 0, 0);

        var alm = Panel("ALARMS");
        var alist = new GoListBox { BoxColor = Pnl2, BorderColor = Line, FontSize = 12, TextColor = "#C7D0DC", Dock = GoDockStyle.Fill };
        foreach (var (t, ic) in new[] { ("12:01 Overpressure warning", "fa-triangle-exclamation"), ("11:47 Heater PID deviation", "fa-temperature-high"), ("11:30 Filter Δp high", "fa-filter") })
            alist.Items.Add(new GoListItem { Text = t, IconString = ic });
        alm.Childrens.Add(alist);
        bottom.Childrens.Add(alm, 2, 0);
        main.Childrens.Add(bottom, 0, 6);

        root.Childrens.Add(main);
        page.Childrens.Add(root);
        return page;
    }

    static GoDesign Build()
    {
        var d = new GoDesign { Name = "s3", DesignWidth = W, DesignHeight = H };
        d.AddPage(Overview());
        d.AddPage(Process());
        return d;
    }

    private static void RenderPage(GoDesign d, string name, string file)
    {
        d.SetPage(name);
        var bmp = new SKBitmap(W, H);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(new SKColor(0x0E, 0x11, 0x17));
            d.CurrentPage!.Bounds = new SKRect(0, 0, W, H);
            d.CurrentPage.FireDraw(canvas, d.Theme);
            canvas.Flush();
        }
        Directory.CreateDirectory(ArtifactDir);
        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var fs = File.Create(Path.Combine(ArtifactDir, file));
        data.SaveTo(fs);
        bmp.Dispose();
    }

    [Fact]
    public void Build_S3()
    {
        var d = Build();
        d.SetSize(W, H);
        d.Init();
        RenderPage(d, "Overview", "sample-s3-overview.png");
        RenderPage(d, "Process", "sample-s3-process.png");

        Build().SerializeGudx(Path.Combine(Dest, "Master.gudx"));
        Assert.True(File.Exists(Path.Combine(Dest, "Pages", "Overview.gudx")));
        Assert.True(File.Exists(Path.Combine(Dest, "Pages", "Process.gudx")));
    }
}
