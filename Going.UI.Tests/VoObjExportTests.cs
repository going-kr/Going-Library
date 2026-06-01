using System;
using System.IO;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.ViewObjects;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests;

/// <summary>
/// 천장 테스트: AI가 조립한 복잡한 HMI 대시보드(원형 게이지 포함)를 렌더하고,
/// UIEditor로 열 수 있는 GoDesign(.gudx) 프로젝트로 바탕화면 VoTest 폴더에 내보냅니다.
/// </summary>
public class VoObjExportTests
{
    private static readonly string ArtifactDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "voobj-artifacts");

    [Fact]
    public void Dashboard_Render_And_ExportToDesktop()
    {
        var vc = BuildDashboard();

        // 1) 미리보기 PNG
        const int W = 1280, H = 720;
        vc.Bounds = new SKRect(0, 0, W, H);
        var bmp = new SKBitmap(W, H);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(new SKColor(0x0E, 0x11, 0x16));
            vc.FireDraw(canvas, GoTheme.DarkTheme);
            canvas.Flush();
        }
        Directory.CreateDirectory(ArtifactDir);
        using (var img = SKImage.FromBitmap(bmp))
        using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
        using (var fs = File.Create(Path.Combine(ArtifactDir, "voobj-dashboard-full.png")))
            data.SaveTo(fs);
        bmp.Dispose();

        // 2) UIEditor용 GoDesign 프로젝트로 내보내기 (바탕화면\VoTest\Master.gudx)
        var page = new GoPage { Name = "MainPage" };
        var host = BuildDashboard();
        host.Dock = GoDockStyle.Fill;
        page.Childrens.Add(host);

        var design = new GoDesign { Name = "VoTest", DesignWidth = W, DesignHeight = H };
        design.AddPage(page);

        var dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "VoTest");
        Directory.CreateDirectory(dest);
        design.SerializeGudx(Path.Combine(dest, "Master.gudx"));

        Assert.True(File.Exists(Path.Combine(dest, "Master.gudx")));
        Assert.True(File.Exists(Path.Combine(dest, "Pages", "MainPage.gudx")));
    }

    // ── 화면 조립 ───────────────────────────────────────────────
    private static VoControl BuildDashboard() => new()
    {
        Children =
        {
            new VoBox
            {
                Background = "#0E1116", Padding = new(24),
                Children =
                {
                    new VoGrid
                    {
                        Rows = ["44px", "20px", "230px", "20px", "*"],
                        Children =
                        {
                            new VoText { Text = "PRODUCTION LINE 7  —  OVERVIEW", TextColor = "#E8F1F5", FontSize = 24, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.MiddleLeft, Row = 0 },

                            // 상단: 스탯 카드 3 + 게이지 카드
                            new VoGrid
                            {
                                Row = 2,
                                Columns = ["*", "18px", "*", "18px", "*", "18px", "300px"],
                                Children =
                                {
                                    StatCard("CPU LOAD", "48%", "#34C3FF", "#39D98A", 0.48F, 0),
                                    StatCard("MEMORY", "81%", "#5B8DEF", "#9B6BEF", 0.81F, 2),
                                    StatCard("THROUGHPUT", "63%", "#F59E0B", "#EF4444", 0.63F, 4),
                                    GaugeCard("OEE", "72%", 0.72F, 6),
                                }
                            },

                            // 하단: 라인 상태 패널
                            new VoBox
                            {
                                Row = 4, Background = "#171C24", FillType = VoFillType.Linear, FillColor2 = "#12161D",
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
                            },
                        }
                    }
                }
            }
        }
    };

    private static VoBox StatCard(string label, string value, string c1, string c2, float pct, int col) => new()
    {
        Col = col, Background = "#1B212B", FillType = VoFillType.Linear, FillColor2 = "#141921",
        BorderRadius = 16, ShadowColor = "#000000", ShadowY = 6, ShadowBlur = 16, Padding = new(20),
        Children =
        {
            new VoGrid
            {
                Rows = ["18px", "6px", "44px", "16px", "14px", "*"],
                Children =
                {
                    new VoText { Text = label, TextColor = "#8FA1B3", FontSize = 13, Alignment = GoContentAlignment.MiddleLeft, Row = 0 },
                    new VoText { Text = value, TextColor = "#FFFFFF", FontSize = 36, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.MiddleLeft, Row = 2 },
                    new VoBox
                    {
                        Row = 4, Background = "#2A323F", BorderRadius = 7,
                        Children =
                        {
                            new VoGrid
                            {
                                Columns = [$"{(int)(pct * 100)}%", $"{100 - (int)(pct * 100)}%"],
                                Children = { new VoBox { Col = 0, Background = c1, FillType = VoFillType.Linear, FillColor2 = c2, GradientAngle = 0, BorderRadius = 7 } }
                            }
                        }
                    },
                }
            }
        }
    };

    private static VoBox GaugeCard(string label, string value, float pct, int col) => new()
    {
        Col = col, Background = "#1B212B", FillType = VoFillType.Linear, FillColor2 = "#141921",
        BorderRadius = 16, ShadowColor = "#000000", ShadowY = 6, ShadowBlur = 16, Padding = new(16),
        Children =
        {
            new VoGrid
            {
                Rows = ["18px", "*"],
                Children =
                {
                    new VoText { Text = label, TextColor = "#8FA1B3", FontSize = 13, Alignment = GoContentAlignment.MiddleLeft, Row = 0 },
                    // 게이지: 트랙 호 + 값 호 + 중앙 값 (같은 셀에 겹침)
                    new VoArc { Row = 1, StartAngle = 135, SweepAngle = 270, Thickness = 16, Color = "#2A323F", RoundCap = true },
                    new VoArc { Row = 1, StartAngle = 135, SweepAngle = 270 * pct, Thickness = 16, Color = "#34C3FF", Color2 = "#9B6BEF", RoundCap = true },
                    new VoText { Row = 1, Text = value, TextColor = "#FFFFFF", FontSize = 38, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.MiddleCenter },
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
                    new VoBox { Col = 0, Row = 0, RowSpan = 2, Background = color, BorderRadius = 7, Margin = new(0, 6, 0, 6) },
                    new VoText { Col = 2, Row = 0, Text = name, TextColor = "#E8F1F5", FontSize = 16, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.BottomLeft },
                    new VoText { Col = 2, Row = 1, Text = state, TextColor = color, FontSize = 12, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.TopLeft },
                }
            }
        }
    };
}
