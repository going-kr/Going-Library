# GoStep 분리 + Input/Value Title 확장 — 디자인 스펙

- 작성일: 2026-05-03
- 버전 대상: v1.2.9 (예정)

## 목적

1. **GoInput / GoValue Title 영역**의 폰트 크기와 정렬을 본문(Value)과 분리해서 지정할 수 있도록 한다.
2. 현재의 `GoStep` 컨트롤을 **`GoStepGauge`로 rename**하고, 절차 진행을 시각화하는 **`GoStepBar`를 신설**한다.

## Part 1. GoInput / GoValue Title 확장

### 신규 속성

`GoInput` (abstract base) 와 `GoValue` 양쪽에 동일하게 추가:

```csharp
[GoProperty(PCategory.Control, 20)]
public float TitleFontSize { get; set; } = 12f;

[GoProperty(PCategory.Control, 21)]
public GoContentAlignment TitleContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;

[GoProperty(PCategory.Control, 22)]
public GoAutoFontSize AutoTitleFontSize { get; set; } = GoAutoFontSize.NotUsed;
```

> Property index는 기존 마지막(19, BorderWidth) 다음 번호 부여.

### 시맨틱

- `TitleFontSize`: Title 영역에서 사용할 명시적 폰트 크기.
- `AutoTitleFontSize`: `GoAutoFontSize` enum (기존 패턴 그대로). `NotUsed`가 아니면 Title rect 높이 기준 비율로 환산 (`Util.FontSize(enum, height)`). 환산 결과가 우선, fallback이 `TitleFontSize`.
- `TitleContentAlignment`: Title 텍스트(+아이콘) 정렬. 기존 `DrawTextIcon` 기본값(MiddleCenter)와 호환.

### 렌더 변경

`GoInput.OnDraw` Title 블록 (현 152라인) 1줄 + 1줄 추가:

```csharp
// 기존
Util.DrawTextIcon(canvas, Title, FontName, FontStyle, fsz, IconString, isz,
    GoDirectionHV.Horizon, IconGap, rtTitle, cText);

// 신규
var tfsz = Util.FontSize(AutoTitleFontSize, rtTitle.Height) ?? TitleFontSize;
Util.DrawTextIcon(canvas, Title, FontName, FontStyle, tfsz, IconString, isz,
    GoDirectionHV.Horizon, IconGap, rtTitle, cText, TitleContentAlignment);
```

`GoValue.OnDraw` Title 블록 (현 140라인)도 동일 패턴.

### 호환성

- 기본값 `TitleFontSize=12f` = 기존 `FontSize` 기본값과 동일
- `TitleContentAlignment=MiddleCenter` = 현 `DrawTextIcon` 무인자 기본 정렬
- `AutoTitleFontSize=NotUsed` = 기존엔 본문 `AutoFontSize`를 따랐으나 별도 분리 (의도적 변경 — 본문 크기와 다른 크기 지정 가능해짐)
- 기존 design.json은 이 속성들이 직렬화 안 돼 있어도 default 값으로 채워짐 → 시각적 변화: **본문 `AutoFontSize` 사용 시** Title 크기가 기존엔 본문과 함께 변했지만 이제는 12f 고정. 이게 의도된 변경이며 사용자가 명시적으로 `AutoTitleFontSize`를 매칭시켜 동일 동작을 복원할 수 있음.

## Part 2. GoStep → GoStepGauge

### 변경 사항

- 파일: `Going.UI/Controls/GoStep.cs` → `GoStepGauge.cs`
- 클래스: `class GoStep` → `class GoStepGauge`
- 동작/속성/이벤트 100% 동일 (이름만 변경)
- `GoJsonConverter.ControlTypes` 등록 결과: `"GoStepGauge"`만 등록 (`"GoStep"` 제거)

### 호환성

- **외부 design.json에 `"Type": "GoStep"`이 있으면 deserialize 시 unknown type 에러** — 사용자가 수동으로 `"GoStep"` → `"GoStepGauge"` 일괄 치환 필요
- 본 repo 내부: `controls.html` 문서만 `GoStep` 언급 (rename 시 함께 갱신), `.cs` 사용처 0건 — 코드 영향 없음
- 외부 `Test167` 프로젝트: `GoStep` 사용 0건 (검증 완료)

## Part 3. GoStepBar 신설

### 파일

- `Going.UI/Controls/GoStepBar.cs` 신규
- `Going.UI/Datas/GoStepItem.cs` 신규 (또는 GoStepBar.cs 내부)
- `Going.UI/Enums/GoStepBarMode.cs` 신규 (또는 GoStepBar.cs 내부)

### 타입

```csharp
public class GoStepItem
{
    public string? Text { get; set; }
    public string? IconString { get; set; }
}

public enum GoStepBarMode
{
    Story,  // index 1..Step 누적 채움
    Point,  // index Step만 단일 채움
}
```

### GoStepBar 클래스

`GoControl` 상속. 속성:

```csharp
[GoProperty(PCategory.Control, 0)] public List<GoStepItem> Steps { get; set; } = [];

[GoProperty(PCategory.Control, 1)]
public int Step { get; set; } = 0;          // 0=none, 1..Steps.Count=현재

[GoProperty(PCategory.Control, 2)] public GoStepBarMode Mode { get; set; } = GoStepBarMode.Story;
[GoProperty(PCategory.Control, 3)] public bool UseClick { get; set; } = false;

[GoProperty(PCategory.Control, 4)] public string DotColor { get; set; } = "Base2";
[GoProperty(PCategory.Control, 5)] public string ActiveColor { get; set; } = "Select";
[GoProperty(PCategory.Control, 6)] public string LineColor { get; set; } = "Base2";
[GoProperty(PCategory.Control, 7)] public string ActiveTextColor { get; set; } = "Fore";
[GoProperty(PCategory.Control, 8)] public string InactiveTextColor { get; set; } = "Base3";

[GoFontNameProperty(PCategory.Control, 9)] public string FontName { get; set; } = "나눔고딕";
[GoProperty(PCategory.Control, 10)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
[GoProperty(PCategory.Control, 11)] public float FontSize { get; set; } = 12f;

[GoProperty(PCategory.Control, 12)] public float DotSize { get; set; } = 18f;
[GoProperty(PCategory.Control, 13)] public float IconSize { get; set; } = 12f;
[GoProperty(PCategory.Control, 14)] public float LabelGap { get; set; } = 6f;

public event EventHandler? StepChanged;
```

`Step` setter는 `[0, Steps.Count]` 범위로 constrain하고 변경 시 `StepChanged` 발화 (`GoStep`/`GoStepGauge`와 동일 패턴).

### 렌더 규칙

**레이아웃**:
- Content rect 가로 균등 분할 (`Steps.Count` 셀, 각 셀 폭 = `Content.Width / Steps.Count`)
- 각 셀 중앙 가로 좌표에 dot 배치, dot 세로 위치는 셀 상단부 (`DotSize` 외경 = 지름)
- Dot 아래 `LabelGap` 만큼 띄우고 라벨 텍스트 (라벨은 셀 중앙 정렬)
- Connector: 인접 dot **중심**을 연결하는 직선. 그리기 순서는 connector → dot 순으로 그려 dot이 line 위에 보이게 함
- `Steps.Count == 0`: 아무것도 그리지 않음
- `Steps.Count == 1`: dot 1개만, connector 없음

**Dot 채움 규칙** (1-based index `i ∈ 1..Steps.Count`):
- Story: `i ≤ Step`이면 채움(ActiveColor), 그 외 빈 원(stroke=DotColor)
- Point: `i == Step`이면 채움(ActiveColor), 그 외 빈 원(stroke=DotColor)

**Connector**: 모든 segment를 동일 `LineColor`로 그림 (사용자 요구 — 채움 여부로 진행 표현, line은 단순)

**Icon**:
- `Steps[i-1].IconString` 있으면 dot 중앙에 아이콘 표시 (`IconSize` 적용)
- 채워진 dot의 아이콘 색: `thm.Back` (대비)
- 빈 dot의 아이콘 색: `DotColor`

**Label**:
- 채워진 dot의 라벨: `ActiveTextColor`
- 빈 dot의 라벨: `InactiveTextColor`
- `Step=0`이면 모든 dot 빈 원, 모든 라벨 비활성색

### 마우스 (UseClick=true 시)

- `OnMouseDown`: 어느 dot 위인지 hit-test (셀 단위)
- `OnMouseUp`: 동일 dot 위에서 release되면 해당 1-based index를 `Step`에 setter
- Hover 효과는 P1 범위에 포함하지 않음 (요구 없음)

### Areas

```csharp
public override Dictionary<string, SKRect> Areas()
{
    var dic = base.Areas();
    var rt = dic["Content"];
    // 셀 단위 rect는 Areas에 노출하지 않고 stepLoop 내부에서 계산
    return dic;
}
```

## 영향 범위

### 변경 파일

- `Going.UI/Controls/GoInput.cs` — Title 속성 3개 + 렌더 1줄 교체
- `Going.UI/Controls/GoValue.cs` — 동일
- `Going.UI/Controls/GoStep.cs` → `GoStepGauge.cs` (rename)
- `Going.UI/Controls/GoStepBar.cs` (신규)
- `Going.UI/Datas/GoStepItem.cs` (신규, 또는 GoStepBar.cs 내부)
- `Going.UI/Enums/GoStepBarMode.cs` (신규, 또는 GoStepBar.cs 내부)
- `Going.UI/Json/GoJsonConverter.cs` — `ControlTypes` 자동 스캔이라 코드 변경 없음 (rename만 반영)
- `docs/ui/controls.html` — `GoStep` 섹션 → `GoStepGauge`, `GoStepBar` 섹션 추가, GoInput/GoValue Title 속성 3개 추가
- `Going.UI.Tests/` — `GoStepBarTests.cs` 신규 (Step constrain, Mode 시각 검증, JSON round-trip)
- `CHANGELOG.md` — v1.2.9 항목

### 빌드 검증

- 풀 테스트 (현 71건) 회귀 0
- `GoStepBar` round-trip 테스트 신규
- 외부 (`Test167`) 의 design.json은 `GoStep` 사용 없음 확인 (이미 grep 결과 0건)

## 비고

- `GoInputText`, `GoInputNumber<T>` 등 8개 파생은 base의 OnDraw를 통해 자동으로 새 Title 동작을 받음 — 별도 수정 불필요
- `Mode` enum 값 추가 가능성은 향후로 미룸 (예: Bar fill, Numbered) — YAGNI
