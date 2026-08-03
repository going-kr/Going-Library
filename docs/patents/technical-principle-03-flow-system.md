# 구체적 기술원리 03 — 설비 토폴로지 기반 흐름·혼합 전파 및 실제 I/O 이상진단

## 0. 문서 목적과 구현 경계

이 문서는 `invention-disclosure-03-flow-system.md`를 구체적인 실시 알고리즘으로 발전시킨 기술 부속서다. 현재 코드에는 편집 가능한 port/connection topology, 장치 통과상태 기반 재귀 전파, 색상 혼합 표시와 유동 particle animation이 구현돼 있다. 반면 sensor observation과 예상 상태의 불일치로 이상 원인을 산출하는 기능은 아직 구현되지 않은 출원 권장 확장안이다.

| 구분 | 의미 |
|---|---|
| 현재 구현 | 저장소에서 확인되는 화면상 흐름·혼합 시각화 |
| 결함 보완 실시형태 | 현재 목적을 안정적으로 수행하기 위해 cycle·분기·합류를 보완한 graph 알고리즘 |
| 진단 확장 F-04 | live I/O를 결합해 blockage, leak, backflow, 오염 경로를 추론하는 미구현안 |

현재 구현을 유량·압력의 물리 simulation 또는 실제 유체 이동의 증명으로 표현하면 안 된다. 이는 설비 상태와 연결 topology에 따른 **논리적 예상 흐름 상태**를 계산해 시각화하는 엔진이다.

## 1. 기술적 문제

산업 HMI에서 pump가 켜지고 valve가 열려 있다는 사실만으로 downstream 전체가 실제로 흐르는지 운전자가 즉시 판단하기 어렵다. tee/cross 분기, bidirectional port, 복수 source와 공유 line이 포함되면 다음 문제가 생긴다.

- 어느 line segment가 현재 통과 가능한지 수동 추적해야 한다.
- 설계상 connection의 start/end 저장 방향과 실제 흐름 방향이 다를 수 있다.
- 다른 유체가 공통 line에 도달할 때 합류지점과 영향 경로를 알기 어렵다.
- 화면상 예상 flow와 실제 flow/pressure sensor가 불일치해도 원인 후보를 따로 추론해야 한다.
- cycle이 있는 topology에서는 단순 재귀가 종료되지 않거나 같은 edge를 반복 처리할 수 있다.

본 기술은 화면에 이미 존재하는 설비 객체와 port 연결을 graph로 재구성하고, 장치의 통과상태 및 source 정보를 graph에 전파하여 활성 edge, 방향, 유체 identity 집합을 계산한다. 확장안은 이 예상 상태를 live observation과 비교해 최소 원인 후보와 영향 경로를 출력한다.

## 2. 현재 구현의 데이터 모델

### 2.1 설비 node

모든 설비 객체는 `FsFlowObject`를 상속하고 다음 계약을 제공한다.

```text
IoPorts() -> 활성 port 목록
GetPort(name) -> 이름에 대응하는 활성 port 또는 null
IsFlow -> 현재 장치를 유체가 통과할 수 있는지/유체를 공급할 수 있는지
```

### 2.2 port

```text
ConnectPort = {
    Name,                       // control 내부 식별자
    Position,                   // panel 좌표
    Type: Input|Output|Bidirectional,
    Direction: Left|Right|Top|Bottom,
    Control,                    // runtime owner reference
    State: None|Input|Output    // 해당 cycle의 계산 결과
}
```

port identity는 논리적으로 `(ControlId, PortName)`이다. `Position`과 `Direction`은 graph 연결뿐 아니라 orthogonal pipe geometry와 animation 방향에도 사용된다.

### 2.3 connection edge

```text
FlowConnection = {
    Id,
    StartControlId, StartPortName,
    EndControlId, EndPortName,
    Nodes: [PipeNode(Direction, Position)],

    StartPort, EndPort,         // runtime resolved reference
    Points, DrawingPoints,      // geometry cache
    IsFlow, LiquidColor,        // cycle result
    Bubbles, TotalDistance      // animation state
}
```

저장된 `Start`와 `End`는 file상 연결 양단을 구분할 뿐, bidirectional network의 실제 유동 방향을 반드시 고정하지 않는다. 전파가 어느 port에서 connection으로 진입했는지에 따라 반대쪽 port를 다음 endpoint로 선택한다.

### 2.4 장치별 현재 gate 조건

| 객체 | port | 현재 `IsFlow` 의미 |
|---|---|---|
| cylinder tank | optional Input, optional Output | `Value > 0 && UseOutlet` |
| silo tank | optional Input, 항상 Output | `Value > 0` |
| pump | optional Input, Output | `OnOff` |
| valve | bidirectional 2개 | `OnOff` |
| tee | bidirectional 3개 | 항상 true |
| cross | bidirectional 4개 | 항상 true |

pump의 `UseInlet=false`인 경우 pump 자체가 source처럼 시작점이 된다. `UseInlet=true`이면 upstream 유체가 pump를 통과할 때만 downstream으로 이어지는 구조다.

## 3. topology cache 재구성

### 3.1 cache 구조

`RefreshCache`는 child와 connection을 다음 세 index로 변환한다.

```text
ControlById[controlId] = FsFlowObject
ConnectionByPort[controlId + ":" + portName] = FlowConnection
ConnectionsByControl[controlId] = [FlowConnection...]
```

현재 `ConnectionByPort`는 동일 port에 복수 connection이 있으면 첫 connection만 보존한다. 이는 하나의 port당 하나의 pipe가 연결된다는 암묵적 topology 제약이다. 복수 edge가 허용되는 model이라면 값 type을 list로 바꿔야 한다.

### 3.2 source port 선택

현재 source 후보는 다음 control의 `Outlet`이다.

```text
CylinderTank where UseOutlet = true
SiloTank
Pump where UseInlet = false
```

source 후보 등록과 실제 흐름은 분리된다. 전파 정보의 최초 `IsFlow`는 source control의 `IsFlow`에서 얻으므로 빈 tank나 꺼진 pump는 등록돼도 활성 edge를 만들지 않는다.

### 3.3 cache 갱신 시점

초기화 시 `RefreshCache`가 호출되고 외부에서도 호출할 수 있다. topology를 runtime에 변경한 직후 자동으로 모든 index가 갱신된다고 단정할 수 없으므로, editor 명령 완료 후 명시적 refresh 또는 collection change event 기반 refresh가 권장된다.

## 4. connection geometry의 구체 원리

### 4.1 endpoint resolution

각 connection은 `StartControlId/PortName`, `EndControlId/PortName`으로 control과 port를 찾고, 찾은 port에 owner control reference를 연결한다.

### 4.2 orthogonal polyline 생성

`PipeNode.Direction`이 L/R이면 현재 X를 node.Position으로, T/B이면 현재 Y를 node.Position으로 바꾼다.

```text
points := [start.Position]
(x,y) := start.Position
for node in Nodes:
    if node.Direction in {L,R}: x := node.Position
    else:                       y := node.Position
    if (x,y) differs from last point: append (x,y)
append end.Position if different
```

따라서 각 node는 절대 2차원 점 전체가 아니라 한 축의 bend 위치를 기록한다. editor에서 한 축만 움직여 배관의 수평/수직 구간을 유지할 수 있다.

### 4.3 node 정규화

연속된 node가 같은 방향축이면 마지막 위치로 병합하고, 마지막 node 및 반대축의 목표 node를 end port 위치에 맞춘다. node가 없으면 start direction 축을 기준으로 end 좌표에 이르는 기본 node 하나를 만든다.

### 4.4 corner smoothing

각 내부 corner에서 이전·다음 segment 길이의 절반과 설정 radius 중 작은 값을 실제 radius로 정한다.

```text
r = min(configuredRadius, len(prevSegment)/2, len(nextSegment)/2)
cornerStart = corner - unit(prev) * r
cornerEnd   = corner + unit(next) * r
```

`cornerStart`, 원래 corner, `cornerEnd`를 제어점으로 하는 quadratic Bézier를 현재 8개 구간으로 sampling한다. 이 결과가 `DrawingPoints`이며 전체 길이와 particle 위치 계산에도 사용된다.

### 4.5 현재 geometry 갱신 한계

현재 `FlowConnection.Update`의 change 판정은 panel bounds 변화 또는 design mode 여부에 주로 의존한다. runtime에서 child control 위치만 바뀌는 경우 자동 재계산을 보장하지 않는다. endpoint position/version을 cache key에 포함하는 것이 보완안이다.

## 5. 현재 구현의 flow cycle

매 draw cycle의 논리 순서는 다음과 같다.

```text
1. 각 child control의 update를 실행한다.
2. 각 connection endpoint와 geometry를 update한다.
3. 모든 connection의 IsFlow/LiquidColor와 endpoint port state를 초기화한다.
4. 각 source port에 대해 source flow와 source color를 만든다.
5. source에서 재귀 propagation을 실행한다.
6. 계산된 connection state와 geometry로 pipe를 그린다.
7. 별도 10ms timer가 활성 connection의 bubble progress를 갱신한다.
```

source color는 tank의 `FillColor`를 theme color로 바꾸고, 독립 pump source는 현재 `DeepSkyBlue` 계열 default를 사용한다. mixed color는 panel의 `MixedColor` theme 설정에서 얻는다.

## 6. 현재 재귀 전파 알고리즘

### 6.1 입력 상태

```text
PropagationInfo = {
    LiquidColor,       // 해당 source의 색
    MixedColor,        // 합류 표시색
    IsFlow             // 현재까지의 통과 가능성
}
```

### 6.2 현재 코드에 충실한 의사코드

```text
function PROPAGATE(startPort, info):
    if startPort.Control is null: return
    connection := ConnectionByPort[(control.Id, startPort.Name)]
    if connection is null: return

    endPort := the other endpoint of connection relative to startPort
    startPort.State := Output

    info.IsFlow := info.IsFlow AND startPort.Control.IsFlow
    connection.IsFlow := connection.IsFlow OR info.IsFlow

    if info.IsFlow:
        if startPort.Control is Tee or Cross:
            colors := distinct non-null LiquidColor values of connections
                      incident to all ports of the tee/cross
            if colors.Count = 1: connection.LiquidColor := colors[0]
            if colors.Count > 1: connection.LiquidColor := info.MixedColor
        else:
            connection.LiquidColor := info.LiquidColor

    if endPort exists
       AND info.IsFlow
       AND endPort.Control.IsFlow
       AND endPort.Type in {Input, Bidirectional}:

        for each other connection incident to endPort.Control:
            endPort.State := Input
            recurse from the endpoint of that other connection
            which belongs to endPort.Control
```

일반 장치에서는 입력으로 도달한 connection을 제외한 다른 connection으로 이어지고, tank/silo에 도달하면 더 전파하지 않는다. tee/cross에서는 port state를 참고해 역전파를 제한하려는 분기가 존재한다.

### 6.3 direction 표시

전파가 시작된 port는 `Output`, 도착 port는 `Input`으로 기록된다. bubble의 normalized progress `r = position/total`을 geometry상의 좌표로 바꾸되, stored `EndPort`가 `Output` 상태이면 `r := 1-r`로 뒤집는다. 따라서 file의 Start→End 방향과 실제 계산 방향이 반대여도 particle 이동을 반전시킬 수 있다.

## 7. 현재 구현에서 확인된 한계

### 7.1 cycle 종료 보장 부재

재귀 인자에 visited port/edge가 없다. valve와 bidirectional junction으로 cycle을 만들면 동일 connection을 다시 방문해 깊은 재귀 또는 무한 순환이 발생할 수 있다. 현재 connection의 `IsFlow`가 이미 true인지가 재귀 중단조건으로 사용되지 않는다.

### 7.2 분기 간 mutable 상태 공유

`PropagationInfo` 객체 하나를 sibling branch 재귀에 공유하고 `info.IsFlow &= control.IsFlow`로 직접 변경한다. 한 branch에서 false가 되면 뒤에 처리되는 sibling branch도 false를 물려받을 수 있다. 분기마다 immutable/copy state를 사용해야 한다.

### 7.3 tee/cross port-state 조건의 불완전성

현재 tee/cross 분기에는 start-side 가능 여부와 end-side 가능 여부를 각각 계산하려는 local 값이 있으나 한 값은 실제 조건에 사용되지 않고 동일한 end-side 조건이 양쪽 분기에 사용된다. 이를 의도된 방향 제어가 완성됐다고 표현해서는 안 된다.

### 7.4 혼합 결과의 순서 의존 가능성

합류 node의 incident connection에 지금까지 기록된 색을 읽어 현재 connection 색을 정한다. source를 순차 처리하므로 먼저 처리된 outbound edge가 나중 source 도착 후 mixed로 재계산되지 않을 수 있다. 전체 graph가 안정될 때까지 provenance 집합을 반복 전파하는 고정점 계산이 아니다.

### 7.5 물리량 부재

유량, 압력손실, line volume, 농도, 체류시간을 계산하지 않는다. `IsFlow`는 논리 flag이며 `LiquidColor`는 유체 식별의 시각적 proxy다. 따라서 물질수지나 실제 도달시간을 산출한다고 주장할 수 없다.

### 7.6 동시성 및 시험 범위

10ms `System.Timers.Timer`가 bubble list를 변경하고 draw가 이를 읽으며, lock 대신 snapshot/예외 무시로 방어한다. topology 전파 자체의 cycle·branch·merge 회귀시험은 현재 확인되지 않았고, 존재하는 FlowSystem 관련 시험은 주로 GUDX child/connection/node 왕복을 검증한다.

## 8. 결함 보완 실시형태 — 방향 edge와 유체 출처 집합의 고정점 전파

출원용 권장 실시형태는 단일 mutable 색 대신 provenance 집합을 상태로 사용하고 work queue로 graph의 고정점을 구한다.

### 8.1 정규화 graph

```text
PortKey  = (ControlId, PortName)
EdgeKey  = ConnectionId
ArcKey   = (ConnectionId, FromPortKey, ToPortKey)

Node = control
Port = node boundary with declared PortType
Edge = physical/logical pipe connection
Arc  = an allowed traversal direction of an edge
```

connection이 bidirectional이면 두 arc를 만들 수 있고, `Output→Input`, `Output→Bidirectional`, `Bidirectional→Input`, `Bidirectional↔Bidirectional` 등 정책 table로 허용 방향을 정한다. 단순히 저장된 start/end 방향으로 제한하지 않는다.

### 8.2 source descriptor

```text
SourceDescriptor = {
    SourceId,
    FluidId,              // 색이 아닌 안정적 유체 식별자
    DisplayColor,
    Enabled,
    Timestamp,
    Quality,
    OptionalFlowRate
}
```

색은 표시 속성이고 혼합 판정은 `FluidId` 집합으로 수행한다. 서로 다른 source라도 `FluidId`가 같으면 동일 유체로 취급할 수 있다.

### 8.3 edge state

```text
EdgeState = {
    IsExpectedActive,
    ForwardSourceIds,
    ReverseSourceIds,
    FluidIds,
    Direction: None|Forward|Reverse|Conflict,
    Revision
}
```

### 8.4 gate 함수

각 control은 단순 `IsFlow` 외에 들어온 port와 나갈 port를 포함한 gate 함수를 제공한다.

```text
CanPass(control, incomingPort, outgoingPort, liveState) -> bool
```

예:

- closed valve: 모든 port pair false
- open two-port valve: Port1↔Port2 true
- running inlet pump: Inlet→Outlet true, 역방향 false
- tee/cross: incoming과 다른 활성 bidirectional port pair true
- tank: inlet은 sink로 수용하되 inlet→outlet 즉시 관통 여부는 공정정책으로 결정

이 구조는 `control.IsFlow=true` 하나만으로 pump 역류와 tank 관통을 동일하게 취급하는 문제를 막는다.

### 8.5 queue algorithm

```text
function SOLVE_EXPECTED_FLOW(graph, sources, liveState):
    clear all EdgeState
    queue := empty

    for each enabled source s:
        queue.push(State(
            port=s.Outlet,
            sourceIds={s.SourceId},
            fluidIds={s.FluidId},
            incomingArc=null))

    while queue not empty:
        state := queue.pop()

        for each allowed outgoing arc a from state.port:
            if a is the immediate reverse of state.incomingArc
               and policy prohibits backtracking: continue

            nextPort := a.To
            changed := UNION_EDGE_STATE(a, state.sourceIds, state.fluidIds)
            if not changed: continue

            node := nextPort.Owner
            for each candidate outgoing port q of node:
                if q = nextPort: continue
                if not CanPass(node, nextPort, q, liveState): continue

                nextState := copy(state)
                nextState.port := q
                nextState.incomingArc := a
                queue.push(nextState)

    for each edge:
        edge.IsExpectedActive := edge.FluidIds.Count > 0
        edge.DisplayState :=
            Empty if count=0
            Single(edge.FluidIds[0]) if count=1
            Mixed(edge.FluidIds) if count>1
```

`UNION_EDGE_STATE`가 새 source/fluid를 추가했을 때만 다음 node를 queue에 넣는다. 유한한 source 집합과 edge 집합에서 각 `(arc, sourceId)`는 최초 추가 시에만 downstream work를 만들므로 cycle이 있어도 종료한다.

### 8.6 분기 독립성과 합류 고정점

queue item은 immutable value 또는 branch별 copy이므로 차단된 branch가 sibling state를 변경하지 않는다. 합류 후 새로운 `FluidId`가 edge state에 추가되면 `changed=true`가 되어 downstream을 다시 계산하므로 source 순서와 무관한 집합 고정점에 수렴한다.

### 8.7 방향 충돌

같은 connection에 양쪽 source가 반대방향으로 도달하면 `ForwardSourceIds`와 `ReverseSourceIds`가 모두 비어 있지 않다. 이때 `Direction=Conflict`로 표시하고, 물리모델이 없는 기본 실시형태에서는 어느 쪽이 우세한지 임의 결정하지 않는다. pressure/flow observation이 있으면 확장 rule로 실제 방향을 선택할 수 있다.

## 9. 혼합·오염 경로 계산

### 9.1 최초 합류점

각 port/node/edge에 도달한 `FluidIds` 집합을 유지한다. 어떤 element의 집합 cardinality가 처음 1에서 2 이상으로 변한 revision을 기록한다.

```text
MergeEvent = {
    ElementId,
    IncomingFluidIds,
    IncomingSourceIds,
    FirstDetectedRevision,
    UpstreamArcs
}
```

source별 predecessor를 함께 저장하면 각 fluid가 합류점까지 온 경로를 역추적할 수 있다.

### 9.2 영향 sink

merge element에서 downstream arc를 다시 탐색해 tank, product outlet, drain 등 sink type에 도달하면 contamination candidate로 기록한다.

```text
ContaminationRisk = {
    MergeElement,
    FluidSet,
    Sink,
    Path,
    Severity
}
```

금지 조합 table `ForbiddenMix(fluidA, fluidB, destination)`을 적용해 단순 mixed 표시와 위험 alarm을 구분한다.

### 9.3 도달시간 확장

edge별 line volume `V_e`와 추정 flow `Q_e`가 있으면 traversal delay를 `Δt_e = V_e / max(Q_e, ε)`로 계산하고 path delay를 합산한다. 이 값이 없으면 도달시간을 출력하지 않고 연결 가능성만 출력한다.

## 10. 실제 I/O 결합 이상진단 F-04

이 절은 현재 미구현이지만 명세서에서 실시 가능하도록 구체화한 확장안이다.

### 10.1 입력 정규화

```text
Observation = {
    TagId,
    MappedElementId,
    Kind: PumpRun|ValvePosition|Flow|Pressure|Level|Quality,
    Value,
    Timestamp,
    Quality,
    EngineeringUnit
}
```

command와 feedback을 분리한다. 예컨대 valve open command가 true여도 position feedback이 closed이면 gate의 신뢰도를 낮추거나 서로 다른 예상 graph scenario를 만든다.

### 10.2 시간·quality 정합

진단시각 `T`에 대해 observation age가 허용 window를 넘으면 stale로 표시한다. bad quality는 정상값처럼 사용하지 않고 sensor fault 후보 또는 낮은 weight constraint로 변환한다.

```text
usable(o) = o.Quality is Good AND T-o.Timestamp <= MaxAge[o.Kind]
```

### 10.3 예상 상태 생성

pump/valve feedback으로 `CanPass`를 평가하고 8절의 solver를 실행해 각 edge의 예상 active/direction/fluid set을 얻는다. command와 feedback이 불일치하면 다음 두 scenario를 병렬 계산할 수 있다.

- commanded scenario: 제어 의도 기준
- feedback scenario: 실제 위치·운전 feedback 기준

두 scenario의 차이는 actuator non-response constraint가 된다.

### 10.4 observation mapping

flow meter는 edge 또는 node boundary에, pressure는 port/node에, level은 tank에 매핑한다. 방향성 flow 값은 부호로 forward/reverse를 구별한다.

### 10.5 constraint 생성

| 식별자 | 조건 | 후보 설명 |
|---|---|---|
| C1 | expected active, measured flow ≤ threshold | blockage, pump fault, closed/mispositioned valve, flow sensor fault |
| C2 | expected inactive, measured flow > threshold | valve leak, alternate unmodeled path, backflow, sensor fault |
| C3 | pump run command=true, feedback=false | pump/drive non-response |
| C4 | valve open command와 position 불일치 | actuator/limit switch fault |
| C5 | upstream flow − downstream flow > tolerance | intermediate leak, branch consumption, meter bias |
| C6 | measured direction ≠ expected direction | backflow, reversed installation, topology error |
| C7 | downstream pressure > upstream pressure beyond tolerance | backpressure/backflow candidate |
| C8 | forbidden fluid set reaches sink | contamination risk |

각 constraint는 자신을 설명할 수 있는 candidate cause 집합과 weight를 갖는다.

```text
Violation = {
    Id,
    Severity,
    Evidence,
    CandidateCauses,
    Weight
}
```

### 10.6 최소 원인 집합

여러 violation을 동시에 설명하는 작은 cause 집합을 weighted hitting-set 방식으로 찾는다.

```text
score(CauseSet) =
    Σ unexplainedViolation.Weight
  + λ * |CauseSet|
  + Σ causePriorPenalty
```

가능한 cause set 중 score가 낮은 상위 K개를 반환하고, 각 후보가 설명하는 sensor evidence와 영향 path를 함께 제시한다. 설비를 자동 정지시키는 결정과 운전자 설명용 후보 표시는 분리한다.

### 10.7 confidence

```text
confidence = coverageOfViolations
           × observationQualityFactor
           × topologyCompletenessFactor
           × temporalConsistencyFactor
```

정확한 계수는 현장 calibration으로 정한다. stale/bad sensor가 많거나 topology mapping이 불완전하면 원인명을 단정하지 않고 후보와 낮은 confidence를 표시한다.

## 11. 상태 갱신 architecture

### 11.1 revision 기반 snapshot

렌더링 thread와 I/O thread가 같은 mutable collection을 직접 공유하지 않도록 다음 snapshot을 권장한다.

```text
TopologySnapshot(version)
LiveStateSnapshot(timestamp, values)
FlowResultSnapshot(topologyVersion, liveTimestamp, edgeStates, diagnoses)
```

solver는 immutable snapshot을 입력으로 새 result를 만들고, 완료 후 atomic reference swap으로 renderer에 전달한다. topology version이 계산 중 바뀌면 결과를 폐기하거나 재계산한다.

### 11.2 incremental update

한 valve 상태만 바뀌면 전체 graph를 항상 다시 계산하는 대신 영향 node에서 downstream provenance를 invalidate하고 재전파할 수 있다. 다만 최초 구현은 정확성을 우선해 전체 fixed-point solve를 수행하고 graph 규모를 측정한 후 incremental mode를 도입한다.

## 12. animation과 계산 결과의 결합

각 edge의 `Direction`에 따라 particle progress를 증가 또는 감소시키고, `FluidIds.Count`에 따라 단일색, striped/gradient mixed, 위험색을 선택한다.

```text
if Direction=Forward: position += speed * dt
if Direction=Reverse: position -= speed * dt
if Direction=Conflict: render bidirectional warning pattern
```

현재 구현은 고정 tick당 `Position += 5`이고 약 300ms마다 bubble을 추가한다. 보완안은 elapsed time `dt`와 pixel-per-second 또는 flow-correlated speed를 사용해 timer jitter에 독립적으로 만든다.

## 13. 실시예

### 13.1 tank–pump–valve 직렬 path

```text
Tank(Output) → Pump(Inlet/Outlet) → Valve(Port1/Port2) → ProductTank(Input)
```

- source tank level > 0
- pump feedback running
- valve feedback open

이면 세 connection에 tank `FluidId`가 전파된다. valve가 closed이면 valve 이후 connection에는 source set이 추가되지 않는다. pump running인데 downstream flow meter가 0이면 C1 violation이 생성되고 pump fault, blockage, valve feedback 오류가 후보가 된다.

### 13.2 두 source의 tee 합류

```text
Source A(fluid=Water) ─┐
                       Tee → ProductTank
Source B(fluid=Oil) ───┘
```

tee downstream edge의 `FluidIds={Water,Oil}`이 되며 처음 cardinality가 2가 된 tee/edge를 first merge로 기록한다. ProductTank에서 Water/Oil 조합이 금지돼 있으면 contamination risk와 두 upstream provenance path를 표시한다.

### 13.3 loop

cycle을 포함해도 `(directed arc, sourceId)`가 edge state에 최초 추가될 때만 queue work를 만들기 때문에 유한 단계에서 종료한다. 같은 source가 loop를 돌아 다시 들어오면 union 결과가 변하지 않아 재전파하지 않는다.

### 13.4 반대방향 source 충돌

하나의 shared line 양쪽에서 source가 활성화되면 forward/reverse set이 동시에 채워진다. pressure 정보가 없으면 `Conflict`로 표시한다. 실제 signed flow가 있으면 관측 방향과 일치하는 scenario의 confidence를 높인다.

## 14. 시험 설계

### 14.1 현재 구현 회귀시험

| Scenario | 검증 |
|---|---|
| open serial path | 모든 예상 connection `IsFlow=true` |
| closed valve | valve downstream false |
| reversed stored endpoints | port state와 bubble 방향 반전 |
| same-fluid two sources | single fluid 표시 |
| different-fluid two sources | mixed 표시 |
| topology GUDX round-trip | control ID, port name, nodes 보존 |

### 14.2 보완 solver 시험

| Scenario | 불변조건 |
|---|---|
| source 순서 permutation | 최종 edge fluid set 동일 |
| sibling 중 한 branch closed | 다른 branch 결과 불변 |
| cycle | 제한시간 내 종료, 동일 source 중복 없음 |
| diamond split/merge | merge downstream provenance union |
| counter-flow | direction conflict 검출 |
| duplicate connection at port | validation error 또는 명시적 multi-edge 처리 |

### 14.3 진단 시험

- pump command/run feedback 불일치
- expected-active edge의 zero flow
- closed valve downstream의 nonzero flow
- upstream/downstream mass imbalance
- reverse signed flow와 pressure inversion
- stale/bad sensor가 confidence에 미치는 영향
- 복수 violation을 하나의 blockage가 설명하는 경우
- topology mapping 누락 시 과도한 단정 방지

## 15. 기술적 효과와 측정치

| 효과 | 측정 |
|---|---|
| cycle 안전성 | cyclic graph 종료시간, 최대 queue size |
| 순서 독립 혼합 | source insertion order별 결과 hash 비교 |
| 분기 독립성 | 한 branch gate 변경 전후 비영향 branch state 비교 |
| 오염 추적 | known merge/sink fixture의 precision/recall |
| 진단 설명력 | fault injection별 top-K cause recall |
| 운전자 대응시간 | 기존 화면 대비 이상 위치 식별시간 |
| 렌더 안정성 | concurrent I/O update 중 frame exception/drop 측정 |

## 16. 도면 구성안

### 도 1 — topology model

```text
100 FlowSystem panel
110 equipment node
120 port
130 connection edge
140 pipe node/geometry
150 topology cache
```

### 도 2 — 현재 flow 처리

reset → source 선택 → recursive propagation → 혼합색 결정 → port direction → animation.

### 도 3 — 보완 fixed-point solver

source descriptor → work queue → directed arc → gate function → provenance union → changed 여부 → 재queue.

### 도 4 — 합류와 오염 영향

서로 다른 fluid source, first merge, mixed downstream edge, affected sinks를 표시한다.

### 도 5 — 실제 I/O 진단

command/feedback/sensor → time-quality normalize → expected graph → violation → minimal cause set → HMI explanation.

### 도 6 — snapshot concurrency

topology snapshot과 live snapshot을 solver가 받아 immutable result snapshot으로 renderer에 교체하는 구조다.

## 17. 청구항 구성 방향

### 17.1 현재 구현 중심 독립항

1. HMI의 복수 설비 객체에서 port type과 connection을 포함하는 graph를 구성
2. 하나 이상의 source와 각 설비 객체의 통과상태에 따라 connection 활성상태를 전파
3. connection에 도달한 복수 source의 유체 식별정보에 따라 단일 또는 혼합상태 결정
4. 전파 진입방향과 connection geometry에 따라 유동 표시의 진행방향 결정

다만 동적 process flow display 선행기술과 가까우므로 독립항의 강도는 제한될 수 있다.

### 17.2 권장 보완 독립항

1. port pair별 통과 가능성을 나타내는 gate를 구성
2. source별 유체 식별자 집합을 방향 arc에 전파
3. arc의 기존 집합과 union 결과가 변할 때만 downstream work를 생성
4. 합류 element에서 복수 유체 식별자를 결정하고 downstream sink를 추적
5. 반대 방향 source set이 공존하면 direction conflict를 결정

### 17.3 F-04 진단 독립항

1. HMI topology와 actuator feedback으로 예상 active path 생성
2. time/quality가 정규화된 sensor observation을 node/edge에 매핑
3. 예상 상태와 관측 상태의 불일치 constraint 생성
4. 복수 constraint를 설명하는 edge/node/actuator/sensor의 원인 후보 집합 결정
5. 후보와 evidence path 및 영향 sink를 HMI에 표시

현재 구현 중심 항, fixed-point 혼합 항, 실제 I/O 진단 항을 하나의 독립항에 모두 넣으면 불필요하게 좁아질 수 있다. 선행조사 후 복수 독립항 또는 연속 출원으로 설계한다.

## 18. 구현·출원 전 보강 목록

| 항목 | 현재 상태 | 필요한 조치 |
|---|---|---|
| cycle | visited 없음 | queue + provenance union solver 구현 |
| branch state | mutable 공유 | immutable state/copy 적용 |
| tee/cross guard | 조건 사용 불완전 | port pair gate로 대체 |
| mixing | source 순서 의존 가능 | fluid set fixed point |
| topology update | 명시적 refresh 중심 | version/event 기반 rebuild |
| geometry update | child runtime 이동 감지 부족 | endpoint version cache key |
| bubble concurrency | lock 없는 timer/draw | result snapshot 또는 animation thread 일원화 |
| flow tests | serialization 위주 | cycle/branch/merge test 추가 |
| live I/O diagnosis | 미구현 | simulator/prototype 및 fault injection 자료 |
| physical quantities | 없음 | claim에서 물리 simulation 과장 금지 |

## 19. 코드 근거

| 구성 | 위치 |
|---|---|
| port, direction, flow enum과 base object | `Going.UI/FlowSystem/FsFlowObject.cs` |
| cache, source 선택, current propagation | `Going.UI/FlowSystem/FsFlowSystemPanel.cs` |
| connection geometry, draw, bubble tick | 같은 파일의 `FlowConnection` |
| orthogonal path, normalize, smoothing, location | `Going.UI/Tools/PipeTool.cs` |
| pump gate와 port | `Going.UI/FlowSystem/FsPump.cs` |
| valve gate와 bidirectional port | `Going.UI/FlowSystem/FsValve.cs` |
| tank source/sink 조건 | `FsCylinderTank.cs`, `FsSiloTank.cs` |
| tee/cross junction | `FsTeePipe.cs`, `FsCrossPipe.cs` |
| topology 직렬화 시험 | `Going.UI.Tests/Gudx/GudxFlowSystemTests.cs` |

## 20. 선행기술 조사 검색 단위

- HMI topology expected flow propagation pump valve state
- port graph fluid identity propagation mixing visualization
- cyclic process flow graph provenance fixed point
- industrial HMI expected versus measured flow anomaly diagnosis
- valve lineup diagnosis graph sensor inconsistency
- contamination first merge affected sink graph
- minimal hitting set industrial process fault diagnosis
- bidirectional pipe flow direction conflict visualization

조사 시 “배관 animation”만 비교하지 말고 다음 결합별로 claim chart를 만든다.

```text
A. 화면 설계 객체의 port topology 직접 재사용
B. 장치 feedback 기반 port-pair gate
C. source/fluid provenance의 cycle-safe 고정점 전파
D. first merge 및 affected sink 추적
E. 예상 graph와 실제 sensor constraint의 최소 원인 집합
```

