# Going UI — 컨트롤 JSON (확장)

> 이 문서는 `ui-json.md`의 하위 참조 문서. 확장 컨트롤 19종의 JSON 구조를 다룬다.
> 기본 컨트롤(GoLabel, GoButton, GoLamp 등)은 `ui-json-controls.md` 참조.
> 공통 속성, Enum, 테마 등은 `ui-json.md` 참조.

## 목차

| # | 컨트롤 | # | 컨트롤 | # | 컨트롤 |
|---|--------|---|--------|---|--------|
| 1 | GoDataGrid | 8 | GoPicture | 15 | GoBarGraph |
| 2 | GoCheckBox | 9 | GoAnimate | 16 | GoCircleGraph |
| 3 | GoRadioBox | 10 | GoCalendar | 17 | GoLineGraph |
| 4 | GoIconButton | 11 | GoNavigator | 18 | GoTimeGraph |
| 5 | GoGauge | 12 | GoListBox | 19 | GoTrendGraph |
| 6 | GoMeter | 13 | GoTreeView | |  |
| 7 |  | 14 | GoToolBox | |  |

---

## 확장 컨트롤 JSON

> 각 컨트롤 JSON에는 **공통 속성**(Id, Name, Visible, Enabled, Selectable, Bounds, Dock, Margin 등)이 포함되어 있다.
> 공통 속성의 기본값 정의는 **`ui-json.md` > "공통 속성 (GoControl 기본)"이 정본**이다.
> 아래 JSON에서 공통 속성 기본값과 다른 값이 있으면(예: `Selectable: true`) 해당 컨트롤의 실제 기본값이다.

### GoDataGrid
```json
{
  "Type": "GoDataGrid",
  "Value": {
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore",
    "RowColor": "Base2", "SummaryRowColor": "Base1",
    "ColumnColor": "Base1", "SelectedRowColor": "Select",
    "BoxColor": "Base2", "ScrollBarColor": "Base1",
    "RowHeight": 30, "ColumnHeight": 30,
    "ScrollMode": 0, "SelectionMode": 0,
    "ColumnGroups": [],
    "Columns": [],
    "SummaryRows": [],
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `GoDataGridSelectionMode` — None=0, Selector=1, Single=2, Multi=3, MultiPC=4
> `ScrollMode` — Horizon=0, Vertical=1, Both=2

> **Columns/SummaryRows는 보통 코드에서 구성** (ui-control-sample.md 참조). 아래는 JSON으로 직접 정의할 때의 구조.

#### GoDataGrid 컬럼 타입별 JSON

모든 컬럼 공통 속성:

```json
{
  "Type": "GoDataGridLabelColumn",
  "Name": "PropertyName",       // 데이터 객체의 프로퍼티명과 매칭
  "HeaderText": "표시 텍스트",
  "Size": "100px",              // "100px" 또는 "30%" 또는 "100%"(나머지 채움)
  "UseFilter": false,
  "UseSort": false,
  "TextColor": null,            // null이면 Grid.TextColor 사용
  "Fixed": false                // true면 수평 스크롤 시 고정
}
```

**읽기 전용 컬럼:**

```json
// LabelColumn — 텍스트 표시
{ "Type": "GoDataGridLabelColumn", "Name": "Name", "HeaderText": "장치명", "Size": "100px",
  "FormatString": null }

// NumberColumn<T> — 숫자 포맷 표시
{ "Type": "GoDataGridNumberColumn<Double>", "Name": "Temperature", "HeaderText": "온도", "Size": "80px",
  "FormatString": "0.0" }
// T: Int32, Int64, Single, Double, Decimal 등 .NET 타입명 사용

// LampColumn — bool 값 램프 표시
{ "Type": "GoDataGridLampColumn", "Name": "IsOnline", "HeaderText": "통신", "Size": "60px",
  "OnColor": "Good" }
// OnColor: 테마키 또는 직접 색상 ("red", "#FF0000" 등)

// CheckBoxColumn — bool 값 체크박스 표시 (읽기 전용)
{ "Type": "GoDataGridCheckBoxColumn", "Name": "UseFilter", "HeaderText": "필터", "Size": "60px" }
```

**편집 가능 컬럼:**

```json
// InputTextColumn — 텍스트 편집
{ "Type": "GoDataGridInputTextColumn", "Name": "Name", "HeaderText": "이름", "Size": "100px" }

// InputNumberColumn<T> — 숫자 편집
{ "Type": "GoDataGridInputNumberColumn<Int32>", "Name": "Count", "HeaderText": "수량", "Size": "80px",
  "Minimum": 0, "Maximum": 999, "FormatString": null }

// InputBoolColumn — ON/OFF 토글
{ "Type": "GoDataGridInputBoolColumn", "Name": "IsOnline", "HeaderText": "상태", "Size": "80px",
  "OnText": "ON", "OffText": "OFF" }

// InputTimeColumn — DateTime 편집
{ "Type": "GoDataGridInputTimeColumn", "Name": "LastUpdate", "HeaderText": "최종갱신", "Size": "140px",
  "DateTimeStyle": 0, "DateFormat": "yyyy-MM-dd", "TimeFormat": "HH:mm:ss" }
// GoDateTimeKind — DateTime=0, Date=1, Time=2

// InputColorColumn — 색상 선택
{ "Type": "GoDataGridInputColorColumn", "Name": "TagColor", "HeaderText": "색상", "Size": "80px" }

// InputComboColumn — 드롭다운 선택
{ "Type": "GoDataGridInputComboColumn", "Name": "State", "HeaderText": "상태", "Size": "100px",
  "MaximumViewCount": 8, "ItemHeight": 30,
  "Items": [
    { "Text": "Idle", "Value": "Idle" },
    { "Text": "Running", "Value": "Running" },
    { "Text": "Error", "Value": "Error" }
  ] }
```

**동작 컬럼:**

```json
// ButtonColumn — 행별 동작 버튼 (Name 불필요)
{ "Type": "GoDataGridButtonColumn", "HeaderText": "삭제", "Size": "60px",
  "Text": "DEL", "ButtonColor": "Danger", "SelectButtonColor": "Select-light",
  "IconString": null, "IconSize": 12, "IconGap": 5 }
```

#### GoDataGrid 요약행 JSON

```json
"SummaryRows": [
  { "Type": "GoDataGridSumSummaryRow",     "Title": "합계", "TitleColumnIndex": 0, "TitleColSpan": 3 },
  { "Type": "GoDataGridAverageSummaryRow",  "Title": "평균", "TitleColumnIndex": 0, "TitleColSpan": 3 }
]
```

> SummaryRow는 숫자형 컬럼(NumberColumn, InputNumberColumn)에 자동 적용됨

### GoCheckBox
```json
{
  "Type": "GoCheckBox",
  "Value": {
    "Text": "checkbox",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "BoxColor": "Base1", "CheckColor": "Fore",
    "Checked": false,
    "BoxSize": 24, "Gap": 10,
    "ContentAlignment": 4, "AutoFontSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoRadioBox
```json
{
  "Type": "GoRadioBox",
  "Value": {
    "Text": "radiobox",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "BoxColor": "Base1", "CheckColor": "Fore",
    "Checked": false,
    "BoxSize": 24, "Gap": 10,
    "ContentAlignment": 4, "AutoFontSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> GoRadioBox는 같은 부모 내에서 자동으로 형제 선택 해제 (exclusive).

### GoIconButton
```json
{
  "Type": "GoIconButton",
  "Value": {
    "IconString": null, "Rotate": 0,
    "ButtonColor": "Base3",
    "ClickBoundsExtends": false,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoGauge
```json
{
  "Type": "GoGauge",
  "Value": {
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 18,
    "Title": "Title", "TitleFontSize": 12,
    "TextColor": "Fore", "FillColor": "Good",
    "EmptyColor": "Base1", "BorderColor": "Base1",
    "Value": 0, "Minimum": 0, "Maximum": 100,
    "Format": "0",
    "StartAngle": 135, "SweepAngle": 270,
    "BarSize": 24, "Gap": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoMeter
```json
{
  "Type": "GoMeter",
  "Value": {
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 18,
    "Title": "Title", "TitleFontSize": 12, "RemarkFontSize": 10,
    "TextColor": "Fore", "NeedleColor": "Fore", "NeedlePointColor": "Red",
    "RemarkColor": "Base5",
    "Value": 0, "Minimum": 0, "Maximum": 100,
    "GraduationLarge": 10, "GraduationSmall": 2,
    "Format": "0", "Gap": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> GoMeter는 아날로그 계기판. `NeedleColor`는 GoMeter 전용 — GoKnob의 `CursorColor`와 혼동 주의.

### GoPicture
```json
{
  "Type": "GoPicture",
  "Value": {
    "Image": null,
    "ScaleMode": 0,
    "Round": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `ScaleMode`: GoImageScaleMode — Real=0, Stretch=1, Uniform=2, UniformFill=3.
> `Round`: GoRoundType — GoPicture는 기본 Rect(0).

### GoAnimate
```json
{
  "Type": "GoAnimate",
  "Value": {
    "OnImage": null, "OffImage": null,
    "ScaleMode": 0, "Round": 0,
    "Time": 30, "OnOff": false,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> 이미지 시퀀스 애니메이션. `Time`은 프레임 간격(ms 아닌 프레임 틱).

### GoCalendar
```json
{
  "Type": "GoCalendar",
  "Value": {
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "BoxColor": "Base3", "SelectColor": "Select",
    "Round": 1, "BackgroundDraw": true,
    "MultiSelect": false, "NoneSelect": false,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `CurrentYear`/`CurrentMonth`/`SelectedDays`는 [JsonIgnore] — 런타임 전용.

### GoNavigator
```json
{
  "Type": "GoNavigator",
  "Value": {
    "IconSize": 12, "IconDirection": 0, "IconGap": 5,
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore",
    "Indent": 20, "MenuGap": 30,
    "Menus": [],
    "Direction": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `Menus`: ObservableList\<GoMenuItem\>. GoMenuItem 구조:
>
> | 속성 | 타입 | 설명 |
> |------|------|------|
> | `IconString` | string? | 아이콘 문자열 (FontAwesome 등) |
> | `Text` | string? | 메뉴 표시 텍스트 |
> | `Tag` | object? | 사용자 정의 데이터 |
> | `PageName` | string? | 연결할 페이지 이름. 지정 시 메뉴 클릭으로 `Design.SetPage(PageName)` 자동 호출 |
>
> **PageName 자동 전환**: GoMenuItem에 `PageName`을 지정하면 해당 메뉴 클릭 시 별도 코드 없이 자동으로 페이지가 전환된다.

### GoListBox
```json
{
  "Type": "GoListBox",
  "Value": {
    "IconSize": 12, "IconGap": 5,
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "BoxColor": "Base1",
    "BorderColor": "Base3", "SelectColor": "Select",
    "Round": 1, "BackgroundDraw": true,
    "ItemHeight": 30, "ItemAlignment": 4,
    "SelectionMode": 0,
    "Items": [],
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `SelectionMode`: GoItemSelectionMode — None=0, Single=1, Multi=2, MultiPC=3.
> `Items`: ObservableList\<GoListItem\>.

### GoTreeView
```json
{
  "Type": "GoTreeView",
  "Value": {
    "IconSize": 12, "IconGap": 5,
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "BoxColor": "Base1",
    "BorderColor": "Base3", "SelectColor": "Select",
    "Round": 1, "BackgroundDraw": true,
    "DragMode": false, "ItemHeight": 30,
    "SelectionMode": 0,
    "Nodes": [],
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `Nodes`: ObservableList\<GoTreeNode\> (재귀 트리 구조).

### GoToolBox
```json
{
  "Type": "GoToolBox",
  "Value": {
    "IconSize": 12, "IconGap": 5,
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "BoxColor": "Base1",
    "BorderColor": "Base3", "SelectColor": "Select",
    "CategoryColor": "Base2",
    "Round": 1, "BackgroundDraw": true,
    "ItemHeight": 30,
    "Categories": [],
    "DragMode": true,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `Categories`: ObservableList\<GoToolCategory\> (GoToolItem 중첩).

### GoBarGraph
```json
{
  "Type": "GoBarGraph",
  "Value": {
    "GridColor": "Base3", "TextColor": "Fore",
    "RemarkColor": "Base2", "GraphColor": "Back",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "GraduationCount": 10, "FormatString": null,
    "Mode": 0, "Direction": 1,
    "Series": [],
    "BarSize": 20, "BarGap": 20,
    "Minimum": null, "Maximum": null,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `Mode`: GoBarGraphMode — List=0, Stack=1.
> `Series`: List\<GoGraphSeries\>.

### GoCircleGraph
```json
{
  "Type": "GoCircleGraph",
  "Value": {
    "GridColor": "Base3", "TextColor": "Fore",
    "RemarkColor": "Base2",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "Series": [],
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> 파이/도넛 차트. `Series`: List\<GoGraphSeries\>.

### GoLineGraph
```json
{
  "Type": "GoLineGraph",
  "Value": {
    "GridColor": "Base3", "TextColor": "Fore",
    "RemarkColor": "Base2", "GraphColor": "Back",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "GraduationCount": 10, "FormatString": null,
    "Series": [],
    "PointWidth": 70,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `Series`: List\<GoLineGraphSeries\>.

### GoTimeGraph
```json
{
  "Type": "GoTimeGraph",
  "Value": {
    "GridColor": "Base3", "TextColor": "Fore",
    "RemarkColor": "Base2", "GraphColor": "Back",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "XScale": "01:00:00",
    "XAxisGraduationTime": "00:10:00",
    "YAxisGraduationCount": 10,
    "TimeFormatString": null, "ValueFormatString": null,
    "Series": [],
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoTrendGraph
```json
{
  "Type": "GoTrendGraph",
  "Value": {
    "GridColor": "Base3", "TextColor": "Fore",
    "RemarkColor": "Base2", "GraphColor": "Back",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "MaximumXScale": "1.00:00:00",
    "XScale": "01:00:00",
    "XAxisGraduationTime": "00:10:00",
    "YAxisGraduationCount": 10,
    "TimeFormatString": null, "ValueFormatString": null,
    "Interval": 1000, "IsStart": false,
    "Series": [],
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> GoTrendGraph: 실시간 트렌드. `Interval`(ms) 주기로 데이터 수집. `Pause`로 일시정지.

### GoColorSelector
```json
{
  "Type": "GoColorSelector",
  "Value": {
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "InputColor": "Base1", "BorderColor": "Base3",
    "ContentAlignment": 4,
    "Value": 4294967295,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `Value`: SKColor uint(ARGB). HSV 색상 선택기.
