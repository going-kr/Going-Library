# Going UI — C# 코드 작성 패턴

UIEditor에서 코드 생성 후 Claude Code가 `*.cs` 파일을 작성할 때 따르는 패턴.

---

## Going.UIEditor Code 생성 시스템

### 생성 파일 구조

UIEditor에서 Code 생성(MakeCode()) 실행 시 아래 파일들이 자동 생성됨:

```
[프로젝트 루트]
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

### ExistsCheck 규칙

| 파일 | ExistsCheck | 동작 |
|------|------------|------|
| `*.cs` (사용자 파일) | `true` | 이미 존재하면 덮어쓰지 않음 → 사용자 코드 보호 |
| `*.Designer.cs` | `false` | 항상 덮어씀 → 수동 수정 무의미 |
| `design.json` | `true` | UIEditor 전용, 코드에서 수정 금지 |

### 클로드 코드 작업 규칙

> **절대 수정 금지**: `*.Designer.cs`, `design.json`
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

> **GoViewWindow** 상속. `GoManualWindow`는 사용자가 수동으로 변경 시 사용.

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
    <PackageReference Include="Going.Basis" Version="1.0.0" />
    <PackageReference Include="Going.UI.OpenTK" Version="1.0.4" />
  </ItemGroup>
</Project>
```

---

## 사용자 확장 패턴 (UIEditor 생성 후 사용자가 추가)

아래 패턴들은 UIEditor가 생성하지 않지만, 사용자가 프로젝트에 추가하는 표준 구조.

### 전역 싱글턴 패턴 (Main.cs)

전역 접근이 필요한 Manager와 Window를 정적 프로퍼티로 노출.

```csharp
public class Main
{
    public static MainWindow Window { get; set; }
    public static DeviceManager DevMgr { get; set; }
    public static DataManager DataMgr { get; set; }
}
```

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

> `GoManualWindow`를 사용하면 `Interval` 프로퍼티로 렌더 루프 간격(ms) 설정 가능.

---

## 설정 데이터 패턴 (Datas/SystemSetting.cs)

JSON 직렬화 가능한 설정 DTO. `[SystemProp]` 어트리뷰트로 설정 페이지 자동 연동.

```csharp
public class SystemSetting
{
    [SystemProp(1, "통신 포트")]               public string PortName  { get; set; } = "COM1";
    [SystemProp(2, "통신 속도", 9600, 115200)] public int    Baudrate  { get; set; } = 115200;
    [SystemProp(3, "타임아웃",  100,  5000)]   public int    Timeout   { get; set; } = 500;
}

[AttributeUsage(AttributeTargets.Property)]
public class SystemPropAttribute(int no, string name, int min = int.MinValue, int max = int.MaxValue) : Attribute
{
    public int    No   => no;
    public string Name => name;
    public int    Min  => min;
    public int    Max  => max;
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
            Main.Window.Keypad.ShowKeypad(Main.DevMgr.Data.Setpoint, 1.0, (value) =>
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
