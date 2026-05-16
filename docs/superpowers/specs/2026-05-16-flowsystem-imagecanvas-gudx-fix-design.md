# FlowSystem & ImageCanvas Gudx 직렬화 누락 보정

날짜: 2026-05-16
대상 어셈블리: `Going.UI`
관련 테스트: `Going.UI.Tests/Gudx`

## 1. 배경

`GoGudxConverter`는 GoControl 객체 그래프를 XML(Gudx)로 직렬화하는 리플렉션 기반 컨버터다. [PATTERNS.md](../../../Going.UI/Gudx/PATTERNS.md) 명세대로 **명시적 마커 어트리뷰트가 없으면 emit하지 않는다** (inclusion is explicit).

- 스칼라: `[GoProperty]` (또는 파생) + `IsScalar(type)` 통과 시 P1 (XML attribute)
- IGoControl 리스트: `[GoChildList]` (P2)
- 비-IGoControl 래퍼 리스트: `[GoChildWrappers]` (P4)

현재 다음 두 곳이 마커가 누락되어 디자인 데이터가 Gudx 라운드트립에서 손실된다:

### 1.1 FlowSystem

`FsFlowSystemPanel`은 `GoContainer`를 직접 상속하는데, 베이스 [GoContainer.Childrens:28](../../../Going.UI/Containers/GoContainer.cs:28)는 `[GoChildList]`가 없다 (이건 `GoPanel`/`GoPage` 등 구체 컨테이너에서 각각 부착). 따라서 [FsFlowSystemPanel.Childrens:115](../../../Going.UI/FlowSystem/FsFlowSystemPanel.cs:115)는 `[JsonInclude]`만 있고 Gudx에는 노출 안 됨.

[FsFlowSystemPanel.Connections:160](../../../Going.UI/FlowSystem/FsFlowSystemPanel.cs:160)(`List<FlowConnection>`) 역시 마커 없음. `FlowConnection`/`PipeNode`는 IGoControl이 아니므로 `[GoChildWrappers]` (P4)가 필요하며, 각자의 스칼라 필드도 `[GoProperty]` 부착이 필요.

결과: `FsFlowSystemPanel`의 스칼라(`PipeSize`/`FrameSize`/색상 키 등)만 저장되고 자식 컴포넌트 배치와 배관 연결망 전체가 사라짐.

### 1.2 ImageCanvas

[IcContainer.Childrens:36](../../../Going.UI/ImageCanvas/IcContainer.cs:36)는 FlowSystem과 동일한 누락. `IcPage`는 베이스 `GoPage.Childrens`가 이미 `[GoChildList]`(Inherited=true)이므로 영향 없음.

[IcState.StateImages:25](../../../Going.UI/ImageCanvas/IcState.cs:25)는 `List<StateImage>`에 `[GoProperty]`만 붙어 있다. `GoGudxConverter.ScalarProperties`는 `IsScalar` 통과 못하는 타입을 조용히 누락([GoGudxConverter.cs:1111](../../../Going.UI/Gudx/GoGudxConverter.cs:1111))하므로 `[GoChildWrappers]` 추가 부착이 필요. 기존 `[GoProperty]`는 디자이너 속성 그리드 노출용으로 유지(아래 1.3 참조).

### 1.3 프로젝트 컨벤션 — `[GoProperty]` + `[GoChildWrappers]` 동시 부착

기존 코드 전수 확인 결과 **모든** `[GoChildWrappers]` 프로퍼티에 `[GoProperty]`가 같이 붙어 있다. 두 어트리뷰트는 직교 — `[GoProperty]`는 디자이너 속성 그리드 노출(category/order), `[GoChildWrappers]`는 Gudx 직렬화.

예: [GoPanel.Buttons:97-98](../../../Going.UI/Containers/GoPanel.cs:97), [GoTabControl.TabPages:115-116](../../../Going.UI/Containers/GoTabControl.cs:115), [GoTreeNode.Nodes:33-34](../../../Going.UI/Datas/GoTreeNode.cs:33), [GoGridLayoutPanel.Rows:28-29](../../../Going.UI/Containers/GoGridLayoutPanel.cs:28) 등.

본 작업도 컨벤션에 맞춰 새로 부착하는 모든 wrapper 리스트에 둘 다 부착한다.

## 2. 목표

Gudx 라운드트립 시 다음이 보존되어야 한다.

- `FsFlowSystemPanel`의 자식 `FsFlowObject` 트리 (위치/회전/상태 포함)
- `FsFlowSystemPanel.Connections`의 모든 `FlowConnection` (양 끝 컨트롤 Id + 포트명 + 중간 PipeNode들)
- `IcContainer`의 자식 IGoControl 트리
- `IcState.StateImages`의 모든 `StateImage` 항목 (`State`/`Image` 보존)

비목표:
- 런타임 캐시(`StartPort`/`EndPort`/`Points`/`DrawingPoints`/`IsFlow`/`LiquidColor`)는 `[JsonIgnore]`라서 직렬화 대상 아님 (그대로 유지).
- 직렬화 포맷 자체 변경 없음 (기존 Gudx 패턴 그대로).

## 3. 변경 사항

### 3.1 `Going.UI/FlowSystem/FsFlowSystemPanel.cs`

**Childrens — `[GoChildList]` 부착 (P2)**

[FsFlowSystemPanel.cs:115](../../../Going.UI/FlowSystem/FsFlowSystemPanel.cs:115):
```csharp
[GoChildList]
[JsonInclude] public override List<IGoControl> Childrens { get; } = [];
```

**Connections — `[GoChildWrappers]` + `[GoProperty]` 부착 (P4)**

[FsFlowSystemPanel.cs:160](../../../Going.UI/FlowSystem/FsFlowSystemPanel.cs:160):
```csharp
[GoChildWrappers]
[GoProperty(PCategory.Control, 7)]
public List<FlowConnection> Connections { get; set; } = [];
```

`PCategory.Control` order=7은 기존 스칼라(`PipeSize`=3, `FrameSize`=4, `EmptyColor`=5, `MixedColor`=5, `FrameColor`=6) 다음 번호.

**FlowConnection 스칼라 필드 — `[GoProperty]` 부착**

[FsFlowSystemPanel.cs:698-729](../../../Going.UI/FlowSystem/FsFlowSystemPanel.cs:698) 내부:
```csharp
[GoProperty(PCategory.Basic, 0)] public Guid Id { get; set; } = Guid.NewGuid();
[GoProperty(PCategory.Basic, 1)] public Guid? StartControlId { get; set; }
[GoProperty(PCategory.Basic, 2)] public Guid? EndControlId { get; set; }
[GoProperty(PCategory.Basic, 3)] public string? StartPortName { get; set; }
[GoProperty(PCategory.Basic, 4)] public string? EndPortName { get; set; }

[GoChildWrappers]
[GoProperty(PCategory.Basic, 5)] public List<PipeNode> Nodes { get; set; } = [];
```

기존 `[JsonIgnore]` 프로퍼티(`StartPort`/`EndPort`/`Points`/`DrawingPoints`/`IsFlow`/`LiquidColor`)는 `ScalarProperties`가 자동 제외하므로 추가 작업 없음.

### 3.2 `Going.UI/FlowSystem/FsFlowObject.cs`

**PipeNode 스칼라 필드 — `[GoProperty]` 부착**

[FsFlowObject.cs:146-157](../../../Going.UI/FlowSystem/FsFlowObject.cs:146):
```csharp
[GoProperty(PCategory.Basic, 0)] public PortDirection Direction { get; set; }
[GoProperty(PCategory.Basic, 1)] public float Position { get; set; }
```

### 3.3 `Going.UI/ImageCanvas/IcContainer.cs`

**Childrens — `[GoChildList]` 부착**

[IcContainer.cs:36](../../../Going.UI/ImageCanvas/IcContainer.cs:36):
```csharp
[GoChildList]
[JsonInclude] public override List<IGoControl> Childrens { get; } = [];
```

### 3.4 `Going.UI/ImageCanvas/IcState.cs`

**StateImages — `[GoChildWrappers]` 부착 (`[GoProperty]` 유지)**

[IcState.cs:25](../../../Going.UI/ImageCanvas/IcState.cs:25):
```csharp
[GoChildWrappers]
[GoProperty(PCategory.Control, 0)] public List<StateImage> StateImages { get; set; } = [];
```

`StateImage` 자체는 이미 [IcState.cs:62-66](../../../Going.UI/ImageCanvas/IcState.cs:62)에서 `Image` (`[GoImageProperty]`)와 `State` (`[GoProperty]`) 스칼라가 부착되어 있으므로 추가 작업 없음.

## 4. Gudx 동작 검증 포인트

리플렉션 인덱스가 정적 생성자에서 한 번 빌드되므로([GoGudxConverter.cs:154](../../../Going.UI/Gudx/GoGudxConverter.cs:154)) 어트리뷰트 부착만으로 자동 등록된다. 추가 확인 사항:

| 항목 | 확인 |
|---|---|
| `Fs*` 컨트롤들이 `_gudxControlTypes` 레지스트리에 등록되어 있는가 | `IGoControl` 인터페이스 충족 → `GoJsonConverter` 정적 ctor에서 자동 등록 ✓ |
| `FsFlowSystemPanel`/`IcContainer`의 P2 자식 읽기 시 자식 태그 필터 | `_gudxControlTypes`에 등록된 IGoControl 태그만 통과 — Fs*/Ic* 모두 등록되어 통과 |
| `FlowConnection`/`PipeNode`/`StateImage`가 wrapper 인덱스에 등록 | `InitializeWrapperTypeIndex`가 `[GoChildWrappers]` 마킹된 프로퍼티의 element type을 스캔 → 부착 후 자동 등록 |
| `Guid?` 스칼라 라운드트립 | `IsScalar`가 `Nullable<T>` 언래핑 처리([GoGudxConverter.cs:1117](../../../Going.UI/Gudx/GoGudxConverter.cs:1117)), null이면 attribute 자체가 생략 |
| `PortDirection` enum 라운드트립 | `IsScalar`/`FormatScalar`/`ParseScalar`의 enum 분기 통과 |
| `float Position` 라운드트립 | `InvariantCulture` 기반 포맷/파싱 |

## 5. 테스트

기존 [Going.UI.Tests/Gudx/](../../../Going.UI.Tests/Gudx) 디렉토리의 패턴(GudxPattern2/4 라운드트립 테스트, GudxT13RoundTripTests)을 따라 두 파일 추가.

### 5.1 `GudxFlowSystemTests.cs`

- **Childrens P2 라운드트립**: `FsFlowSystemPanel` 안에 모든 6종 Fs* 컴포넌트(`FsPump`/`FsValve`/`FsCylinderTank`/`FsSiloTank`/`FsTeePipe`/`FsCrossPipe`)를 다양한 위치/회전/상태로 배치 → Serialize → Deserialize → 자식 개수/타입/주요 스칼라(`Bounds`/`Rotate`/`OnOff`/`Value` 등) 동등성 검증
- **Connections P4 + Nodes 라운드트립**: 시작 → 펌프 → 탱크 경로에 PipeNode 2~3개로 꺾는 연결 2건 정의 → 라운드트립 후 `StartControlId`/`EndControlId`/`StartPortName`/`EndPortName`/`Nodes` 개수와 각 `Direction`/`Position` 동등성 검증
- **FlowConnection.Id 보존**: 라운드트립 전후 `Id` Guid 동등성 검증

### 5.2 `GudxImageCanvasTests.cs`

- **IcContainer 자식 P2 라운드트립**: `IcContainer` 안에 `IcButton`/`IcLabel`/`IcOnOff`/`IcProgress`/`IcSlider`/`IcState` 배치 → 라운드트립 후 동등성 검증
- **IcPage 자식 P2 라운드트립** (회귀 방지): 기존 `GoPage` 경로가 동작하는지 확인
- **IcState.StateImages P4 라운드트립**: 여러 `StateImage` 항목(`State` 다른 값, `Image` 리소스 이름) 라운드트립 → 항목 개수/`State`/`Image` 동등성 검증

테스트 프레임워크: `xUnit` (기존 Gudx 테스트들이 `Xunit` + `Assert.*` 사용. FluentAssertions 미사용).

## 6. 위험 및 영향

- **하위 호환 영향 없음** — 어트리뷰트 추가만으로 신규 직렬화 경로 개설. 기존 JSON 직렬화는 `[JsonInclude]`로 독립 동작 중이라 변경 영향 없음.
- **Gudx 파일 포맷 확장만 발생** — 기존 Gudx 파일에는 `<Childrens>`/`<Connections>`/`<StateImages>` 그룹 요소가 아예 없었음. 추가 후 새 그룹이 emit되며 누락된 이전 파일도 빈 그룹으로 읽힘(P2/P4는 빈 그룹 허용).
- **디자이너 속성 그리드** — `Connections`에 `[GoProperty]`를 새로 부착하므로 속성 그리드에 컬렉션 편집기가 표시될 수 있음. 기존 패턴(`GoTabControl.TabPages` 등)과 동일한 동작.
- **wrapper 베이스 추가** — `FlowConnection`/`PipeNode`/`StateImage`가 `_wrapperDerivedTypesCache`에 추가됨. 정적 ctor 1회 비용만 증가, O(1) 조회 유지.

## 7. 참고 파일

- [Going.UI/Gudx/PATTERNS.md](../../../Going.UI/Gudx/PATTERNS.md) — 패턴 명세
- [Going.UI/Gudx/GoGudxConverter.cs](../../../Going.UI/Gudx/GoGudxConverter.cs) — 컨버터 본체
- [Going.UI/Gudx/GoChildListAttribute.cs](../../../Going.UI/Gudx/GoChildListAttribute.cs)
- [Going.UI/Gudx/GoChildWrappersAttribute.cs](../../../Going.UI/Gudx/GoChildWrappersAttribute.cs)
