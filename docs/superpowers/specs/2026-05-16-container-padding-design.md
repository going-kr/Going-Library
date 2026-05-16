# 6개 컨테이너에 Padding 속성 추가

날짜: 2026-05-16
대상 어셈블리: `Going.UI`
관련 테스트: `Going.UI.Tests/Containers`

## 1. 배경

현재 컨테이너 컨트롤들은 자식 컨트롤이 컨테이너 경계까지 꽉 차게 배치되며, 내부 여백을 조절할 수단이 없다. WPF/WinForms의 `Padding`과 같은 일반적인 컨테이너 속성을 도입해 자식 영역을 안쪽으로 인셋시키는 기능이 필요.

대상 컨테이너 6개:
- `Going.UI.Design.GoPage`
- `Going.UI.Design.GoWindow`
- `Going.UI.Design.GoSideBar`
- `Going.UI.Containers.GoGroupBox`
- `Going.UI.Containers.GoPanel`
- `Going.UI.Containers.GoBoxPanel`

비대상 (의도적 제외):
- `GoScrollablePanel` / `GoScalePanel`: 스크롤/스케일 변환과 Padding 의미 충돌 가능
- `GoPicturePanel`: 이미지 영역 의미가 별개
- `GoGridLayoutPanel` / `GoTableLayoutPanel`: 셀 기반 레이아웃과 Padding 충돌 가능
- `GoTabControl` / `GoSwitchPanel`: 하위 페이지에 위임하는 구조
- `GoTitleBar` / `GoFooter`: 디자인 상수 영역, 자식 패딩 의미 약함
- `FsFlowSystemPanel`: 스케일 변환 + 흐름 시각화 특수성

## 2. 기존 구조 요약

### 2.1 `PanelBounds`와 `OnLayout`의 역할

`GoContainer`의 두 핵심 추상:

- **[PanelBounds](../../../Going.UI/Containers/GoContainer.cs:33)**: 자식 컨트롤이 사는 좌표계의 시작점/영역.
  - `OnDraw`에서 `canvas.Translate(PanelBounds.Left, PanelBounds.Top)` ([GoContainer.cs:74](../../../Going.UI/Containers/GoContainer.cs:74)) — 절대 좌표 자식이 자동 인셋됨
  - 마우스 이벤트도 `x - PanelBounds.Left, y - PanelBounds.Top` ([GoContainer.cs:92](../../../Going.UI/Containers/GoContainer.cs:92)) — 좌표 자동 보정
- **[OnLayout](../../../Going.UI/Containers/GoContainer.cs:156)**: Dock 자식의 위치/크기 결정. 기본은 `Areas()["Content"]` 사용.

### 2.2 현재 6개 컨테이너 상태

| 컨테이너 | PanelBounds 오버라이드 | OnLayout 오버라이드 |
|---|---|---|
| GoPanel | ✅ `Areas()["Panel"]` | ✅ 동일 패턴 |
| GoGroupBox | ✅ `Areas()["Panel"]` | ✅ 동일 패턴 |
| GoWindow | ✅ `Areas()["Panel"]` | ✅ 동일 패턴 |
| GoPage | ❌ (base 사용) | ❌ (base 사용) |
| GoBoxPanel | ❌ | ❌ |
| GoSideBar | ❌ | ❌ |

### 2.3 활용 가능한 인프라

- **`GoPadding`** 타입 이미 존재 (`Going.UI.Datas`)
- **`Util.FromRect(SKRect, GoPadding)`** 헬퍼 존재 ([Util.cs:120](../../../Going.UI/Utils/Util.cs:120)) — 사각형을 패딩만큼 인셋
- **Gudx/JSON 직렬화** — `GoPadding`은 `GoGudxConverter.IsScalar` 통과 ([GoGudxConverter.cs:1125](../../../Going.UI/Gudx/GoGudxConverter.cs:1125)), 전용 `Format/ParseGoPadding` 컨버터 존재 ([GudxSpecialConverters.cs:73](../../../Going.UI/Gudx/GudxSpecialConverters.cs:73))
- 즉, `[GoProperty]` 부착만으로 직렬화 자동 동작.

## 3. 목표

각 컨테이너에 `Padding` 속성을 추가해 자식 영역을 안쪽으로 인셋시킨다.

- **절대 좌표 자식**: `PanelBounds.Left/Top`이 Padding만큼 안쪽으로 이동 → `canvas.Translate`가 자동 처리 → 자식 그래픽 인셋
- **Dock 자식**: `OnLayout`의 도킹 영역(Width/Height)이 Padding만큼 줄어 Dock=Fill 자식이 인셋 영역에 맞춰짐
- **마우스 hit**: `PanelBounds` 기반 좌표 보정으로 자동 동작
- **기본값 `new GoPadding()` = (0,0,0,0)**: 기존 컨트롤 동작 변화 없음 (opt-in)
- **Gudx/JSON 라운드트립**: `[GoProperty]`로 자동 직렬화

비목표:
- 다른 7개 컨테이너 (ScrollablePanel, ScalePanel, PicturePanel, TabControl, GridLayoutPanel, TableLayoutPanel, SwitchPanel, TitleBar, Footer 등)에는 미추가
- `OnLayout` 코드 중복 해소를 위한 베이스 리팩터링 — 별도 작업
- `Padding`이 자식의 `Margin`과 혼합되는 새 규칙 — 기존 Margin 동작 그대로 (Padding은 영역 인셋, Margin은 자식별 간격)

## 4. 변경 사항

### 4.1 공통 패턴 — 모든 6개에 적용

```csharp
/// <summary>
/// 자식 컨트롤 영역의 내부 여백을 가져오거나 설정합니다.
/// 자식들은 이 값만큼 안쪽으로 인셋된 영역에 배치됩니다.
/// </summary>
[GoProperty(PCategory.Control, N)] public GoPadding Padding { get; set; } = new();
```

`N`은 각 컨테이너의 기존 마지막 `[GoProperty]` order + 1.

### 4.2 PanelBounds/OnLayout 오버라이드 있는 3개 — GoPanel / GoGroupBox / GoWindow

**`PanelBounds` 수정** (각 파일):
```csharp
[JsonIgnore] public override SKRect PanelBounds => Util.FromRect(Areas()["Panel"], Padding);
```

**`OnLayout` 수정** — 첫 줄 변경:
```csharp
// 변경 전
var rtPanel = rts["Panel"];

// 변경 후
var rtPanel = PanelBounds;
```

(나머지 OnLayout 로직 — nofills/fills 분류, ApplyDocking, 채우기 — 그대로.)

### 4.3 오버라이드 없는 3개 — GoPage / GoBoxPanel / GoSideBar

**`PanelBounds` 신규 오버라이드** (각 파일):
```csharp
/// <summary>
/// 자식 컨트롤이 배치되는 패널 영역을 가져옵니다. Padding만큼 인셋됩니다.
/// </summary>
[JsonIgnore] public override SKRect PanelBounds
    => Util.FromRect(Util.FromRect(0, 0, Width, Height), Padding);
```

**`OnLayout` 신규 오버라이드** (각 파일):
```csharp
/// <inheritdoc/>
protected override void OnLayout()
{
    var rtPanel = PanelBounds;

    var nofills = Childrens.Where(c => c.Dock != GoDockStyle.Fill).ToList();
    var fills = Childrens.Where(c => c.Dock == GoDockStyle.Fill).ToList();
    var bnds = Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height);
    foreach (var control in nofills) bnds = ApplyDocking(control, bnds);
    foreach (var c in fills)
    {
        c.Left = bnds.Left + c.Margin.Left;
        c.Top = bnds.Top + c.Margin.Top;
        c.Right = bnds.Right - c.Margin.Right;
        c.Bottom = bnds.Bottom - c.Margin.Bottom;
    }
}
```

(기존 `GoContainer.OnLayout` ([GoContainer.cs:156](../../../Going.UI/Containers/GoContainer.cs:156))과 동일한 패턴, 단 시작 영역이 `PanelBounds`로.)

### 4.4 Property Order 배정

| 컨테이너 | 기존 최대 order | Padding order |
|---|---|---|
| `GoPanel` | 17 (Elevation) | 18 |
| `GoBoxPanel` | 5 (Elevation) | 6 |
| `GoGroupBox` | 12 (BorderWidth) | 13 |
| `GoPage` | (Basic 5 = BackgroundImage) | Control 0 |
| `GoWindow` | 12 (BorderWidth) | 13 |
| `GoSideBar` | 2 (Expand) | 3 |

### 4.5 using 추가

`GoPage`, `GoBoxPanel`, `GoSideBar`의 `OnLayout` 신규 오버라이드는 `Util.FromRect` 및 `Childrens.Where` LINQ 사용 — 기존 using 확인 후 누락 시 추가:
- `Going.UI.Utils` (Util)
- `Going.UI.Enums` (GoDockStyle)
- `System.Linq`

대부분 이미 포함됨. 추가 사항은 구현 시 확인.

## 5. 테스트 — `Going.UI.Tests/Containers/PaddingTests.cs` (신규)

### 5.1 PanelBounds 인셋 검증 (6개 컨테이너 × 1 테스트 = 6 케이스)

각 컨테이너에 대해:
1. 인스턴스 생성, `Width=200`, `Height=150`
2. `Padding = new GoPadding(10, 5, 20, 15)` 설정
3. `PanelBounds` getter 호출
4. 검증:
   - `PanelBounds.Left == 기존_Left + 10`
   - `PanelBounds.Top == 기존_Top + 5`
   - `PanelBounds.Right == 기존_Right - 20`
   - `PanelBounds.Bottom == 기존_Bottom - 15`

GoPanel/GoGroupBox/GoWindow는 Title 영역을 잘라낸 후의 Panel 영역 기준. GoPage/GoBoxPanel/GoSideBar는 (0,0,Width,Height) 기준.

### 5.2 Gudx 라운드트립 (6개 × 1 = 6 케이스)

각 컨테이너:
1. 인스턴스 생성, `Padding = new GoPadding(3, 7, 11, 13)` 설정
2. `GoGudxConverter.SerializeControl` → `DeserializeControl`
3. 복원된 인스턴스의 `Padding.Left/Top/Right/Bottom`이 원본과 동일한지 검증

### 5.3 Dock=Fill 자식 위치 검증 (1 케이스, GoBoxPanel 대표)

가장 단순한 컨테이너 GoBoxPanel로:
1. `GoBoxPanel { Width=200, Height=150, Padding=new(10,10,10,10) }`
2. 자식 `new GoButton { Dock = GoDockStyle.Fill, Margin = new() }` 추가 (Margin 기본값은 `(4,4,4,4)`이므로 명시적으로 0으로 설정)
3. 1×1 `SKBitmap`/`SKCanvas` 만들어 `container.FireDraw(canvas, GoTheme.DarkTheme)` 호출 → 내부적으로 `OnDraw` → `OnLayout` 트리거
4. 검증: 자식 `Left==0, Top==0, Right==180, Bottom==130` — `PanelBounds`(0,0,180,130) 영역에 Margin 0 적용한 결과

`FireDraw`는 [GoControl.cs:307](../../../Going.UI/Controls/GoControl.cs:307)에 공개 메서드로 존재.

## 6. 위험 및 영향

- **하위 호환**: `Padding` 기본 `(0,0,0,0)` → 기존 동작 그대로. 신규 opt-in 속성.
- **다른 7개 컨테이너**: 변경 없음. 명시적 제외.
- **PanelBounds 의미 변경**: 기존 `Areas()["Panel"]` 직접 참조 코드가 있다면 차이 발생 가능. 6개 컨테이너 내부 `OnDraw`에서 `rts["Panel"]`로 그리는 코드 — Title 영역 시각 그리기는 인셋 안 받아야 하므로 그대로 `Areas()["Panel"]` 유지 (PanelBounds로 바꾸지 않음). 자식 레이아웃만 인셋.
- **Gudx 직렬화 포맷 확장**: 신규 `Padding="L,T,R,B"` 속성 추가. 미존재 파일은 기본값 `0,0,0,0`으로 로드. 기존 Gudx 파일 호환 ✓.

## 7. 참고

- [Going.UI/Containers/GoContainer.cs](../../../Going.UI/Containers/GoContainer.cs) — base PanelBounds / OnLayout
- [Going.UI/Utils/Util.cs:120](../../../Going.UI/Utils/Util.cs:120) — FromRect(SKRect, GoPadding) 헬퍼
- [Going.UI/Datas/GoPadding.cs](../../../Going.UI/Datas/GoPadding.cs) — Padding 타입
- [Going.UI/Gudx/GudxSpecialConverters.cs:73](../../../Going.UI/Gudx/GudxSpecialConverters.cs:73) — GoPadding 직렬화
