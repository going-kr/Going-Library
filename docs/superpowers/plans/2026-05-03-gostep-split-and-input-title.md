# GoStep Split + Input/Value Title 확장 — 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** GoInput/GoValue Title 영역의 폰트·정렬을 본문과 분리해 지정 가능하게 하고, 기존 `GoStep`을 `GoStepGauge`로 rename하면서 절차 인디케이터 `GoStepBar`(Story/Point 모드)를 신설한다.

**Architecture:** 기존 `[GoProperty]` + Skia 직접 렌더링 패턴 그대로. Title 속성은 base에 한 번 추가해 8개 파생이 자동 상속. GoStepBar는 `GoControl` 직접 상속, `Steps: List<GoStepItem>`로 구성. 직렬화는 기존 `GoJsonConverter` 자동 스캔(`ControlTypes`)에 맡긴다.

**Tech Stack:** .NET 8, C# 12, SkiaSharp 3.x, System.Text.Json (with custom converters), xUnit.

---

## File Structure

**Modify**
- `Going.UI/Controls/GoInput.cs` — Title 속성 3개 추가 + 렌더 1줄 교체
- `Going.UI/Controls/GoValue.cs` — 동일
- `Going.UI/Controls/GoStep.cs` → `Going.UI/Controls/GoStepGauge.cs` (rename, 클래스명도 변경)
- `docs/ui/controls.html` — GoStep → GoStepGauge, GoStepBar 추가, Title 속성 3개 추가
- `CHANGELOG.md` — v1.2.9

**Create**
- `Going.UI/Controls/GoStepBar.cs` — 컨트롤 본체 (`GoStepItem`, `GoStepBarMode` 동일 파일 내 nested 또는 같은 namespace 별 type)
- `Going.UI.Tests/InputTitleTests.cs` — Title 속성 round-trip + AutoTitleFontSize 환산 검증
- `Going.UI.Tests/GoStepBarTests.cs` — Step constrain + JSON round-trip + StepChanged 발화

**Test naming convention:** xUnit `[Fact]` / `[Theory]`, `assert` via `Xunit.Assert`. Tests colocated under `Going.UI.Tests/` (no subfolder for new files in this plan, matching `SmokeTests.cs`/`GoDataGridColumnConverterTests.cs` 위치).

---

## Task 1: GoInput Title 속성 추가

**Files:**
- Modify: `Going.UI/Controls/GoInput.cs:79` (BorderWidth 다음 자리)
- Modify: `Going.UI/Controls/GoInput.cs:152` (Title 렌더 줄)

- [ ] **Step 1: BorderWidth 직후에 Title 신규 속성 3개 삽입**

`GoInput.cs` 79번 줄 (`TitleBoxDraw` 직전)에 다음 코드 삽입:

```csharp
        /// <summary>Title 영역 글꼴 크기. AutoTitleFontSize=NotUsed일 때 사용됩니다.</summary>
        [GoProperty(PCategory.Control, 20)] public float TitleFontSize { get; set; } = 12F;
        /// <summary>Title 텍스트/아이콘 정렬</summary>
        [GoProperty(PCategory.Control, 21)] public GoContentAlignment TitleContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        /// <summary>Title 영역 자동 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 22)] public GoAutoFontSize AutoTitleFontSize { get; set; } = GoAutoFontSize.NotUsed;
```

- [ ] **Step 2: Title 렌더 줄 교체**

현재 라인:
```csharp
                Util.DrawTextIcon(canvas, Title, FontName, FontStyle, fsz, IconString, isz, GoDirectionHV.Horizon, IconGap, rtTitle, cText);
```

위 1줄을 다음 2줄로 교체:
```csharp
                var tfsz = Util.FontSize(AutoTitleFontSize, rtTitle.Height) ?? TitleFontSize;
                Util.DrawTextIcon(canvas, Title, FontName, FontStyle, tfsz, IconString, isz, GoDirectionHV.Horizon, IconGap, rtTitle, cText, TitleContentAlignment);
```

- [ ] **Step 3: 빌드 확인**

```
dotnet build "D:/Project/Going/library/src/Going/Going.UI/Going.UI.csproj" -nologo -v:q
```
Expected: `오류 0개`

- [ ] **Step 4: 풀 테스트 회귀**

```
dotnet test "D:/Project/Going/library/src/Going/Going.UI.Tests/Going.UI.Tests.csproj" --nologo -v:q
```
Expected: 71/71 통과 (변경 없음)

- [ ] **Step 5: 커밋**

```
git add Going.UI/Controls/GoInput.cs
git commit -m "GoInput: Title 영역 별도 폰트/정렬 속성 신설 (TitleFontSize/TitleContentAlignment/AutoTitleFontSize)"
```

---

## Task 2: GoValue Title 속성 추가

**Files:**
- Modify: `Going.UI/Controls/GoValue.cs` (BorderWidth 직후)
- Modify: `Going.UI/Controls/GoValue.cs:140` (Title 렌더 줄)

- [ ] **Step 1: 위치 확인**

`GoValue.cs`의 `TitleBoxDraw` 직전 (line 71 근처) 확인. property index가 GoInput과 동일하게 20/21/22 가능한지 확인 후 충돌이 있으면 빈 번호로 조정.

- [ ] **Step 2: 동일 3개 속성 삽입**

`TitleBoxDraw` 라인 위에 (Task 1 Step 1과 동일 코드):

```csharp
        /// <summary>Title 영역 글꼴 크기. AutoTitleFontSize=NotUsed일 때 사용됩니다.</summary>
        [GoProperty(PCategory.Control, 20)] public float TitleFontSize { get; set; } = 12F;
        /// <summary>Title 텍스트/아이콘 정렬</summary>
        [GoProperty(PCategory.Control, 21)] public GoContentAlignment TitleContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        /// <summary>Title 영역 자동 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 22)] public GoAutoFontSize AutoTitleFontSize { get; set; } = GoAutoFontSize.NotUsed;
```

(GoValue의 기존 property index와 충돌 시 23/24/25 등으로 조정.)

- [ ] **Step 3: Title 렌더 줄 교체**

`GoValue.cs:140` 의:
```csharp
                Util.DrawTextIcon(canvas, Title, FontName, FontStyle, fsz, IconString, isz, GoDirectionHV.Horizon, IconGap, rtTitle, cText);
```
위 1줄 → 2줄로:
```csharp
                var tfsz = Util.FontSize(AutoTitleFontSize, rtTitle.Height) ?? TitleFontSize;
                Util.DrawTextIcon(canvas, Title, FontName, FontStyle, tfsz, IconString, isz, GoDirectionHV.Horizon, IconGap, rtTitle, cText, TitleContentAlignment);
```

- [ ] **Step 4: 빌드 + 풀 테스트**

```
dotnet build "D:/Project/Going/library/src/Going/Going.UI/Going.UI.csproj" -nologo -v:q
dotnet test "D:/Project/Going/library/src/Going/Going.UI.Tests/Going.UI.Tests.csproj" --nologo -v:q
```
Expected: 빌드 0 오류, 71/71 통과

- [ ] **Step 5: 커밋**

```
git add Going.UI/Controls/GoValue.cs
git commit -m "GoValue: Title 영역 별도 폰트/정렬 속성 신설"
```

---

## Task 3: Title 속성 round-trip 테스트

**Files:**
- Create: `Going.UI.Tests/InputTitleTests.cs`

- [ ] **Step 1: 테스트 작성**

```csharp
using System.Text.Json;
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Json;
using Xunit;

namespace Going.UI.Tests;

public class InputTitleTests
{
    [Fact]
    public void GoValue_TitleProps_Defaults()
    {
        var v = new GoValue();
        Assert.Equal(12F, v.TitleFontSize);
        Assert.Equal(GoContentAlignment.MiddleCenter, v.TitleContentAlignment);
        Assert.Equal(GoAutoFontSize.NotUsed, v.AutoTitleFontSize);
    }

    [Fact]
    public void GoValue_TitleProps_RoundTrip()
    {
        var src = new GoValue
        {
            TitleFontSize = 16F,
            TitleContentAlignment = GoContentAlignment.MiddleLeft,
            AutoTitleFontSize = GoAutoFontSize.M,
        };
        var json = JsonSerializer.Serialize(src, GoJsonConverter.Options);
        var back = JsonSerializer.Deserialize<GoValue>(json, GoJsonConverter.Options);

        Assert.NotNull(back);
        Assert.Equal(16F, back!.TitleFontSize);
        Assert.Equal(GoContentAlignment.MiddleLeft, back.TitleContentAlignment);
        Assert.Equal(GoAutoFontSize.M, back.AutoTitleFontSize);
    }

    [Fact]
    public void GoInputText_InheritsTitleProps()
    {
        // GoInput 파생도 동일 속성 노출 확인
        var t = new GoInputText
        {
            TitleFontSize = 14F,
            AutoTitleFontSize = GoAutoFontSize.L,
            TitleContentAlignment = GoContentAlignment.TopLeft,
        };
        var json = JsonSerializer.Serialize<IGoControl>(t, GoJsonConverter.Options);
        var back = JsonSerializer.Deserialize<IGoControl>(json, GoJsonConverter.Options) as GoInputText;

        Assert.NotNull(back);
        Assert.Equal(14F, back!.TitleFontSize);
        Assert.Equal(GoAutoFontSize.L, back.AutoTitleFontSize);
        Assert.Equal(GoContentAlignment.TopLeft, back.TitleContentAlignment);
    }
}
```

- [ ] **Step 2: 테스트 실행**

```
dotnet test "D:/Project/Going/library/src/Going/Going.UI.Tests/Going.UI.Tests.csproj" --filter "FullyQualifiedName~InputTitleTests" --nologo -v:q
```
Expected: 3/3 통과

- [ ] **Step 3: 풀 회귀**

```
dotnet test "D:/Project/Going/library/src/Going/Going.UI.Tests/Going.UI.Tests.csproj" --nologo -v:q
```
Expected: 74/74 통과

- [ ] **Step 4: 커밋**

```
git add Going.UI.Tests/InputTitleTests.cs
git commit -m "test: GoInput/GoValue Title 속성 round-trip + 상속 검증"
```

---

## Task 4: GoStep → GoStepGauge rename

**Files:**
- Rename: `Going.UI/Controls/GoStep.cs` → `Going.UI/Controls/GoStepGauge.cs`
- Modify (in renamed file): `class GoStep` → `class GoStepGauge`, XML doc 갱신

- [ ] **Step 1: 파일 rename + 클래스명 변경 (`git mv` 필수)**

```
git mv "Going.UI/Controls/GoStep.cs" "Going.UI/Controls/GoStepGauge.cs"
```

`GoStepGauge.cs` 안의 다음을 모두 치환:
- `class GoStep` → `class GoStepGauge`
- `public GoStep()` → `public GoStepGauge()`
- XML doc `<see cref="GoStep"/>` → `<see cref="GoStepGauge"/>`
- `GoStep` 클래스의 summary 코멘트도 "스텝 게이지 표시" 식으로 갱신 (의미 명확화):
  ```csharp
  /// <summary>
  /// 스텝 게이지 표시 컨트롤. 단계별 진행 상태를 막대 형태로 시각화하며 prev/next 버튼을 포함합니다.
  /// </summary>
  ```

- [ ] **Step 2: 빌드 + 풀 테스트**

```
dotnet build "D:/Project/Going/library/src/Going/Going.UI/Going.UI.csproj" -nologo -v:q
dotnet test "D:/Project/Going/library/src/Going/Going.UI.Tests/Going.UI.Tests.csproj" --nologo -v:q
```
Expected: 0 오류, 74/74 통과 (Going.UI 내부에 `GoStep` 사용처가 없는지 빌드 결과로 확인됨)

- [ ] **Step 3: ControlTypes 등록 확인**

`GoJsonConverter.ControlTypes`는 자동 스캔이라 코드 변경 없음. `"GoStepGauge"`가 등록됐는지 단위 테스트로 확인:

```
dotnet test "D:/Project/Going/library/src/Going/Going.UI.Tests/Going.UI.Tests.csproj" --filter "FullyQualifiedName~GoDataGridColumnConverterTests.Registry_NonGeneric_Registered" --nologo -v:q
```
(이미 통과하던 테스트 — 회귀 확인용. 새 ControlTypes 검증은 Task 8에서 GoStepBar와 함께 추가.)

- [ ] **Step 4: 커밋**

```
git commit -m "GoStep → GoStepGauge rename (게이지 표시 컨트롤)"
```

(`git mv`가 이미 staging에 들어가 있어 `-a` 불필요)

---

## Task 5: GoStepItem + GoStepBarMode + GoStepBar 골격

**Files:**
- Create: `Going.UI/Controls/GoStepBar.cs`

- [ ] **Step 1: 골격 작성 (속성 + 이벤트만, 렌더는 빈 OnDraw)**

```csharp
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Going.UI.Controls
{
    /// <summary>스텝 항목 (Text + Icon)</summary>
    public class GoStepItem
    {
        /// <summary>표시 텍스트</summary>
        public string? Text { get; set; }
        /// <summary>아이콘 문자열 (FontAwesome 등). 없으면 null.</summary>
        public string? IconString { get; set; }
    }

    /// <summary>GoStepBar 표시 모드</summary>
    public enum GoStepBarMode
    {
        /// <summary>지나온 단계 모두 채움 (1..Step)</summary>
        Story,
        /// <summary>현재 단계 한 개만 채움 (Step)</summary>
        Point,
    }

    /// <summary>
    /// 절차 진행 상태를 표시하는 인디케이터. dot + 라벨 + connector 직선으로 구성.
    /// Step=0이면 모든 dot 비활성, Step=N이면 Steps[N-1] 단계가 현재.
    /// </summary>
    public class GoStepBar : GoControl
    {
        #region Properties
        /// <summary>스텝 항목 목록</summary>
        [GoProperty(PCategory.Control, 0)] public List<GoStepItem> Steps { get; set; } = [];

        private int nStep = 0;
        /// <summary>현재 단계 (0=비활성, 1..Steps.Count=Steps[N-1] 활성)</summary>
        [GoProperty(PCategory.Control, 1)]
        public int Step
        {
            get => nStep;
            set
            {
                var v = Convert.ToInt32(MathTool.Constrain(value, 0, Steps.Count));
                if (nStep != v)
                {
                    nStep = v;
                    StepChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>표시 모드</summary>
        [GoProperty(PCategory.Control, 2)] public GoStepBarMode Mode { get; set; } = GoStepBarMode.Story;
        /// <summary>dot 클릭으로 Step 변경 가능 여부</summary>
        [GoProperty(PCategory.Control, 3)] public bool UseClick { get; set; } = false;

        /// <summary>비활성 dot 색 (테마 키)</summary>
        [GoProperty(PCategory.Control, 4)] public string DotColor { get; set; } = "Base2";
        /// <summary>활성 dot 색 (테마 키)</summary>
        [GoProperty(PCategory.Control, 5)] public string ActiveColor { get; set; } = "Select";
        /// <summary>connector 직선 색 (테마 키, 단일)</summary>
        [GoProperty(PCategory.Control, 6)] public string LineColor { get; set; } = "Base2";
        /// <summary>활성 라벨 색 (테마 키)</summary>
        [GoProperty(PCategory.Control, 7)] public string ActiveTextColor { get; set; } = "Fore";
        /// <summary>비활성 라벨 색 (테마 키)</summary>
        [GoProperty(PCategory.Control, 8)] public string InactiveTextColor { get; set; } = "Base3";

        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 9)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일</summary>
        [GoProperty(PCategory.Control, 10)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 11)] public float FontSize { get; set; } = 12F;

        /// <summary>dot 외경 지름</summary>
        [GoProperty(PCategory.Control, 12)] public float DotSize { get; set; } = 18F;
        /// <summary>아이콘 크기</summary>
        [GoProperty(PCategory.Control, 13)] public float IconSize { get; set; } = 12F;
        /// <summary>dot 아래 라벨 간격</summary>
        [GoProperty(PCategory.Control, 14)] public float LabelGap { get; set; } = 6F;
        #endregion

        #region Member Variable
        private int hitDownIndex = -1; // UseClick mouse 추적용 (1-based, 없으면 -1)
        #endregion

        #region Event
        /// <summary>Step 값이 변경되었을 때 발생</summary>
        public event EventHandler? StepChanged;
        #endregion

        #region Constructor
        /// <summary><see cref="GoStepBar"/>의 새 인스턴스를 초기화합니다.</summary>
        public GoStepBar()
        {
            Selectable = true;
        }
        #endregion

        #region Override (renderer는 Task 6에서 구현)
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);
            // 렌더링 구현은 Task 6
        }
        #endregion
    }
}
```

- [ ] **Step 2: 빌드**

```
dotnet build "D:/Project/Going/library/src/Going/Going.UI/Going.UI.csproj" -nologo -v:q
```
Expected: 0 오류

- [ ] **Step 3: 풀 회귀**

```
dotnet test "D:/Project/Going/library/src/Going/Going.UI.Tests/Going.UI.Tests.csproj" --nologo -v:q
```
Expected: 74/74 통과

- [ ] **Step 4: 커밋**

```
git add Going.UI/Controls/GoStepBar.cs
git commit -m "GoStepBar 신설: 속성/이벤트 골격 (렌더는 후속)"
```

---

## Task 6: GoStepBar 렌더링 구현

**Files:**
- Modify: `Going.UI/Controls/GoStepBar.cs` (`OnDraw` 본문)

- [ ] **Step 1: OnDraw 본문 작성**

`GoStepBar.cs` 의 `OnDraw` (Task 5에서 비워둔)를 다음으로 교체:

```csharp
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var n = Steps.Count;
            if (n == 0) { base.OnDraw(canvas, thm); return; }

            var cDot = thm.ToColor(DotColor);
            var cActive = thm.ToColor(ActiveColor);
            var cLine = thm.ToColor(LineColor);
            var cActiveTxt = thm.ToColor(ActiveTextColor);
            var cInactiveTxt = thm.ToColor(InactiveTextColor);
            var cBack = thm.Back;

            var cellW = rtContent.Width / n;
            var dotR = DotSize / 2F;
            var dotCY = rtContent.Top + dotR + 2F; // 약간 띄움
            // 셀 중앙 X 계산 함수
            float CenterX(int i1) => rtContent.Left + cellW * (i1 - 0.5F); // i1=1-based

            // 1) connector 먼저 그림 (dot 아래로 깔리도록)
            if (n >= 2)
            {
                using var p = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = 2F, Color = cLine };
                for (int i = 1; i < n; i++)
                {
                    var x1 = CenterX(i);
                    var x2 = CenterX(i + 1);
                    canvas.DrawLine(x1, dotCY, x2, dotCY, p);
                }
            }

            // 2) dot + icon + label
            using var pFill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            using var pStroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2F };
            for (int i = 1; i <= n; i++)
            {
                var item = Steps[i - 1];
                bool filled = Mode switch
                {
                    GoStepBarMode.Story => i <= Step,
                    GoStepBarMode.Point => i == Step,
                    _ => false,
                };

                var cx = CenterX(i);

                // dot
                if (filled)
                {
                    pFill.Color = cActive;
                    canvas.DrawCircle(cx, dotCY, dotR, pFill);
                }
                else
                {
                    pFill.Color = cBack;
                    canvas.DrawCircle(cx, dotCY, dotR, pFill);
                    pStroke.Color = cDot;
                    canvas.DrawCircle(cx, dotCY, dotR - 1F, pStroke);
                }

                // icon
                if (!string.IsNullOrEmpty(item.IconString))
                {
                    var cIcon = filled ? cBack : cDot;
                    var rtIcon = Util.FromRect(cx - IconSize / 2F, dotCY - IconSize / 2F, IconSize, IconSize);
                    Util.DrawIcon(canvas, item.IconString, IconSize, rtIcon, cIcon);
                }

                // label
                if (!string.IsNullOrEmpty(item.Text))
                {
                    var labelTop = dotCY + dotR + LabelGap;
                    var labelHeight = rtContent.Bottom - labelTop;
                    if (labelHeight > 1F)
                    {
                        var rtLabel = Util.FromRect(rtContent.Left + cellW * (i - 1), labelTop, cellW, labelHeight);
                        var cTxt = filled ? cActiveTxt : cInactiveTxt;
                        Util.DrawText(canvas, item.Text, FontName, FontStyle, FontSize, rtLabel, cTxt, GoContentAlignment.TopCenter);
                    }
                }
            }

            base.OnDraw(canvas, thm);
        }
```

- [ ] **Step 2: 빌드 확인 (rendering 컴파일)**

```
dotnet build "D:/Project/Going/library/src/Going/Going.UI/Going.UI.csproj" -nologo -v:q
```
Expected: 0 오류

- [ ] **Step 3: 커밋**

```
git add Going.UI/Controls/GoStepBar.cs
git commit -m "GoStepBar: Story/Point 모드 렌더 구현 (dot + connector + icon + label)"
```

---

## Task 7: GoStepBar 마우스 처리 (UseClick)

**Files:**
- Modify: `Going.UI/Controls/GoStepBar.cs` (`OnMouseDown`/`OnMouseUp` 추가)

- [ ] **Step 1: 마우스 hit-test 헬퍼 + 핸들러 추가**

`OnDraw` 메서드 다음에 다음 코드 삽입:

```csharp
        #region Mouse
        // 셀 단위 hit-test: x, y 가 어느 1-based 인덱스 셀에 있는지 (없으면 -1)
        int HitIndex(float x, float y)
        {
            if (Steps.Count == 0) return -1;
            var rt = Areas()["Content"];
            if (!Tools.CollisionTool.Check(rt, x, y)) return -1;
            var cellW = rt.Width / Steps.Count;
            if (cellW <= 0) return -1;
            var idx0 = (int)((x - rt.Left) / cellW);
            if (idx0 < 0) idx0 = 0;
            if (idx0 >= Steps.Count) idx0 = Steps.Count - 1;
            return idx0 + 1;
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            if (UseClick)
            {
                hitDownIndex = HitIndex(x, y);
            }
            base.OnMouseDown(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            if (UseClick && hitDownIndex >= 1)
            {
                var up = HitIndex(x, y);
                if (up == hitDownIndex)
                {
                    Step = hitDownIndex;
                }
            }
            hitDownIndex = -1;
            base.OnMouseUp(x, y, button);
        }
        #endregion
```

- [ ] **Step 2: 빌드**

```
dotnet build "D:/Project/Going/library/src/Going/Going.UI/Going.UI.csproj" -nologo -v:q
```
Expected: 0 오류

- [ ] **Step 3: 커밋**

```
git add Going.UI/Controls/GoStepBar.cs
git commit -m "GoStepBar: UseClick 마우스 처리 (down/up 동일 셀 → Step 변경)"
```

---

## Task 8: GoStepBar 테스트

**Files:**
- Create: `Going.UI.Tests/GoStepBarTests.cs`

- [ ] **Step 1: 테스트 작성**

```csharp
using System.Text.Json;
using Going.UI.Controls;
using Going.UI.Json;
using Xunit;

namespace Going.UI.Tests;

public class GoStepBarTests
{
    [Fact]
    public void Defaults_NoSteps()
    {
        var b = new GoStepBar();
        Assert.Equal(0, b.Step);
        Assert.Equal(GoStepBarMode.Story, b.Mode);
        Assert.False(b.UseClick);
        Assert.Empty(b.Steps);
    }

    [Fact]
    public void Step_ConstrainsToStepsCount()
    {
        var b = new GoStepBar();
        b.Steps.Add(new GoStepItem { Text = "A" });
        b.Steps.Add(new GoStepItem { Text = "B" });

        b.Step = 5;
        Assert.Equal(2, b.Step); // Steps.Count

        b.Step = -3;
        Assert.Equal(0, b.Step); // 0=none

        b.Step = 1;
        Assert.Equal(1, b.Step);
    }

    [Fact]
    public void StepChanged_FiresOnlyOnDelta()
    {
        var b = new GoStepBar();
        b.Steps.Add(new GoStepItem { Text = "A" });
        int fires = 0;
        b.StepChanged += (s, e) => fires++;

        b.Step = 1;
        b.Step = 1; // 동일값 — 발화 안 함
        b.Step = 0;

        Assert.Equal(2, fires);
    }

    [Fact]
    public void Json_RoundTrip_PreservesAllProps()
    {
        var src = new GoStepBar
        {
            Mode = GoStepBarMode.Point,
            UseClick = true,
            DotSize = 22F,
            FontSize = 14F,
        };
        src.Steps.Add(new GoStepItem { Text = "인터뷰", IconString = "fa-comment" });
        src.Steps.Add(new GoStepItem { Text = "계획" });
        src.Steps.Add(new GoStepItem { Text = "구현", IconString = "fa-code" });
        src.Step = 2;

        var json = JsonSerializer.Serialize(src, GoJsonConverter.Options);
        var back = JsonSerializer.Deserialize<GoStepBar>(json, GoJsonConverter.Options);

        Assert.NotNull(back);
        Assert.Equal(GoStepBarMode.Point, back!.Mode);
        Assert.True(back.UseClick);
        Assert.Equal(22F, back.DotSize);
        Assert.Equal(14F, back.FontSize);
        Assert.Equal(3, back.Steps.Count);
        Assert.Equal("인터뷰", back.Steps[0].Text);
        Assert.Equal("fa-comment", back.Steps[0].IconString);
        Assert.Null(back.Steps[1].IconString);
        Assert.Equal(2, back.Step);
    }

    [Fact]
    public void ControlTypes_Registers_GoStepGauge_And_GoStepBar()
    {
        Assert.Contains("GoStepGauge", GoJsonConverter.ControlTypes.Keys);
        Assert.Contains("GoStepBar", GoJsonConverter.ControlTypes.Keys);
        Assert.DoesNotContain("GoStep", GoJsonConverter.ControlTypes.Keys);
    }
}
```

- [ ] **Step 2: 테스트 실행**

```
dotnet test "D:/Project/Going/library/src/Going/Going.UI.Tests/Going.UI.Tests.csproj" --filter "FullyQualifiedName~GoStepBarTests" --nologo -v:q
```
Expected: 5/5 통과

- [ ] **Step 3: 풀 회귀**

```
dotnet test "D:/Project/Going/library/src/Going/Going.UI.Tests/Going.UI.Tests.csproj" --nologo -v:q
```
Expected: 79/79 통과

- [ ] **Step 4: 커밋**

```
git add Going.UI.Tests/GoStepBarTests.cs
git commit -m "test: GoStepBar 기본/Step constrain/이벤트/JSON round-trip + ControlTypes 등록 검증"
```

---

## Task 9: docs/ui/controls.html 갱신

**Files:**
- Modify: `docs/ui/controls.html` (GoStep → GoStepGauge, GoStepBar 섹션 추가, GoInput/GoValue Title 속성 3개 추가)

- [ ] **Step 1: GoStep 섹션 → GoStepGauge로 치환**

`docs/ui/controls.html` 에서:
- `id="class-GoStep"` → `id="class-GoStepGauge"`
- `data-search="gostep ...` → `data-search="gostepgauge ...`
- 표시 클래스명 `GoStep` → `GoStepGauge`
- 설명 문장 "스텝 표시 컨트롤" → "스텝 게이지 표시 컨트롤"

- [ ] **Step 2: GoStepGauge 섹션 직후에 GoStepBar 섹션 추가**

기존 GoStepGauge 카드(`<div id="class-GoStepGauge" ...>` `</div>`) 직후 다음 마크업 삽입:

```html
    <hr class="class-separator">

    <!-- Class: GoStepBar -->
    <div id="class-GoStepBar" class="class-card" data-search="gostepbar class procedure step indicator dot story point">
      <div class="class-card-header" onclick="toggleCard(this)">
        <div class="class-signature"><span class="kw">public class</span> <span class="nm">GoStepBar</span> <span class="base">: GoControl</span></div>
        <div class="class-desc">절차 진행 상태 인디케이터. dot + 라벨 + connector. Story 모드는 1..Step 누적 채움, Point 모드는 Step 한 개만 채움.</div>
      </div>
      <div class="class-card-body">
        <div class="member-section">
          <div class="member-section-title">Properties</div>
          <table class="api-table">
            <tr><td class="col-type">List&lt;GoStepItem&gt;</td><td class="col-name">Steps <span class="accessor">get set</span></td><td class="col-desc">스텝 항목 목록 (Text+IconString)</td></tr>
            <tr><td class="col-type">int</td><td class="col-name">Step <span class="accessor">get set</span></td><td class="col-desc">현재 단계 (0=비활성, 1..Steps.Count). 변경 시 StepChanged 발화</td></tr>
            <tr><td class="col-type">GoStepBarMode</td><td class="col-name">Mode <span class="accessor">get set</span></td><td class="col-desc">Story | Point</td></tr>
            <tr><td class="col-type">bool</td><td class="col-name">UseClick <span class="accessor">get set</span></td><td class="col-desc">dot 클릭으로 Step 변경 (기본 false)</td></tr>
            <tr><td class="col-type">string</td><td class="col-name">DotColor / ActiveColor / LineColor / ActiveTextColor / InactiveTextColor <span class="accessor">get set</span></td><td class="col-desc">테마 색상 키 (기본 Base2 / Select / Base2 / Fore / Base3)</td></tr>
            <tr><td class="col-type">float</td><td class="col-name">FontSize / DotSize / IconSize / LabelGap <span class="accessor">get set</span></td><td class="col-desc">기본 12 / 18 / 12 / 6</td></tr>
          </table>
        </div>
        <div class="member-section">
          <div class="member-section-title">Events</div>
          <table class="api-table">
            <tr><td class="event-args">EventHandler</td><td class="col-method-name">StepChanged</td><td class="col-desc">Step 값이 변경되었을 때 발생</td></tr>
          </table>
        </div>
      </div>
    </div>
```

- [ ] **Step 3: GoInput / GoValue Title 속성 3개 추가**

GoInput/GoValue 섹션의 Properties 표 안에 다음 행 3개 삽입 (TitleBoxDraw 행 위에):

```html
            <tr><td class="col-type">float</td><td class="col-name">TitleFontSize <span class="accessor">get set</span></td><td class="col-desc">Title 영역 글꼴 크기 (기본 12)</td></tr>
            <tr><td class="col-type">GoContentAlignment</td><td class="col-name">TitleContentAlignment <span class="accessor">get set</span></td><td class="col-desc">Title 텍스트/아이콘 정렬 (기본 MiddleCenter)</td></tr>
            <tr><td class="col-type">GoAutoFontSize</td><td class="col-name">AutoTitleFontSize <span class="accessor">get set</span></td><td class="col-desc">Title 자동 글꼴 크기 프리셋 (기본 NotUsed)</td></tr>
```

- [ ] **Step 4: 커밋**

```
git add docs/ui/controls.html
git commit -m "docs: GoStep → GoStepGauge, GoStepBar 섹션 추가, GoInput/GoValue Title 속성 3개 반영"
```

---

## Task 10: CHANGELOG + 버전 태그

**Files:**
- Modify: `CHANGELOG.md` (v1.2.9 항목 신규)

- [ ] **Step 1: 최근 v1.2.x 항목 형식 확인 후 v1.2.9 항목 추가**

```
git log --oneline -5
```

(이전 커밋 메시지 형식에 맞춰 작성)

CHANGELOG.md 상단에 다음 추가:

```markdown
## v1.2.9 (2026-05-03)

- GoInput / GoValue Title 영역 별도 폰트 속성 신설: `TitleFontSize`, `TitleContentAlignment`, `AutoTitleFontSize`. (기존엔 본문 fsz/MiddleCenter 고정)
- `GoStep` → `GoStepGauge` rename. 외부 design.json은 `"Type": "GoStep"` → `"GoStepGauge"` 수동 치환 필요.
- `GoStepBar` 신설: 절차 진행 인디케이터. `Steps: List<GoStepItem>`, `Step` 1-based (0=비활성), `Mode = Story|Point`. `UseClick=true`로 클릭 토글 가능.
```

- [ ] **Step 2: 풀 회귀**

```
dotnet test "D:/Project/Going/library/src/Going/Going.UI.Tests/Going.UI.Tests.csproj" --nologo -v:q
```
Expected: 79/79 통과

- [ ] **Step 3: 커밋 + 태그 + push**

```
git add CHANGELOG.md
git commit -m "v1.2.9: GoStep split (Gauge/Bar) + Input/Value Title 영역 별도 폰트 속성"
git tag v1.2.9
git push origin master
git push origin v1.2.9
```

---

## Self-Review Checklist (실행 전 본인 확인용)

- [ ] Task 4 — `GoStep.cs` 안에서 `GoStep` 외에 `Selectable = true` 같은 ctor 호출은 그대로 유지되는지 확인
- [ ] Task 5 — `GoStepItem`, `GoStepBarMode`가 `Going.UI.Controls` namespace에 존재해도 사용에 문제 없는지 (다른 `Going.UI.Datas`에 있던 enum/class와 일관 — 필요시 옮겨도 무방)
- [ ] Task 6 — `Util.DrawIcon` 시그니처 (4-arg vs 5-arg with stroke) 호출부 컴파일 통과 확인. 실패 시 시그니처 grep 후 맞춤
- [ ] Task 6 — `MathTool.Constrain` 시그니처가 int/float/double 어느 오버로드인지 — 현 GoStep에서 `Convert.ToInt32(MathTool.Constrain(value, 0, StepCount-1))` 패턴 그대로 따라감
- [ ] Task 7 — `GoMouseButton`, `Tools.CollisionTool.Check` 사용 — 기존 컨트롤 패턴 그대로
- [ ] Task 8 — `GoStepBar.Steps` 직렬화는 `List<GoStepItem>` 단순 컬렉션 (Polymorphic 아님) — 기본 STJ로 충분
