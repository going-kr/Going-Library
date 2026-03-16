# Going UI — GoInput / GoValue 계열 JSON

> 이 문서는 `ui-json.md`의 하위 참조 문서 (기본 컨트롤은 `ui-json-controls.md`, 확장 컨트롤은 `ui-json-controls-ext.md` 참조).
> GoInput(입력) / GoValue(표시) 계열 컨트롤의 JSON 구조를 다룬다.
> 공통 속성, Enum, 테마 등은 `ui-json.md` 참조.

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
