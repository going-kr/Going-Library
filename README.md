<p align="center">
  <img src="icon/Going_logo_blue.png" alt="Going Library" width="120">
</p>

<h1 align="center">Going Library</h1>

<p align="center">
  .NET 8.0 기반 산업용 HMI/SCADA UI 프레임워크
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Going.Basis"><img src="https://img.shields.io/nuget/v/Going.Basis?label=Going.Basis&color=blue" alt="NuGet"></a>
  <a href="https://www.nuget.org/packages/Going.UI"><img src="https://img.shields.io/nuget/v/Going.UI?label=Going.UI&color=blue" alt="NuGet"></a>
  <a href="https://www.nuget.org/packages/Going.UI.OpenTK"><img src="https://img.shields.io/nuget/v/Going.UI.OpenTK?label=Going.UI.OpenTK&color=blue" alt="NuGet"></a>
  <a href="https://www.nuget.org/packages/Going.UI.Forms"><img src="https://img.shields.io/nuget/v/Going.UI.Forms?label=Going.UI.Forms&color=blue" alt="NuGet"></a>
</p>

---

## 개요

**Going Library**는 C# .NET 8.0 기반의 산업용 HMI/SCADA UI 프레임워크입니다. SkiaSharp 커스텀 렌더링으로 구현되어 임베디드 터치 패널에 최적화된 40개 이상의 산업용 컨트롤을 제공합니다.

### 주요 특징

- **40개 이상의 산업용 컨트롤** — GoButton, GoLamp, GoDataGrid, GoSlider, GoGauge, GoMeter, GoChart 등
- **비주얼 UI 에디터** — 드래그 앤 드롭으로 .gud 파일 설계, C# 코드 자동 생성
- **산업용 통신** — Modbus RTU/TCP, MQTT, LS Electric CNet, Mitsubishi MC
- **크로스 플랫폼** — Raspberry Pi(linux-arm64), Windows, Linux 지원
- **다크 테마 기본 제공** — 전문적인 다크 UI와 커스터마이징 가능한 테마
- **AI 개발 지원** — [Claude Code 스킬](https://github.com/going-kr/going-ui-skill)을 통한 AI 기반 HMI 개발

---

## 패키지

| 패키지 | 설명 | 대상 |
|--------|------|------|
| **Going.UI** | 플랫폼 독립 UI 코어 (컨트롤, 컨테이너, 테마, 디자인) | `net8.0` |
| **Going.UI.OpenTK** | OpenTK 어댑터 (임베디드/Raspberry Pi) | `net8.0` |
| **Going.UI.Forms** | WinForms 어댑터 (Windows 데스크톱) | `net8.0-windows` |
| **Going.Basis** | 통신 및 유틸리티 (Modbus, MQTT, CNet, MC) | `net8.0` |

## 빠른 시작

```bash
dotnet new console -n MyHMI
cd MyHMI
dotnet add package Going.UI.OpenTK
dotnet add package Going.Basis
```

```csharp
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;

using var view = new MainWindow();
view.Run();

public class MainWindow : GoViewWindow
{
    public MainWindow() : base(1024, 600, WindowBorder.Hidden)
    {
        InitializeComponent();
    }
}
```

## UI 에디터

**Going UI Editor**는 `.gud` 파일을 시각적으로 설계하는 도구입니다. JSON 기반 UI 레이아웃을 만들고 C# 코드를 자동 생성합니다.

[Releases](https://github.com/going-kr/Going-Library/releases)에서 다운로드할 수 있습니다.

### 작업 흐름

```
1. UIEditor에서 UI 설계 (.gud 파일)
2. MakeCode → .cs 파일 자동 생성
3. 이벤트 핸들러 및 통신 코드 작성
4. 빌드 및 대상 장치에 배포
```

## 통신

```csharp
// Modbus RTU Master
var rtu = new MasterRTU();
rtu.WordAreas.Add(0x0000, "D");
rtu.MonitorWord_F3(1, 0x0000, 50);
rtu.Start();

var value = rtu.GetWord("D0");   // 캐시에서 읽기
rtu.SetWord(1, "D0", 100);      // 장치에 쓰기
```

| 프로토콜 | 클래스 |
|----------|--------|
| Modbus RTU | `MasterRTU`, `SlaveRTU`, `ModbusRTUMaster` |
| Modbus TCP | `MasterTCP`, `SlaveTCP`, `ModbusTCPMaster` |
| MQTT | `MQClient` |
| LS Electric CNet | `CNet` |
| Mitsubishi MC | `MC` |

## Claude Code로 개발 환경 구성

[Going UI Skill](https://github.com/going-kr/going-ui-skill)을 사용하면 AI가 HMI 애플리케이션 개발을 지원합니다.

> **Claude Code / Claude Desktop 사용자**: 이 README URL을 Claude에게 전달하고 "개발 환경 구성해줘"라고 요청하면 자동으로 셋업됩니다.

<!-- claude-setup
Claude Code가 사용자의 "개발 환경 구성" 요청을 받으면 아래 단계를 순서대로 수행하라.

## 1단계: going-ui-skill 설치
```bash
git clone https://github.com/going-kr/going-ui-skill "$HOME/.claude/skills/going-ui-skill"
```
- 이미 존재하면 `git -C "$HOME/.claude/skills/going-ui-skill" pull` 로 업데이트

## 2단계: UIEditor 다운로드 및 설치
- 사용자에게 UIEditor 설치 경로를 질문 (기본값: ~/UIEditor)
- GitHub Release에서 다운로드 및 압축 해제:
```bash
gh release download --pattern "UIEditor.zip" -R going-kr/Going-Library -D "<설치경로>"
```
- 압축 해제:
  - Windows: `powershell -Command "Expand-Archive -Path '<설치경로>/UIEditor.zip' -DestinationPath '<설치경로>' -Force"`
  - Linux/macOS: `unzip -o "<설치경로>/UIEditor.zip" -d "<설치경로>"`
- zip 파일 삭제

## 3단계: 바탕화면 바로가기 생성 (Windows)
```powershell
$ws = New-Object -ComObject WScript.Shell
$sc = $ws.CreateShortcut("$HOME\Desktop\UIEditor.lnk")
$sc.TargetPath = "<설치경로>\UIEditor.exe"
$sc.WorkingDirectory = "<설치경로>"
$sc.Save()
```

## 4단계: 완료 안내
아래 내용을 사용자에게 안내:
1. 바탕화면의 UIEditor 바로가기로 실행
2. UIEditor에서 UI 설계 → MakeCode로 C# 프로젝트 자동 생성
3. 생성된 프로젝트 폴더를 Claude Code 작업 디렉터리로 열면 AI 코딩 시작
-->

### 수동 설치

**방법 1. Git Clone (CLI)**

```bash
# 프로젝트 단위 — 해당 프로젝트에서만 사용
cd your-project
git clone https://github.com/going-kr/going-ui-skill .claude/skills/going-ui-skill

# 전역 — 모든 프로젝트에서 사용
git clone https://github.com/going-kr/going-ui-skill ~/.claude/skills/going-ui-skill
```

**방법 2. Claude 데스크톱 앱**

`설정 → 스킬 → 스킬 업로드`에서 `.zip` 파일을 드래그 앤 드롭하거나 `SKILL.md` 파일을 직접 업로드합니다.

디자인 파일(.gud) 생성, C# 코드 작성, 통신 설정, 장치 배포를 AI가 지원합니다.

## 프로젝트 구조

```
Going.UI           — 플랫폼 독립 UI 코어
Going.UI.OpenTK    — OpenTK 어댑터 (임베디드/Raspberry Pi)
Going.UI.Forms     — WinForms 어댑터
Going.Basis        — 통신 및 유틸리티
Going.UIEditor     — 비주얼 UI 설계 도구
```

## 라이선스

MIT
