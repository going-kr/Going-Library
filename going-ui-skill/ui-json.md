# Going UI — .gud 파일 / JSON 구조

**이 문서의 모든 속성명·기본값은 소스코드 [GoProperty] 속성에서 검증됨.**

---

## .gud 파일 최상위 구조

```json
{
  "Name": "프로젝트명",
  "Width": 1024,
  "Height": 600,
  "ProjectFolder": "None",
  "Design": "<GoDesign JSON 문자열 (이중 직렬화)>"
}
```

> `Design` 필드는 GoDesign 객체를 `GoJsonConverter.Options`로 직렬화한 **문자열**.
> Project 자체는 `JsonOpt.Options`(ProjectConverter)로 직렬화.
> Design 값은 JSON string — `{\u0022Pages\u0022:...}` 형태로 이스케이프됨.

---

## GoDesign JSON 구조

```json
{
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

---

## 컨테이너

### GoTableLayoutPanel (핵심 레이아웃)

```json
{
  "Type": "GoTableLayoutPanel",
  "Value": {
    "Columns": ["25%", "25%", "25%", "25%"],
    "Rows": ["40px", "50px", "10%", "10%"],
    "Childrens": {
      "indexes": {
        "UUID-of-control": { "Row": 0, "Column": 0, "RowSpan": 1, "ColSpan": 2 }
      },
      "ls": [
        { "Type": "GoLabel", "Value": { "Id": "UUID-of-control", ... } }
      ]
    },
    "Id": "...", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,1024,600", "Dock": 0,
    "Margin": { "Left": 0, "Top": 0, "Right": 0, "Bottom": 0 }
  }
}
```

**핵심 규칙:**
- `Childrens`가 배열이 아닌 **객체** `{ "indexes": {...}, "ls": [...] }`
- `indexes`의 키 = 컨트롤의 `Id` (UUID)
- `ColSpan`이지 `ColumnSpan`이 아님
- Columns/Rows 포맷: `"25%"` 또는 `"40px"`

### GoBoxPanel (균등 분할)

```json
{
  "Type": "GoBoxPanel",
  "Value": {
    "Childrens": [ ...컨트롤 배열... ],
    "Direction": 0,
    "ItemSize": null,
    "BorderColor": "Base3",
    "BoxColor": "Base2",
    "Round": 1,
    "BackgroundDraw": true,
    "BorderSize": 1,
    "Id": "...", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,500,400", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoGridLayoutPanel (유연 그리드)

```json
{
  "Type": "GoGridLayoutPanel",
  "Value": {
    "Rows": [
      { "Height": "50px", "Columns": ["50%", "50%"] },
      { "Height": "10%", "Columns": ["33%", "33%", "34%"] }
    ],
    "Childrens": {
      "indexes": {
        "UUID": { "Row": 0, "Column": 0, "RowSpan": 1, "ColSpan": 1 }
      },
      "ls": [
        { "Type": "GoLabel", "Value": { "Id": "UUID", ... } }
      ]
    },
    "Id": "...", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,800,600", "Dock": 0,
    "Margin": { "Left": 0, "Top": 0, "Right": 0, "Bottom": 0 }
  }
}
```

### GoScrollablePanel (스크롤)

```json
{
  "Type": "GoScrollablePanel",
  "Value": {
    "Childrens": [ ...컨트롤 배열... ],
    "BaseWidth": null,
    "EditorHeight": null,
    "Id": "...", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,500,400", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoScalePanel (비율 스케일링)

```json
{
  "Type": "GoScalePanel",
  "Value": {
    "Childrens": [ ...컨트롤 배열... ],
    "BaseWidth": null,
    "BaseHeight": null,
    "PanelAlignment": 4,
    "Id": "...", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,500,400", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoSwitchPanel (페이지 전환)

```json
{
  "Type": "GoSwitchPanel",
  "Value": {
    "Pages": [
      { "Name": "Sub1", "Childrens": [ ...컨트롤 배열... ] },
      { "Name": "Sub2", "Childrens": [ ...컨트롤 배열... ] }
    ],
    "Id": "...", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,500,400", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `Pages`: `List<GoSubPage>`. `SetPage("Sub1")`으로 전환.

### GoTabControl (탭)

```json
{
  "Type": "GoTabControl",
  "Value": {
    "IconDirection": 0, "IconSize": 12, "IconGap": 5,
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "TabColor": "Base2", "TabBorderColor": "Base3",
    "TabPosition": 0, "NavSize": 40,
    "TabPages": [
      { "Name": "Tab1", "IconString": null, "Text": "Tab1", "Childrens": [...] }
    ],
    "Id": "...", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,500,400", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `TabPosition`: GoDirection — Up=0, Down=1, Left=2, Right=3

### GoPanel (제목 패널)

```json
{
  "Type": "GoPanel",
  "Value": {
    "Childrens": [ ... ],
    "IconString": null, "IconSize": 12, "IconGap": 5,
    "Text": "Panel", "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "PanelColor": "Base2",
    "Round": 1, "BackgroundDraw": true, "BorderOnly": false,
    "TitleHeight": 40,
    "Buttons": [], "ButtonWidth": null,
    "Id": "...", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,500,400", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoGroupBox (그룹 박스)

```json
{
  "Type": "GoGroupBox",
  "Value": {
    "Childrens": [ ... ],
    "IconString": null, "IconSize": 12, "IconGap": 5,
    "Text": "Panel", "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "BorderColor": "Base3",
    "Round": 1, "BorderWidth": 1,
    "Buttons": [], "ButtonWidth": null,
    "Id": "...", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,500,400", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoPicturePanel (이미지 패널)

```json
{
  "Type": "GoPicturePanel",
  "Value": {
    "Childrens": [ ... ],
    "Image": null,
    "ScaleMode": 0,
    "Round": 1,
    "Id": "...", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,500,400", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `ScaleMode`: GoImageScaleMode — Real=0, Stretch=1, Uniform=2, UniformFill=3

---

## 주요 컨트롤 JSON

### GoLabel
```json
{
  "Type": "GoLabel",
  "Value": {
    "IconString": null, "IconSize": 12, "IconDirection": 0, "IconGap": 5,
    "Text": "label",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextPadding": { "Left": 0, "Top": 0, "Right": 0, "Bottom": 0 },
    "TextColor": "Fore", "LabelColor": "Base2", "BorderColor": "Base2",
    "Round": 1, "BorderWidth": 1,
    "BackgroundDraw": true, "BorderOnly": false,
    "ContentAlignment": 4,
    "AutoFontSize": 0, "AutoIconSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoButton
```json
{
  "Type": "GoButton",
  "Value": {
    "IconString": null, "IconSize": 12, "IconDirection": 0, "IconGap": 5,
    "Text": "button",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "ButtonColor": "Base3", "BorderColor": "Base3",
    "Round": 1, "BorderWidth": 1,
    "BackgroundDraw": true, "BorderOnly": false,
    "FillStyle": 0, "ContentAlignment": 4,
    "AutoFontSize": 0, "AutoIconSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 },
    "UseLongClick": false, "LongClickTime": null
  }
}
```

### GoLamp
```json
{
  "Type": "GoLamp",
  "Value": {
    "Text": "lamp",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "OnColor": "Good", "OffColor": "Base2",
    "OnOff": false,
    "LampSize": 24, "Gap": 10,
    "ContentAlignment": 4, "AutoFontSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> GoLamp에는 OnText/OffText/Round 속성이 **없음**. Text 하나로 표시.

### GoOnOff
```json
{
  "Type": "GoOnOff",
  "Value": {
    "DrawText": true, "OnText": "On", "OffText": "Off",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "BoxColor": "Base1", "BorderColor": "Base3",
    "CursorColor": "Base3",
    "OnColor": "lime", "OffColor": "gray",
    "CursorIconDraw": true, "CursorIconString": "fa-power-off",
    "CursorIconSize": null, "Corner": null,
    "OnOff": false,
    "AutoFontSize": 0, "AutoCursorIconSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> OnColor/OffColor 기본값은 테마명이 아닌 CSS 색상명 `"lime"`, `"gray"`.

### GoSwitch
```json
{
  "Type": "GoSwitch",
  "Value": {
    "OnText": "On", "OffText": "Off",
    "OnIconString": null, "OffIconString": null,
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "IconSize": 12, "IconDirection": 0, "IconGap": 5,
    "OnTextColor": "Fore", "OffTextColor": "Base5",
    "BoxColor": "Base1", "BorderColor": "Base3", "SwitchColor": "Base3",
    "OnIconColor": "lime", "OffIconColor": "red",
    "OnOff": false,
    "AutoFontSize": 0, "AutoIconSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> GoSwitch에는 `Value`/`TextColor`/`OnColor`/`OffColor`/`Round` 속성 없음!
> 텍스트색은 `OnTextColor`+`OffTextColor`, 상태는 `OnOff`, 스위치 외형은 `SwitchColor`.

### GoToggleButton
```json
{
  "Type": "GoToggleButton",
  "Value": {
    "IconString": null, "IconSize": 12, "IconDirection": 0, "IconGap": 5,
    "Text": "button", "CheckedText": "button",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore",
    "ButtonColor": "Base3", "CheckedButtonColor": "Select",
    "BorderColor": "Base3", "CheckedBorderColor": "Select",
    "Round": 1, "BorderWidth": 1, "FillStyle": 0,
    "Checked": false, "AllowToggle": true,
    "AutoFontSize": 0, "AutoIconSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `AllowToggle` 기본값 = **true**.

### GoRadioButton
```json
{
  "Type": "GoRadioButton",
  "Value": {
    "IconString": null, "IconSize": 12, "IconDirection": 0, "IconGap": 5,
    "Text": "button",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore",
    "ButtonColor": "Base3", "CheckedButtonColor": "Select",
    "BorderColor": "Base3", "CheckedBorderColor": "Select",
    "Round": 1, "BorderWidth": 1, "FillStyle": 0,
    "Checked": false,
    "AutoFontSize": 0, "AutoIconSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

### GoLampButton
```json
{
  "Type": "GoLampButton",
  "Value": {
    "Text": "button",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore",
    "ButtonColor": "Base3", "BorderColor": "Base3",
    "OnColor": "Good", "OffColor": "Base2",
    "Round": 1, "BorderWidth": 1, "FillStyle": 0,
    "OnOff": false,
    "LampSize": 24, "Gap": 10,
    "AutoFontSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> GoLampButton에는 OnText/OffText 속성 없음.

### GoProgress
```json
{
  "Type": "GoProgress",
  "Value": {
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 18,
    "ValueFontSize": 14,
    "TextColor": "Fore", "FillColor": "Good",
    "EmptyColor": "Base1", "BorderColor": "Transparent",
    "Direction": 0,
    "Value": 0, "Minimum": 0, "Maximum": 100,
    "Format": "0",
    "Gap": 5, "CornerRadius": 5,
    "BarSize": null, "ShowValueLabel": false,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `EmptyColor` (not BarColor), `Format` (not FormatString), `ShowValueLabel` (not DrawText).
> GoProgress에 `Round` 속성 없음!

### GoSlider
```json
{
  "Type": "GoSlider",
  "Value": {
    "IconString": null, "IconSize": 12, "IconGap": 5, "IconDirection": 0,
    "Text": "slider",
    "FontName": "나눔고딕", "FontSize": 12,
    "BackgroundDraw": false, "BorderOnly": false,
    "TextColor": "Fore", "BoxColor": "Back",
    "SliderColor": "Base5", "ProgressColor": "Base1",
    "BorderColor": "danger", "Round": 1,
    "Direction": 0,
    "ShowValueLabel": true, "ValueFormat": "0",
    "BarSize": 4, "HandleRadius": 15, "EnableShadow": true,
    "HandleHoverScale": 1.05,
    "Tick": null, "ShowTicks": false, "TickCount": 5, "TickSize": 10,
    "Value": 0, "ValueString": "0",
    "Minimum": 0, "Maximum": 100,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> GoSlider는 이전 스킬의 속성명과 완전히 다름.
> `SliderColor` (not CursorColor), `ProgressColor` (not BarColor), `ValueFormat` (not FormatString).

### GoRangeSlider
```json
{
  "Type": "GoRangeSlider",
  "Value": {
    "IconString": null, "IconSize": 12, "IconGap": 5, "IconDirection": 0,
    "Text": "slider",
    "FontName": "나눔고딕", "FontSize": 12,
    "BackgroundDraw": false, "BorderOnly": false,
    "TextColor": "Fore", "BoxColor": "Back",
    "SliderColor": "Base5", "ProgressColor": "Base1",
    "BorderColor": "danger", "Round": 1,
    "Direction": 0,
    "ShowValueLabel": true, "ValueFormat": "0",
    "BarSize": 4, "HandleRadius": 15, "EnableShadow": true,
    "HandleHoverScale": 1.05,
    "Tick": null, "ShowTicks": false, "TickCount": 5, "TickSize": 10,
    "LowerValue": 25, "UpperValue": 75,
    "LowerValueString": "25", "UpperValueString": "75",
    "Minimum": 0, "Maximum": 100,
    "MinHandleSeparation": 0.05,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `LowerValue`/`UpperValue` (not Start/End).

### GoKnob
```json
{
  "Type": "GoKnob",
  "Value": {
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "KnobColor": "Base3", "CursorColor": "Fore",
    "Value": 0, "Minimum": 0, "Maximum": 100,
    "Tick": null, "Format": "0", "SweepAngle": 270,
    "DrawText": true,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `KnobColor` (not BackColor), `CursorColor` (not NeedleColor).

### GoNumberBox
```json
{
  "Type": "GoNumberBox",
  "Value": {
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "Direction": 0,
    "TextColor": "Fore", "BorderColor": "Base3",
    "ButtonColor": "Base3", "ValueColor": "Base1",
    "Round": 1,
    "Value": 0, "Minimum": 0, "Maximum": 100,
    "Tick": 1, "Format": null,
    "ButtonSize": 40,
    "AutoFontSize": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `Tick` (not Step), `Format` (not FormatString), Minimum=0 Maximum=100 (not null).

### GoStep
```json
{
  "Type": "GoStep",
  "Value": {
    "PrevIconString": "fa-chevron-left",
    "NextIconString": "fa-chevron-right",
    "ButtonColor": "Base3", "StepColor": "Base2", "SelectColor": "Select",
    "IsCircle": false, "UseButton": true,
    "StepCount": 7, "Step": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> GoStep에는 Items/FontName/FontSize/TextColor/Direction 속성 없음!
> `Step` (not CurrentStep), `StepCount`(int)로 개수 지정.

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
    "Columns": [
      {
        "Name": "col1", "Text": "Column",
        "Width": 150, "MinWidth": 50,
        "HeaderAlignment": 4, "CellAlignment": 4,
        "Sortable": false, "SizeModifiable": true, "Visible": true
      }
    ],
    "SummaryRows": [],
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `GoDataGridSelectionMode` — Single=0, Multi=1, None=2

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

> `Menus`: ObservableList\<GoMenuItem\>.

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

> `SelectionMode`: GoItemSelectionMode — Single=0, Multi=1.
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
    "Visible": true, "Enabled": true, "Selectable": false,
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
    "Visible": true, "Enabled": true, "Selectable": false,
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
    "Visible": true, "Enabled": true, "Selectable": false,
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
    "Pause": false,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
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

---

## GoInput 계열

GoInput은 추상 기본 클래스. 하위 타입:

| 타입 | 설명 |
|------|------|
| `GoInputString` | 문자열 입력 |
| `GoInputNumber<T>` | 숫자 입력 (T = byte~decimal) |
| `GoInputBoolean` | 불리언 (ON/OFF) |
| `GoInputCombo` | 드롭다운 |
| `GoInputSelector` | 좌우 선택기 |
| `GoInputColor` | 색상 선택 |
| `GoInputDateTime` | 날짜/시간 |

### GoInput 공통 속성
```json
{
  "IconString": null, "IconSize": 12, "IconGap": 5,
  "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
  "Direction": 0,
  "TextColor": "Fore", "BorderColor": "Base3",
  "FillColor": "Base3", "ValueColor": "Base1",
  "Round": 1,
  "TitleSize": null, "Title": null,
  "ButtonSize": null, "Buttons": [],
  "AutoFontSize": 0, "AutoIconSize": 0
}
```

> `FillColor` = "Base3" (not "Base2"), `ValueColor` = "Base1" (not "Fore").

### GoInputNumber<T> 추가 속성
```json
{
  "Value": 0,
  "Minimum": null, "Maximum": null,
  "FormatString": null,
  "Unit": null, "UnitSize": null
}
```

### GoInputBoolean 추가 속성
```json
{
  "Value": false,
  "OnText": "ON", "OffText": "OFF",
  "OnIconString": null, "OffIconString": null
}
```

### GoInputCombo 추가 속성
```json
{
  "Items": [],
  "ItemHeight": 30, "MaximumViewCount": 8
}
```

### GoInputSelector 추가 속성
```json
{ "Items": [] }
```

### GoInputColor 추가 속성
```json
{ "Value": 4294967295 }
```

> SKColor uint 값.

### GoInputDateTime 추가 속성
```json
{
  "Value": "2024-01-01T00:00:00",
  "DateTimeStyle": 0,
  "DateFormat": "yyyy-MM-dd",
  "TimeFormat": "HH:mm:ss"
}
```

---

## GoValue 계열

| 타입 | 설명 |
|------|------|
| `GoValueString` | 문자열 표시 |
| `GoValueNumber<T>` | 숫자 표시 |
| `GoValueBoolean` | 불리언 표시 |

### GoValue 공통 속성
```json
{
  "IconString": null, "IconSize": 12, "IconGap": 5,
  "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
  "Direction": 0,
  "TextColor": "Fore", "BorderColor": "Base3",
  "FillColor": "Base3", "ValueColor": "Base2",
  "Round": 1,
  "TitleSize": null, "Title": null,
  "ButtonSize": null, "Buttons": [],
  "AutoFontSize": 0, "AutoIconSize": 0
}
```

> `ValueColor` = "Base2" (not "Fore").

### GoValueNumber<T> 추가 속성
```json
{
  "Value": 0,
  "FormatString": null,
  "Unit": null, "UnitFontSize": 12, "UnitSize": null,
  "AutoUnitFontSize": 0
}
```

### GoValueString 추가 속성
```json
{ "Value": null }
```

### GoValueBoolean 추가 속성
```json
{
  "Value": false,
  "OnText": "ON", "OffText": "OFF",
  "OnIconString": null, "OffIconString": null
}
```

---

## ImageCanvas 컨트롤

ImageCanvas는 **이미지 기반 UI**. 부모(IcPage/IcContainer)의 OffImage/OnImage에서 컨트롤 영역을 잘라 표시.
벡터 드로잉 대신 비트맵 슬라이싱으로 렌더링 — HMI 스킨 UI에 적합.

### IcContainer
```json
{
  "Type": "IcContainer",
  "Value": {
    "BackgroundColor": "white",
    "OffImage": null, "OnImage": null,
    "Childrens": [],
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,500,400", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> IcContainer는 GoContainer 상속. 자체 OffImage/OnImage를 가져 중첩 이미지 레이어 가능.

### IcLabel
```json
{
  "Type": "IcLabel",
  "Value": {
    "IconString": null, "IconSize": 12, "IconDirection": 0, "IconGap": 5,
    "Text": "label",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Black",
    "ContentAlignment": 4,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `TextColor` 기본값 = "Black" (GoLabel의 "Fore"와 다름).
> 배경 없이 텍스트만 표시 — 이미지 위에 텍스트 오버레이용.

### IcButton
```json
{
  "Type": "IcButton",
  "Value": {
    "IconString": null, "IconSize": 12, "IconDirection": 0, "IconGap": 5,
    "Text": "button",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Black",
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> 누르면 부모의 OnImage에서, 뗀 상태에서는 OffImage에서 해당 영역 표시.
> Event: `ButtonClicked`.

### IcOnOff
```json
{
  "Type": "IcOnOff",
  "Value": {
    "IconString": null, "IconSize": 12, "IconDirection": 0, "IconGap": 5,
    "Text": "onoff",
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Black",
    "OnOff": false, "ToggleMode": false,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `OnOff`=true → OnImage, false → OffImage 영역 표시.
> `ToggleMode`=true면 클릭 시 자동 토글. Event: `ValueChanged`.

### IcState
```json
{
  "Type": "IcState",
  "Value": {
    "StateImages": [],
    "State": 0,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `StateImages`: List\<StateImage\> — `[{ "Image": "리소스명", "State": 0 }, ...]`
> `State` 값에 해당하는 이미지 표시.

### IcProgress
```json
{
  "Type": "IcProgress",
  "Value": {
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Black",
    "OnBarImage": null, "OffBarImage": null,
    "FormatString": "0",
    "Minimum": 0, "Maximum": 100, "Value": 0,
    "DrawText": true,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": false,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> `OnBarImage`/`OffBarImage`가 null이면 부모 이미지에서 슬라이싱.
> 별도 바 이미지를 지정하면 독립 이미지 모드. Event: `ValueChanged`.

### IcSlider
```json
{
  "Type": "IcSlider",
  "Value": {
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Black",
    "OffBarImage": null, "OnBarImage": null,
    "OffCursorImage": null, "OnCursorImage": null,
    "FormatString": "0",
    "Minimum": 0, "Maximum": 100, "Value": 0,
    "Tick": null, "DrawText": true,
    "Id": "UUID", "Name": null,
    "Visible": true, "Enabled": true, "Selectable": true,
    "Bounds": "0,0,70,30", "Dock": 0,
    "Margin": { "Left": 3, "Top": 3, "Right": 3, "Bottom": 3 }
  }
}
```

> 바+커서 이미지 조합. 부모 이미지 슬라이싱 또는 독립 이미지 모드.
> `OffCursorImage`/`OnCursorImage`로 커서 이미지 지정. Event: `ValueChanged`.

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
Up=0, Down=1, Left=2, Right=3

### GoAutoFontSize
NotUsed=0, XXS=1, XS=2, S=3, M=4, L=5, XL=6, XXL=7

### GoButtonFillStyle
Flat=0, Gradient=1, Outline=2

### ScrollMode
Vertical=0, Horizontal=1, Both=2

### GoDataGridSelectionMode
Single=0, Multi=1, None=2

### ProgressDirection
LeftToRight=0, RightToLeft=1, BottomToTop=2, TopToBottom=3

### GoDateTimeKind
DateTime=0, Date=1, Time=2

### GoImageScaleMode
Real=0, Stretch=1, Uniform=2, UniformFill=3

### GoBarGraphMode
List=0, Stack=1

### GoItemSelectionMode
Single=0, Multi=1

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
2. GoDesign JSON 구성
3. 각 컨트롤마다 고유 UUID `Id` 생성
4. Project 구조로 감싸기 (`Design` 필드는 GoDesign을 문자열로 직렬화)
5. `.gud` 파일로 저장
