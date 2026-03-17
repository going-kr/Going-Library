# Going UI — C# 코드 작성 패턴

UIEditor에서 코드 생성 후 Claude Code가 `*.cs` 파일을 작성할 때 따르는 패턴.

---

## using 네임스페이스 참조표

> ⚠ **필요한 using만 선언할 것.** 아래 표에 없는 네임스페이스는 사용 금지 (절대 규칙 #2 — API 추측 금지).

| 네임스페이스 | 포함 내용 | 사용 위치 |
|-----------|---------|---------|
| `Going.UI.Controls` | GoButton, GoLabel, GoLamp, GoSlider, GoInput 등 기본 컨트롤 | Page/Window/MainWindow |
| `Going.UI.Containers` | GoTableLayoutPanel, GoBoxPanel, GoSwitchPanel 등 컨테이너 | Page/Window/MainWindow |
| `Going.UI.Datas` | GoDataGrid, DataGridColumn 관련 | DataGrid 사용 시 |
| `Going.UI.Design` | GoDesign, GoPage, GoWindow, IcPage | Page/Window/MainWindow |
| `Going.UI.Dialogs` | GoDialogs (MessageBox 대체) | 대화상자 사용 시 |
| `Going.UI.Json` | GoJsonConverter | Designer.cs (자동생성) |
| `Going.UI.Utils` | Util.AllControls, Util.FromArgb 등 유틸리티 | Designer.cs (자동생성) |
| `Going.UI.Themes` | GoTheme 커스텀 테마 | Designer.cs (자동생성) |
| `Going.UI.ImageCanvas` | IcPage, IcButton, IcLabel 등 ImageCanvas | IcPage 사용 시 |
| `Going.UI.OpenTK.Windows` | GoViewWindow | MainWindow |
| `Going.Basis.Communications` | MasterRTU, MasterTCP, SlaveRTU, SlaveTCP, ModbusRTUMaster 등 | DeviceManager |
| `Going.Basis.Datas` | DataManager (설정 파일) | Program.cs / MainWindow |
| `OpenTK.Windowing.Common` | WindowBorder, VSyncMode | MainWindow |

**파일별 using 패턴:**

```csharp
// Program.cs
using ProjectName;

// MainWindow.cs (사용자 파일)
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;
using ProjectName.Pages;
using ProjectName.Windows;

// PageName.cs (사용자 파일)
using Going.UI.Design;
// + 필요한 것만 추가 (Controls, Containers, Dialogs 등)

// DeviceManager.cs
using Going.Basis.Communications;
```

---

## Going.UIEditor Code 생성 시스템

### 생성 파일 구조

UIEditor에서 Code 생성(MakeCode()) 실행 시 아래 파일들이 자동 생성됨:

```
[프로젝트 루트]
├── ProjectName.sln          ← 솔루션 파일
├── ProjectName.csproj       ← 프로젝트 파일 (NuGet 참조, design.json 빌드 복사 설정 포함)
├── design.json              ← GoDesign 직렬화 (런타임 로드용, UIEditor 전용)
├── Program.cs               ← 진입점 (최소 구조)
├── MainWindow.cs            ← partial class, GoViewWindow 상속 (사용자 영역)
└── MainWindow.Designer.cs   ← InitializeComponent() 자동생성 (항상 덮어씀)

Pages/
├── PageName.cs              ← partial class, GoPage 또는 IcPage 상속 (사용자 로직 영역)
└── PageName.Designer.cs     ← 자동생성 (항상 덮어씀)

Windows/
├── WindowName.cs            ← partial class, GoWindow 상속 (사용자 로직 영역)
└── WindowName.Designer.cs   ← 자동생성 (항상 덮어씀)
```

> **design.json 자동 포함**: UIEditor의 MakeCode 실행 시 `design.json`이 프로젝트 루트에 자동 생성되며, `.csproj`에 빌드 출력 복사 설정이 포함됨. `dotnet publish` 시 자동으로 출력 디렉터리에 포함되므로 별도 복사 불필요.

### ExistsCheck 규칙

| 파일 | ExistsCheck | 동작 |
|------|------------|------|
| `*.cs` (사용자 파일) | `true` | 이미 존재하면 덮어쓰지 않음 → 사용자 코드 보호 |
| `*.Designer.cs` | `false` | 항상 덮어씀 → 수동 수정 무의미 |
| `design.json` | `true` | UIEditor 전용, 코드에서 수정 금지 |

### 클로드 코드 작업 규칙

> ⚠ `*.Designer.cs`, `design.json` 수정 금지 — 상세는 **SKILL.md "절대 규칙 #1"** 참조.
> **작업 대상**: `*.cs` (사용자 파일)만 수정

```
MainWindow.cs      → 서비스 초기화, 전역 이벤트 바인딩
Pages/PageName.cs  → 페이지별 비즈니스 로직, 컨트롤 이벤트
Windows/WndName.cs → 팝업 동작 로직
```

---

## UIEditor가 생성하는 코드 (실제 MakeCode 출력)

### Program.cs (자동생성 — ExistsCheck=true)

```csharp
using ProjectName;

using var view = new MainWindow();
view.Run();
```

> UIEditor는 최소한의 진입점만 생성. 사용자가 DataManager/DeviceManager 등을 추가.

### MainWindow.cs (자동생성 — ExistsCheck=true)

```csharp
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;
using ProjectName.Pages;
using ProjectName.Windows;

namespace ProjectName
{
    public partial class MainWindow : GoViewWindow
    {
        public MainWindow() : base(1024, 600, WindowBorder.Hidden)
        {
            InitializeComponent();
        }
    }
}
```

> **GoViewWindow** 상속. 모든 프로젝트에서 `GoViewWindow`를 사용.

### MainWindow.Designer.cs (자동생성 — ExistsCheck=false)

```csharp
using System.Text;
using System.Text.Json;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Json;
using Going.UI.OpenTK.Windows;
using Going.UI.Utils;
using Going.UI.Themes;
using OpenTK.Windowing.Common;
using ProjectName.Pages;
using ProjectName.Windows;

namespace ProjectName
{
    public partial class MainWindow
    {
        #region declare
        internal GoDesign DS { get; private set; }
        // TitleBar/SideBar/Footer 안의 Name 있는 컨트롤들이 여기 선언됨
        GoButton btnSideMenu;

        public PageMain PageMain { get; private set; }
        public PageSetting PageSetting { get; private set; }

        public Keypad Keypad { get; private set; }
        #endregion

        #region static
        public static MainWindow Current { get; private set; }
        #endregion

        public void InitializeComponent()
        {
            #region Design Load
            if (!File.Exists("design.json")) throw new Exception("design.json 파일이 없습니다.");
            var json = File.ReadAllText("design.json");
            var ds = GoDesign.JsonDeserialize(json);
            DS = ds;
            #endregion

            #region Window Setting
            Current = this;
            Title = "ProjectName";
            Debug = false;
            VSync = VSyncMode.On;
            CenterWindow();
            #endregion

            #region Design Setting
            Design.UseTitleBar = ds.UseTitleBar;
            Design.UseLeftSideBar = ds.UseLeftSideBar;
            Design.UseRightSideBar = ds.UseRightSideBar;
            Design.UseFooter = ds.UseFooter;
            Design.OverlaySideBar = ds.OverlaySideBar;
            Design.BarColor = ds.BarColor;
            Design.ExpandLeftSideBar = ds.ExpandLeftSideBar;
            Design.ExpandRightSideBar = ds.ExpandRightSideBar;
            #endregion

            #region Theme Setting
            Design.CustomTheme = ds.CustomTheme;
            #endregion

            #region Resources
            foreach (var v in ds.GetImages()) Design.AddImage(v.name, v.images);
            foreach (var v in ds.GetFonts())
                foreach (var f in v.fonts)
                        Design.AddFont(v.name, f);
            #endregion

            #region TitleBar
            {
                var c = ds.TitleBar;
                // MakeDesignBarCode: [GoProperty] 속성 복사 → Childrens 복사 → 컨트롤 매핑
                Design.TitleBar.BarSize = c.BarSize;
                Design.TitleBar.Title = c.Title;
                // ... (모든 [GoProperty] 속성)
                Design.TitleBar.Childrens.AddRange(c.Childrens);

                var dic = Util.AllControls(Design.TitleBar).ToDictionary(x => x.Id.ToString(), y => y);
                btnSideMenu = (GoButton)dic["uuid-here"];
            }
            #endregion

            #region LeftSideBar
            {
                var c = ds.LeftSideBar;
                // ... MakeDesignBarCode 동일 패턴
            }
            #endregion

            // RightSideBar, Footer도 동일

            #region Pages
            PageMain = new PageMain();
            PageSetting = new PageSetting();

            Design.AddPage(PageMain);
            Design.AddPage(PageSetting);

            Design.SetPage("PageMain");  // 첫 번째 페이지가 기본
            #endregion

            #region Windows
            Keypad = new Keypad();

            Design.Windows.Add(Keypad.Name, Keypad);
            #endregion
        }
    }
}
```

### PageName.cs (자동생성 — ExistsCheck=true)

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Design;
using Going.UI.ImageCanvas;

namespace ProjectName.Pages
{
    public partial class PageMain : GoPage    // IcPage인 경우: IcPage
    {
        public PageMain()
        {
            InitializeComponent();
        }
    }
}
```

### PageName.Designer.cs (자동생성 — ExistsCheck=false)

```csharp
using System.Text;
using System.Text.Json;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Json;
using Going.UI.OpenTK.Windows;
using Going.UI.Utils;
using Going.UI.ImageCanvas;
using OpenTK.Windowing.Common;

namespace ProjectName.Pages
{
    partial class PageMain
    {
        #region declare
        GoButton btnStart;
        GoLabel lblStatus;
        GoLamp lampComm;
        #endregion

        public void InitializeComponent()
        {
            #region base
            var c = MainWindow.Current.DS.Pages["PageMain"];
            // IcPage인 경우: var c = MainWindow.Current.DS.Pages["PageMain"] as IcPage;

            // MakeDesignBarCode 패턴:
            // 1. [GoProperty] 속성 복사
            this.BackgroundImage = c.BackgroundImage;
            // ... (모든 [GoProperty] 속성)

            // 2. Childrens 복사
            this.Childrens.AddRange(c.Childrens);

            // 3. Name 있는 컨트롤 Id 기준 매핑
            var dic = Util.AllControls(this).ToDictionary(x => x.Id.ToString(), y => y);
            btnStart = (GoButton)dic["a5f8b62e-7135-4cda-86c5-3675b5eeeae1"];
            lblStatus = (GoLabel)dic["7adf6f04-d55c-4476-9eea-672314a2a431"];
            lampComm = (GoLamp)dic["60cc7c64-3502-49db-b64d-8690165dccb3"];
            #endregion
        }
    }
}
```

### WindowName.cs (자동생성 — ExistsCheck=true)

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Design;
using Going.UI.ImageCanvas;

namespace ProjectName.Windows
{
    public partial class Keypad : GoWindow
    {
        public Keypad()
        {
            InitializeComponent();
        }
    }
}
```

### WindowName.Designer.cs (자동생성 — ExistsCheck=false)

```csharp
namespace ProjectName.Windows
{
    partial class Keypad
    {
        #region declare
        GoLabel lblValue;
        GoButton btnOk;
        GoButton btnCancel;
        #endregion

        public void InitializeComponent()
        {
            #region base
            var c = MainWindow.Current.DS.Windows["Keypad"];

            // MakeDesignBarCode 동일 패턴
            this.IconString = c.IconString;
            this.Text = c.Text;
            // ... (모든 [GoProperty] 속성)
            this.Childrens.AddRange(c.Childrens);

            var dic = Util.AllControls(this).ToDictionary(x => x.Id.ToString(), y => y);
            lblValue = (GoLabel)dic["uuid-here"];
            btnOk = (GoButton)dic["uuid-here"];
            btnCancel = (GoButton)dic["uuid-here"];
            #endregion
        }
    }
}
```

---

## MakeDesignBarCode 동작 원리

모든 Page/Window/TitleBar/SideBar/Footer의 Designer.cs에서 사용되는 핵심 패턴:

```csharp
// 1. design.json에서 로드된 객체(c)의 [GoProperty] 속성을 this에 복사
{varname}.{PropertyName} = c.{PropertyName};  // 각 [GoProperty] 속성마다

// 2. 자식 컨트롤 복사
{varname}.Childrens.AddRange(c.Childrens);

// 3. Name 있는 컨트롤을 Id(UUID) 기준으로 딕셔너리 매핑
var dic = Util.AllControls({varname}).ToDictionary(x => x.Id.ToString(), y => y);
{controlName} = ({TypeName})dic["{controlId}"];  // 각 Name 있는 컨트롤마다
```

> **[GoProperty] 속성만 복사됨** — `[GoPropertyAttribute]`, `[GoSizePropertyAttribute]`, `[GoSizesPropertyAttribute]` 중 하나가 있고, `[JsonIgnore]`가 아닌 public get/set 프로퍼티만 대상.

---

## MakePropCode 지원 타입

Designer.cs에서 코드로 자동 변환되는 타입 목록 (Code.cs MakePropCode에서 직접 추출):

| 분류 | 타입 |
|------|------|
| 정수형 | `byte`, `ushort`, `uint`, `ulong`, `sbyte`, `short`, `int`, `long` 및 Nullable(`?`) 버전 |
| 실수형 | `float`(F접미사), `double`, `decimal`(M접미사) 및 Nullable(`?`) 버전 |
| 기타 기본 | `bool`, `string` |
| Going 타입 | `GoPadding`, `SKColor`(Util.FromArgb), `Enum`(FullName.Value), `Enum?`(nullable enum) |
| 단순 컬렉션 | `List<string>`, `List<GoListItem>`, `List<GoButtonItem>`, `List<GoButtonsItem>`, `List<GoGraphSeries>`, `List<GoLineGraphSeries>`, `List<StateImage>` |
| 구조 컬렉션 | `List<GoSubPage>`, `List<GoTabPage>`, `List<GoGridLayoutPanelRow>` |
| Observable | `ObservableList<GoListItem>`, `ObservableList<GoMenuItem>`, `ObservableList<GoToolCategory>` (중첩 GoToolItem), `ObservableList<GoTreeNode>` (재귀 트리) |

### TypeName 규칙

제네릭 컨트롤의 Designer.cs 선언 형태:
- `GoInputNumber<int>` → `GoInputNumber<Int32>` (.NET 타입명 사용)
- `GoValueNumber<double>` → `GoValueNumber<Double>`

---

## .csproj 패키지 참조

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Going.Basis" Version="1.0.4" />
    <PackageReference Include="Going.UI" Version="1.0.8" />
    <PackageReference Include="Going.UI.OpenTK" Version="1.0.8" />
  </ItemGroup>
</Project>
```

---

## 사용자 확장 패턴 (UIEditor 생성 후 사용자가 추가)

아래 패턴들은 UIEditor가 생성하지 않지만, 사용자가 프로젝트에 추가하는 표준 구조.

### 전역 싱글턴 패턴 (Main.cs)

Going 프로젝트의 핵심 진입 패턴. `Main` 클래스는 프로젝트 전역에서 매니저와 윈도우에 접근하기 위한 정적 허브 역할.
UIEditor가 생성하지 않으므로 사용자가 직접 정의해야 하며, 프로젝트 전체에서 `Main.Window`, `Main.DevMgr`, `Main.DataMgr`로 참조.

**왜 필요한가**: Going 라이브러리는 DI 컨테이너 없이 경량 정적 접근 패턴을 사용. 이벤트 핸들러, 페이지 간 통신, DeviceManager 접근 등 모든 곳에서 `Main.XXX`로 참조하므로 이 클래스가 없으면 프로젝트가 동작하지 않음.

```csharp
public class Main
{
    public static MainWindow Window { get; set; }
    public static DeviceManager DevMgr { get; set; }
    public static DataManager DataMgr { get; set; }
}
```

> `Main`은 Going 프레임워크 클래스가 아닌 사용자 정의 클래스. 프로퍼티 구성은 프로젝트 요구에 따라 자유롭게 확장 가능 (예: `LogManager`, `AuthManager` 등 추가).

### 진입점 확장 패턴 (Program.cs)

UIEditor 생성 후 통신 등 서비스를 추가할 때:

```csharp
using ProjectName;

var dataMgr = Main.DataMgr = new DataManager();
var devMgr  = Main.DevMgr  = new DeviceManager();
var wnd     = Main.Window  = new MainWindow();

devMgr.Start();   // 통신 시작
wnd.Run();        // 렌더 루프 (블로킹)
devMgr.Stop();    // 통신 종료

wnd.Dispose();
```

### 이벤트 바인딩 규칙

| 이벤트 | 소속 | 용도 | 예시 |
|--------|------|------|------|
| `ButtonClicked` | GoButton 전용 | 버튼 클릭 처리 | `btnStart.ButtonClicked += (o, s) => { };` |
| `MouseClicked` | GoControl 베이스 (모든 컨트롤) | 일반 클릭 — Keypad 호출, 팝업 등 | `lblValue.MouseClicked += (o, s) => { };` |
| `MouseDoubleClicked` | GoControl 베이스 | 더블클릭 | `ctrl.MouseDoubleClicked += (o, s) => { };` |
| `MouseLongClicked` | GoControl 베이스 | 롱클릭 (길게 누르기) | `ctrl.MouseLongClicked += (o, s) => { };` |

> **GoButton은 `ButtonClicked`를 사용하고, 다른 컨트롤(GoLabel, GoLamp 등)의 클릭은 `MouseClicked`를 사용한다.** `MouseClick`이나 `Click` 이벤트는 존재하지 않는다.

### MainWindow 확장 패턴 (MainWindow.cs)

UIEditor가 생성한 `GoViewWindow` 기반에 사용자 로직 추가:

```csharp
public partial class MainWindow : GoViewWindow
{
    public MainWindow() : base(1024, 600, WindowBorder.Hidden)
    {
        InitializeComponent();

        // 컨트롤 이벤트 바인딩
        btnSomething.ButtonClicked += (o, s) => Main.DevMgr.DoSomething();
    }

    // 매 프레임 호출 — UI 상태 갱신
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        lblTitle.Text = Main.DataMgr.Setting.Title;
        lampComm.OnOff = Main.DevMgr.IsConnected;

        base.OnUpdateFrame(args);
    }

    // 커스텀 렌더링이 필요할 때만 오버라이드
    protected override void OnDraw(SKCanvas canvas, GoTheme thm)
    {
        base.OnDraw(canvas, thm);
    }
}
```

> `Design.SetPage("PageName")`으로 페이지 전환. 자세한 패턴은 아래 **페이지 전환 패턴** 참조.

---

## 페이지 전환 패턴

`Design.SetPage("PageName")`으로 현재 표시 페이지를 변경. Page 클래스명(= .gud에서 설정한 페이지 이름)을 문자열로 전달.

### 기본 전환 (버튼 클릭)

```csharp
// MainWindow.cs 또는 Page.cs 생성자에서
btnGoMain.ButtonClicked += (o, s) => Design.SetPage("PageMain");
btnGoSetting.ButtonClicked += (o, s) => Design.SetPage("PageSetting");
btnGoDetail.ButtonClicked += (o, s) => Design.SetPage("PageDetail");
```

### SideBar 메뉴 전환

```csharp
// MainWindow.cs 생성자 — SideBar 버튼으로 페이지 전환
btnMenuMain.ButtonClicked += (o, s) => Design.SetPage("PageMain");
btnMenuSetting.ButtonClicked += (o, s) => Design.SetPage("PageSetting");
```

### TitleBar 뒤로가기

```csharp
// MainWindow.cs 생성자 — TitleBar의 뒤로가기 버튼
btnBack.ButtonClicked += (o, s) => Design.SetPage("PageMain");  // 메인으로 복귀
```

### GoNavigator 자동 전환 (코드 불필요)

GoNavigator의 GoMenuItem에 `PageName`을 지정하면 메뉴 클릭 시 자동으로 페이지가 전환된다. 별도 이벤트 핸들러 코드가 필요 없다.

```json
// .gud 파일 — TitleBar에 GoNavigator 배치 (Dock: Fill)
"Menus": [
  { "Text": "모니터링", "IconString": "fa-tv", "PageName": "PageMonitor" },
  { "Text": "설정",     "IconString": "fa-cog", "PageName": "PageSetting" }
]
```

> GoNavigator를 TitleBar 컨테이너에 Dock: 5(Fill)로 배치하면 상단 네비게이션 바 역할을 한다.

> **주의**: `Design.SetPage()`의 인자는 Page 클래스명 문자열 (예: `"PageMain"`, `"PageSetting"`). Designer.cs에서 `Design.AddPage()`로 등록한 이름과 일치해야 함. GoMenuItem의 `PageName`도 동일 규칙.

---

## 설정 데이터 패턴 (Datas/SystemSetting.cs)

JSON 직렬화 가능한 설정 DTO. 프로젝트 요구에 맞게 프로퍼티를 자유롭게 구성.

```csharp
public class SystemSetting
{
    public string PortName  { get; set; } = "COM1";
    public int    Baudrate  { get; set; } = 115200;
    public int    Timeout   { get; set; } = 500;
}
```

---

## DataManager 패턴 (Managers/DataManager.cs)

설정 파일 로드/저장 담당.

```csharp
public class DataManager
{
    const string PATH = "setting.json";
    public SystemSetting Setting { get; set; }

    public DataManager()
    {
        Setting = new SystemSetting();
        if (File.Exists(PATH))
        {
            var obj = JsonSerializer.Deserialize<SystemSetting>(File.ReadAllText(PATH));
            if (obj != null) Setting = obj;
        }
    }

    public void Save()
    {
        File.WriteAllText(PATH, JsonSerializer.Serialize(Setting));
    }
}
```

---

## Page 패턴 (Pages/PageXxx.cs)

생성자에서 이벤트 바인딩, `OnUpdate()`에서 데이터 → 컨트롤 반영.

```csharp
public partial class PageMain : GoPage
{
    public PageMain()
    {
        InitializeComponent();

        // 버튼 이벤트
        btnStart.ButtonClicked += (o, s) =>
        {
            Main.DevMgr.SetOutput(0x0010, 1);
        };

        // 값 입력 — Keypad 팝업
        lblSetpoint.MouseClicked += (o, s) =>
        {
            Main.Window.Keypad.ShowKeypad(Main.DevMgr.Data.Setpoint, (value) =>
            {
                Main.DevMgr.SetSetpoint(value);
            });
        };
    }

    // OnUpdate — 매 프레임 컨트롤 상태 갱신 (OnUpdateFrame 대신 Page 단위로)
    protected override void OnUpdate()
    {
        var data = Main.DevMgr.Data;

        // 통신 상태에 따른 색상
        lampComm.OnOff       = data.CommState;
        lblTemp.Text         = data.CommState ? $"{data.Temperature:F1} °C" : "---";
        lblTemp.TextColor    = data.CommState ? "Fore" : "Base4";

        // 운전 상태에 따른 버튼 색상
        btnStart.ButtonColor = data.RunState ? "Good"  : "Base3";
        btnStop.ButtonColor  = data.RunState ? "Base3" : "Error";

        base.OnUpdate();
    }
}
```

---

## 컨트롤 샘플 코드 (별도 파일)

그래프/데이터그리드 등 복잡한 컨트롤의 C# 샘플은 **ui-control-sample.md** 참조.

포함 내용:
- GoBarGraph / GoLineGraph / GoTimeGraph — `SetDataSource` 패턴 + 디자인 설정
- GoTrendGraph — `Start` / `SetData` 실시간 스트리밍 패턴
- GoGauge / GoMeter / GoKnob — `OnUpdate` 값 연동
- GoSwitchPanel — `SetPage` 서브페이지 전환
- GoDataGrid — 컬럼 12종, SummaryRow, 이벤트, 데이터 바인딩
- GoGraphSeries / GoLineGraphSeries 구조 정의

---

## 테마 색상 키 레퍼런스

코드에서 `TextColor`, `ButtonColor`, `OnColor` 등의 색상 프로퍼티에 사용할 수 있는 값.

### 테마 키 (문자열)

| 키 | 용도 | DarkTheme 기본값 |
|---|---|---|
| `Fore` | 전경(텍스트) | White |
| `Back` | 배경 | #323232 |
| `Window` | 윈도우 배경 | #202020 |
| `WindowBorder` | 윈도우 테두리 | #5A5A5A |
| `Point` | 포인트 색 | DarkRed |
| `Title` | 타이틀바 | #464646 |
| `Base0` ~ `Base5` | 단계별 회색 | #000 → #969696 |
| `Good` | 정상/ON | Green |
| `Warning` | 경고 | DarkOrange |
| `Danger` | 위험 | DarkRed |
| `Error` | 에러 | Red |
| `Highlight` | 하이라이트/포커스 | Cyan |
| `Select` | 선택 행/항목 | Teal |
| `User1` ~ `User9` | 사용자 정의 색 | Red, Green, Blue, Magenta, Yellow, Cyan, Teal, DeepSkyBlue, DarkRed |

### 밝기 서픽스

키 뒤에 `-` 서픽스를 붙여 밝기 조절 가능:

```
"Good-light"       // +15% 밝게
"Good-dark"        // -15% 어둡게
"Select-lightlight" // +30% 밝게
"Select-darkdark"   // -30% 어둡게
```

### 직접 지정

테마 키 대신 직접 색상값 사용 가능:

```
"#FF5722"           // HEX RGB
"#80FF5722"         // HEX ARGB
"255,87,34"         // R,G,B
"128,255,87,34"     // A,R,G,B
"red"               // System.Drawing.Color 이름
```

### ⚠ 주의

- `"Bad"` 같은 미등록 키 사용 시 `Color.FromName()` 으로 fallback → 매칭 실패 시 투명색 반환 → **컨트롤이 안 보임**
- 의미별 권장: 정상=`Good`, 경고=`Warning`, 위험=`Danger`, 에러=`Error`

---

## Window(팝업) 패턴 (Windows/XxxWindow.cs)

`GoWindow` 상속. 헬퍼 메서드로 콜백 기반 표시.

```csharp
public partial class Keypad : GoWindow
{
    Action<int>? _callback;

    public Keypad()
    {
        InitializeComponent();

        btnOk.ButtonClicked += (o, s) =>
        {
            if (int.TryParse(lblValue.Text, out var v))
                _callback?.Invoke(v);
            Close();
        };

        btnCancel.ButtonClicked += (o, s) => Close();
    }

    // 외부에서 호출하는 헬퍼
    public void ShowKeypad(int current, Action<int> callback)
    {
        _callback = callback;
        lblValue.Text = current.ToString();
        Show();
    }
}
```

### 범용 설정 윈도우 콜백 패턴

여러 값을 입력받고 결과를 콜백으로 반환하는 범용 패턴:

```csharp
public partial class SettingWindow : GoWindow
{
    Action<string, int>? _callback;

    public SettingWindow()
    {
        InitializeComponent();

        btnOk.ButtonClicked += (o, s) =>
        {
            _callback?.Invoke(inpName.Text, (int)inpTimeout.Value);
            Close();
        };

        btnCancel.ButtonClicked += (o, s) => Close();
    }

    public void ShowSetting(string name, int timeout, Action<string, int> callback)
    {
        _callback = callback;
        inpName.Text = name;
        inpTimeout.Value = timeout;
        Show();
    }
}

// 호출 측 (Page에서)
btnSetting.ButtonClicked += (o, s) =>
{
    var setting = Main.DataMgr.Setting;
    Main.Window.SettingWnd.ShowSetting(setting.Name, setting.Timeout, (name, timeout) =>
    {
        setting.Name = name;
        setting.Timeout = timeout;
        Main.DataMgr.Save();
    });
};
```

> **핵심 패턴**: Show 헬퍼에서 현재 값 세팅 + `Action` 콜백 저장 → `Show()` 호출 → OK 시 콜백 실행 + `Close()`. WinForms의 `ShowDialog()`/`DialogResult`와 다름 — Going은 **비동기 콜백** 방식.

---

## GoDialogs 패턴 (시스템 다이얼로그)

`GoDialogs`는 전역 싱글턴 다이얼로그. 별도 Window 정의 없이 사용 가능.
콜백 기반 — 결과를 `Action`으로 수신.

```csharp
using Going.UI.Dialogs;

// 1. MessageBox — 확인/취소/예/아니요
GoDialogs.MessageBox.ShowMessageBoxOk("알림", "작업이 완료되었습니다.", (result) =>
{
    // result: GoDialogResult.Ok
});

GoDialogs.MessageBox.ShowMessageBoxOkCancel("확인", "저장하시겠습니까?", (result) =>
{
    if (result == GoDialogResult.Ok)
        Main.DataMgr.Save();
});

GoDialogs.MessageBox.ShowMessageBoxYesNo("종료", "프로그램을 종료하시겠습니까?", (result) =>
{
    if (result == GoDialogResult.Yes)
        Environment.Exit(0);
});

// 2. InputBox — 값 입력
GoDialogs.InputBox.ShowString("이름 입력", "홍길동", (value) =>
{
    if (value != null) Main.DataMgr.Setting.UserName = value;
});

GoDialogs.InputBox.ShowNumber<int>("설정값", 100, 0, 1000, (value) =>
{
    if (value.HasValue) Main.DevMgr.SetOutput(1, "D100", value.Value);
});

GoDialogs.InputBox.ShowBool("자동 모드", true, "자동", "수동", (value) =>
{
    if (value.HasValue) Main.DevMgr.SetBitOutput(1, "P0", value.Value);
});

// 3. SelectorBox — 항목 선택
GoDialogs.SelectorBox.ShowRadio<MyEnum>("모드 선택", 2, MyEnum.Mode1, (value) =>
{
    if (value.HasValue) Main.DataMgr.Setting.Mode = value.Value;
});
```

> GoDialogs.SystemWindows(MessageBox, SelectorBox, InputBox)는 프레임워크가 자동 관리함.
> `GoDesign.Init()`이 SystemWindows를 자동 초기화하고, `ShowWindow()`가 `Design.Windows`와 `GoDialogs.SystemWindows` 양쪽을 자동 탐색하므로 별도 등록 코드 불필요.

---

## 컨트롤 묶음 패턴

반복되는 컨트롤 그룹(채널, 슬롯 등)을 클래스로 묶어 관리.
Designer.cs에서 Name으로 컨트롤을 찾아 바인딩.

```csharp
// 컨트롤 묶음 클래스
public class ChannelUI(int idx, GoLabel name, GoLamp state, GoLabel value)
{
    public int     Index  => idx;
    public GoLabel Name   => name;
    public GoLamp  State  => state;
    public GoLabel Value  => value;

    public bool Visible
    {
        get => name.Visible;
        set => name.Visible = state.Visible = value.Visible = value;
    }

    public void SetTag()
    {
        name.Tag = state.Tag = value.Tag = this;
    }
}

// PageMain 생성자에서 Name 기반으로 컨트롤 묶음 초기화
var dic = tbl.Childrens.Where(x => x.Name != null).ToDictionary(x => x.Name!, y => y);
for (int i = 0; i < 16; i++)
{
    if (dic.TryGetValue($"lblName{i+1}",  out var vn) &&
        dic.TryGetValue($"lamp{i+1}",     out var vl) &&
        dic.TryGetValue($"lblValue{i+1}", out var vv) &&
        vn is GoLabel ln && vl is GoLamp ll && vv is GoLabel lv)
    {
        Channels[i] = new ChannelUI(i, ln, ll, lv);
        Channels[i].SetTag();
    }
}
```
