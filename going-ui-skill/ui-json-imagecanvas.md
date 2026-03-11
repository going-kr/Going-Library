# Going UI — ImageCanvas 컨트롤 JSON

> 이 문서는 `ui-json.md`의 하위 참조 문서. ImageCanvas 컨트롤의 JSON 구조를 다룬다.
> 공통 속성, Enum, 테마 등은 `ui-json.md` 참조.

---

ImageCanvas는 **이미지 기반 UI**. 부모(IcPage/IcContainer)의 OffImage/OnImage에서 컨트롤 영역을 잘라 표시.
벡터 드로잉 대신 비트맵 슬라이싱으로 렌더링 — HMI 스킨 UI에 적합.

## IcContainer
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

## IcLabel
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

## IcButton
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

## IcOnOff
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

## IcState
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

## IcProgress
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

## IcSlider
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
