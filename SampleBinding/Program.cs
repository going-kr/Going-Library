using System.Xml.Linq;
using Going.UI.Collections;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;

namespace SampleBinding;

// ─────────────────────────────────────────────────────────────────────────────
// 이번 작업 3종 — 선언적 바인딩 · 컴포넌트 · GoItemList — 을 한 창에 띄우는 데모.
//
//   1) 마크업(gudx)으로 화면을 정의       (Gudx 상수)
//   2) GoDesign.Deserialize... 로 로드      (ReadGoDesign)
//   3) WireBindings(hub) 한 줄로 데이터 연결
//   4) 렌더 루프가 매 프레임 자동 동기화
//
// 헤드리스 환경에서는 창이 안 뜨지만, 데스크톱(Windows/Linux)에서 실행하면
// 펌프 RPM·로그가 실시간으로 갱신되는 걸 볼 수 있다.
// ─────────────────────────────────────────────────────────────────────────────

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var hub = new AppHub();
        var design = GoGudxConverter.ReadGoDesign(XElement.Parse(Markup.Gudx))
                     ?? throw new InvalidOperationException("gudx 파싱 실패");

        // 헤드리스 자가 점검: 창 없이 바인딩만 검증하고 종료 (디스플레이 불필요).
        if (args.Contains("--smoke")) { Smoke.Run(design, hub); return; }

        using var win = new DemoWindow(design, hub) { Title = "Going · Binding Sample" };
        win.Run();
    }
}

/// <summary>
/// GoViewWindow를 상속해 (a) 페이지/바인딩 셋업과 (b) UI 스레드에서의 라이브 데이터 변경을 담당.
/// 데이터 변경을 OnUpdateFrame(UI 스레드)에서 하므로 컬렉션 열거 중 변경 충돌이 없다.
/// </summary>
internal sealed class DemoWindow : GoViewWindow
{
    private readonly AppHub hub;
    private int frame;

    public DemoWindow(GoDesign design, AppHub hub)
        : base(360, 470, WindowBorder.Fixed, touchMode: false)
    {
        Design = design;
        this.hub = hub;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        Design.SetPage("Main");        // CurrentPage 지정 → Update가 펌프
        Design.WireBindings(hub);      // ← 마크업의 {…}를 hub에 연결 (한 줄)
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        frame++;

        // 라이브: RPM은 부드럽게 변동, 로그는 주기적으로 추가(최근 8줄 유지)
        if (frame % 6 == 0)
        {
            hub.PumpA.Rpm = 1400 + Math.Sin(frame / 40.0) * 120;
            hub.PumpB.Rpm = 950 + Math.Cos(frame / 55.0) * 80;
        }
        if (frame % 90 == 0)
        {
            hub.Logs.Add(new LogVM { Message = $"이벤트 #{frame / 90}  ({DateTime.Now:HH:mm:ss})" });
            while (hub.Logs.Count > 8) hub.Logs.RemoveAt(0);
        }

        base.OnUpdateFrame(args);
    }
}
