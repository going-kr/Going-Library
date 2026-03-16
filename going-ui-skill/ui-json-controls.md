# Going UI — 컨트롤 JSON (기본)

> 이 문서는 `ui-json.md`의 하위 참조 문서. 기본 컨트롤 14종의 JSON 구조를 다룬다.
> 확장 컨트롤(GoDataGrid, 그래프, 목록 등)은 `ui-json-controls-ext.md` 참조.
> GoInput/GoValue 계열은 `ui-json-input-value.md` 참조.
> 공통 속성, Enum, 테마 등은 `ui-json.md` 참조.

## 목차

| # | 컨트롤 | # | 컨트롤 |
|---|--------|---|--------|
| 1 | GoLabel | 8 | GoLampButton |
| 2 | GoButton | 9 | GoProgress |
| 3 | GoLamp | 10 | GoSlider |
| 4 | GoOnOff | 11 | GoRangeSlider |
| 5 | GoSwitch | 12 | GoKnob |
| 6 | GoToggleButton | 13 | GoNumberBox |
| 7 | GoRadioButton | 14 | GoStep |

---

## 기본 컨트롤 JSON

> 각 컨트롤 JSON에는 **공통 속성**(Id, Name, Visible, Enabled, Selectable, Bounds, Dock, Margin 등)이 포함되어 있다.
> 공통 속성의 기본값 정의는 **`ui-json.md` > "공통 속성 (GoControl 기본)"이 정본**이다.
> 아래 JSON에서 공통 속성 기본값과 다른 값이 있으면(예: `Selectable: true`) 해당 컨트롤의 실제 기본값이다.

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
