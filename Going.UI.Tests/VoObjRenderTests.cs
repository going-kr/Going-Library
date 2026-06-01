using System.IO;
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.ViewObjects;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests;

/// <summary>
/// VoObj 프로토타입: 조립→렌더, gudx 직렬화 왕복, 예쁜 샘플 산출물 확인.
/// </summary>
public class VoObjRenderTests
{
    private static readonly string ArtifactDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "voobj-artifacts");

    private static SKBitmap Render(VoControl vc, int w, int h)
    {
        vc.Bounds = new SKRect(0, 0, w, h);
        var bmp = new SKBitmap(w, h);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(new SKColor(0x14, 0x16, 0x1E));
        vc.FireDraw(canvas, GoTheme.DarkTheme);
        canvas.Flush();
        return bmp;
    }

    private static void Save(SKBitmap bmp, string name)
    {
        Directory.CreateDirectory(ArtifactDir);
        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var fs = File.Create(Path.Combine(ArtifactDir, name));
        data.SaveTo(fs);
    }

    [Fact]
    public void VoStack_Vertical_PlacesChildrenInThirds()
    {
        var vc = new VoControl
        {
            Children =
            {
                new VoStack
                {
                    Direction = GoDirectionHV.Vertical,
                    Children =
                    {
                        new VoBox { Background = "#FF0000" },
                        new VoBox { Background = "#00FF00" },
                        new VoBox { Background = "#0000FF" },
                    }
                }
            }
        };

        using var bmp = Render(vc, 120, 300);
        var top = bmp.GetPixel(60, 50);
        var mid = bmp.GetPixel(60, 150);
        var bot = bmp.GetPixel(60, 250);

        Assert.True(top.Red > 200 && top.Green < 60 && top.Blue < 60, $"top={top}");
        Assert.True(mid.Green > 200 && mid.Red < 60 && mid.Blue < 60, $"mid={mid}");
        Assert.True(bot.Blue > 200 && bot.Red < 60 && bot.Green < 60, $"bot={bot}");
    }

    [Fact]
    public void EmptyControl_DoesNotThrow()
    {
        var vc = new VoControl();
        using var bmp = Render(vc, 50, 50);
        Assert.Equal(new SKColor(0x14, 0x16, 0x1E), bmp.GetPixel(25, 25));
    }

    [Fact]
    public void Gudx_RoundTrip_PreservesTreeAndRendersIdentically()
    {
        var vc = SampleCard();

        // 직렬화 → XML
        var xml = GoGudxConverter.SerializeControl(vc);
        Directory.CreateDirectory(ArtifactDir);
        File.WriteAllText(Path.Combine(ArtifactDir, "voobj-card.gudx"), xml);

        // 트리가 XML에 실렸는지
        Assert.Contains("<VoControl", xml);
        Assert.Contains("<VoBox", xml);
        Assert.Contains("<VoText", xml);
        Assert.Contains("<VoGrid", xml);

        // 역직렬화 → 같은 타입 복원
        var back = GoGudxConverter.DeserializeControl(xml);
        var vc2 = Assert.IsType<VoControl>(back);

        // 원본/복원본 렌더가 동일한지 (몇 개 픽셀 비교)
        using var b1 = Render(vc, 360, 200);
        using var b2 = Render(vc2, 360, 200);
        foreach (var (x, y) in new[] { (30, 30), (180, 100), (320, 170), (40, 150) })
            Assert.Equal(b1.GetPixel(x, y), b2.GetPixel(x, y));
    }

    [Fact]
    public void PrettySample_ProducesArtifact()
    {
        var vc = SampleCard();
        using var bmp = Render(vc, 360, 200);
        Save(bmp, "voobj-card.png");

        // 카드 영역이 배경과 다르게 그려졌는지 (실제로 뭔가 그려짐)
        Assert.NotEqual(new SKColor(0x14, 0x16, 0x1E), bmp.GetPixel(180, 100));
    }

    /// <summary>
    /// "AI 저작" 시나리오: 손으로 작성한 raw .gudx XML이 역직렬화되어 화면이 나오는지.
    /// = 제품 핵심 가설(AI가 vset을 데이터로 직접 작성) 검증.
    /// </summary>
    [Fact]
    public void AiAuthored_RawXml_DeserializesAndRenders()
    {
        const string xml = """
        <VoControl Bounds="0,0,640,360">
          <Children>
            <VoBox Background="#171A24" Padding="18,18,18,18">
              <Children>
                <VoGrid Rows="34px,14px,*">
                  <Children>
                    <VoText Text="System Dashboard" TextColor="#FFFFFF" FontSize="22" FontStyle="Bold" Alignment="MiddleLeft" Row="0" />
                    <VoGrid Row="2" Columns="*,16px,*">
                      <Children>
                        <VoBox Col="0" Background="#2E3242" FillType="Linear" FillColor2="#21242F" GradientAngle="90" BorderRadius="14" ShadowColor="#000000" ShadowY="6" ShadowBlur="14" Padding="18,18,18,18">
                          <Children>
                            <VoGrid Rows="18px,6px,38px,12px,14px,*">
                              <Children>
                                <VoText Text="CPU" TextColor="#8E94AB" FontSize="13" Alignment="MiddleLeft" Row="0" />
                                <VoText Text="48%" TextColor="#FFFFFF" FontSize="32" FontStyle="Bold" Alignment="MiddleLeft" Row="2" />
                                <VoBox Row="4" Background="#3A3F52" BorderRadius="7">
                                  <Children>
                                    <VoGrid Columns="48%,52%">
                                      <Children>
                                        <VoBox Col="0" Background="#34C3FF" FillType="Linear" FillColor2="#39D98A" GradientAngle="0" BorderRadius="7" />
                                      </Children>
                                    </VoGrid>
                                  </Children>
                                </VoBox>
                              </Children>
                            </VoGrid>
                          </Children>
                        </VoBox>
                        <VoBox Col="2" Background="#2E3242" FillType="Linear" FillColor2="#21242F" GradientAngle="90" BorderRadius="14" ShadowColor="#000000" ShadowY="6" ShadowBlur="14" Padding="18,18,18,18">
                          <Children>
                            <VoGrid Rows="18px,6px,38px,12px,14px,*">
                              <Children>
                                <VoText Text="MEMORY" TextColor="#8E94AB" FontSize="13" Alignment="MiddleLeft" Row="0" />
                                <VoText Text="81%" TextColor="#FFFFFF" FontSize="32" FontStyle="Bold" Alignment="MiddleLeft" Row="2" />
                                <VoBox Row="4" Background="#3A3F52" BorderRadius="7">
                                  <Children>
                                    <VoGrid Columns="81%,19%">
                                      <Children>
                                        <VoBox Col="0" Background="#5B8DEF" FillType="Linear" FillColor2="#9B6BEF" GradientAngle="0" BorderRadius="7" />
                                      </Children>
                                    </VoGrid>
                                  </Children>
                                </VoBox>
                              </Children>
                            </VoGrid>
                          </Children>
                        </VoBox>
                      </Children>
                    </VoGrid>
                  </Children>
                </VoGrid>
              </Children>
            </VoBox>
          </Children>
        </VoControl>
        """;

        var vc = Assert.IsType<VoControl>(GoGudxConverter.DeserializeControl(xml));
        using var bmp = Render(vc, 640, 360);
        Save(bmp, "voobj-dashboard.png");

        Assert.NotEqual(new SKColor(0x14, 0x16, 0x1E), bmp.GetPixel(160, 200)); // 카드 A 영역
        Assert.NotEqual(new SKColor(0x14, 0x16, 0x1E), bmp.GetPixel(480, 200)); // 카드 B 영역
    }

    /// <summary>
    /// 값 노드: Node&lt;T&gt;로 참조를 1회 얻어 값만 바꿔도 렌더가 달라지는지.
    /// (setter는 필드 대입, 해석은 draw에서 — OnUpdate 반복 호출 무부담 설계 검증)
    /// </summary>
    [Fact]
    public void ValueNodes_DriveRendering_Cheaply()
    {
        var vc = new VoControl
        {
            Children =
            {
                new VoBox
                {
                    Background = "#161B22", Padding = new(16),
                    Children =
                    {
                        new VoGrid
                        {
                            Rows = ["*", "16px", "26px"],
                            Children =
                            {
                                new VoArc { Name = "gauge", Row = 0, StartAngle = 135, SweepAngle = 270, Thickness = 18, Color = "#34C3FF", Color2 = "#9B6BEF", Max = 100 },
                                new VoProgress { Name = "bar", Row = 2, FillColor = "#39D98A", FillColor2 = "#34C3FF", TrackColor = "#2A323F", Max = 100 },
                            }
                        }
                    }
                }
            }
        };

        // 참조는 1회만 해석해서 캐시 (이후엔 값 대입만)
        var gauge = vc.Node<VoArc>("gauge");
        var bar = vc.Node<VoProgress>("bar");
        Assert.NotNull(gauge);
        Assert.NotNull(bar);

        gauge!.Value = 30; bar!.Value = 30;
        using var lo = Render(vc, 240, 260);
        Save(lo, "voobj-gauge-30.png");

        gauge.Value = 78; bar.Value = 78;   // 필드 대입만
        using var hi = Render(vc, 240, 260);
        Save(hi, "voobj-gauge-78.png");

        // 값이 커지면 게이지 호가 더 그려져서 우상단 영역 픽셀이 달라져야 함
        Assert.NotEqual(lo.GetPixel(195, 70), hi.GetPixel(195, 70));
    }

    /// <summary>대시보드 카드 한 장 — 라운드/그라데이션/그림자/패딩/그리드/텍스트/프로그레스.</summary>
    private static VoControl SampleCard() => new()
    {
        Children =
        {
            // 카드 (margin으로 바깥 여백, padding으로 안쪽 여백)
            new VoBox
            {
                Background = "#2E3242", FillType = VoFillType.Linear, FillColor2 = "#21242F", GradientAngle = 90,
                BorderRadius = 16, ShadowColor = "#000000", ShadowY = 6, ShadowBlur = 14,
                Margin = new(10), Padding = new(20),
                Children =
                {
                    new VoGrid
                    {
                        Rows = ["18px", "6px", "40px", "10px", "18px", "*"],
                        Children =
                        {
                            new VoText { Text = "SYSTEM LOAD", TextColor = "#8E94AB", FontSize = 13, Alignment = GoContentAlignment.MiddleLeft, Row = 0 },
                            new VoText { Text = "72%", TextColor = "#FFFFFF", FontSize = 34, FontStyle = GoFontStyle.Bold, Alignment = GoContentAlignment.MiddleLeft, Row = 2 },
                            // 프로그레스 트랙 + 채움
                            new VoBox
                            {
                                Background = "#3A3F52", BorderRadius = 8, Row = 4,
                                Children =
                                {
                                    new VoGrid
                                    {
                                        Columns = ["72%", "28%"],
                                        Children =
                                        {
                                            new VoBox { Background = "#5B8DEF", FillType = VoFillType.Linear, FillColor2 = "#9B6BEF", GradientAngle = 0, BorderRadius = 8, Col = 0 },
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
}
