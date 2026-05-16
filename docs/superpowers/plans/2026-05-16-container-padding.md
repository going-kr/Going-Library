# 6개 컨테이너 Padding 속성 추가 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `GoPage`/`GoWindow`/`GoSideBar`/`GoGroupBox`/`GoPanel`/`GoBoxPanel` 6개 컨테이너에 `Padding` 속성을 추가해 자식 컨트롤 영역을 내부 여백만큼 인셋시킨다.

**Architecture:** 각 컨테이너에 `[GoProperty]` 부착된 `GoPadding Padding` 추가. `PanelBounds`를 `Util.FromRect(기존_영역, Padding)`로 인셋. `OnLayout`을 `PanelBounds.Width/Height` 기반으로 변경(3개는 기존 수정, 3개는 신규 오버라이드). 기본값 `new GoPadding()` = 0,0,0,0 — 기존 동작 영향 없음.

**Tech Stack:** .NET 8, C# 12, SkiaSharp, xUnit 2.5.3.

**Spec:** [docs/superpowers/specs/2026-05-16-container-padding-design.md](../specs/2026-05-16-container-padding-design.md)

**Build/Test 명령:**
- 빌드: `dotnet build Going.sln -c Debug --nologo` (working dir = `D:\Project\Going\library\src\Going`)
- 단일 테스트: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~<name>"`
- 전체 Containers 테스트: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~Going.UI.Tests.Containers"`

---

## Files Plan

| 작업 | 경로 | 책임 |
|---|---|---|
| Modify | `Going.UI/Containers/GoBoxPanel.cs` | Padding 추가 + PanelBounds/OnLayout 신규 오버라이드 |
| Modify | `Going.UI/Design/GoPage.cs` | 동상 (Page 전용) |
| Modify | `Going.UI/Design/GoSideBar.cs` | 동상 (SideBar 전용 — 같은 파일 내 다른 클래스인 GoTitleBar/GoFooter는 손대지 않음) |
| Modify | `Going.UI/Containers/GoPanel.cs` | Padding 추가 + 기존 PanelBounds/OnLayout 수정 |
| Modify | `Going.UI/Containers/GoGroupBox.cs` | 동상 |
| Modify | `Going.UI/Design/GoWindow.cs` | 동상 |
| Create | `Going.UI.Tests/Containers/PaddingTests.cs` | 6×PanelBounds 인셋, 6×Gudx 라운드트립, 1×Dock=Fill 위치 (총 13 테스트) |

---

## Task 1: 사전 준비 — Spec 및 Plan 문서 커밋

**Files:**
- Commit: `docs/superpowers/specs/2026-05-16-container-padding-design.md` (이미 디스크에 존재, untracked)
- Commit: `docs/superpowers/plans/2026-05-16-container-padding.md` (지금 보고 있는 파일)

- [ ] **Step 1: 두 문서 추가 + 커밋**

```bash
git add docs/superpowers/specs/2026-05-16-container-padding-design.md docs/superpowers/plans/2026-05-16-container-padding.md
git commit -m "$(cat <<'EOF'
docs: 6개 컨테이너 Padding 속성 추가 spec/plan

GoPage/GoWindow/GoSideBar/GoGroupBox/GoPanel/GoBoxPanel 6개에
Padding 프로퍼티를 추가하는 작업의 설계 문서와 구현 플랜.
EOF
)"
```

---

## Task 2: GoBoxPanel — 가장 단순한 컨테이너부터 (exemplar)

GoBoxPanel은 기존 PanelBounds/OnLayout 오버라이드 없음 + 추가 영역 계산 없음 → 6개 중 가장 단순. 여기서 패턴 + 테스트 인프라 구축.

**Files:**
- Create: `Going.UI.Tests/Containers/PaddingTests.cs`
- Modify: `Going.UI/Containers/GoBoxPanel.cs:19-66` (Padding 프로퍼티 + PanelBounds + OnLayout 오버라이드 추가)

- [ ] **Step 1: 실패 테스트 작성 (테스트 파일 신규 생성, 3 테스트)**

Create `Going.UI.Tests/Containers/PaddingTests.cs`:

```csharp
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests.Containers;

public class PaddingTests
{
    [Fact]
    public void GoBoxPanel_PanelBounds_insetByPadding()
    {
        var p = new GoBoxPanel { Left = 0, Top = 0, Width = 200, Height = 150 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;

        Assert.Equal(10f, pb.Left);
        Assert.Equal(5f, pb.Top);
        Assert.Equal(200f - 20f, pb.Right);
        Assert.Equal(150f - 15f, pb.Bottom);
    }

    [Fact]
    public void GoBoxPanel_Padding_gudxRoundTrips()
    {
        var p = new GoBoxPanel { Name = "bp" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (GoBoxPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }

    [Fact]
    public void GoBoxPanel_DockFillChild_respectsPadding()
    {
        var p = new GoBoxPanel { Left = 0, Top = 0, Width = 200, Height = 150 };
        p.Padding = new GoPadding(10, 10, 10, 10);

        var child = new GoButton
        {
            Dock = GoDockStyle.Fill,
            Margin = new GoPadding(0, 0, 0, 0)
        };
        p.Childrens.Add(child);

        // FireDraw가 OnDraw → OnLayout 트리거. 실제 픽셀은 안 그리지만 레이아웃 계산은 일어남.
        using var bmp = new SKBitmap(1, 1);
        using var canvas = new SKCanvas(bmp);
        p.FireDraw(canvas, GoTheme.DarkTheme);

        Assert.Equal(0f, child.Left);
        Assert.Equal(0f, child.Top);
        Assert.Equal(180f, child.Right);   // PanelBounds.Width = 200 - 10 - 10 = 180
        Assert.Equal(130f, child.Bottom);  // PanelBounds.Height = 150 - 10 - 10 = 130
    }
}
```

- [ ] **Step 2: 빌드 + 테스트 실행 → FAIL 확인**

Run:
```
dotnet build Going.sln -c Debug --nologo
```
Expected: build FAIL — `GoBoxPanel.Padding` 프로퍼티 미존재.

(빌드 자체가 실패하므로 테스트 실행 단계는 생략. 빌드 통과 후 → Step 4에서 실패 어서션 확인.)

- [ ] **Step 3: GoBoxPanel에 Padding + PanelBounds + OnLayout 추가**

Edit `Going.UI/Containers/GoBoxPanel.cs`. 파일 상단 using 블록 확인 — `Going.UI.Datas`(GoPadding) 및 `System.Linq`(Childrens.Where)가 있는지. 기존:

```csharp
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
```

`Going.UI.Datas`가 누락 — 추가:

```csharp
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
```

기존 Elevation 프로퍼티 ([GoBoxPanel.cs:49-53](src/Going/Going.UI/Containers/GoBoxPanel.cs:49)) 다음에 Padding 프로퍼티 추가:

```csharp
        /// <summary>
        /// Elevation 단계 (0~5). 0이면 그림자 없음, 1~5는 점진적으로 강한 드롭 그림자가 외곽에 그려져 카드가 떠있는 효과를 만듭니다.
        /// 그림자는 부모 컨테이너에서 그려지므로 카드 외부 여백이 충분해야 잘리지 않습니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public int Elevation { get; set; } = 0;

        /// <summary>
        /// 자식 컨트롤 영역의 내부 여백을 가져오거나 설정합니다.
        /// 자식들은 이 값만큼 안쪽으로 인셋된 영역에 배치됩니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public GoPadding Padding { get; set; } = new();

        /// <summary>
        /// 자식 컨트롤이 배치되는 패널 영역을 가져옵니다. Padding만큼 인셋됩니다.
        /// </summary>
        [JsonIgnore] public override SKRect PanelBounds
            => Util.FromRect(Util.FromRect(0, 0, Width, Height), Padding);
        #endregion
```

(기존 `#endregion`이 Properties region을 닫는 위치 그대로 유지.)

기존 Constructor region ([GoBoxPanel.cs:56-67](src/Going/Going.UI/Containers/GoBoxPanel.cs:56)) 직전 또는 직후, 또는 Override region 안에 OnLayout 추가. 깔끔하게 Override region 안 OnDraw 다음에:

```csharp
        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cBox = thm.ToColor(BoxColor);
            var cBorder = thm.ToColor(BorderColor);
            var rts = Areas();
            var rtBox = rts["Content"];

            if (BackgroundDraw) Util.DrawBox(canvas, rtBox, cBox, cBorder, Round, thm.Corner, true, BorderWidth);

            base.OnDraw(canvas, thm);
        }

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
        #endregion
```

- [ ] **Step 4: 빌드 + 테스트 실행 → PASS 확인**

```
dotnet build Going.sln -c Debug --nologo
dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~PaddingTests.GoBoxPanel"
```
Expected: build PASS, 3 tests PASS (GoBoxPanel_PanelBounds_insetByPadding, GoBoxPanel_Padding_gudxRoundTrips, GoBoxPanel_DockFillChild_respectsPadding).

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Containers/GoBoxPanel.cs Going.UI.Tests/Containers/PaddingTests.cs
git commit -m "$(cat <<'EOF'
GoBoxPanel: Padding 프로퍼티 추가

자식 컨트롤 영역을 Padding만큼 안쪽으로 인셋. PanelBounds + OnLayout
오버라이드 추가. 기본값 (0,0,0,0)으로 기존 동작 영향 없음.

PanelBounds 인셋, Gudx 라운드트립, Dock=Fill 위치 검증 테스트 추가.
EOF
)"
```

---

## Task 3: GoPage — 단순 컨테이너 두 번째

**Files:**
- Modify: `Going.UI.Tests/Containers/PaddingTests.cs` (테스트 2개 추가)
- Modify: `Going.UI/Design/GoPage.cs:19-43` (Padding + PanelBounds + OnLayout 추가)

- [ ] **Step 1: 실패 테스트 2개 추가**

Edit `Going.UI.Tests/Containers/PaddingTests.cs`. 기존 클래스 마지막 `}` 직전(다른 `[Fact]` 끝나는 위치) 다음 2개 메서드 추가:

```csharp
    [Fact]
    public void GoPage_PanelBounds_insetByPadding()
    {
        var p = new Going.UI.Design.GoPage { Left = 0, Top = 0, Width = 800, Height = 600 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;

        Assert.Equal(10f, pb.Left);
        Assert.Equal(5f, pb.Top);
        Assert.Equal(800f - 20f, pb.Right);
        Assert.Equal(600f - 15f, pb.Bottom);
    }

    [Fact]
    public void GoPage_Padding_gudxRoundTrips()
    {
        var p = new Going.UI.Design.GoPage { Name = "pg" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (Going.UI.Design.GoPage)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }
```

- [ ] **Step 2: 빌드 → FAIL 확인**

```
dotnet build Going.sln -c Debug --nologo
```
Expected: build FAIL — `GoPage.Padding` 미존재.

- [ ] **Step 3: GoPage에 Padding + PanelBounds + OnLayout 추가**

Edit `Going.UI/Design/GoPage.cs`. using 블록 확인 — `Going.UI.Datas`, `Going.UI.Enums` 누락 확인. 기존:

```csharp
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
```

변경 후 (`Going.UI.Datas`, `Going.UI.Enums` 추가):

```csharp
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
```

기존 `BackgroundImage` 프로퍼티 ([GoPage.cs:27-30](src/Going/Going.UI/Design/GoPage.cs:27)) 다음에 Padding + PanelBounds 추가:

```csharp
        /// <summary>
        /// 배경 이미지 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 5)] public string? BackgroundImage { get; set; }

        /// <summary>
        /// 자식 컨트롤 영역의 내부 여백을 가져오거나 설정합니다.
        /// 자식들은 이 값만큼 안쪽으로 인셋된 영역에 배치됩니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public GoPadding Padding { get; set; } = new();

        /// <summary>
        /// 자식 컨트롤이 배치되는 패널 영역을 가져옵니다. Padding만큼 인셋됩니다.
        /// </summary>
        [JsonIgnore] public override SKRect PanelBounds
            => Util.FromRect(Util.FromRect(0, 0, Width, Height), Padding);
        #endregion
```

기존 Override region의 OnDraw 다음에 OnLayout 추가:

```csharp
        #region Override
        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            OnBackgroundDraw(canvas, thm);

            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        protected virtual void OnBackgroundDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            if (Design != null && BackgroundImage != null)
            {
                var bg = Design.GetImage(BackgroundImage);
                if (bg != null && bg.Count > 0) canvas.DrawImage(bg[0], rtContent, Util.Sampling);
            }
        }
        #endregion

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
        #endregion
```

- [ ] **Step 4: 빌드 + 테스트 → PASS**

```
dotnet build Going.sln -c Debug --nologo
dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~PaddingTests.GoPage"
```
Expected: build PASS, 2 tests PASS.

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Design/GoPage.cs Going.UI.Tests/Containers/PaddingTests.cs
git commit -m "$(cat <<'EOF'
GoPage: Padding 프로퍼티 추가

자식 컨트롤 영역을 Padding만큼 인셋. PanelBounds + OnLayout 오버라이드 추가.
PanelBounds 인셋 및 Gudx 라운드트립 검증 테스트 추가.
EOF
)"
```

---

## Task 4: GoSideBar — 단순 컨테이너 세 번째

`GoSideBar.cs` 파일에는 `GoTitleBar`, `GoSideBar`, `GoFooter` 3개 클래스가 같이 정의됨. `GoSideBar` 클래스에만 적용.

**Files:**
- Modify: `Going.UI.Tests/Containers/PaddingTests.cs` (테스트 2개 추가)
- Modify: `Going.UI/Design/GoSideBar.cs` (GoSideBar 클래스에만 Padding + PanelBounds + OnLayout 추가)

- [ ] **Step 1: 실패 테스트 2개 추가**

Edit `Going.UI.Tests/Containers/PaddingTests.cs`. 기존 클래스 마지막 `}` 직전 다음 2개 추가:

```csharp
    [Fact]
    public void GoSideBar_PanelBounds_insetByPadding()
    {
        var p = new Going.UI.Design.GoSideBar { Left = 0, Top = 0, Width = 150, Height = 600 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;

        Assert.Equal(10f, pb.Left);
        Assert.Equal(5f, pb.Top);
        Assert.Equal(150f - 20f, pb.Right);
        Assert.Equal(600f - 15f, pb.Bottom);
    }

    [Fact]
    public void GoSideBar_Padding_gudxRoundTrips()
    {
        var p = new Going.UI.Design.GoSideBar { Name = "sb" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (Going.UI.Design.GoSideBar)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }
```

- [ ] **Step 2: 빌드 → FAIL**

```
dotnet build Going.sln -c Debug --nologo
```
Expected: FAIL — `GoSideBar.Padding` 미존재.

- [ ] **Step 3: GoSideBar에 Padding + PanelBounds + OnLayout 추가**

Edit `Going.UI/Design/GoSideBar.cs`. using 블록 확인 — `Going.UI.Datas`, `Going.UI.Enums`(GoDockStyle), `Going.UI.Utils`(Util) 필요. 기존:

```csharp
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
```

`Going.UI.Datas` 누락 — 추가:

```csharp
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
```

`GoSideBar` 클래스 ([GoSideBar.cs:221](src/Going/Going.UI/Design/GoSideBar.cs:221))의 Childrens 프로퍼티([GoSideBar.cs:245-249](src/Going/Going.UI/Design/GoSideBar.cs:245)) 다음, Expand 프로퍼티([GoSideBar.cs:251-282](src/Going/Going.UI/Design/GoSideBar.cs:251)) 직전 또는 직후에 Padding + PanelBounds 추가:

기존 Expand 프로퍼티 다음 (Properties region 마지막):

```csharp
        /// <summary>
        /// 사이드바 확장 여부를 가져오거나 설정합니다. Fixed가 true이면 항상 true입니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)]
        [JsonIgnore]
        public bool Expand
        {
            // ... 기존 코드 ...
        }

        /// <summary>
        /// 자식 컨트롤 영역의 내부 여백을 가져오거나 설정합니다.
        /// 자식들은 이 값만큼 안쪽으로 인셋된 영역에 배치됩니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public GoPadding Padding { get; set; } = new();

        /// <summary>
        /// 자식 컨트롤이 배치되는 패널 영역을 가져옵니다. Padding만큼 인셋됩니다.
        /// </summary>
        [JsonIgnore] public override SKRect PanelBounds
            => Util.FromRect(Util.FromRect(0, 0, Width, Height), Padding);
        #endregion
```

(Properties region 닫는 `#endregion` 그대로.)

`GoSideBar` 클래스는 Override region이 따로 없음 (Constructor 다음 바로 클래스 끝). Constructor region 다음에 새 Override region 추가:

```csharp
        #region Constructor
        // ... 기존 ctor 2개 ...
        #endregion

        #region Override
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
        #endregion
    }
```

(클래스 닫는 `}` 그대로.)

- [ ] **Step 4: 빌드 + 테스트 → PASS**

```
dotnet build Going.sln -c Debug --nologo
dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~PaddingTests.GoSideBar"
```
Expected: build PASS, 2 tests PASS.

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Design/GoSideBar.cs Going.UI.Tests/Containers/PaddingTests.cs
git commit -m "$(cat <<'EOF'
GoSideBar: Padding 프로퍼티 추가

자식 컨트롤 영역을 Padding만큼 인셋. PanelBounds + OnLayout 오버라이드 추가.
같은 파일 내 GoTitleBar/GoFooter는 변경 없음.
PanelBounds 인셋 및 Gudx 라운드트립 검증 테스트 추가.
EOF
)"
```

---

## Task 5: GoPanel — 기존 PanelBounds/OnLayout 수정

기존 오버라이드를 수정. PanelBounds는 `Areas()["Panel"]`을 Padding으로 인셋. OnLayout은 첫 줄을 `PanelBounds`로 변경.

**Files:**
- Modify: `Going.UI.Tests/Containers/PaddingTests.cs` (테스트 2개 추가)
- Modify: `Going.UI/Containers/GoPanel.cs:115-118` (PanelBounds), `:195-215` (OnLayout), Properties 영역 (Padding 추가)

- [ ] **Step 1: 실패 테스트 2개 추가**

Edit `Going.UI.Tests/Containers/PaddingTests.cs`. 기존 클래스 마지막 `}` 직전 다음 2개 추가:

```csharp
    [Fact]
    public void GoPanel_PanelBounds_insetByPadding()
    {
        // GoPanel: Title 영역(TitleHeight 기본 30px)을 잘라낸 후 Panel 영역에서 Padding 인셋
        var p = new GoPanel { Left = 0, Top = 0, Width = 200, Height = 150 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;
        var panelArea = p.Areas()["Panel"];

        Assert.Equal(panelArea.Left + 10f, pb.Left);
        Assert.Equal(panelArea.Top + 5f, pb.Top);
        Assert.Equal(panelArea.Right - 20f, pb.Right);
        Assert.Equal(panelArea.Bottom - 15f, pb.Bottom);
    }

    [Fact]
    public void GoPanel_Padding_gudxRoundTrips()
    {
        var p = new GoPanel { Name = "pnl" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (GoPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }
```

- [ ] **Step 2: 빌드 → FAIL**

```
dotnet build Going.sln -c Debug --nologo
```
Expected: FAIL — `GoPanel.Padding` 미존재.

- [ ] **Step 3: GoPanel 수정 — using, Padding 추가, PanelBounds 수정, OnLayout 수정**

Edit `Going.UI/Containers/GoPanel.cs`. using 블록 확인 — `Going.UI.Datas` 누락 확인. 파일 첫 줄들:

```csharp
// 기존 using 블록 그대로 시작...
```

위쪽에 `using Going.UI.Datas;`가 이미 있는지 확인. 없으면 추가 (알파벳 순 — `Going.UI.Controls` 다음).

기존 Elevation 프로퍼티 ([GoPanel.cs:104-108](src/Going/Going.UI/Containers/GoPanel.cs:104)) 다음에 Padding 추가:

```csharp
        /// <summary>
        /// Elevation 단계 (0~5). 0이면 그림자 없음, 1~5는 점진적으로 강한 드롭 그림자가 외곽에 그려져 카드가 떠있는 효과를 만듭니다.
        /// 그림자는 부모 컨테이너에서 그려지므로 카드 외부 여백이 충분해야 잘리지 않습니다.
        /// </summary>
        [GoProperty(PCategory.Control, 17)] public int Elevation { get; set; } = 0;

        /// <summary>
        /// 자식 컨트롤 영역의 내부 여백을 가져오거나 설정합니다.
        /// 자식들은 이 값만큼 안쪽으로 인셋된 영역에 배치됩니다.
        /// </summary>
        [GoProperty(PCategory.Control, 18)] public GoPadding Padding { get; set; } = new();
```

기존 PanelBounds ([GoPanel.cs:115-118](src/Going/Going.UI/Containers/GoPanel.cs:115)) 수정:

```csharp
        // 변경 전
        [JsonIgnore] public override SKRect PanelBounds => Areas()["Panel"];

        // 변경 후
        [JsonIgnore] public override SKRect PanelBounds => Util.FromRect(Areas()["Panel"], Padding);
```

기존 OnLayout ([GoPanel.cs:195-215](src/Going/Going.UI/Containers/GoPanel.cs:195)) 첫 줄 수정:

```csharp
        // 변경 전
        protected override void OnLayout()
        {
            var rts = Areas();
            var rtPanel = rts["Panel"];
            // ...

        // 변경 후
        protected override void OnLayout()
        {
            var rtPanel = PanelBounds;
            // ...
```

`rts` 지역 변수는 안 쓰면 제거. 변경 후 전체 OnLayout:

```csharp
        protected override void OnLayout()
        {
            var rtPanel = PanelBounds;

            var nofills = Childrens.Where(c => c.Dock != GoDockStyle.Fill).ToList();
            var fills = Childrens.Where(c => c.Dock == GoDockStyle.Fill).ToList();
            var bnds = Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height); //Util.FromRect(rtPanel);
            foreach (var control in nofills) bnds = ApplyDocking(control, bnds);
            foreach (var c in fills)
            {
                c.Left = bnds.Left + c.Margin.Left;
                c.Top = bnds.Top + c.Margin.Top;
                c.Right = bnds.Right - c.Margin.Right;
                c.Bottom = bnds.Bottom - c.Margin.Bottom;
            }

            //base.OnLayout();
        }
```

- [ ] **Step 4: 빌드 + 테스트 → PASS**

```
dotnet build Going.sln -c Debug --nologo
dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~PaddingTests.GoPanel"
```
Expected: build PASS, 2 tests PASS.

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Containers/GoPanel.cs Going.UI.Tests/Containers/PaddingTests.cs
git commit -m "$(cat <<'EOF'
GoPanel: Padding 프로퍼티 추가

기존 PanelBounds를 Areas()['Panel']에서 Padding 인셋한 영역으로 변경.
OnLayout이 PanelBounds를 사용하도록 수정.
PanelBounds 인셋 및 Gudx 라운드트립 검증 테스트 추가.
EOF
)"
```

---

## Task 6: GoGroupBox — 기존 PanelBounds/OnLayout 수정

**Files:**
- Modify: `Going.UI.Tests/Containers/PaddingTests.cs` (테스트 2개 추가)
- Modify: `Going.UI/Containers/GoGroupBox.cs:90-93` (PanelBounds), `:173-194` (OnLayout), Properties 영역 (Padding 추가)

- [ ] **Step 1: 실패 테스트 2개 추가**

Edit `Going.UI.Tests/Containers/PaddingTests.cs`. 마지막 `}` 직전 다음 2개 추가:

```csharp
    [Fact]
    public void GoGroupBox_PanelBounds_insetByPadding()
    {
        var p = new GoGroupBox { Left = 0, Top = 0, Width = 200, Height = 150 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;
        var panelArea = p.Areas()["Panel"];

        Assert.Equal(panelArea.Left + 10f, pb.Left);
        Assert.Equal(panelArea.Top + 5f, pb.Top);
        Assert.Equal(panelArea.Right - 20f, pb.Right);
        Assert.Equal(panelArea.Bottom - 15f, pb.Bottom);
    }

    [Fact]
    public void GoGroupBox_Padding_gudxRoundTrips()
    {
        var p = new GoGroupBox { Name = "gb" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (GoGroupBox)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }
```

- [ ] **Step 2: 빌드 → FAIL**

```
dotnet build Going.sln -c Debug --nologo
```
Expected: FAIL — `GoGroupBox.Padding` 미존재.

- [ ] **Step 3: GoGroupBox 수정 — using, Padding, PanelBounds, OnLayout**

Edit `Going.UI/Containers/GoGroupBox.cs`. using 블록 — `Going.UI.Datas`가 있는지 확인. 기존:

```csharp
using Going.UI.Controls;
using Going.UI.Datas;
```

`Going.UI.Datas`는 이미 있음 (line 2) ✓.

기존 BorderWidth 프로퍼티 ([GoGroupBox.cs:79-82](src/Going/Going.UI/Containers/GoGroupBox.cs:79)) 다음에 Padding 추가:

```csharp
        /// <summary>
        /// 테두리 두께를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 12)] public float BorderWidth { get; set; } = 1.5f;

        /// <summary>
        /// 자식 컨트롤 영역의 내부 여백을 가져오거나 설정합니다.
        /// 자식들은 이 값만큼 안쪽으로 인셋된 영역에 배치됩니다.
        /// </summary>
        [GoProperty(PCategory.Control, 13)] public GoPadding Padding { get; set; } = new();
```

(BorderWidth는 직전 커밋에서 1.5F로 변경되었음 — 본 plan의 prep 커밋. 참고용.)

기존 PanelBounds ([GoGroupBox.cs:90-93](src/Going/Going.UI/Containers/GoGroupBox.cs:90)) 수정:

```csharp
        // 변경 전
        [JsonIgnore] public override SKRect PanelBounds => Areas()["Panel"];

        // 변경 후
        [JsonIgnore] public override SKRect PanelBounds => Util.FromRect(Areas()["Panel"], Padding);
```

기존 OnLayout ([GoGroupBox.cs:173-194](src/Going/Going.UI/Containers/GoGroupBox.cs:173)) 수정:

```csharp
        // 변경 후 전체
        protected override void OnLayout()
        {
            var rtPanel = PanelBounds;

            var nofills = Childrens.Where(c => c.Dock != GoDockStyle.Fill).ToList();
            var fills = Childrens.Where(c => c.Dock == GoDockStyle.Fill).ToList();
            var bnds = Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height); //Util.FromRect(rtPanel);
            foreach (var control in nofills) bnds = ApplyDocking(control, bnds);
            foreach (var c in fills)
            {
                c.Left = bnds.Left + c.Margin.Left;
                c.Top = bnds.Top + c.Margin.Top;
                c.Right = bnds.Right - c.Margin.Right;
                c.Bottom = bnds.Bottom - c.Margin.Bottom;
            }

            //base.OnLayout();
        }
```

- [ ] **Step 4: 빌드 + 테스트 → PASS**

```
dotnet build Going.sln -c Debug --nologo
dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~PaddingTests.GoGroupBox"
```
Expected: build PASS, 2 tests PASS.

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Containers/GoGroupBox.cs Going.UI.Tests/Containers/PaddingTests.cs
git commit -m "$(cat <<'EOF'
GoGroupBox: Padding 프로퍼티 추가

기존 PanelBounds를 Padding 인셋된 Panel 영역으로 변경. OnLayout이
PanelBounds를 사용하도록 수정.
PanelBounds 인셋 및 Gudx 라운드트립 검증 테스트 추가.
EOF
)"
```

---

## Task 7: GoWindow — 기존 PanelBounds/OnLayout 수정

**Files:**
- Modify: `Going.UI.Tests/Containers/PaddingTests.cs` (테스트 2개 추가)
- Modify: `Going.UI/Design/GoWindow.cs:87-90` (PanelBounds), `:188-209` (OnLayout), Properties 영역 (Padding 추가)

- [ ] **Step 1: 실패 테스트 2개 추가**

Edit `Going.UI.Tests/Containers/PaddingTests.cs`. 마지막 `}` 직전 다음 2개 추가:

```csharp
    [Fact]
    public void GoWindow_PanelBounds_insetByPadding()
    {
        var p = new Going.UI.Design.GoWindow { Left = 0, Top = 0, Width = 400, Height = 300 };
        p.Padding = new GoPadding(10, 5, 20, 15);

        var pb = p.PanelBounds;
        var panelArea = p.Areas()["Panel"];

        Assert.Equal(panelArea.Left + 10f, pb.Left);
        Assert.Equal(panelArea.Top + 5f, pb.Top);
        Assert.Equal(panelArea.Right - 20f, pb.Right);
        Assert.Equal(panelArea.Bottom - 15f, pb.Bottom);
    }

    [Fact]
    public void GoWindow_Padding_gudxRoundTrips()
    {
        var p = new Going.UI.Design.GoWindow { Name = "wnd" };
        p.Padding = new GoPadding(3, 7, 11, 13);

        var xml = GoGudxConverter.SerializeControl(p);
        var restored = (Going.UI.Design.GoWindow)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3f, restored.Padding.Left);
        Assert.Equal(7f, restored.Padding.Top);
        Assert.Equal(11f, restored.Padding.Right);
        Assert.Equal(13f, restored.Padding.Bottom);
    }
```

- [ ] **Step 2: 빌드 → FAIL**

```
dotnet build Going.sln -c Debug --nologo
```
Expected: FAIL — `GoWindow.Padding` 미존재.

- [ ] **Step 3: GoWindow 수정**

Edit `Going.UI/Design/GoWindow.cs`. using 블록 확인 — `Going.UI.Datas`가 있는지. 기존:

```csharp
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
```

이미 `Going.UI.Datas` 포함됨 ✓.

기존 TitleHeight 프로퍼티 ([GoWindow.cs:76-79](src/Going/Going.UI/Design/GoWindow.cs:76)) 다음, Childrens 직전에 Padding 추가:

```csharp
        /// <summary>
        /// 타이틀바 높이를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 11)] public float TitleHeight { get; set; } = 40;

        /// <summary>
        /// 자식 컨트롤 영역의 내부 여백을 가져오거나 설정합니다.
        /// 자식들은 이 값만큼 안쪽으로 인셋된 영역에 배치됩니다.
        /// </summary>
        [GoProperty(PCategory.Control, 13)] public GoPadding Padding { get; set; } = new();

        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [GoChildList]
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
```

(`BorderWidth`이 order 12 기존 사용 → Padding은 13.)

기존 PanelBounds ([GoWindow.cs:87-90](src/Going/Going.UI/Design/GoWindow.cs:87)) 수정:

```csharp
        // 변경 전
        [JsonIgnore] public override SKRect PanelBounds => Areas()["Panel"];

        // 변경 후
        [JsonIgnore] public override SKRect PanelBounds => Util.FromRect(Areas()["Panel"], Padding);
```

기존 OnLayout ([GoWindow.cs:188-209](src/Going/Going.UI/Design/GoWindow.cs:188)) 수정:

```csharp
        // 변경 후 전체
        protected override void OnLayout()
        {
            var rtPanel = PanelBounds;

            var nofills = Childrens.Where(c => c.Dock != GoDockStyle.Fill).ToList();
            var fills = Childrens.Where(c => c.Dock == GoDockStyle.Fill).ToList();
            var bnds = Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height); //Util.FromRect(rtPanel);
            foreach (var control in nofills) bnds = ApplyDocking(control, bnds);
            foreach (var c in fills)
            {
                c.Left = bnds.Left + c.Margin.Left;
                c.Top = bnds.Top + c.Margin.Top;
                c.Right = bnds.Right - c.Margin.Right;
                c.Bottom = bnds.Bottom - c.Margin.Bottom;
            }

            //base.OnLayout();
        }
```

- [ ] **Step 4: 빌드 + 테스트 → PASS**

```
dotnet build Going.sln -c Debug --nologo
dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~PaddingTests.GoWindow"
```
Expected: build PASS, 2 tests PASS.

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Design/GoWindow.cs Going.UI.Tests/Containers/PaddingTests.cs
git commit -m "$(cat <<'EOF'
GoWindow: Padding 프로퍼티 추가

기존 PanelBounds를 Padding 인셋된 Panel 영역으로 변경. OnLayout이
PanelBounds를 사용하도록 수정.
PanelBounds 인셋 및 Gudx 라운드트립 검증 테스트 추가.
EOF
)"
```

---

## Task 8: 전체 회귀 확인

**Files:** 변경 없음 — 검증만.

- [ ] **Step 1: 전체 Containers 테스트**

```
dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~Going.UI.Tests.Containers"
```
Expected: 13 tests (1 GoBoxPanel Dock + 6×2 PanelBounds+Gudx) all PASS.

- [ ] **Step 2: 전체 테스트 스위트 회귀 확인**

```
dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo
```
Expected: 0 failures. 신규 13 + 기존 106 = 119 tests PASS.

회귀 발생 시 멈추고 보고. Padding 기본값이 (0,0,0,0)이므로 기존 동작 변경 없어야 함 — 회귀면 의외의 상호작용 의심, root cause 분석.

- [ ] **Step 3: 전체 솔루션 빌드 회귀 확인**

```
dotnet build Going.sln -c Debug --nologo
```
Expected: 0 errors. SampleOpenTK 등 다른 프로젝트도 정상.

---

## Self-Review

**Spec coverage:**
- ✅ Spec §3 목표 (자식 영역 인셋) → Tasks 2-7 PanelBounds + OnLayout 수정
- ✅ Spec §4.1 공통 패턴 (Padding 프로퍼티) → 각 task Step 3
- ✅ Spec §4.2 기존 오버라이드 3개 (Panel/GroupBox/Window) → Tasks 5/6/7
- ✅ Spec §4.3 신규 오버라이드 3개 (Page/BoxPanel/SideBar) → Tasks 2/3/4
- ✅ Spec §4.4 Property Order 배정 → 각 task 명시
- ✅ Spec §4.5 using 추가 → 각 task의 using 확인 단계
- ✅ Spec §5.1 PanelBounds 인셋 검증 (6 cases) → 각 task의 Step 1 첫 번째 테스트
- ✅ Spec §5.2 Gudx 라운드트립 (6 cases) → 각 task의 Step 1 두 번째 테스트
- ✅ Spec §5.3 Dock=Fill 위치 검증 (GoBoxPanel 1 case) → Task 2 세 번째 테스트

**Placeholder scan:** TBD/TODO/"적절한"/"비슷한 식으로" 없음. 모든 step에 실제 코드 또는 명령 포함.

**Type consistency:**
- `GoPadding` 생성자 `new GoPadding(L, T, R, B)` 또는 `new()` — 일관 사용 ✓
- `Util.FromRect(SKRect, GoPadding)` — 모든 task에서 동일 시그니처 ✓
- `PanelBounds`/`OnLayout` 시그니처 — 기존 베이스 시그니처 그대로 ✓

**Sequence:** 단순 컨테이너(BoxPanel→Page→SideBar) → 기존 오버라이드 있는 컨테이너(Panel→GroupBox→Window) 순서로 위험도 낮은 것부터 진행.
