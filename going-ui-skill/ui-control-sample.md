# Going UI — 컨트롤 샘플 코드

SampleOpenTK 프로젝트에서 추출한 실전 샘플 코드.
그래프/데이터그리드 등 복잡한 컨트롤의 **C# 코드 + 디자인 설정**을 함께 정리.

> **디자인(시리즈, 축, 컬러 등)은 UIEditor(.gud)에서 설정**하고, **코드(.cs)에서는 데이터 바인딩과 이벤트만 작성**하는 것이 기본 패턴.

---

## 시리즈 구조 정의

그래프 컨트롤에서 사용하는 시리즈 클래스:

```
GoGraphSeries                         ← GoBarGraph 사용
├─ Name: string         (데이터 매칭 키, 데이터 객체의 프로퍼티명과 일치)
├─ Alias: string        (범례 표시명)
├─ Color: string        (시리즈 색상, 테마키 또는 직접 지정)
└─ Visible: bool        (표시 여부)

GoLineGraphSeries : GoGraphSeries     ← GoLineGraph, GoTimeGraph, GoTrendGraph 사용
├─ Minimum: double      (Y축 최소)
└─ Maximum: double      (Y축 최대)
```

---

## 샘플 데이터 클래스

```csharp
// 월별 통계 — GoLineGraph, GoBarGraph 용
public class Data1
{
    public string Date { get; set; }    // X축 라벨 (string)
    public double Temp { get; set; }
    public double Hum { get; set; }
    public double Rad { get; set; }
}

// 시계열 — GoTimeGraph 용
public class Data2
{
    public DateTime Date { get; set; }  // X축 (DateTime)
    public double Temp { get; set; }
    public double Hum { get; set; }
    public double Rad { get; set; }
}
```

> 프로퍼티명(`Temp`, `Hum`, `Rad`)이 **Series.Name과 정확히 일치**해야 데이터 바인딩됨.

---

## GoBarGraph

### 디자인 설정 (UIEditor)

```
Series:
  - Name: "Temp",  Alias: "온도",  Color: "Red"
  - Name: "Hum",   Alias: "습도",  Color: "deepskyblue"
  - Name: "Rad",   Alias: "광량",  Color: "orange"

GridColor: "Base3"          # 격자선 색
TextColor: "Fore"           # 텍스트 색
RemarkColor: "Base2"        # 범례 배경색
GraphColor: "Back"          # 그래프 영역 배경색
GraduationCount: 10         # Y축 눈금 수
Minimum: 0                  # Y축 최소
Maximum: 100                # Y축 최대
BarSize: 20                 # 바 너비(px)
BarGap: 20                  # 바 간격(px)
Mode: 1                     # 0=Stack(누적), 1=List(나란히)
Direction: 1                # 0=Horizon, 1=Vertical
```

### C# 코드

```csharp
public partial class PageBarGraph : GoPage
{
    public PageBarGraph()
    {
        InitializeComponent();

        // SetDataSource<T>(XAxisPropertyName, data)
        // - 첫 번째 인자: X축 라벨로 사용할 프로퍼티명 (string 타입)
        // - 두 번째 인자: 데이터 리스트
        grpBar.SetDataSource("Date", SampleData.Datas);
    }
}
```

> `SetDataSource`의 XAxisName(`"Date"`)은 **string 타입 프로퍼티**여야 함. 숫자형 프로퍼티(`Temp`, `Hum`, `Rad`)는 Series.Name과 매칭되어 자동 바인딩.

---

## GoLineGraph

### 디자인 설정 (UIEditor)

```
Series:  (GoLineGraphSeries — 개별 Y축 범위 지정 가능)
  - Name: "Temp",  Alias: "온도",  Color: "Red",         Minimum: 0, Maximum: 100
  - Name: "Hum",   Alias: "습도",  Color: "deepskyblue", Minimum: 0, Maximum: 100
  - Name: "Rad",   Alias: "광량",  Color: "orange",      Minimum: 0, Maximum: 100

GridColor: "Base3"
TextColor: "Fore"
RemarkColor: "Base2"
GraphColor: "Back"
GraduationCount: 10
PointWidth: 70              # 데이터 포인트 간 간격(px)
```

### C# 코드

```csharp
public partial class PageLineGraph : GoPage
{
    public PageLineGraph()
    {
        InitializeComponent();

        // GoBarGraph와 동일한 SetDataSource 패턴
        grpLine.SetDataSource("Date", SampleData.Datas);
    }
}
```

> **GoLineGraphSeries**는 `Minimum`/`Maximum`이 있어 시리즈별 Y축 스케일을 개별 설정 가능. GoBarGraph의 GoGraphSeries와의 핵심 차이점.

---

## GoTimeGraph

### 디자인 설정 (UIEditor)

```
Series:  (GoLineGraphSeries)
  - Name: "Temp",  Alias: "온도",  Color: "Red",         Minimum: 0, Maximum: 100
  - Name: "Hum",   Alias: "습도",  Color: "deepskyblue", Minimum: 0, Maximum: 100
  - Name: "Rad",   Alias: "광량",  Color: "orange",      Minimum: 0, Maximum: 100

GridColor: "Base3"
TextColor: "Fore"
RemarkColor: "Base2"
GraphColor: "Back"
XScale: "01:00:00"                  # 화면에 표시되는 시간 범위 (1시간)
XAxisGraduationTime: "00:10:00"     # X축 눈금 간격 (10분)
YAxisGraduationCount: 10            # Y축 눈금 수
TimeFormatString: "M/d\r\nHH:mm"   # X축 시간 포맷 (\r\n으로 줄바꿈)
```

### C# 코드

```csharp
public partial class PageTimeGraph : GoPage
{
    public PageTimeGraph()
    {
        InitializeComponent();

        // GoTimeGraph: XAxisName은 DateTime 타입 프로퍼티
        grpTime.SetDataSource("Date", SampleData.Datas2);
    }
}
```

### 데이터 생성 예시 (시계열)

```csharp
// 1초 간격, 6시간 분량의 시계열 데이터
var now = DateTime.Now;
double t = 50.0, h = 50.0, r = 50.0;
for (DateTime dt = now.Date; dt < now + TimeSpan.FromHours(6); dt += TimeSpan.FromSeconds(1))
{
    t = MathTool.Constrain(t + (rand.Next() % 2 == 0 ? 0.3 : -0.3), 0, 100);
    h = MathTool.Constrain(h + (rand.Next() % 2 == 0 ? 0.3 : -0.3), 0, 100);
    r = MathTool.Constrain(r + (rand.Next() % 2 == 0 ? 0.3 : -0.3), 0, 100);

    Datas2.Add(new Data2 { Date = dt, Temp = t, Hum = h, Rad = r });
}
```

> **GoLineGraph vs GoTimeGraph**: LineGraph의 X축은 string(카테고리), TimeGraph의 X축은 DateTime(시간축). TimeGraph는 `XScale`(표시 범위)과 `XAxisGraduationTime`(눈금 간격)이 **TimeSpan** 형식.

---

## GoTrendGraph (실시간 스트리밍)

### 디자인 설정 (UIEditor)

```
Series:  (GoLineGraphSeries)
  - Name: "Temp",  Alias: "온도",  Color: "Red",         Minimum: 0, Maximum: 100
  - Name: "Hum",   Alias: "습도",  Color: "deepskyblue", Minimum: 0, Maximum: 100
  - Name: "Rad",   Alias: "광량",  Color: "orange",      Minimum: 0, Maximum: 100

GridColor: "Base3"
TextColor: "Fore"
RemarkColor: "Base2"
GraphColor: "Back"
XScale: "00:00:01"                          # 화면 표시 범위 (1초)
MaximumXScale: "00:01:00"                   # 최대 확대 범위 (1분)
XAxisGraduationTime: "00:00:00.2000000"     # X축 눈금 간격 (0.2초)
YAxisGraduationCount: 10
Interval: 10                                # 데이터 수집 주기 (ms)
TimeFormatString: "HH:mm:ss"
```

### C# 코드

```csharp
public partial class PageTrendGraph : GoPage
{
    Data2 value;    // 실시간 데이터 소스 객체

    public PageTrendGraph()
    {
        InitializeComponent();

        // 1. 초기값으로 데이터 객체 생성
        value = new Data2
        {
            Temp = sldTemp.Value,
            Hum = sldHum.Value,
            Rad = sldRad.Value,
        };

        // 2. Start<T>(template) — 트렌드 수집 시작
        //    T의 프로퍼티명이 Series.Name과 매칭됨
        grpTrend.Start(value);
    }

    // 3. OnUpdate — 매 프레임 데이터 갱신
    protected override void OnUpdate()
    {
        value.Temp = sldTemp.Value;
        value.Hum = sldHum.Value;
        value.Rad = sldRad.Value;

        // SetData로 소스 객체 갱신 → 다음 Interval에 반영
        grpTrend.SetData(value);
        base.OnUpdate();
    }
}
```

### 실전 패턴 (통신 데이터 연결)

```csharp
// 실제 프로젝트에서의 패턴
public partial class PageTrend : GoPage
{
    Data2 value = new Data2();

    public PageTrend()
    {
        InitializeComponent();
        grpTrend.Start(value);
    }

    protected override void OnUpdate()
    {
        // DeviceManager에서 통신 데이터를 읽어 트렌드에 반영
        var data = Main.DevMgr.Data;
        value.Temp = data.Temperature;
        value.Hum = data.Humidity;
        value.Rad = data.Radiation;
        grpTrend.SetData(value);

        base.OnUpdate();
    }
}
```

> **흐름**: `Start(obj)` → 매 프레임 `SetData(obj)` → 내부에서 `Interval`(ms) 주기로 자동 샘플링.
> `Stop()`으로 수집 중단 가능.

---

## GoGauge / GoMeter / GoKnob

### 디자인 설정 (UIEditor)

```
GoGauge:
  Title: "Gauge"
  FillColor: "Good"             # 채움 색상
  EmptyColor: "Base1"           # 빈 영역 색상
  BorderColor: "Base1"
  StartAngle: 135               # 시작 각도
  SweepAngle: 270               # 스윕 각도
  BarSize: 24                   # 바 두께
  Gap: 40                       # 내부 여백
  Minimum: 0, Maximum: 100
  Format: "0"

GoMeter:
  Title: "Meter"
  NeedleColor: "Fore"           # 바늘 색상
  NeedlePointColor: "Red"       # 바늘 중심점
  RemarkColor: "Base5"          # 눈금 텍스트
  GraduationLarge: 10           # 큰 눈금 수
  GraduationSmall: 2            # 큰 눈금 사이 작은 눈금 수
  Gap: 5
  Minimum: 0, Maximum: 100
  Format: "0"

GoKnob:
  KnobColor: "Base3"            # 노브 색상
  CursorColor: "Fore"           # 커서 색상
  SweepAngle: 270
  DrawText: true                # 값 텍스트 표시
  Minimum: 0, Maximum: 100
  Format: "0"
```

### C# 코드

```csharp
public partial class PageGauge : GoPage
{
    public PageGauge()
    {
        InitializeComponent();
    }

    // OnUpdate에서 값 연동
    protected override void OnUpdate()
    {
        // 입력(knob) → 출력(gauge, meter)
        meter.Value = knob.Value;
        gauge.Value = knob.Value;
        base.OnUpdate();
    }
}
```

### 실전 패턴 (통신값 연결)

```csharp
protected override void OnUpdate()
{
    var data = Main.DevMgr.Data;
    gauge.Value = data.Temperature;
    meter.Value = data.Pressure;
    base.OnUpdate();
}
```

> GoGauge/GoMeter는 **읽기 전용 표시**, GoKnob는 **사용자 입력**. `Value` 프로퍼티를 매 프레임 갱신하면 애니메이션 자동 적용.

---

## GoSwitchPanel (서브페이지 전환)

### C# 코드

```csharp
public partial class PageSwitchPanel : GoPage
{
    public PageSwitchPanel()
    {
        InitializeComponent();

        // 초기 페이지 설정
        swpnl.SetPage("pnl1");

        // 라디오 버튼으로 서브페이지 전환
        btnPage1.ButtonClicked += (s, e) => swpnl.SetPage("pnl1");
        btnPage2.ButtonClicked += (s, e) => swpnl.SetPage("pnl2");
        btnPage3.ButtonClicked += (s, e) => swpnl.SetPage("pnl3");
        btnPage4.ButtonClicked += (s, e) => swpnl.SetPage("pnl4");
    }

    // 페이지 표시 시 버튼 상태 동기화
    protected override void OnShow()
    {
        btnPage1.Checked = swpnl.SelectedPage?.Name == "pnl1";
        btnPage2.Checked = swpnl.SelectedPage?.Name == "pnl2";
        btnPage3.Checked = swpnl.SelectedPage?.Name == "pnl3";
        btnPage4.Checked = swpnl.SelectedPage?.Name == "pnl4";
        base.OnShow();
    }
}
```

> **GoSwitchPanel**: 하나의 영역에 여러 서브페이지를 겹쳐두고 `SetPage("name")`으로 전환.
> `Design.SetPage()`는 **메인 페이지** 전환, `GoSwitchPanel.SetPage()`는 **서브페이지** 전환.

---

## GoDataGrid (종합 샘플)

### 디자인 설정 (UIEditor)

```
RowColor: "Base2"               # 행 배경색
SummaryRowColor: "Base1"        # 요약행 배경색
ColumnColor: "Base1"            # 헤더행 배경색
SelectedRowColor: "Select"      # 선택행 색상
BoxColor: "Base2"               # 입력박스 색상
ScrollBarColor: "Base1"
RowHeight: 30
ColumnHeight: 30
SelectionMode: 2                # 0=None, 1=Selector, 2=Single, 3=Multi, 4=MultiPC
```

> DataGrid 컬럼은 UIEditor가 아닌 **코드에서 직접 구성**하는 것이 일반적.

### 데이터 클래스

```csharp
public class DataGridSampleData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public double Temperature { get; set; }
    public float Pressure { get; set; }
    public int ErrorCount { get; set; }
    public bool IsOnline { get; set; }
    public bool IsAlarm { get; set; }
    public bool UseFilter { get; set; }
    public DateTime LastUpdate { get; set; }
    public string TagColor { get; set; }        // "#RRGGBB" 형식
    public string State { get; set; }           // Combo용 문자열
}
```

### C# 코드 — 컬럼 구성

```csharp
public partial class PageDataGrid : GoPage
{
    public PageDataGrid()
    {
        InitializeComponent();

        dg.ScrollMode = Going.UI.Utils.ScrollMode.Both;
        dg.Columns.Clear();

        #region 읽기 전용 컬럼
        // LabelColumn — 텍스트 표시 (편집 불가)
        dg.Columns.Add(new GoDataGridLabelColumn { Name = "Id", HeaderText = "ID", Size = "60px" });
        dg.Columns.Add(new GoDataGridLabelColumn { Name = "Name", HeaderText = "장치명", Size = "100px" });
        dg.Columns.Add(new GoDataGridLabelColumn { Name = "Location", HeaderText = "위치", Size = "80px" });

        // NumberColumn<T> — 숫자 포맷 표시 (편집 불가)
        dg.Columns.Add(new GoDataGridNumberColumn<double> { Name = "Temperature", HeaderText = "온도", Size = "80px", FormatString = "0.0" });
        dg.Columns.Add(new GoDataGridNumberColumn<float> { Name = "Pressure", HeaderText = "압력", Size = "80px", FormatString = "0.00" });

        // LampColumn — bool 값을 램프로 표시
        dg.Columns.Add(new GoDataGridLampColumn { Name = "IsOnline", HeaderText = "통신", Size = "60px" });
        dg.Columns.Add(new GoDataGridLampColumn { Name = "IsAlarm", HeaderText = "알람", Size = "60px", OnColor = "red" });

        // CheckBoxColumn — bool 값을 체크박스로 표시 (읽기 전용)
        dg.Columns.Add(new GoDataGridCheckBoxColumn { Name = "UseFilter", HeaderText = "필터", Size = "60px" });
        #endregion

        #region 편집 가능 컬럼
        // InputNumberColumn<T> — 숫자 편집
        dg.Columns.Add(new GoDataGridInputNumberColumn<int> { Name = "ErrorCount", HeaderText = "에러수", Size = "80px", Minimum = 0, Maximum = 999 });

        // InputBoolColumn — ON/OFF 토글
        dg.Columns.Add(new GoDataGridInputBoolColumn { Name = "IsOnline", HeaderText = "상태", Size = "80px", OnText = "ON", OffText = "OFF" });

        // InputTextColumn — 텍스트 편집
        dg.Columns.Add(new GoDataGridInputTextColumn { Name = "Name", HeaderText = "이름 편집", Size = "100px" });

        // InputTimeColumn — DateTime 편집
        dg.Columns.Add(new GoDataGridInputTimeColumn { Name = "LastUpdate", HeaderText = "최종갱신", Size = "140px" });

        // InputColorColumn — 색상 선택
        dg.Columns.Add(new GoDataGridInputColorColumn { Name = "TagColor", HeaderText = "태그색", Size = "80px" });

        // InputComboColumn — 드롭다운 선택
        dg.Columns.Add(new GoDataGridInputComboColumn
        {
            Name = "State", HeaderText = "상태", Size = "100px",
            Items = [
                new GoDataGridInputComboItem { Text = "Idle", Value = "Idle" },
                new GoDataGridInputComboItem { Text = "Running", Value = "Running" },
                new GoDataGridInputComboItem { Text = "Warning", Value = "Warning" },
                new GoDataGridInputComboItem { Text = "Error", Value = "Error" },
                new GoDataGridInputComboItem { Text = "Maintenance", Value = "Maintenance" },
            ]
        });
        #endregion

        #region 버튼 컬럼
        // ButtonColumn — 행별 동작 버튼 (Name 불필요, 데이터 바인딩 대상 아님)
        dg.Columns.Add(new GoDataGridButtonColumn { HeaderText = "삭제", Size = "60px", Text = "DEL", ButtonColor = "Danger" });
        #endregion

        #region 요약행
        dg.SummaryRows.Add(new GoDataGridSumSummaryRow { Title = "합계", TitleColumnIndex = 0, TitleColSpan = 3 });
        dg.SummaryRows.Add(new GoDataGridAverageSummaryRow { Title = "평균", TitleColumnIndex = 0, TitleColSpan = 3 });
        #endregion

        #region 이벤트
        // 버튼 클릭
        dg.CellButtonClick += (s, e) =>
        {
            if (e.Cell.Row.Source is DataGridSampleData data)
            {
                // data 기반으로 삭제 등 처리
            }
        };

        // 값 변경 (Input 계열 컬럼에서 발생)
        dg.ValueChanged += (s, e) =>
        {
            // e.Cell.Row.Source로 변경된 행 데이터 접근
        };

        // 행 선택 변경
        dg.SelectedChanged += (s, e) =>
        {
            // 선택된 행 처리
        };
        #endregion

        // 데이터 바인딩
        dg.SetDataSource(SampleData.Datas3);
    }
}
```

### 컬럼 타입 요약

| 타입 | 용도 | 주요 프로퍼티 |
|------|------|-------------|
| `LabelColumn` | 텍스트 표시 | — |
| `NumberColumn<T>` | 숫자 포맷 표시 | `FormatString` |
| `LampColumn` | bool → 램프 | `OnColor`, `OffColor` |
| `CheckBoxColumn` | bool → 체크박스 | — |
| `ButtonColumn` | 행별 버튼 | `Text`, `ButtonColor` |
| `InputTextColumn` | 텍스트 편집 | — |
| `InputNumberColumn<T>` | 숫자 편집 | `Minimum`, `Maximum` |
| `InputBoolColumn` | ON/OFF 토글 | `OnText`, `OffText` |
| `InputComboColumn` | 드롭다운 선택 | `Items[]` |
| `InputTimeColumn` | DateTime 편집 | — |
| `InputColorColumn` | 색상 선택 | — |

### 데이터 바인딩 규칙

- `dg.SetDataSource<T>(IEnumerable<T>)` — 리플렉션 기반
- **Column.Name**이 **T의 프로퍼티명**과 일치해야 바인딩됨
- 하나의 프로퍼티를 여러 컬럼에서 참조 가능 (예: `IsOnline` → LampColumn + InputBoolColumn)
- ButtonColumn은 데이터 바인딩 대상이 아니므로 Name 불필요
