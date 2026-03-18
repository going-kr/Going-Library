# Going UI — .gud 파일 / JSON 구조

**이 문서의 모든 속성명·기본값은 소스코드 [GoProperty] 속성에서 검증됨.**

## 하위 참조 문서

| 작업 | 파일 | 설명 |
|------|------|------|
| 기본 컨트롤 배치 | `ui-json-controls.md` | GoLabel, GoButton, GoLamp, GoSlider 등 기본 14종 |
| 확장 컨트롤 배치 | `ui-json-controls-ext.md` | GoDataGrid, GoGauge, GoBarGraph, GoNavigator 등 확장 19종 |
| GoInput/GoValue 배치 | `ui-json-input-value.md` | GoInputString, GoInputNumber 등 입력 7종 + GoValue 표시 3종 |
| 컨테이너 배치 | `ui-json-containers.md` | GoTableLayoutPanel, GoBoxPanel, GoTabControl 등 9종 |
| ImageCanvas UI | `ui-json-imagecanvas.md` | IcButton, IcLabel, IcSlider 등 이미지 기반 컨트롤 7종 |

**필요한 컨트롤의 JSON을 확인할 때 해당 파일을 `Read` 도구로 읽을 것.**

---

## .gud 파일 구조

.gud 파일은 GoDesign 객체를 직접 직렬화한 **단일 JSON**이다 (이중 직렬화 없음).

```json
{
  "Name": "ProjectName",
  "DesignWidth": 1024,
  "DesignHeight": 600,
  "ProjectFolder": "None",
  "Pages": { ... },
  "Windows": { ... },
  "Images": {},
  "Fonts": {},
  "TitleBar": { "Visible": false, "BarSize": 50, "Title": "Title", ... },
  "LeftSideBar": { "Visible": false, "BarSize": 150 },
  "RightSideBar": { "Visible": false, "BarSize": 150 },
  "Footer": { "Visible": false },
  "UseTitleBar": false,
  "UseLeftSideBar": false,
  "UseRightSideBar": false,
  "UseFooter": false,
  "BarColor": "Base2",
  "OverlaySideBar": false,
  "ExpandLeftSideBar": false,
  "ExpandRightSideBar": false,
  "CustomTheme": null
}
```

> **⚠️ Name 필드 규칙 (필수)**
> - `Name`은 C# 네임스페이스로 직접 사용되므로 **반드시 유효한 C# 식별자**여야 함
> - **영문자·숫자·언더스코어만 허용** (한글, 띄어쓰기, 특수문자 불가)
> - **영문자 또는 언더스코어로 시작** (숫자로 시작 불가)
> - 예시: `DataLogger` ✅, `OilRecovery_V09` ✅, `유증기 GUI` ❌, `My Project` ❌, `3DViewer` ❌
> - 사용자가 한글·띄어쓰기가 포함된 이름을 요청하면 적절한 **영문 PascalCase 이름을 제안**할 것

### CustomTheme

null이면 기본 테마. 커스텀 시:

```json
"CustomTheme": {
  "Dark": true,
  "Fore": 4294967295, "Back": 4281479730,
  "Window": 4280295456, "WindowBorder": 4284111450,
  "Point": 4287299584, "Title": 4282795590,
  "Base0": 4278190080, "Base1": 4280163870, "Base2": 4282137660,
  "Base3": 4284111450, "Base4": 4286085240, "Base5": 4288059030,
  "User1": 4278222848, "User2": 4291979550, "User3": 4279508500,
  "User4": 4280161300, "User5": 4292519200, "User6": 4278222976,
  "User7": 4278222976, "User8": 4278239231, "User9": 4287299584,
  "ScrollBar": 4280163870, "ScrollCursor": 4284111450,
  "Good": 4278222848, "Warning": 4291979550, "Danger": 4287299584,
  "Error": 4294901760, "Highlight": 4278221516, "Select": 4278221516,
  "Corner": 5, "Alpha": 0, "ShadowAlpha": 180,
  "DownBrightness": -0.25, "BorderBrightness": -0.3,
  "HoverBorderBrightness": 0.5, "HoverFillBrightness": 0.15,
  "StageLineBrightness": 0
}
```

SKColor는 uint(ARGB). 예: White=4294967295, Black=4278190080

---

## Pages / Windows 구조

> **⚠️ 절대 규칙: Pages/Windows는 Array `[]`가 아니라 Dictionary `{}` 형태**
> - ✅ 올바름: `"Pages": { "PageMain": { "Type": "GoPage", "Value": { ... } } }`
> - ❌ 틀림: `"Pages": [{ "Type": "GoPage", "Name": "PageMain", ... }]`
> - Key는 페이지/윈도우의 Name, Value는 `{ "Type": "...", "Value": { ... } }` 래핑 객체
> - `"Value"` 래핑이 반드시 있어야 함 (GoPagesConverter/GoWindowsConverter가 `Type` → `Value` 순서로 읽음)
> - 컨트롤 배열 프로퍼티명은 `"Childrens"` (Controls 아님)

### GoPage
```json
"Pages": {
  "PageMain": {
    "Type": "GoPage",
    "Value": {
      "Childrens": [ ...컨트롤 배열... ],
      "BackgroundImage": null,
      "Id": "UUID", "Name": "PageMain",
      "Visible": true, "Enabled": true, "Selectable": false,
      "Bounds": "0,0,1024,600",
      "Dock": 0,
      "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
    }
  }
}
```

### GoWindow (팝업)
```json
"Windows": {
  "Keypad": {
    "Type": "GoWindow",
    "Value": {
      "IconString": "fa-calculator",
      "IconSize": 12, "IconGap": 10,
      "Text": "Window",
      "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
      "TextColor": "Fore", "WindowColor": "Back",
      "BorderColor": "Base2", "Round": 1, "TitleHeight": 40,
      "Childrens": [ ...컨트롤 배열... ],
      "Id": "UUID", "Name": "Keypad",
      "Visible": true, "Enabled": true, "Selectable": false,
      "Bounds": "0,0,400,500",
      "Dock": 0,
      "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
    }
  }
}
```

> GoWindow에는 `IconDirection` 속성 없음. `IconGap`만 존재.

### GoTitleBar
```json
"TitleBar": {
  "Visible": false,
  "BarSize": 50,
  "Title": "Title", "TitleImage": null,
  "LeftExpandIconString": "fa-bars",
  "LeftCollapseIconString": "fa-chevron-left",
  "RightExpandIconString": "fa-ellipsis-vertical",
  "RightCollapseIconString": "fa-chevron-right",
  "IconSize": 16,
  "TextColor": "Fore",
  "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 16,
  "Childrens": [],
  "Id": "UUID", "Name": null,
  "Bounds": "0,0,0,0", "Dock": 0,
  "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
}
```

### GoSideBar
```json
"LeftSideBar": {
  "Visible": false,
  "BarSize": 150,
  "Fixed": false,
  "Childrens": [],
  "Id": "UUID", "Name": null,
  "Bounds": "0,0,0,0", "Dock": 0,
  "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
}
```

> `Fixed`: true면 접기/펴기 불가. `Expand`는 런타임 전용(JsonIgnore).

### GoFooter
```json
"Footer": {
  "Visible": false,
  "BarSize": 40,
  "Childrens": [],
  "Id": "UUID", "Name": null,
  "Bounds": "0,0,0,0", "Dock": 0,
  "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
}
```

### IcPage (ImageCanvas 페이지)
```json
"Pages": {
  "PageMain": {
    "Type": "IcPage",
    "Value": {
      "BackgroundColor": "white",
      "OffImage": null, "OnImage": null,
      "Childrens": [ ...IcButton, IcLabel 등... ],
      "BackgroundImage": null,
      "Id": "UUID", "Name": "PageMain",
      "Visible": true, "Enabled": true, "Selectable": false,
      "Bounds": "0,0,1024,600", "Dock": 0,
      "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
    }
  }
}
```

> IcPage는 GoPage를 상속. `OffImage`/`OnImage`는 이미지 리소스명.
> 자식 Ic 컨트롤들은 부모의 OffImage/OnImage에서 자기 영역을 잘라 표시.

---

## 공통 속성 (GoControl 기본)

```json
{
  "Id": "UUID",
  "Name": null,
  "Visible": true,
  "Enabled": true,
  "Selectable": false,
  "Bounds": "0,0,70,30",
  "Dock": 0,
  "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 },
  "UseLongClick": false,
  "LongClickTime": null
}
```

> **`Fill` 속성은 존재하지 않음!** `Dock: GoDockStyle` enum(Fill=5)을 사용.
> `Selectable`은 `protected set` — JSON에 포함되지만 직접 설정 불가.

> **기본 폰트 크기**: `FontSize`의 기본값은 **12**. 텍스트가 포함된 컨트롤(GoLabel, GoButton, GoInput 등)에서 별도 지정 없으면 `"FontSize": 12`를 사용할 것. TitleBar 등 특수 영역만 16 이상으로 설정.

---

## Enum 값 참조표

### GoFontStyle
Normal=0, Italic=1, Bold=2, BoldItalic=3

### GoRoundType
Rect=0, All=1, L=2, R=3, T=4, B=5, LT=6, RT=7, LB=8, RB=9, Ellipse=10

### GoDockStyle
None=0, Left=1, Top=2, Right=3, Bottom=4, Fill=5

### GoContentAlignment
TopLeft=0, TopCenter=1, TopRight=2,
MiddleLeft=3, **MiddleCenter=4**, MiddleRight=5,
BottomLeft=6, BottomCenter=7, BottomRight=8

### GoDirectionHV
Horizon=0, Vertical=1

### GoDirection (탭 등)
Left=0, Right=1, Up=2, Down=3

### GoAutoFontSize
NotUsed=0, XXS=1, XS=2, S=3, M=4, L=5, XL=6, XXL=7

### GoButtonFillStyle
Flat=0, Emboss=1, Gradient=2

### ScrollMode
Horizon=0, Vertical=1, Both=2

### GoDataGridSelectionMode
None=0, Selector=1, Single=2, Multi=3, MultiPC=4

### ProgressDirection
LeftToRight=0, RightToLeft=1, BottomToTop=2, TopToBottom=3

### GoDateTimeKind
DateTime=0, Date=1, Time=2

### GoImageScaleMode
Real=0, CenterImage=1, Stretch=2, Zoom=3

| 값 | 동작 |
|----|------|
| `Real` | 원본 크기 그대로 좌상단에 표시 |
| `CenterImage` | 원본 크기 그대로 영역 중앙에 배치 |
| `Stretch` | 영역 크기에 맞게 늘리기 (비율 무시) |
| `Zoom` | 비율 유지하면서 영역에 맞게 확대/축소 |

### GoBarGraphMode
Stack=0, List=1

### GoItemSelectionMode
None=0, Single=1, Multi=2, MultiPC=3

---

## 테마 색상

| 이름 | 용도 |
|------|------|
| Fore | 기본 텍스트 |
| Back | 기본 배경 |
| Window | 윈도우 배경 |
| WindowBorder | 윈도우 테두리 |
| Point | 포인트 컬러 |
| Title | 타이틀바 배경 |
| Base0~Base5 | 계층별 배경/테두리 (낮을수록 어두움) |
| Good | 정상/OK |
| Warning | 경고 |
| Danger | 위험 |
| Error | 에러 |
| Highlight | 하이라이트 |
| Select | 선택 |
| User1~User9 | 사용자 정의 색상 |
| ScrollBar | 스크롤바 배경 |
| ScrollCursor | 스크롤바 커서 |
| Transparent | 투명 |

> 색상값은 테마명(string)으로 지정. 또한 CSS 색상명("lime", "red", "gray" 등)도 사용 가능.

---

## 아이콘 (Font Awesome)

`"fa-power-off"`, `"fa-play"`, `"fa-stop"`, `"fa-pause"`, `"fa-gear"`, `"fa-gears"`,
`"fa-triangle-exclamation"`, `"fa-circle-check"`, `"fa-circle-xmark"`,
`"fa-chart-line"`, `"fa-chart-bar"`, `"fa-table"`, `"fa-arrows-rotate"`,
`"fa-house"`, `"fa-lock"`, `"fa-unlock"`, `"fa-user"`, `"fa-users"`,
`"fa-bell"`, `"fa-wrench"`, `"fa-plus"`, `"fa-minus"`, `"fa-xmark"`,
`"fa-chevron-left"`, `"fa-chevron-right"`, `"fa-chevron-up"`, `"fa-chevron-down"`,
`"fa-bars"`, `"fa-ellipsis-vertical"`, `"fa-calculator"`, `"fa-key"`, `"fa-check"`,
`"fa-palette"`, `"fa-calendar"`, `"fa-clock"`, `"fa-angle-up"`, `"fa-angle-down"`,
`"fa-angle-left"`, `"fa-angle-right"`

---

## 자주 하는 실수

- `Bounds`: `"L,T,R,B"` 형식 — `"0,0,200,50"` = 좌상(0,0) 우하(200,50)
- `Margin`/`TextPadding`: 문자열 아님 → `{"Left":0,"Top":0,"Right":0,"Bottom":0}` 객체
- Enum: 문자열 아님 → 숫자 (`"Normal"` ❌ → `0` ✅)
- `GoTableLayoutPanel.Childrens`: 배열 아님 → `{"indexes":{...},"ls":[...]}` 객체
- `ColSpan`이지 `ColumnSpan`이 아님
- 모든 컨트롤에 고유 `Id`(UUID) 필수
- **`Fill` 속성 없음** — `"Dock": 5`가 Fill 역할
- GoSwitch: `OnOff` 속성 사용 (`Value` 아님)
- GoStep: `Step`+`StepCount` 사용 (`CurrentStep`/`Items` 아님)
- GoProgress: `EmptyColor`/`Format`/`ShowValueLabel` 사용 (`BarColor`/`FormatString`/`DrawText` 아님)
- GoSlider: `SliderColor`/`ValueFormat` 사용 (`CursorColor`/`FormatString` 아님)
- GoKnob: `KnobColor`/`CursorColor` 사용 (`BackColor`/`NeedleColor` 아님)
- GoNumberBox: `Tick`/`Format` 사용 (`Step`/`FormatString` 아님)
- GoRangeSlider: `LowerValue`/`UpperValue` 사용 (`Start`/`End` 아님)
- GoLamp/GoLampButton: `OnText`/`OffText` 속성 없음
- CSS 색상명도 유효 — `"lime"`, `"red"`, `"gray"`, `"danger"` 등

---

## .gud 파일 생성 절차

1. 요구사항 파악 (화면 크기, 페이지 구성, 컨트롤 배치)
2. **프로젝트 이름 검증** — 영문+숫자+언더스코어만, 영문/언더스코어로 시작, 띄어쓰기·한글·특수문자 불가. 위반 시 영문 PascalCase 이름 제안
3. GoDesign JSON 구성 (Name, DesignWidth, DesignHeight, ProjectFolder 포함)
4. 각 컨트롤마다 고유 UUID `Id` 생성
5. `.gud` 파일로 저장 (단일 GoDesign JSON — 이중 직렬화 없음)
