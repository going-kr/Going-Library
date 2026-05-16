# FlowSystem & ImageCanvas Gudx 직렬화 보정 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `FsFlowSystemPanel`/`IcContainer`/`IcState`에 누락된 Gudx 마커 어트리뷰트를 부착하여 자식 컨트롤·연결망·상태 이미지가 Gudx 라운드트립에서 보존되도록 한다.

**Architecture:** 마커 어트리뷰트만 추가 (코드 로직 0줄). `[GoChildList]` (P2), `[GoChildWrappers]+[GoProperty]` (P4), `[GoProperty]` (P1) — 모두 기존 `GoGudxConverter` 리플렉션 인덱스가 정적 ctor에서 자동 픽업. TDD: 각 태스크마다 라운드트립 xUnit 테스트 작성 → FAIL 확인 → 마커 부착 → PASS 확인 → 커밋.

**Tech Stack:** .NET 8, C# 12, xUnit 2.5.3, SkiaSharp. Going.UI 어셈블리만 변경.

**Spec:** [docs/superpowers/specs/2026-05-16-flowsystem-imagecanvas-gudx-fix-design.md](../specs/2026-05-16-flowsystem-imagecanvas-gudx-fix-design.md)

**Build/Test 명령:**
- 빌드: `dotnet build Going.sln -c Debug --nologo` (working dir = `D:\Project\Going\library\src\Going`)
- 테스트: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter <name>`
- 전체 Gudx 테스트: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter FullyQualifiedName~Going.UI.Tests.Gudx`

---

## Files Plan

| 작업 | 경로 | 책임 |
|---|---|---|
| Modify | `Going.UI/FlowSystem/FsFlowSystemPanel.cs` | Childrens에 `[GoChildList]`, Connections에 `[GoChildWrappers]+[GoProperty]`, FlowConnection 클래스 5개 스칼라 + Nodes에 마커 부착 |
| Modify | `Going.UI/FlowSystem/FsFlowObject.cs` | PipeNode 클래스 2개 스칼라(`Direction`/`Position`)에 `[GoProperty]` 부착 |
| Modify | `Going.UI/ImageCanvas/IcContainer.cs` | Childrens에 `[GoChildList]` 부착 |
| Modify | `Going.UI/ImageCanvas/IcState.cs` | StateImages에 `[GoChildWrappers]` 부착 (`[GoProperty]` 유지) |
| Create | `Going.UI.Tests/Gudx/GudxFlowSystemTests.cs` | 2개 라운드트립 테스트 (Childrens, Connections+Nodes) |
| Create | `Going.UI.Tests/Gudx/GudxImageCanvasTests.cs` | 2개 라운드트립 테스트 (IcContainer.Childrens, IcState.StateImages) |

---

## Task 1: FsFlowSystemPanel.Childrens P2 라운드트립

**Files:**
- Create: `Going.UI.Tests/Gudx/GudxFlowSystemTests.cs`
- Modify: `Going.UI/FlowSystem/FsFlowSystemPanel.cs:113-115` (Childrens 프로퍼티)

- [ ] **Step 1: 실패 테스트 작성**

Create `Going.UI.Tests/Gudx/GudxFlowSystemTests.cs`:

```csharp
using Going.UI.Enums;
using Going.UI.FlowSystem;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxFlowSystemTests
{
    [Fact]
    public void FsFlowSystemPanel_roundTripsChildren()
    {
        var panel = new FsFlowSystemPanel { Name = "fs" };
        panel.Childrens.Add(new FsPump { Name = "pump1", OnOff = true, Rotate = ObjectRotate.Deg90 });
        panel.Childrens.Add(new FsValve { Name = "valve1", OnOff = false });
        panel.Childrens.Add(new FsCylinderTank { Name = "tank1", Value = 75, UseInlet = true });

        var xml = GoGudxConverter.SerializeControl(panel);
        var restored = (FsFlowSystemPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3, restored.Childrens.Count);
        Assert.IsType<FsPump>(restored.Childrens[0]);
        Assert.Equal("pump1", restored.Childrens[0].Name);
        Assert.True(((FsPump)restored.Childrens[0]).OnOff);
        Assert.Equal(ObjectRotate.Deg90, ((FsPump)restored.Childrens[0]).Rotate);
        Assert.IsType<FsValve>(restored.Childrens[1]);
        Assert.Equal("valve1", restored.Childrens[1].Name);
        Assert.IsType<FsCylinderTank>(restored.Childrens[2]);
        Assert.Equal(75d, ((FsCylinderTank)restored.Childrens[2]).Value);
        Assert.True(((FsCylinderTank)restored.Childrens[2]).UseInlet);
    }
}
```

- [ ] **Step 2: 테스트 빌드 & 실행 → FAIL 확인**

Run: `dotnet build Going.sln -c Debug --nologo`
Expected: build succeeds.

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter FullyQualifiedName~GudxFlowSystemTests.FsFlowSystemPanel_roundTripsChildren`
Expected: 1 test, FAIL — `restored.Childrens.Count`이 `0` (마커 누락으로 자식이 직렬화 안 됨), 어서션 `Assert.Equal(3, 0)` 실패.

- [ ] **Step 3: `[GoChildList]` 마커 부착**

Edit `Going.UI/FlowSystem/FsFlowSystemPanel.cs:113-115`. 기존:

```csharp
        /// <summary>
        /// 자식 컨트롤 컬렉션을 가져옵니다.
        /// </summary>
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
```

변경 후:

```csharp
        /// <summary>
        /// 자식 컨트롤 컬렉션을 가져옵니다.
        /// </summary>
        [GoChildList]
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
```

파일 상단에 `using Going.UI.Gudx;`가 이미 없으면 추가. (현재 파일은 Going.UI.Gudx를 import 안 하고 있으므로 추가 필요.)

`Going.UI/FlowSystem/FsFlowSystemPanel.cs:1-14` (using 블록):

```csharp
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Timer = System.Timers.Timer;
```

(알파벳 순서 유지 — Going.UI.Gudx는 Going.UI.Extensions와 Going.UI.Themes 사이.)

- [ ] **Step 4: 테스트 실행 → PASS 확인**

Run: `dotnet build Going.sln -c Debug --nologo`
Expected: build succeeds.

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter FullyQualifiedName~GudxFlowSystemTests.FsFlowSystemPanel_roundTripsChildren`
Expected: 1 test, PASS.

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/FlowSystem/FsFlowSystemPanel.cs Going.UI.Tests/Gudx/GudxFlowSystemTests.cs
git commit -m "FlowSystem Gudx: FsFlowSystemPanel.Childrens에 [GoChildList] 부착

자식 Fs* 컴포넌트 P2 라운드트립 회복. xUnit 테스트 추가."
```

---

## Task 2: FlowConnection / PipeNode / Connections P4 라운드트립

마커가 서로 의존 — FsFlowSystemPanel.Connections에 `[GoChildWrappers]` 부착하려면 FlowConnection이 wrapper로 직렬화 가능해야 하고, 그러려면 FlowConnection 스칼라/Nodes에 마커가 있어야 함. 단일 태스크에서 4개 변경 같이 진행.

**Files:**
- Modify: `Going.UI.Tests/Gudx/GudxFlowSystemTests.cs` (테스트 추가)
- Modify: `Going.UI/FlowSystem/FsFlowSystemPanel.cs:158-160` (Connections), `:702-729` (FlowConnection 스칼라+Nodes)
- Modify: `Going.UI/FlowSystem/FsFlowObject.cs:142-157` (PipeNode 스칼라)

- [ ] **Step 1: 실패 테스트 추가**

Edit `Going.UI.Tests/Gudx/GudxFlowSystemTests.cs`. 기존 `}` 닫는 괄호 바로 위에 다음 `[Fact]` 메서드를 추가 (클래스 안에):

```csharp
    [Fact]
    public void FsFlowSystemPanel_roundTripsConnectionsWithNodes()
    {
        var panel = new FsFlowSystemPanel { Name = "fs" };
        var tank = new FsCylinderTank { Name = "tank", UseOutlet = true };
        var pump = new FsPump { Name = "pump", UseInlet = true };
        panel.Childrens.Add(tank);
        panel.Childrens.Add(pump);

        var conn = new FlowConnection
        {
            StartControlId = tank.Id,
            StartPortName = "Outlet",
            EndControlId = pump.Id,
            EndPortName = "Inlet"
        };
        conn.Nodes.Add(new PipeNode { Direction = PortDirection.R, Position = 400 });
        conn.Nodes.Add(new PipeNode { Direction = PortDirection.B, Position = 200 });
        var originalConnId = conn.Id;
        panel.Connections.Add(conn);

        var xml = GoGudxConverter.SerializeControl(panel);
        var restored = (FsFlowSystemPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Single(restored.Connections);
        var r = restored.Connections[0];
        Assert.Equal(originalConnId, r.Id);
        Assert.Equal(tank.Id, r.StartControlId);
        Assert.Equal("Outlet", r.StartPortName);
        Assert.Equal(pump.Id, r.EndControlId);
        Assert.Equal("Inlet", r.EndPortName);
        Assert.Equal(2, r.Nodes.Count);
        Assert.Equal(PortDirection.R, r.Nodes[0].Direction);
        Assert.Equal(400f, r.Nodes[0].Position);
        Assert.Equal(PortDirection.B, r.Nodes[1].Direction);
        Assert.Equal(200f, r.Nodes[1].Position);
    }
```

- [ ] **Step 2: 테스트 실행 → FAIL 확인**

Run: `dotnet build Going.sln -c Debug --nologo`
Expected: build succeeds.

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter FullyQualifiedName~GudxFlowSystemTests.FsFlowSystemPanel_roundTripsConnectionsWithNodes`
Expected: 1 test, FAIL — `restored.Connections.Count`이 `0`, `Assert.Single`이 실패.

- [ ] **Step 3: PipeNode 스칼라에 `[GoProperty]` 부착**

Edit `Going.UI/FlowSystem/FsFlowObject.cs:142-157`. 파일 상단 using에 `using Going.UI.Controls;`가 이미 있는지 확인 (PCategory가 GoControl.cs에 정의되어 있음). 없으면 추가.

`Going.UI/FlowSystem/FsFlowObject.cs:1-9` (using 블록) 확인 — `Going.UI.Controls`가 이미 있음 ✓.

기존 PipeNode 클래스 ([FsFlowObject.cs:142-158](src/Going/Going.UI/FlowSystem/FsFlowObject.cs:142)):

```csharp
    #region class : PipeNode
    /// <summary>
    /// 파이프 노드를 나타내는 클래스
    /// </summary>
    public class PipeNode
    {
        /// <summary>
        /// 파이프 노드의 방향을 가져오거나 설정합니다.
        /// </summary>
        public PortDirection Direction { get; set; }

        /// <summary>
        /// 파이프 노드의 위치를 가져오거나 설정합니다.
        /// </summary>
        public float Position { get; set; }
    }
    #endregion
```

변경 후:

```csharp
    #region class : PipeNode
    /// <summary>
    /// 파이프 노드를 나타내는 클래스
    /// </summary>
    public class PipeNode
    {
        /// <summary>
        /// 파이프 노드의 방향을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 0)] public PortDirection Direction { get; set; }

        /// <summary>
        /// 파이프 노드의 위치를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 1)] public float Position { get; set; }
    }
    #endregion
```

- [ ] **Step 4: FlowConnection 스칼라 + Nodes에 마커 부착**

Edit `Going.UI/FlowSystem/FsFlowSystemPanel.cs:702-729`. (Task 1에서 `using Going.UI.Gudx;`는 이미 추가됨.)

기존 FlowConnection 프로퍼티 영역:

```csharp
        /// <summary>
        /// 연결의 고유 식별자를 가져오거나 설정합니다.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 시작 컨트롤의 ID를 가져오거나 설정합니다.
        /// </summary>
        public Guid? StartControlId { get; set; }

        /// <summary>
        /// 끝 컨트롤의 ID를 가져오거나 설정합니다.
        /// </summary>
        public Guid? EndControlId { get; set; }

        /// <summary>
        /// 시작 포트의 이름을 가져오거나 설정합니다.
        /// </summary>
        public string? StartPortName { get; set; }

        /// <summary>
        /// 끝 포트의 이름을 가져오거나 설정합니다.
        /// </summary>
        public string? EndPortName { get; set; }

        /// <summary>
        /// 중간 파이프 노드 목록을 가져오거나 설정합니다.
        /// </summary>
        public List<PipeNode> Nodes { get; set; } = [];
```

변경 후:

```csharp
        /// <summary>
        /// 연결의 고유 식별자를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 0)] public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 시작 컨트롤의 ID를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 1)] public Guid? StartControlId { get; set; }

        /// <summary>
        /// 끝 컨트롤의 ID를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 2)] public Guid? EndControlId { get; set; }

        /// <summary>
        /// 시작 포트의 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 3)] public string? StartPortName { get; set; }

        /// <summary>
        /// 끝 포트의 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 4)] public string? EndPortName { get; set; }

        /// <summary>
        /// 중간 파이프 노드 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoChildWrappers]
        [GoProperty(PCategory.Basic, 5)] public List<PipeNode> Nodes { get; set; } = [];
```

- [ ] **Step 5: FsFlowSystemPanel.Connections에 마커 부착**

Edit `Going.UI/FlowSystem/FsFlowSystemPanel.cs:158-160`. 기존:

```csharp
        /// <summary>
        /// 플로우 연결 목록을 가져오거나 설정합니다.
        /// </summary>
        public List<FlowConnection> Connections { get; set; } = [];
```

변경 후:

```csharp
        /// <summary>
        /// 플로우 연결 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoChildWrappers]
        [GoProperty(PCategory.Control, 7)] public List<FlowConnection> Connections { get; set; } = [];
```

(`PCategory.Control` order=7은 기존 FrameColor=6 다음. BaseWidth=0, BaseHeight=1, PanelAlignment=2, PipeSize=3, FrameSize=4, EmptyColor=5, MixedColor=5, FrameColor=6.)

- [ ] **Step 6: 테스트 실행 → PASS 확인**

Run: `dotnet build Going.sln -c Debug --nologo`
Expected: build succeeds.

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter FullyQualifiedName~GudxFlowSystemTests`
Expected: 2 tests, all PASS (Task 1 테스트도 회귀 없이 통과).

- [ ] **Step 7: 커밋**

```bash
git add Going.UI/FlowSystem/FsFlowSystemPanel.cs Going.UI/FlowSystem/FsFlowObject.cs Going.UI.Tests/Gudx/GudxFlowSystemTests.cs
git commit -m "FlowSystem Gudx: Connections/FlowConnection/PipeNode에 마커 부착

FsFlowSystemPanel.Connections를 [GoChildWrappers]+[GoProperty]로 P4 직렬화.
FlowConnection의 Id/StartControlId/EndControlId/StartPortName/EndPortName/Nodes,
PipeNode의 Direction/Position 모두 [GoProperty] 부착. Id 보존 + 중간 노드 라운드트립
검증 테스트 추가."
```

---

## Task 3: IcContainer.Childrens P2 라운드트립

**Files:**
- Create: `Going.UI.Tests/Gudx/GudxImageCanvasTests.cs`
- Modify: `Going.UI/ImageCanvas/IcContainer.cs:34-36` (Childrens)

- [ ] **Step 1: 실패 테스트 작성**

Create `Going.UI.Tests/Gudx/GudxImageCanvasTests.cs`:

```csharp
using Going.UI.Gudx;
using Going.UI.ImageCanvas;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxImageCanvasTests
{
    [Fact]
    public void IcContainer_roundTripsChildren()
    {
        var container = new IcContainer { Name = "ic", OffImage = "bg_off", OnImage = "bg_on" };
        container.Childrens.Add(new IcButton { Name = "btn1", Text = "go" });
        container.Childrens.Add(new IcLabel { Name = "lbl1" });

        var xml = GoGudxConverter.SerializeControl(container);
        var restored = (IcContainer)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(2, restored.Childrens.Count);
        Assert.IsType<IcButton>(restored.Childrens[0]);
        Assert.Equal("btn1", restored.Childrens[0].Name);
        Assert.Equal("go", ((IcButton)restored.Childrens[0]).Text);
        Assert.IsType<IcLabel>(restored.Childrens[1]);
        Assert.Equal("bg_off", restored.OffImage);
        Assert.Equal("bg_on", restored.OnImage);
    }
}
```

- [ ] **Step 2: 테스트 빌드 & 실행 → FAIL 확인**

Run: `dotnet build Going.sln -c Debug --nologo`
Expected: build succeeds.

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter FullyQualifiedName~GudxImageCanvasTests.IcContainer_roundTripsChildren`
Expected: 1 test, FAIL — `restored.Childrens.Count`이 `0`.

- [ ] **Step 3: `[GoChildList]` 마커 부착**

Edit `Going.UI/ImageCanvas/IcContainer.cs:1-12` (using 블록 확인). `Going.UI.Gudx` import 추가 필요.

기존 using:

```csharp
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Themes;
using Going.UI.Utils;
```

변경 후 (Going.UI.Gudx 추가):

```csharp
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
```

Edit `Going.UI/ImageCanvas/IcContainer.cs:34-36`. 기존:

```csharp
        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
```

변경 후:

```csharp
        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [GoChildList]
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
```

- [ ] **Step 4: 테스트 실행 → PASS 확인**

Run: `dotnet build Going.sln -c Debug --nologo`
Expected: build succeeds.

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter FullyQualifiedName~GudxImageCanvasTests.IcContainer_roundTripsChildren`
Expected: 1 test, PASS.

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/ImageCanvas/IcContainer.cs Going.UI.Tests/Gudx/GudxImageCanvasTests.cs
git commit -m "ImageCanvas Gudx: IcContainer.Childrens에 [GoChildList] 부착

IcContainer 자식 P2 라운드트립 회복. xUnit 테스트 추가."
```

---

## Task 4: IcState.StateImages P4 라운드트립

**Files:**
- Modify: `Going.UI.Tests/Gudx/GudxImageCanvasTests.cs` (테스트 추가)
- Modify: `Going.UI/ImageCanvas/IcState.cs:23-25` (StateImages)

- [ ] **Step 1: 실패 테스트 추가**

Edit `Going.UI.Tests/Gudx/GudxImageCanvasTests.cs`. 기존 `}` 닫는 괄호 바로 위에 다음 `[Fact]` 메서드 추가 (클래스 안):

```csharp
    [Fact]
    public void IcState_roundTripsStateImages()
    {
        var st = new IcState { Name = "stateLamp", State = 2 };
        st.StateImages.Add(new StateImage { State = 0, Image = "off" });
        st.StateImages.Add(new StateImage { State = 1, Image = "warn" });
        st.StateImages.Add(new StateImage { State = 2, Image = "on" });

        var xml = GoGudxConverter.SerializeControl(st);
        var restored = (IcState)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(2, restored.State);
        Assert.Equal(3, restored.StateImages.Count);
        Assert.Equal(0, restored.StateImages[0].State);
        Assert.Equal("off", restored.StateImages[0].Image);
        Assert.Equal(1, restored.StateImages[1].State);
        Assert.Equal("warn", restored.StateImages[1].Image);
        Assert.Equal(2, restored.StateImages[2].State);
        Assert.Equal("on", restored.StateImages[2].Image);
    }
```

- [ ] **Step 2: 테스트 실행 → FAIL 확인**

Run: `dotnet build Going.sln -c Debug --nologo`
Expected: build succeeds.

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter FullyQualifiedName~GudxImageCanvasTests.IcState_roundTripsStateImages`
Expected: 1 test, FAIL — `restored.StateImages.Count`이 `0`.

- [ ] **Step 3: `[GoChildWrappers]` 마커 부착**

Edit `Going.UI/ImageCanvas/IcState.cs:1-12` (using 블록 확인). `Going.UI.Gudx` import 추가 필요.

기존 using:

```csharp
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
```

변경 후 (Going.UI.Gudx 추가):

```csharp
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
```

Edit `Going.UI/ImageCanvas/IcState.cs:23-25`. 기존:

```csharp
        /// <summary>
        /// 상태별 이미지 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public List<StateImage> StateImages { get; set; } = [];
```

변경 후 (`[GoProperty]` 유지, `[GoChildWrappers]` 추가):

```csharp
        /// <summary>
        /// 상태별 이미지 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoChildWrappers]
        [GoProperty(PCategory.Control, 0)] public List<StateImage> StateImages { get; set; } = [];
```

- [ ] **Step 4: 테스트 실행 → PASS 확인**

Run: `dotnet build Going.sln -c Debug --nologo`
Expected: build succeeds.

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter FullyQualifiedName~GudxImageCanvasTests`
Expected: 2 tests, all PASS (Task 3 테스트도 회귀 없이 통과).

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/ImageCanvas/IcState.cs Going.UI.Tests/Gudx/GudxImageCanvasTests.cs
git commit -m "ImageCanvas Gudx: IcState.StateImages에 [GoChildWrappers] 부착

StateImage P4 라운드트립 회복. 디자이너 그리드 노출용 [GoProperty]는 유지
(기존 컨벤션: GoTabControl.TabPages 등과 동일하게 두 어트리뷰트 공존).
xUnit 테스트 추가."
```

---

## Task 5: 전체 Gudx 테스트 회귀 확인

**Files:** 변경 없음 — 검증만.

- [ ] **Step 1: 전체 Gudx 테스트 실행**

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --no-build --nologo --filter FullyQualifiedName~Going.UI.Tests.Gudx`
Expected: 모든 Gudx 테스트(기존 + 신규 4개) PASS, FAIL 0건.

만약 기존 테스트 중 회귀가 생긴 경우(예: 새 wrapper 인덱스가 기존 인덱스에 영향)에는 멈추고 보고. 회귀 사례 발생 시 root cause 분석부터 — 마커 부착은 누락된 경로 추가일 뿐이라 기존 경로에 영향 없어야 하므로 의외의 실패면 진짜 버그 가능성.

- [ ] **Step 2: 전체 솔루션 빌드 회귀 확인**

Run: `dotnet build Going.sln -c Debug --nologo`
Expected: 0 errors, 0 new warnings. SampleOpenTK 등 다른 프로젝트도 정상 빌드.

---

## Self-Review

**Spec coverage:**
- ✅ Spec §3.1 FsFlowSystemPanel.Childrens `[GoChildList]` → Task 1
- ✅ Spec §3.1 FsFlowSystemPanel.Connections `[GoChildWrappers]+[GoProperty]` → Task 2 Step 5
- ✅ Spec §3.1 FlowConnection 스칼라 5종 + Nodes → Task 2 Step 4
- ✅ Spec §3.2 PipeNode 스칼라 2종 → Task 2 Step 3
- ✅ Spec §3.3 IcContainer.Childrens → Task 3
- ✅ Spec §3.4 IcState.StateImages → Task 4
- ✅ Spec §5.1 GudxFlowSystemTests (2 케이스) → Task 1/2
- ✅ Spec §5.2 GudxImageCanvasTests (2 케이스) → Task 3/4

**Placeholder scan:** TBD/TODO/"appropriate" 없음. 모든 step에 실제 코드 또는 명령 포함.

**Type consistency:** `FlowConnection`/`PipeNode`/`PortDirection`/`ObjectRotate`/`FsPump`/`FsValve`/`FsCylinderTank`/`IcButton`/`IcLabel`/`IcState`/`StateImage`/`IcContainer`/`FsFlowSystemPanel` 모두 spec과 코드베이스 실제 명칭 일치 확인됨.

**예상 위험:**
- Task 2에서 `using Going.UI.Gudx;`가 Task 1에서 이미 추가되어 있어야 함 — 동일 파일이라 의존성 OK.
- `FsFlowSystemPanel` 클래스는 `Going.UI.FlowSystem` 네임스페이스이고, `FlowConnection`/`PipeNode`도 같은 네임스페이스(같은 파일 내부 + `FsFlowObject.cs`) — 테스트는 `Going.UI.FlowSystem` 한 줄만 import.
