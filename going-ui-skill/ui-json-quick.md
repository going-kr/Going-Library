# Going UI — .gud 빠른 생성 가이드

> **이 파일 하나만 읽고 .gud를 생성한다.** 상세 속성 확인이 필요할 때만 개별 파일 참조.

---

## .gud 파일 구조

.gud = GoDesign 단일 JSON (이중 직렬화 없음, Project 래퍼 없음).

```json
{
  "Name": "ProjectName",
  "DesignWidth": 1024, "DesignHeight": 600,
  "ProjectFolder": "None",
  "Pages": { ... },
  "Windows": { ... },
  "Images": {}, "Fonts": {},
  "TitleBar": { "Visible": false, "BarSize": 50, "Title": "Title", "Childrens": [], "Id": "UUID", "Bounds": "0,0,0,0", "Dock": 0, "Margin": {"Left":3,"Top":3,"Right":3,"Bottom":3} },
  "LeftSideBar": { "Visible": false, "BarSize": 150, "Fixed": false, "Childrens": [], "Id": "UUID", "Bounds": "0,0,0,0", "Dock": 0, "Margin": {"Left":3,"Top":3,"Right":3,"Bottom":3} },
  "RightSideBar": { "Visible": false, "BarSize": 150, "Fixed": false, "Childrens": [], "Id": "UUID", "Bounds": "0,0,0,0", "Dock": 0, "Margin": {"Left":3,"Top":3,"Right":3,"Bottom":3} },
  "Footer": { "Visible": false, "BarSize": 40, "Childrens": [], "Id": "UUID", "Bounds": "0,0,0,0", "Dock": 0, "Margin": {"Left":3,"Top":3,"Right":3,"Bottom":3} },
  "UseTitleBar": false, "UseLeftSideBar": false, "UseRightSideBar": false, "UseFooter": false,
  "BarColor": "Base2", "OverlaySideBar": false,
  "ExpandLeftSideBar": false, "ExpandRightSideBar": false,
  "CustomTheme": null
}
```

- `Name`: 유효한 C# 식별자 (영문+숫자+밑줄, 영문/밑줄로 시작, 한글·띄어쓰기 불가)

---

## Pages / Windows — Dictionary 형태 (필수!)

```
✅ "Pages": { "PageMain": { "Type": "GoPage", "Value": { "Childrens": [...], ... } } }
❌ "Pages": [{ "Type": "GoPage", "Name": "PageMain" }]   ← Array 불가!
```

### GoPage

```json
"PageName": {
  "Type": "GoPage",
  "Value": {
    "Childrens": [],
    "BackgroundImage": null,
    "Id": "UUID", "Name": "PageName",
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,1024,600", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoWindow

```json
"WindowName": {
  "Type": "GoWindow",
  "Value": {
    "IconString": "fa-calculator", "IconSize": 12, "IconGap": 10,
    "Text": "Window",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "WindowColor": "Back", "BorderColor": "Base2",
    "Round": 1, "TitleHeight": 40,
    "Childrens": [],
    "Id": "UUID", "Name": "WindowName",
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,400,500", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

---

## 공통 속성 (GoControl)

모든 컨트롤에 포함. 아래가 기본값:

```json
"Id": "UUID",  "Name": null,
"Visible": true, "Enabled": true, "Selectable": false,
"Bounds": "0,0,70,30", "Dock": 0,
"Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
```

---

## Bounds 규칙 (핵심!)

**`"Left,Top,Right,Bottom"` — 좌상(L,T) 우하(R,B) 좌표. W,H가 아님!**

```
"10,20,210,70" = 좌표(10,20)에서 좌표(210,70)까지 → 폭200, 높이50
```

### 이미지 기반 Bounds 계산 절차

레이아웃 이미지가 제공된 경우, 아래 절차를 따라 정확한 좌표를 산출한다.

**Step 1 — 화면 해상도 확인**
프로젝트 해상도 (W×H)를 확인한다. 기본: 1024×600.

**Step 2 — 이미지 내 영역 비율 추출**
이미지에서 각 컨트롤이 차지하는 위치를 **비율(0.0~1.0)**로 추정한다.

```
컨트롤이 이미지 가로의 약 10%~30% 영역, 세로의 약 5%~15% 영역에 위치
→ xRatio = 0.10, yRatio = 0.05, wRatio = 0.20, hRatio = 0.10
```

**Step 3 — 픽셀 좌표 변환**

```
L = round(W × xRatio)     = round(1024 × 0.10) = 102
T = round(H × yRatio)     = round(600 × 0.05)  = 30
R = round(W × (xRatio + wRatio)) = round(1024 × 0.30) = 307
B = round(H × (yRatio + hRatio)) = round(600 × 0.15)  = 90
→ Bounds = "102,30,307,90"
```

**Step 4 — 정렬·간격 보정**
- 같은 행의 컨트롤은 T, B를 통일
- 같은 열의 컨트롤은 L, R을 통일
- 컨트롤 간 간격: 최소 5px, 권장 10px
- 화면 가장자리 여백: 최소 10px

**Step 5 — 검증**
- R > L, B > T 확인
- 컨트롤이 화면 밖으로 나가지 않는지 (R ≤ W, B ≤ H)
- 겹치는 컨트롤이 없는지

### 권장 크기 참고표

| 컨트롤 | 최소 (W×H) | 권장 (W×H) |
|--------|-----------|-----------|
| GoButton | 60×30 | 100×40 |
| GoLabel | 50×25 | 80×30 |
| GoLamp | 60×30 | 80×30 |
| GoValue* | 80×30 | 120×40 |
| GoInput* | 80×30 | 150×40 |
| GoSlider | 120×30 | 200×40 |
| GoGauge/GoMeter | 100×100 | 150×150 |
| GoDataGrid | 200×100 | 400×300 |
| GoProgress | 100×20 | 200×30 |
| GoKnob | 80×80 | 120×120 |

---

## 핵심 직렬화 규칙

| 규칙 | 올바른 예 | 흔한 실수 |
|------|----------|----------|
| 컨트롤 래퍼 | `{ "Type": "GoButton", "Value": { ... } }` | Value 없이 속성 직접 나열 |
| Bounds | `"10,10,210,60"` (L,T,R,B) | W,H로 착각 |
| Enum | 숫자 `0`, `1`, `5` | 문자열 `"Normal"` |
| Margin/TextPadding | `{"Left":0,...}` 객체 | `"0,0,0,0"` 문자열 |
| Dock=Fill | `"Dock": 5` | `"Fill": true` |
| Id | 고유 UUID 필수 | 누락 |
| 컨트롤 배열명 | `"Childrens"` | `"Controls"` |

---

## Enum 빠른 참조

```
GoDockStyle:        None=0 Left=1 Top=2 Right=3 Bottom=4 Fill=5
GoContentAlignment: TL=0 TC=1 TR=2 ML=3 MC=4 MR=5 BL=6 BC=7 BR=8
GoRoundType:        Rect=0 All=1 L=2 R=3 T=4 B=5 LT=6 RT=7 LB=8 RB=9 Ellipse=10
GoFontStyle:        Normal=0 Italic=1 Bold=2 BoldItalic=3
GoDirectionHV:      Horizon=0 Vertical=1
GoDirection:        Left=0 Right=1 Up=2 Down=3
GoAutoFontSize:     NotUsed=0 XXS=1 XS=2 S=3 M=4 L=5 XL=6 XXL=7
GoButtonFillStyle:  Flat=0 Emboss=1 Gradient=2
ProgressDirection:  LTR=0 RTL=1 BTT=2 TTB=3
```

---

## 컨트롤 빠른 참조

> 공통 속성(Id, Name, Visible, Enabled, Selectable, Bounds, Dock, Margin)은 생략.
> `S`=Selectable 기본값 (무표기=false, ✓=true).

### 기본 컨트롤

| 컨트롤 | S | 고유 속성 (기본값) |
|--------|---|-------------------|
| GoLabel | | `Text:"label"` `FontName:"나눔고딕"` `FontSize:12` `FontStyle:0` `TextColor:"Fore"` `LabelColor:"Base2"` `BorderColor:"Base2"` `Round:1` `BorderWidth:1` `BackgroundDraw:true` `BorderOnly:false` `ContentAlignment:4` `AutoFontSize:0` `AutoIconSize:0` `IconString:null` `IconSize:12` `IconDirection:0` `IconGap:5` `TextPadding:{"Left":0,"Top":0,"Right":0,"Bottom":0}` |
| GoButton | ✓ | `Text:"button"` `FontName:"나눔고딕"` `FontSize:12` `FontStyle:0` `TextColor:"Fore"` `ButtonColor:"Base3"` `BorderColor:"Base3"` `Round:1` `BorderWidth:1` `BackgroundDraw:true` `BorderOnly:false` `FillStyle:0` `ContentAlignment:4` `AutoFontSize:0` `AutoIconSize:0` `IconString:null` `IconSize:12` `IconDirection:0` `IconGap:5` |
| GoLamp | | `Text:"lamp"` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `OnColor:"Good"` `OffColor:"Base2"` `OnOff:false` `LampSize:24` `Gap:10` `ContentAlignment:4` `AutoFontSize:0` — **OnText/OffText/Round 없음** |
| GoOnOff | ✓ | `DrawText:true` `OnText:"On"` `OffText:"Off"` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `BoxColor:"Base1"` `BorderColor:"Base3"` `CursorColor:"Base3"` `OnColor:"lime"` `OffColor:"gray"` `CursorIconDraw:true` `CursorIconString:"fa-power-off"` `CursorIconSize:null` `Corner:null` `OnOff:false` `AutoFontSize:0` `AutoCursorIconSize:0` |
| GoSwitch | ✓ | `OnText:"On"` `OffText:"Off"` `OnIconString:null` `OffIconString:null` `FontName:"나눔고딕"` `FontSize:12` `IconSize:12` `IconDirection:0` `IconGap:5` `OnTextColor:"Fore"` `OffTextColor:"Base5"` `BoxColor:"Base1"` `BorderColor:"Base3"` `SwitchColor:"Base3"` `OnIconColor:"lime"` `OffIconColor:"red"` `OnOff:false` `AutoFontSize:0` `AutoIconSize:0` — **Value/TextColor/Round 없음** |
| GoToggleButton | ✓ | `Text:"button"` `CheckedText:"button"` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `ButtonColor:"Base3"` `CheckedButtonColor:"Select"` `BorderColor:"Base3"` `CheckedBorderColor:"Select"` `Round:1` `BorderWidth:1` `FillStyle:0` `Checked:false` `AllowToggle:true` `AutoFontSize:0` `AutoIconSize:0` `IconString:null` `IconSize:12` `IconDirection:0` `IconGap:5` |
| GoRadioButton | ✓ | GoToggleButton과 동일 구조 (AllowToggle 없음, Checked:false) |
| GoLampButton | ✓ | `Text:"button"` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `ButtonColor:"Base3"` `BorderColor:"Base3"` `OnColor:"Good"` `OffColor:"Base2"` `Round:1` `BorderWidth:1` `FillStyle:0` `OnOff:false` `LampSize:24` `Gap:10` `AutoFontSize:0` — **OnText/OffText 없음** |
| GoProgress | | `FontName:"나눔고딕"` `FontSize:18` `ValueFontSize:14` `TextColor:"Fore"` `FillColor:"Good"` `EmptyColor:"Base1"` `BorderColor:"Transparent"` `Direction:0` `Value:0` `Minimum:0` `Maximum:100` `Format:"0"` `Gap:5` `CornerRadius:5` `BarSize:null` `ShowValueLabel:false` — **Round 없음** |
| GoSlider | ✓ | `Text:"slider"` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `BoxColor:"Back"` `SliderColor:"Base5"` `ProgressColor:"Base1"` `BorderColor:"danger"` `Round:1` `Direction:0` `ShowValueLabel:true` `ValueFormat:"0"` `BarSize:4` `HandleRadius:15` `EnableShadow:true` `Value:0` `Minimum:0` `Maximum:100` `BackgroundDraw:false` `BorderOnly:false` |
| GoRangeSlider | ✓ | GoSlider와 동일 + `LowerValue:25` `UpperValue:75` `MinHandleSeparation:0.05` — **Start/End 아님** |
| GoKnob | ✓ | `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `KnobColor:"Base3"` `CursorColor:"Fore"` `Value:0` `Minimum:0` `Maximum:100` `Tick:null` `Format:"0"` `SweepAngle:270` `DrawText:true` — **BackColor/NeedleColor 아님** |
| GoNumberBox | ✓ | `FontName:"나눔고딕"` `FontSize:12` `Direction:0` `TextColor:"Fore"` `BorderColor:"Base3"` `ButtonColor:"Base3"` `ValueColor:"Base1"` `Round:1` `Value:0` `Minimum:0` `Maximum:100` `Tick:1` `Format:null` `ButtonSize:40` `AutoFontSize:0` — **Step/FormatString 아님** |
| GoStep | ✓ | `PrevIconString:"fa-chevron-left"` `NextIconString:"fa-chevron-right"` `ButtonColor:"Base3"` `StepColor:"Base2"` `SelectColor:"Select"` `IsCircle:false` `UseButton:true` `StepCount:7` `Step:0` — **Items/FontName/Direction 없음** |

### 확장 컨트롤

| 컨트롤 | S | 고유 속성 (기본값) |
|--------|---|-------------------|
| GoDataGrid | ✓ | `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `RowColor:"Base2"` `SummaryRowColor:"Base1"` `ColumnColor:"Base1"` `SelectedRowColor:"Select"` `BoxColor:"Base2"` `ScrollBarColor:"Base1"` `RowHeight:30` `ColumnHeight:30` `ScrollMode:0` `SelectionMode:0` `Columns:[]` `ColumnGroups:[]` `SummaryRows:[]` |
| GoCheckBox | ✓ | `Text:"checkbox"` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `BoxColor:"Base1"` `CheckColor:"Fore"` `Checked:false` `BoxSize:24` `Gap:10` `ContentAlignment:4` `AutoFontSize:0` |
| GoRadioBox | ✓ | GoCheckBox와 동일 구조 (형제 간 exclusive) |
| GoIconButton | ✓ | `IconString:null` `Rotate:0` `ButtonColor:"Base3"` `ClickBoundsExtends:false` |
| GoGauge | | `FontName:"나눔고딕"` `FontSize:18` `Title:"Title"` `TitleFontSize:12` `TextColor:"Fore"` `FillColor:"Good"` `EmptyColor:"Base1"` `BorderColor:"Base1"` `Value:0` `Minimum:0` `Maximum:100` `Format:"0"` `StartAngle:135` `SweepAngle:270` `BarSize:24` `Gap:0` |
| GoMeter | | `FontName:"나눔고딕"` `FontSize:18` `Title:"Title"` `TitleFontSize:12` `RemarkFontSize:10` `TextColor:"Fore"` `NeedleColor:"Fore"` `NeedlePointColor:"Red"` `RemarkColor:"Base5"` `Value:0` `Minimum:0` `Maximum:100` `GraduationLarge:10` `GraduationSmall:2` `Format:"0"` `Gap:0` |
| GoPicture | | `Image:null` `ScaleMode:0` `Round:0` |
| GoAnimate | | `OnImage:null` `OffImage:null` `ScaleMode:0` `Round:0` `Time:30` `OnOff:false` |
| GoCalendar | ✓ | `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `BoxColor:"Base3"` `SelectColor:"Select"` `Round:1` `BackgroundDraw:true` `MultiSelect:false` `NoneSelect:false` |
| GoNavigator | | `IconSize:12` `IconDirection:0` `IconGap:5` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `Indent:20` `MenuGap:30` `Menus:[]` `Direction:0` |
| GoListBox | ✓ | `IconSize:12` `IconGap:5` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `BoxColor:"Base1"` `BorderColor:"Base3"` `SelectColor:"Select"` `Round:1` `BackgroundDraw:true` `ItemHeight:30` `ItemAlignment:4` `SelectionMode:0` `Items:[]` |
| GoTreeView | ✓ | `IconSize:12` `IconGap:5` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `BoxColor:"Base1"` `BorderColor:"Base3"` `SelectColor:"Select"` `Round:1` `BackgroundDraw:true` `DragMode:false` `ItemHeight:30` `SelectionMode:0` `Nodes:[]` |
| GoToolBox | ✓ | `IconSize:12` `IconGap:5` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `BoxColor:"Base1"` `BorderColor:"Base3"` `SelectColor:"Select"` `CategoryColor:"Base2"` `Round:1` `BackgroundDraw:true` `ItemHeight:30` `Categories:[]` `DragMode:true` |
| GoBarGraph | ✓ | `GridColor:"Base3"` `TextColor:"Fore"` `RemarkColor:"Base2"` `GraphColor:"Back"` `FontName:"나눔고딕"` `FontSize:12` `GraduationCount:10` `FormatString:null` `Mode:0` `Direction:1` `Series:[]` `BarSize:20` `BarGap:20` `Minimum:null` `Maximum:null` |
| GoCircleGraph | | `GridColor:"Base3"` `TextColor:"Fore"` `RemarkColor:"Base2"` `FontName:"나눔고딕"` `FontSize:12` `Series:[]` |
| GoLineGraph | ✓ | `GridColor:"Base3"` `TextColor:"Fore"` `RemarkColor:"Base2"` `GraphColor:"Back"` `FontName:"나눔고딕"` `FontSize:12` `GraduationCount:10` `FormatString:null` `Series:[]` `PointWidth:70` |
| GoTimeGraph | ✓ | `GridColor:"Base3"` `TextColor:"Fore"` `RemarkColor:"Base2"` `GraphColor:"Back"` `FontName:"나눔고딕"` `FontSize:12` `XScale:"01:00:00"` `XAxisGraduationTime:"00:10:00"` `YAxisGraduationCount:10` `TimeFormatString:null` `ValueFormatString:null` `Series:[]` |
| GoTrendGraph | ✓ | GoTimeGraph + `MaximumXScale:"1.00:00:00"` `Interval:1000` `IsStart:false` |
| GoColorSelector | ✓ | `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `InputColor:"Base1"` `BorderColor:"Base3"` `ContentAlignment:4` `Value:4294967295` |

### GoInput 계열

공통: `IconString:null` `IconSize:12` `IconGap:5` `FontName:"나눔고딕"` `FontStyle:0` `FontSize:12` `Direction:0` `TextColor:"Fore"` `BorderColor:"Base3"` `FillColor:"Base3"` `ValueColor:"Base1"` `Round:1` `TitleSize:null` `Title:null` `ButtonSize:null` `Buttons:[]` `AutoFontSize:0` `AutoIconSize:0` — S=✓

| 타입 | 추가 속성 |
|------|----------|
| GoInputString | (공통만) |
| GoInputNumber\<T\> | `Value:0` `Minimum:null` `Maximum:null` `FormatString:null` `Unit:null` `UnitSize:null` |
| GoInputBoolean | `Value:false` `OnText:"ON"` `OffText:"OFF"` `OnIconString:null` `OffIconString:null` |
| GoInputCombo | `Items:[]` `ItemHeight:30` `MaximumViewCount:8` |
| GoInputSelector | `Items:[]` |
| GoInputColor | `Value:4294967295` |
| GoInputDateTime | `Value:"2024-01-01T00:00:00"` `DateTimeStyle:0` `DateFormat:"yyyy-MM-dd"` `TimeFormat:"HH:mm:ss"` |

### GoValue 계열

공통: GoInput 공통과 동일 구조. `ValueColor:"Base2"` (GoInput은 "Base1") — S=false

| 타입 | 추가 속성 |
|------|----------|
| GoValueString | `Value:null` |
| GoValueNumber\<T\> | `Value:0` `FormatString:null` `Unit:null` `UnitFontSize:12` `UnitSize:null` `AutoUnitFontSize:0` |
| GoValueBoolean | `Value:false` `OnText:"ON"` `OffText:"OFF"` `OnIconString:null` `OffIconString:null` |

### 컨테이너

| 컨트롤 | 구조 요약 |
|--------|----------|
| GoTableLayoutPanel | `Columns:["25%","25%","25%","25%"]` `Rows:["40px","50px"]` `Childrens:{"indexes":{UUID:{"Row":0,"Column":0,"RowSpan":1,"ColSpan":1}},"ls":[...]}` — **Childrens가 객체!** **ColSpan (ColumnSpan 아님)** |
| GoBoxPanel | `Childrens:[...]` `BorderColor:"Base3"` `BoxColor:"Base2"` `Round:1` `BackgroundDraw:true` `BorderSize:1` |
| GoGridLayoutPanel | `Rows:[{"Height":"50px","Columns":["50%","50%"]}]` `Childrens:{"indexes":{...},"ls":[...]}` |
| GoPanel | `Childrens:[...]` `Text:"Panel"` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `PanelColor:"Base2"` `Round:1` `BackgroundDraw:true` `BorderOnly:false` `TitleHeight:40` `IconString:null` `IconSize:12` `IconGap:5` `Buttons:[]` `ButtonWidth:null` |
| GoGroupBox | `Childrens:[...]` `Text:"Panel"` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `BorderColor:"Base3"` `Round:1` `BorderWidth:1` `IconString:null` `IconSize:12` `IconGap:5` `Buttons:[]` `ButtonWidth:null` |
| GoScrollablePanel | `Childrens:[...]` `BaseWidth:null` `EditorHeight:null` |
| GoScalePanel | `Childrens:[...]` `BaseWidth:null` `BaseHeight:null` `PanelAlignment:4` |
| GoSwitchPanel | `Pages:[{"Name":"Sub1","Childrens":[...]}]` |
| GoTabControl | `TabPages:[{"Name":"Tab1","IconString":null,"Text":"Tab1","Childrens":[...]}]` `FontName:"나눔고딕"` `FontSize:12` `TextColor:"Fore"` `TabColor:"Base2"` `TabBorderColor:"Base3"` `TabPosition:2` `NavSize:40` `IconDirection:0` `IconSize:12` `IconGap:5` |
| GoPicturePanel | `Childrens:[...]` `Image:null` `ScaleMode:0` `Round:1` |

---

## 테마 색상 키

Fore, Back, Window, WindowBorder, Point, Title, Base0~Base5, Good, Warning, Danger, Error, Highlight, Select, User1~User9, ScrollBar, ScrollCursor, Transparent
— CSS 색상명도 사용 가능: `"lime"`, `"red"`, `"gray"` 등

---

## 아이콘 (Font Awesome)

`fa-power-off` `fa-play` `fa-stop` `fa-pause` `fa-gear` `fa-gears`
`fa-triangle-exclamation` `fa-circle-check` `fa-circle-xmark`
`fa-chart-line` `fa-chart-bar` `fa-table` `fa-arrows-rotate`
`fa-house` `fa-lock` `fa-unlock` `fa-user` `fa-users`
`fa-bell` `fa-wrench` `fa-plus` `fa-minus` `fa-xmark`
`fa-chevron-left` `fa-chevron-right` `fa-chevron-up` `fa-chevron-down`
`fa-bars` `fa-ellipsis-vertical` `fa-calculator` `fa-key` `fa-check`

---

## 이미지 참조 처리

- 이미지의 레이아웃을 **우선 참조** (텍스트와 충돌 시 이미지 우선)
- 이미지에서 각 컨트롤의 **상대 위치 비율**을 추출하여 Bounds 계산 (위 "이미지 기반 Bounds 계산 절차" 참조)
- 이미지에서 읽기 어려운 컨트롤은 텍스트로 보완
- 레지스터 맵은 이미지만으로 판단 불가 — 반드시 문의

---

## 절대 규칙 (요약)

1. **Designer.cs / design.json 수정 금지** — UIEditor 자동 생성 파일
2. **속성/메서드 추측 금지** — 이 문서에 없으면 `api/` HTML 확인 필수
3. **코드 네이밍: 영문+숫자만** — 한글 식별자 불가, 주석으로 한글 보완
4. **데이터 구조 임의 작성 금지** — 레지스터 맵은 사용자 확인 필수
5. **Pages/Windows는 Dictionary** — Array 불가
6. **Fill 속성 없음** — `"Dock": 5` 사용
