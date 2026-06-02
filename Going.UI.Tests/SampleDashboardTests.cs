using System;
using System.Collections.Generic;
using System.IO;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.ViewObjects;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests;

/// <summary>바탕화면/ui-sample 에 밀도 높은 대시보드 2종 생성: s1=Vo 패밀리, s2=기존 네이티브 컨트롤.</summary>
public class SampleDashboardTests
{
    private const int W = 1600, H = 1000;
    private static readonly string ArtifactDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "voobj-artifacts");
    private static string SampleDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ui-sample");

    // ── 팔레트 ────────────────────────────────────────────────
    const string BG = "#0E1117", Panel = "#161B22", Panel2 = "#10141B", PanelAlt = "#171C24";
    const string Track = "#232B36", Line = "#222A35";
    const string Pri = "#E8EEF5", Sub = "#8B97A8", Muted = "#5E6B7D";
    const string Cyan = "#34C3FF", Blue = "#5B8DEF", Violet = "#9B6BEF", Green = "#39D98A", Amber = "#F59E0B", Red = "#EF4444", Pink = "#F472B6";

    // ── 헬퍼 ──────────────────────────────────────────────────
    static VoBox Panelo(int pad = 16) => new()
    {
        Background = Panel, FillType = VoFillType.Linear, FillColor2 = Panel2, GradientAngle = 90,
        BorderRadius = 14, BorderColor = Line, BorderWidth = 1, ShadowColor = "#000000", ShadowY = 5, ShadowBlur = 16,
        Padding = new(pad),
    };
    static VoText T(string s, float size, string color, GoFontStyle st = GoFontStyle.Normal, GoContentAlignment a = GoContentAlignment.MiddleLeft)
        => new() { Text = s, FontSize = size, TextColor = color, FontStyle = st, Alignment = a };
    static VoGrid G(string[] cols, string[] rows) => new() { Columns = [.. cols], Rows = [.. rows], Dock = GoDockStyle.Fill };
    static VoStack HStack(float sp) => new() { Direction = GoDirectionHV.Horizon, Spacing = sp, Dock = GoDockStyle.Fill };
    static VoStack VStack(float sp) => new() { Direction = GoDirectionHV.Vertical, Spacing = sp, Dock = GoDockStyle.Fill };

    // KPI 카드
    static VoBox Kpi(string label, string value, string unit, float pct, string a1, string a2)
    {
        var card = Panelo(16);
        var g = G(["*"], ["18px", "4px", "42px", "8px", "14px", "*"]);
        g.Childrens.Add(T(label, 12, Sub, GoFontStyle.Bold), 0, 0);
        g.Childrens.Add(T(value + (unit.Length > 0 ? " " + unit : ""), 26, Pri, GoFontStyle.Bold), 0, 2);
        g.Childrens.Add(new VoProgress { Value = pct, Max = 100, TrackColor = Track, FillColor = a1, FillColor2 = a2, Radius = 6 }, 0, 4);
        card.Childrens.Add(g);
        return card;
    }

    // 막대 차트 패널
    static VoBox BarPanel(string title, (float h, string c1, string c2)[] bars, string[] labels)
    {
        var p = Panelo(18);
        var g = G(["*"], ["26px", "14px", "*", "18px"]);
        g.Childrens.Add(T(title, 14, Pri, GoFontStyle.Bold), 0, 0);
        var bs = HStack(12);
        foreach (var (h, c1, c2) in bars)
        {
            int top = (int)(100 - h);
            var col = G(["*"], [$"{top}%", $"{(int)h}%"]);
            col.Childrens.Add(new VoBox { Background = c1, FillType = VoFillType.Linear, FillColor2 = c2, GradientAngle = 90, BorderRadius = 5 }, 0, 1);
            bs.Childrens.Add(col);
        }
        g.Childrens.Add(bs, 0, 2);
        var ls = HStack(12);
        foreach (var l in labels) ls.Childrens.Add(T(l, 11, Muted, GoFontStyle.Normal, GoContentAlignment.MiddleCenter));
        g.Childrens.Add(ls, 0, 3);
        p.Childrens.Add(g);
        return p;
    }

    // 원형 게이지 (트랙+값+중앙텍스트 겹침)
    static VoBox GaugeBox(string label, float pct, string val, string c1, string c2)
    {
        var p = Panelo(14);
        var g = G(["*"], ["20px", "*"]);
        g.Childrens.Add(T(label, 12, Sub, GoFontStyle.Bold), 0, 0);
        var cell = G(["*"], ["*"]);
        cell.Childrens.Add(new VoArc { StartAngle = 135, SweepAngle = 270, Thickness = 14, Color = Track, RoundCap = true }, 0, 0);
        cell.Childrens.Add(new VoArc { StartAngle = 135, SweepAngle = 270, Thickness = 14, Color = c1, Color2 = c2, RoundCap = true, Value = pct, Max = 100 }, 0, 0);
        cell.Childrens.Add(T(val, 26, Pri, GoFontStyle.Bold, GoContentAlignment.MiddleCenter), 0, 0);
        g.Childrens.Add(cell, 0, 1);
        p.Childrens.Add(g);
        return p;
    }

    // 서비스 상태 행
    static VoBox StatusRow(string name, string state, string color)
    {
        var row = new VoBox { Background = "#10151C", BorderRadius = 9, Padding = new(0, 0, 12, 0) };
        var g = G(["4px", "12px", "*", "70px"], ["*"]);
        g.Childrens.Add(new VoBox { Background = color, BorderRadius = 2, Margin = new(0, 7, 0, 7) }, 0, 0);
        g.Childrens.Add(T(name, 13, "#C7D0DC"), 2, 0);
        g.Childrens.Add(T(state, 11, color, GoFontStyle.Bold, GoContentAlignment.MiddleRight), 3, 0);
        row.Childrens.Add(g);
        return row;
    }

    // 작은 스테이션 타일 (밀도용) — dot+id 헤더 + 큰 값
    static VoBox Tile(string id, string val, string color)
    {
        var t = new VoBox { Background = "#10151C", BorderRadius = 9, BorderColor = Line, BorderWidth = 1, Padding = new(12, 9, 12, 9) };
        var g = G(["*"], ["15px", "*"]);
        g.Childrens.Add(T("● " + id, 11, color, GoFontStyle.Bold), 0, 0);
        g.Childrens.Add(T(val, 20, Pri, GoFontStyle.Bold, GoContentAlignment.MiddleLeft), 0, 1);
        t.Childrens.Add(g);
        return t;
    }

    static GoDesign BuildS1()
    {
        var d = new GoDesign { Name = "s1", DesignWidth = W, DesignHeight = H };
        var page = new GoPage { Name = "MainPage", Padding = new(0) };

        var root = new VoBox { Background = BG, Padding = new(20), Dock = GoDockStyle.Fill };
        var main = G(["*"], ["60px", "16px", "124px", "16px", "*", "16px", "300px"]);

        // 헤더
        var header = G(["*", "220px"], ["*"]);
        var titleBlock = G(["*"], ["36px", "*"]);
        titleBlock.Childrens.Add(T("OPERATIONS CONTROL", 24, Pri, GoFontStyle.Bold, GoContentAlignment.BottomLeft), 0, 0);
        titleBlock.Childrens.Add(T("Line 7 · realtime · 2026-06-02", 12, Muted, GoFontStyle.Normal, GoContentAlignment.TopLeft), 0, 1);
        header.Childrens.Add(titleBlock, 0, 0);
        var pill = new VoBox { Background = "#10231A", BorderColor = "#1E5B3F", BorderWidth = 1, BorderRadius = 18, Margin = new(0, 16, 0, 16) };
        pill.Childrens.Add(T("● LIVE", 13, Green, GoFontStyle.Bold, GoContentAlignment.MiddleCenter));
        header.Childrens.Add(pill, 1, 0);
        main.Childrens.Add(header, 0, 0);

        // KPI 5
        var kpis = HStack(16);
        kpis.Childrens.Add(Kpi("THROUGHPUT", "1,284", "u/h", 78, Cyan, Blue));
        kpis.Childrens.Add(Kpi("OEE", "86.4", "%", 86, Green, Cyan));
        kpis.Childrens.Add(Kpi("ENERGY", "412", "kW", 54, Blue, Violet));
        kpis.Childrens.Add(Kpi("DEFECT RATE", "0.42", "%", 12, Amber, Red));
        kpis.Childrens.Add(Kpi("UPTIME", "99.2", "%", 99, Green, Cyan));
        main.Childrens.Add(kpis, 0, 2);

        // 중간: 막대 차트 | 게이지2 | 상태리스트
        var mid = G(["*", "16px", "440px", "16px", "360px"], ["*"]);
        var bars = new (float, string, string)[] { (45, Cyan, Blue), (68, Cyan, Blue), (52, Cyan, Blue), (80, Cyan, Blue), (61, Cyan, Blue), (90, Green, Cyan), (73, Cyan, Blue), (58, Cyan, Blue), (84, Cyan, Blue), (66, Cyan, Blue) };
        mid.Childrens.Add(BarPanel("HOURLY OUTPUT", bars, ["08", "09", "10", "11", "12", "13", "14", "15", "16", "17"]), 0, 0);

        var gauges = G(["*", "14px", "*"], ["*"]);
        gauges.Childrens.Add(GaugeBox("EFFICIENCY", 86, "86%", Cyan, Violet), 0, 0);
        gauges.Childrens.Add(GaugeBox("QUALITY", 94, "94%", Green, Cyan), 2, 0);
        mid.Childrens.Add(gauges, 2, 0);

        var statusP = Panelo(18);
        var sg = G(["*"], ["24px", "12px", "*"]);
        sg.Childrens.Add(T("SERVICE HEALTH", 14, Pri, GoFontStyle.Bold), 0, 0);
        var slist = VStack(8);
        slist.Childrens.Add(StatusRow("API Gateway", "RUN", Green));
        slist.Childrens.Add(StatusRow("Database", "RUN", Green));
        slist.Childrens.Add(StatusRow("Cache", "WARN", Amber));
        slist.Childrens.Add(StatusRow("Storage", "RUN", Green));
        slist.Childrens.Add(StatusRow("Scheduler", "RUN", Green));
        slist.Childrens.Add(StatusRow("Vision AI", "FAULT", Red));
        sg.Childrens.Add(slist, 0, 2);
        statusP.Childrens.Add(sg);
        mid.Childrens.Add(statusP, 4, 0);
        main.Childrens.Add(mid, 0, 4);

        // 하단: 12 타일 그리드 | 로그
        var bottom = G(["*", "16px", "340px"], ["*"]);
        var tilesP = Panelo(16);
        var tg = G(["*"], ["24px", "10px", "*"]);
        tg.Childrens.Add(T("STATION MATRIX", 14, Pri, GoFontStyle.Bold), 0, 0);
        var grid = G(["*", "10px", "*", "10px", "*", "10px", "*", "10px", "*", "10px", "*"], ["*", "10px", "*"]);
        string[] cols2 = { Green, Green, Cyan, Amber, Green, Green };
        var palette = new[] { Green, Cyan, Amber, Green, Red, Green, Cyan, Green, Amber, Green, Cyan, Green };
        int ti = 0;
        for (int r = 0; r < 2; r++)
            for (int c = 0; c < 6; c++)
            {
                grid.Childrens.Add(Tile($"ST-{ti + 1:00}", $"{(ti * 7 + 31) % 90 + 10}", palette[ti % palette.Length]), c * 2, r * 2);
                ti++;
            }
        tg.Childrens.Add(grid, 0, 2);
        tilesP.Childrens.Add(tg);
        bottom.Childrens.Add(tilesP, 0, 0);

        var logP = Panelo(16);
        var lg = G(["*"], ["24px", "10px", "*"]);
        lg.Childrens.Add(T("EVENT LOG", 14, Pri, GoFontStyle.Bold), 0, 0);
        var logs = VStack(6);
        (string t, string c)[] lines = {
            ("12:04  ST-09 cycle complete", Sub), ("12:03  Vision AI fault detected", Red),
            ("12:01  Cache latency warning", Amber), ("11:58  Batch #4471 started", Sub),
            ("11:55  OEE target reached", Green), ("11:52  Operator login: kim", Sub),
            ("11:49  ST-03 idle", Muted),
        };
        foreach (var (t, c) in lines) logs.Childrens.Add(T(t, 12, c));
        lg.Childrens.Add(logs, 0, 2);
        logP.Childrens.Add(lg);
        bottom.Childrens.Add(logP, 2, 0);
        main.Childrens.Add(bottom, 0, 6);

        root.Childrens.Add(main);
        page.Childrens.Add(root);
        d.AddPage(page);
        return d;
    }

    // ── S2: 네이티브 컨트롤 ────────────────────────────────────
    static Going.UI.Containers.GoPanel NPanel(string title, float titleH)
        => new()
        {
            Text = title, TitleHeight = titleH, FontName = "나눔고딕", FontSize = title.Length > 0 && titleH >= 30 ? 14 : 12,
            FontStyle = GoFontStyle.Bold, TextColor = Sub,
            PanelColor = Panel, PanelColor2 = Panel2, BorderColor = Line, Round = GoRoundType.All,
            TitleDivider = false, Elevation = 2, Padding = new(14),
        };
    static GoLabel NLabel(string s, float size, string color, GoFontStyle st = GoFontStyle.Normal, GoContentAlignment a = GoContentAlignment.MiddleLeft)
        => new() { Text = s, FontSize = size, TextColor = color, FontStyle = st, ContentAlignment = a, BackgroundDraw = false, Dock = GoDockStyle.Fill };
    static Going.UI.Containers.GoTableLayoutPanel Tbl(string[] cols, string[] rows)
        => new() { Columns = [.. cols], Rows = [.. rows], Dock = GoDockStyle.Fill };

    static Going.UI.Containers.GoPanel NKpi(string label, string value, float pct, string a1, string a2)
    {
        var p = NPanel(label, 24);
        var t = Tbl(["*"], ["44px", "8px", "14px", "*"]);
        t.Childrens.Add(NLabel(value, 26, Pri, GoFontStyle.Bold), 0, 0);
        t.Childrens.Add(new GoProgress { Minimum = 0, Maximum = 100, Value = pct, FillColor = a1, FillColor2 = a2, EmptyColor = Track, BorderWidth = 0, Corner = 6, ShowValueLabel = false, Dock = GoDockStyle.Fill }, 0, 2);
        p.Childrens.Add(t);
        return p;
    }

    static Going.UI.Containers.GoPanel NGauge(string title, double val, string c1, string c2)
    {
        var p = NPanel("", 0);
        p.Childrens.Add(new GoGauge
        {
            Title = title, TitleFontSize = 12, Value = val, Minimum = 0, Maximum = 100, Format = "0'%'",
            FillColor = c1, FillColor2 = c2, EmptyColor = Track, BorderWidth = 0, BarSize = 16, FontSize = 26,
            TextColor = Pri, Dock = GoDockStyle.Fill,
        });
        return p;
    }

    static GoDesign BuildS2()
    {
        var d = new GoDesign { Name = "s2", DesignWidth = W, DesignHeight = H };
        var page = new GoPage { Name = "MainPage", Padding = new(0) };

        var root = new Going.UI.Containers.GoPanel
        {
            Text = "", TitleHeight = 0, PanelColor = BG, PanelColor2 = BG, BorderColor = BG,
            Round = GoRoundType.Rect, TitleDivider = false, Padding = new(20), Dock = GoDockStyle.Fill,
        };
        var main = Tbl(["*"], ["60px", "16px", "124px", "16px", "*", "16px", "300px"]);

        // 헤더
        var header = Tbl(["*", "220px"], ["*"]);
        var titleBlock = Tbl(["*"], ["36px", "*"]);
        titleBlock.Childrens.Add(NLabel("OPERATIONS CONTROL", 24, Pri, GoFontStyle.Bold, GoContentAlignment.BottomLeft), 0, 0);
        titleBlock.Childrens.Add(NLabel("Line 7 · realtime · 2026-06-02", 12, Muted, GoFontStyle.Normal, GoContentAlignment.TopLeft), 0, 1);
        header.Childrens.Add(titleBlock, 0, 0);
        header.Childrens.Add(new GoLamp { Text = "LIVE", OnOff = true, OnColor = Green, OffColor = Track, TextColor = Green, FontSize = 13, FontStyle = GoFontStyle.Bold, LampSize = 12, ContentAlignment = GoContentAlignment.MiddleRight, Dock = GoDockStyle.Fill }, 1, 0);
        main.Childrens.Add(header, 0, 0);

        // KPI 5
        var kpis = Tbl(["*", "16px", "*", "16px", "*", "16px", "*", "16px", "*"], ["*"]);
        kpis.Childrens.Add(NKpi("THROUGHPUT", "1,284 u/h", 78, Cyan, Blue), 0, 0);
        kpis.Childrens.Add(NKpi("OEE", "86.4 %", 86, Green, Cyan), 2, 0);
        kpis.Childrens.Add(NKpi("ENERGY", "412 kW", 54, Blue, Violet), 4, 0);
        kpis.Childrens.Add(NKpi("DEFECT RATE", "0.42 %", 12, Amber, Red), 6, 0);
        kpis.Childrens.Add(NKpi("UPTIME", "99.2 %", 99, Green, Cyan), 8, 0);
        main.Childrens.Add(kpis, 0, 2);

        // 중간: 막대(세로 GoProgress) | 게이지2 | 서비스 상태
        var mid = Tbl(["*", "16px", "440px", "16px", "360px"], ["*"]);
        var barP = NPanel("HOURLY OUTPUT", 34);
        float[] hs = { 45, 68, 52, 80, 61, 90, 73, 58, 84, 66 };
        var barsCols = new List<string>();
        for (int i = 0; i < hs.Length; i++) { barsCols.Add("*"); if (i < hs.Length - 1) barsCols.Add("10px"); }
        var bars = Tbl([.. barsCols], ["*"]);
        for (int i = 0; i < hs.Length; i++)
            bars.Childrens.Add(new GoProgress { Direction = ProgressDirection.BottomToTop, Minimum = 0, Maximum = 100, Value = hs[i], FillColor = i == 5 ? Green : Cyan, FillColor2 = i == 5 ? Cyan : Blue, EmptyColor = "#10141B", BorderWidth = 0, Corner = 5, Dock = GoDockStyle.Fill }, i * 2, 0);
        barP.Childrens.Add(bars);
        mid.Childrens.Add(barP, 0, 0);

        var gauges = Tbl(["*", "14px", "*"], ["*"]);
        gauges.Childrens.Add(NGauge("EFFICIENCY", 86, Cyan, Violet), 0, 0);
        gauges.Childrens.Add(NGauge("QUALITY", 94, Green, Cyan), 2, 0);
        mid.Childrens.Add(gauges, 2, 0);

        var svcP = NPanel("SERVICE HEALTH", 34);
        var svc = Tbl(["*"], ["*", "*", "*", "*", "*", "*"]);
        (string n, string st, string c)[] svcs = { ("API Gateway", "RUN", Green), ("Database", "RUN", Green), ("Cache", "WARN", Amber), ("Storage", "RUN", Green), ("Scheduler", "RUN", Green), ("Vision AI", "FAULT", Red) };
        for (int i = 0; i < svcs.Length; i++)
        {
            var row = Tbl(["*", "70px"], ["*"]);
            row.Childrens.Add(new GoLamp { Text = svcs[i].n, OnOff = true, OnColor = svcs[i].c, TextColor = "#C7D0DC", FontSize = 13, LampSize = 9, ContentAlignment = GoContentAlignment.MiddleLeft, Gap = 10, Dock = GoDockStyle.Fill }, 0, 0);
            row.Childrens.Add(NLabel(svcs[i].st, 11, svcs[i].c, GoFontStyle.Bold, GoContentAlignment.MiddleRight), 1, 0);
            svc.Childrens.Add(row, 0, i);
        }
        svcP.Childrens.Add(svc);
        mid.Childrens.Add(svcP, 4, 0);
        main.Childrens.Add(mid, 0, 4);

        // 하단: 타일 매트릭스 | 이벤트 로그
        var bottom = Tbl(["*", "16px", "340px"], ["*"]);
        var tileP = NPanel("STATION MATRIX", 34);
        var tg = new List<string>(); for (int i = 0; i < 6; i++) { tg.Add("*"); if (i < 5) tg.Add("12px"); }
        var matrix = Tbl([.. tg], ["*", "12px", "*"]);
        var palette = new[] { Green, Cyan, Amber, Green, Red, Green, Cyan, Green, Amber, Green, Cyan, Green };
        for (int i = 0; i < 12; i++)
        {
            var cell = NPanel("", 0);
            cell.PanelColor = "#10151C"; cell.PanelColor2 = "#10151C"; cell.Elevation = 0; cell.Padding = new(12, 9, 12, 9);
            var cg = Tbl(["*"], ["15px", "*"]);
            cg.Childrens.Add(new GoLamp { Text = $"ST-{i + 1:00}", OnOff = true, OnColor = palette[i], TextColor = Sub, FontSize = 11, FontStyle = GoFontStyle.Bold, LampSize = 8, ContentAlignment = GoContentAlignment.MiddleLeft, Gap = 6, Dock = GoDockStyle.Fill }, 0, 0);
            cg.Childrens.Add(NLabel($"{(i * 7 + 31) % 90 + 10}", 20, Pri, GoFontStyle.Bold, GoContentAlignment.MiddleLeft), 0, 1);
            cell.Childrens.Add(cg);
            matrix.Childrens.Add(cell, (i % 6) * 2, (i / 6) * 2);
        }
        tileP.Childrens.Add(matrix);
        bottom.Childrens.Add(tileP, 0, 0);

        var logP = NPanel("EVENT LOG", 34);
        var log = Tbl(["*"], ["*", "*", "*", "*", "*", "*", "*"]);
        (string t, string c)[] lines = {
            ("12:04  ST-09 cycle complete", Sub), ("12:03  Vision AI fault detected", Red),
            ("12:01  Cache latency warning", Amber), ("11:58  Batch #4471 started", Sub),
            ("11:55  OEE target reached", Green), ("11:52  Operator login: kim", Sub),
            ("11:49  ST-03 idle", Muted),
        };
        for (int i = 0; i < lines.Length; i++) log.Childrens.Add(NLabel(lines[i].t, 12, lines[i].c), 0, i);
        logP.Childrens.Add(log);
        bottom.Childrens.Add(logP, 2, 0);
        main.Childrens.Add(bottom, 0, 6);

        root.Childrens.Add(main);
        page.Childrens.Add(root);
        d.AddPage(page);
        return d;
    }

    private static void RenderAndExport(GoDesign design, string name)
    {
        design.SetSize(W, H);
        design.Init();
        design.SetPage("MainPage");
        var bmp = new SKBitmap(W, H);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(new SKColor(0x0E, 0x11, 0x17));
            design.CurrentPage!.Bounds = new SKRect(0, 0, W, H);
            design.CurrentPage.FireDraw(canvas, design.Theme);
            canvas.Flush();
        }
        Directory.CreateDirectory(ArtifactDir);
        using (var img = SKImage.FromBitmap(bmp))
        using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
        using (var fs = File.Create(Path.Combine(ArtifactDir, $"sample-{name}.png")))
            data.SaveTo(fs);
        bmp.Dispose();

        design.SerializeGudx(Path.Combine(SampleDir, name, "Master.gudx"));
    }

    [Fact]
    public void Build_S1_Vo()
    {
        RenderAndExport(BuildS1(), "s1");
        Assert.True(File.Exists(Path.Combine(SampleDir, "s1", "Pages", "MainPage.gudx")));
    }

    [Fact]
    public void Build_S2_Native()
    {
        RenderAndExport(BuildS2(), "s2");
        Assert.True(File.Exists(Path.Combine(SampleDir, "s2", "Pages", "MainPage.gudx")));
    }
}
