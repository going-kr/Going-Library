using Going.UI.Collections;

namespace SampleBinding;

/// <summary>모터 1대 ViewModel. 객체 참조는 안정적이고 속성만 갱신된다(HMI 전형).</summary>
public sealed class MotorVM
{
    public string Name { get; set; } = "";
    public double Rpm { get; set; }
}

/// <summary>로그 한 줄.</summary>
public sealed class LogVM
{
    public string Message { get; set; } = "";
}

/// <summary>
/// 앱 허브 — <c>WireBindings(root)</c>로 주입하는 단일 컨텍스트 객체.
/// 마크업의 모든 <c>{…}</c> 경로가 이 객체에서 출발한다.
/// </summary>
public sealed class AppHub
{
    public string PlantStatus { get; set; } = "RUN";
    public MotorVM PumpA { get; } = new() { Name = "펌프 A", Rpm = 1450 };
    public MotorVM PumpB { get; } = new() { Name = "펌프 B", Rpm = 980 };
    public ObservableList<LogVM> Logs { get; } = new();
}
