using System;
using System.Collections.Generic;
using System.IO;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using Going.UI.ViewObjects;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests;

/// <summary>
/// 쪼갠 버전: 카드/게이지/패널을 각각 별도 VoControl로 만들어 페이지에 절대 배치.
/// 에디터에서 개별 선택/이동 가능. 미리보기 렌더 + 바탕화면 VoTest로 내보내기.
/// </summary>
public class VoObjExportTests
{
    private const int W = 1280, H = 720;

    private static readonly string ArtifactDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "voobj-artifacts");

    [Fact]
    public void Dashboard_Split_Render_And_ExportToDesktop()
    {
        var controls = BuildControls();

        // 1) 미리보기 — 각 컨트롤을 자기 Bounds 위치에 합성
        var bmp = new SKBitmap(W, H);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(new SKColor(0x0E, 0x11, 0x16));
            foreach (var c in controls)
            {
                var rt = c.Bounds;
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.Translate(rt.Left, rt.Top);
                    c.Bounds = new SKRect(0, 0, rt.Width, rt.Height);
                    c.FireDraw(canvas, GoTheme.DarkTheme);
                    c.Bounds = rt; // 내보내기용으로 절대 좌표 복원
                }
            }
            canvas.Flush();
        }
        Directory.CreateDirectory(ArtifactDir);
        using (var img = SKImage.FromBitmap(bmp))
        using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
        using (var fs = File.Create(Path.Combine(ArtifactDir, "voobj-dashboard-split.png")))
            data.SaveTo(fs);
        bmp.Dispose();

        // 2) UIEditor용 GoDesign — 페이지에 여러 VoControl
        var page = new GoPage { Name = "MainPage" };
        foreach (var c in BuildControls()) page.Childrens.Add(c);

        var design = new GoDesign { Name = "VoTest", DesignWidth = W, DesignHeight = H };
        design.AddPage(page);

        var dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "VoTest");
        Directory.CreateDirectory(dest);
        design.SerializeGudx(Path.Combine(dest, "Master.gudx"));

        Assert.True(File.Exists(Path.Combine(dest, "Pages", "MainPage.gudx")));
        Assert.True(controls.Count >= 6);
    }

    // ── 레이아웃 계산 후 개별 컨트롤 생성 ────────────────────────
    private static List<VoControl> BuildControls()
    {
        var content = new SKRect(24, 24, W - 24, H - 24);
        var rows = Util.Grid(content, ["*"], ["44px", "20px", "230px", "20px", "*"]);
        var headerRt = rows[0, 0];
        var cardsRt = rows[2, 0];
        var stationRt = rows[4, 0];

        var cells = Util.Grid(cardsRt, ["*", "18px", "*", "18px", "*", "18px", "300px"], ["*"]);

        var list = new List<VoControl>
        {
            Host("header", headerRt, new VoText { Text = "PRODUCTION LINE 7  —  OVERVIEW", TextColor = "#E8F1F5", FontSize = 24, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.MiddleLeft }),
            Host("cardCpu",  cells[0, 0], StatCard("CPU LOAD",   "48%", "#34C3FF", "#39D98A", 48)),
            Host("cardMem",  cells[0, 2], StatCard("MEMORY",     "81%", "#5B8DEF", "#9B6BEF", 81)),
            Host("cardThru", cells[0, 4], StatCard("THROUGHPUT", "63%", "#F59E0B", "#EF4444", 63)),
            Host("gauge",    cells[0, 6], GaugeCard("OEE", "72%", 72)),
            Host("station",  stationRt,   StationPanel()),
        };
        return list;
    }

    private static VoControl Host(string name, SKRect bounds, VoObj root) => new()
    {
        Name = name,
        Bounds = bounds,
        Dock = GoDockStyle.None,
        Children = { root }
    };

    // ── 각 컨트롤의 내부 트리 (루트 채움) ────────────────────────
    private static VoBox StatCard(string label, string value, string c1, string c2, float pct) => new()
    {
        Background = "#1B212B", FillType = VoFillType.Linear, FillColor2 = "#141921",
        BorderRadius = 16, ShadowColor = "#000000", ShadowY = 6, ShadowBlur = 16, Padding = new(20),
        Children =
        {
            new VoGrid
            {
                Rows = ["18px", "6px", "44px", "16px", "14px", "*"],
                Children =
                {
                    new VoText { Text = label, TextColor = "#8FA1B3", FontSize = 13, Alignment = GoContentAlignment.MiddleLeft, Row = 0 },
                    new VoText { Name = "value", Text = value, TextColor = "#FFFFFF", FontSize = 36, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.MiddleLeft, Row = 2 },
                    new VoProgress { Name = "bar", Row = 4, Value = pct, TrackColor = "#2A323F", FillColor = c1, FillColor2 = c2, Radius = 7 },
                }
            }
        }
    };

    private static VoBox GaugeCard(string label, string value, float pct) => new()
    {
        Background = "#1B212B", FillType = VoFillType.Linear, FillColor2 = "#141921",
        BorderRadius = 16, ShadowColor = "#000000", ShadowY = 6, ShadowBlur = 16, Padding = new(16),
        Children =
        {
            new VoGrid
            {
                Rows = ["18px", "*"],
                Children =
                {
                    new VoText { Text = label, TextColor = "#8FA1B3", FontSize = 13, Alignment = GoContentAlignment.MiddleLeft, Row = 0 },
                    new VoArc { Row = 1, StartAngle = 135, SweepAngle = 270, Thickness = 16, Color = "#2A323F", RoundCap = true },
                    new VoArc { Name = "gauge", Row = 1, StartAngle = 135, SweepAngle = 270, Thickness = 16, Color = "#34C3FF", Color2 = "#9B6BEF", RoundCap = true, Value = pct, Max = 100 },
                    new VoText { Name = "value", Row = 1, Text = value, TextColor = "#FFFFFF", FontSize = 38, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.MiddleCenter },
                }
            }
        }
    };

    private static VoBox StationPanel() => new()
    {
        Background = "#171C24", FillType = VoFillType.Linear, FillColor2 = "#12161D",
        BorderRadius = 16, ShadowColor = "#000000", ShadowY = 6, ShadowBlur = 16, Padding = new(22),
        Children =
        {
            new VoGrid
            {
                Rows = ["22px", "14px", "*"],
                Children =
                {
                    new VoText { Text = "STATION STATUS", TextColor = "#8FA1B3", FontSize = 14, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.MiddleLeft, Row = 0 },
                    new VoGrid
                    {
                        Row = 2,
                        Columns = ["*", "16px", "*", "16px", "*", "16px", "*"],
                        Children =
                        {
                            StationTile("ST-01", "#39D98A", "RUN", 0),
                            StationTile("ST-02", "#39D98A", "RUN", 2),
                            StationTile("ST-03", "#F59E0B", "IDLE", 4),
                            StationTile("ST-04", "#EF4444", "FAULT", 6),
                        }
                    }
                }
            }
        }
    };

    private static VoBox StationTile(string name, string color, string state, int col) => new()
    {
        Col = col, Background = "#10151C", BorderRadius = 12, BorderColor = "#222A35", BorderWidth = 1, Padding = new(14),
        Children =
        {
            new VoGrid
            {
                Columns = ["14px", "10px", "*"],
                Rows = ["*", "*"],
                Children =
                {
                    new VoBox { Col = 0, Row = 0, RowSpan = 2, Background = color, BorderRadius = 7, Margin = new(0, 14, 0, 14) },
                    new VoText { Col = 2, Row = 0, Text = name, TextColor = "#E8F1F5", FontSize = 16, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.BottomLeft },
                    new VoText { Col = 2, Row = 1, Text = state, TextColor = color, FontSize = 12, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.TopLeft },
                }
            }
        }
    };
}
