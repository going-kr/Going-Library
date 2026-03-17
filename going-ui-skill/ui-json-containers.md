# Going UI — 컨테이너 JSON

> 이 문서는 `ui-json.md`의 하위 참조 문서. 컨테이너 컨트롤의 JSON 구조를 다룬다.
> 공통 속성, Enum, 테마 등은 `ui-json.md` 참조.

---

## GoTableLayoutPanel (핵심 레이아웃)

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
- **`"%"` 비율 우선 사용** — 다양한 해상도에서 레이아웃이 유지되려면 `px` 고정값보다 `"%"` 비율을 기본으로 사용. 타이틀 바나 고정 높이 영역 등 절대 크기가 필요한 경우에만 `"40px"` 사용

## GoBoxPanel (균등 분할)

```json
{
  "Type": "GoBoxPanel",
  "Value": {
    "Childrens": [ ...컨트롤 배열... ],
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

## GoGridLayoutPanel (유연 그리드)

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

## GoScrollablePanel (스크롤)

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

## GoScalePanel (비율 스케일링)

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

## GoSwitchPanel (페이지 전환)

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

## GoTabControl (탭)

```json
{
  "Type": "GoTabControl",
  "Value": {
    "IconDirection": 0, "IconSize": 12, "IconGap": 5,
    "FontName": "나눔고딕", "FontStyle": 0, "FontSize": 12,
    "TextColor": "Fore", "TabColor": "Base2", "TabBorderColor": "Base3",
    "TabPosition": 2, "NavSize": 40,
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

> `TabPosition`: GoDirection — Left=0, Right=1, Up=2, Down=3 (기본: Up=2)

## GoPanel (제목 패널)

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

## GoGroupBox (그룹 박스)

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

## GoPicturePanel (이미지 패널)

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
