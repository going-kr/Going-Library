using System;
using System.IO;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests;

/// <summary>
/// VoObj를 쓰지 않고 기존 네이티브 컨트롤만으로 이쁜 대시보드를 구성 → 미리보기 렌더 + 바탕화면 VoTest2 내보내기.
/// (VoObj 버전과 품질 비교용)
/// </summary>
public class NativeDashboardExportTests
{
    private const int W = 1280, H = 720;

    private static readonly string ArtifactDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "voobj-artifacts");

    [Fact]
    public void NativeDashboard_Render_And_ExportToDesktop()
    {
        // 1) 미리보기 렌더
        var design = NewDesign();
        design.SetSize(W, H);
        design.Init();
        design.SetPage("MainPage");

        var bmp = new SKBitmap(W, H);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(design.Theme.Back);
            design.Draw(canvas);
            canvas.Flush();
        }
        Directory.CreateDirectory(ArtifactDir);
        using (var img = SKImage.FromBitmap(bmp))
        using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
        using (var fs = File.Create(Path.Combine(ArtifactDir, "native-dashboard.png")))
            data.SaveTo(fs);
        bmp.Dispose();

        // 2) 내보내기 (새 인스턴스로 — 렌더와 상태 분리)
        var dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "VoTest2");
        Directory.CreateDirectory(dest);
        NewDesign().SerializeGudx(Path.Combine(dest, "Master.gudx"));

        Assert.True(File.Exists(Path.Combine(dest, "Pages", "MainPage.gudx")));
    }

    private static GoDesign NewDesign()
    {
        var page = new GoPage { Name = "MainPage", Padding = new(20) };

        var root = new GoTableLayoutPanel
        {
            Columns = ["100%"],
            Rows = ["52px", "232px", "*"],
            Dock = GoDockStyle.Fill,
        };
        root.Childrens.Add(Header("PRODUCTION LINE 7  —  OVERVIEW"), 0, 0);

        // 카드 행
        var cards = new GoTableLayoutPanel { Columns = ["*", "*", "*", "330px"], Rows = ["100%"], Dock = GoDockStyle.Fill, Margin = new(0, 12, 0, 0) };
        cards.Childrens.Add(StatCard("CPU LOAD", 48, "Point"), 0, 0);
        cards.Childrens.Add(StatCard("MEMORY", 81, "User2"), 1, 0);
        cards.Childrens.Add(StatCard("THROUGHPUT", 63, "Warning"), 2, 0);
        cards.Childrens.Add(GaugeCard("OEE", 72), 3, 0);
        root.Childrens.Add(cards, 0, 1);

        // 스테이션 행
        root.Childrens.Add(StationPanel(), 0, 2);

        page.Childrens.Add(root);

        var d = new GoDesign { Name = "VoTest2", DesignWidth = W, DesignHeight = H };
        d.AddPage(page);
        return d;
    }

    private static GoLabel Header(string text) => new()
    {
        Text = text, FontName = "Segoe UI", FontStyle = GoFontStyle.Bold, FontSize = 24,
        TextColor = "Fore", ContentAlignment = GoContentAlignment.MiddleLeft, BackgroundDraw = false,
        Dock = GoDockStyle.Fill, Margin = new(2, 0, 0, 0),
    };

    private static GoBoxPanel Card() => new()
    {
        BoxColor = "Base2", BorderColor = "Base4", Round = GoRoundType.All, BackgroundDraw = true,
        BorderWidth = 1, Elevation = 2, Padding = new(18), Dock = GoDockStyle.Fill, Margin = new(0, 0, 14, 0),
    };

    private static GoBoxPanel StatCard(string label, double value, string accent)
    {
        var card = Card();
        var t = new GoTableLayoutPanel { Columns = ["100%"], Rows = ["22px", "54px", "*", "26px"], Dock = GoDockStyle.Fill };
        t.Childrens.Add(new GoLabel { Text = label, FontName = "Segoe UI", FontSize = 13, TextColor = "Base5", ContentAlignment = GoContentAlignment.MiddleLeft, BackgroundDraw = false, Dock = GoDockStyle.Fill }, 0, 0);
        t.Childrens.Add(new GoLabel { Text = $"{value:0}%", FontName = "Segoe UI", FontStyle = GoFontStyle.Bold, FontSize = 38, TextColor = "Fore", ContentAlignment = GoContentAlignment.MiddleLeft, BackgroundDraw = false, Dock = GoDockStyle.Fill }, 0, 1);
        t.Childrens.Add(new GoProgress
        {
            Value = value, Minimum = 0, Maximum = 100, FillColor = accent, EmptyColor = "Base3",
            BorderWidth = 0, CornerRadius = 7, ShowValueLabel = false, Dock = GoDockStyle.Fill,
        }, 0, 3);
        card.Childrens.Add(t);
        return card;
    }

    private static GoBoxPanel GaugeCard(string label, double value)
    {
        var card = Card();
        card.Margin = new(0); // 마지막 카드
        var t = new GoTableLayoutPanel { Columns = ["100%"], Rows = ["22px", "*"], Dock = GoDockStyle.Fill };
        t.Childrens.Add(new GoLabel { Text = label, FontName = "Segoe UI", FontSize = 13, TextColor = "Base5", ContentAlignment = GoContentAlignment.MiddleLeft, BackgroundDraw = false, Dock = GoDockStyle.Fill }, 0, 0);
        t.Childrens.Add(new GoGauge
        {
            Value = value, Minimum = 0, Maximum = 100, Title = "%", FillColor = "Point", EmptyColor = "Base3",
            StartAngle = 135, SweepAngle = 270, BarSize = 16, Format = "0", FontSize = 34,
            TextColor = "Fore", Dock = GoDockStyle.Fill,
        }, 0, 1);
        card.Childrens.Add(t);
        return card;
    }

    private static GoBoxPanel StationPanel()
    {
        var card = new GoBoxPanel { BoxColor = "Base1", BorderColor = "Base3", Round = GoRoundType.All, BackgroundDraw = true, BorderWidth = 1, Padding = new(18), Dock = GoDockStyle.Fill, Margin = new(0, 14, 0, 0) };
        var t = new GoTableLayoutPanel { Columns = ["100%"], Rows = ["24px", "*"], Dock = GoDockStyle.Fill };
        t.Childrens.Add(new GoLabel { Text = "STATION STATUS", FontName = "Segoe UI", FontStyle = GoFontStyle.Bold, FontSize = 14, TextColor = "Base5", ContentAlignment = GoContentAlignment.MiddleLeft, BackgroundDraw = false, Dock = GoDockStyle.Fill }, 0, 0);

        var grid = new GoTableLayoutPanel { Columns = ["*", "*", "*", "*"], Rows = ["100%"], Dock = GoDockStyle.Fill, Margin = new(0, 10, 0, 0) };
        grid.Childrens.Add(StationTile("ST-01", "RUN", "Good"), 0, 0);
        grid.Childrens.Add(StationTile("ST-02", "RUN", "Good"), 1, 0);
        grid.Childrens.Add(StationTile("ST-03", "IDLE", "Warning"), 2, 0);
        grid.Childrens.Add(StationTile("ST-04", "FAULT", "Danger"), 3, 0);
        t.Childrens.Add(grid, 0, 1);

        card.Childrens.Add(t);
        return card;
    }

    private static GoBoxPanel StationTile(string name, string state, string color)
    {
        var tile = new GoBoxPanel { BoxColor = "Base2", BorderColor = "Base4", Round = GoRoundType.All, BackgroundDraw = true, BorderWidth = 1, Padding = new(16), Dock = GoDockStyle.Fill, Margin = new(0, 0, 12, 0) };
        var t = new GoTableLayoutPanel { Columns = ["100%"], Rows = ["*", "30px"], Dock = GoDockStyle.Fill };
        t.Childrens.Add(new GoLamp { Text = name, FontName = "Segoe UI", FontStyle = GoFontStyle.Bold, FontSize = 18, TextColor = "Fore", OnColor = color, OffColor = "Base3", OnOff = true, LampSize = 16, ContentAlignment = GoContentAlignment.MiddleLeft, Dock = GoDockStyle.Fill }, 0, 0);
        t.Childrens.Add(new GoLabel { Text = state, FontName = "Segoe UI", FontStyle = GoFontStyle.Bold, FontSize = 12, TextColor = color, ContentAlignment = GoContentAlignment.MiddleLeft, BackgroundDraw = false, Dock = GoDockStyle.Fill }, 0, 1);
        tile.Childrens.Add(t);
        return tile;
    }
}
