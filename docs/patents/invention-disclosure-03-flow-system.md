# 발명신고서 03 — 설비 흐름 토폴로지 전파 및 이상진단

## 1. 서지 초안

- 발명의 명칭(현재 구현): **산업 설비 연결 토폴로지에 따른 유체 흐름 전파 및 혼합 상태 시각화 방법**
- 발명의 명칭(권장 보강): **산업 설비의 예상 흐름 토폴로지와 실제 입출력 상태의 불일치에 기초한 이상 원인 및 유체 오염 경로 진단 방법**
- 영문 가칭: **Method for Diagnosing Anomaly Causes and Fluid Contamination Paths Based on Inconsistency Between Expected Flow Topology and Live I/O States of Industrial Equipment**
- 발명자/출원인: 미확정
- 우선순위: 현재 구현 B+, 보강 발명 A-
- 국내 출원: 공개 시계와 보강 일정에 따라 결정
- 해외 확장: 실제 I/O 진단을 포함할 때 검토

## 2. 기술분야

산업 HMI/SCADA의 process flow display, 배관 network의 graph 처리, pump·valve·sensor 상태를 이용한 공정 이상진단 및 서로 다른 유체의 혼합·오염 경로 추적에 관한 것이다.

## 3. 현재 구현

### 데이터 모델

- `FsFlowObject`: pump, valve, pipe, tee, cross 등 graph node
- port: input/output/bidirectional 연결점
- `FlowConnection`: 두 port 사이의 edge와 경로 node
- start point: 유체 source와 color를 갖는 시작 node
- `IsFlow`: 각 object/connection의 현재 흐름 가능 상태
- `MixedColor`: 서로 다른 유체가 합류한 경우의 표시 상태

### 처리 순서

1. child control과 connection을 node/port 기준 dictionary로 재구성한다.
2. 매 cycle마다 connection과 port의 이전 flow 상태를 초기화한다.
3. 각 시작점에서 source flow와 liquid color를 가져온다.
4. 연결 graph를 재귀 탐색한다.
5. 경로상 control의 `IsFlow`가 false이면 전파를 차단한다.
6. input/bidirectional port를 통해 다음 node로 진행한다.
7. Tee/Cross에서 현재 incident connection에 기록된 색의 distinct 개수에 따라 단일색 또는 mixed color를 선택한다.
8. flow direction에 따라 bubble animation 방향을 정한다.

### 현재 구현의 중요 한계

- 재귀 방문상태가 없어 cyclic graph에서 종료를 보장하지 않는다.
- sibling branch가 mutable `PropagationInfo`를 공유해 한 branch의 차단이 뒤 branch에 영향을 줄 수 있다.
- Tee/Cross의 port-state guard에는 계산 후 사용되지 않는 조건값이 있고, 양쪽 분기에 동일 조건값이 사용되는 부분이 있다.
- 혼합색은 source provenance 집합의 고정점이 아니라 source별 순차 전파 중 이미 기록된 incident color를 참조하므로 처리 순서에 따라 결과가 달라질 수 있다.
- 현재 기능은 논리적 예상 흐름 시각화이며 유량·압력·질량수지의 물리 simulation이 아니다.

cycle-safe 방향 edge, branch별 immutable 상태 및 유체 식별자 집합의 work-queue 고정점 algorithm은 `technical-principle-03-flow-system.md`에 결함 보완 실시형태로 구체화한다.

코드 근거는 `Going.UI/FlowSystem/FsFlowSystemPanel.cs` 478행 이후의 graph update와 594~681행의 `FlowProcess`, 696행 이후의 `FlowConnection`이다.

## 4. 현재 발명의 문제와 목적

복잡한 설비 화면에서 valve와 pump 상태만 보고 실제 가능한 유체 경로를 운전자가 수동으로 추적하기 어렵다. 특히 branch와 loop가 많거나 복수 유체가 같은 line을 공유하면 어느 connection이 활성인지, 어디서 혼합되는지 즉시 알기 어렵다.

현재 발명은 port topology와 장치 상태를 이용해 활성 경로, 방향과 혼합 상태를 자동 계산·표시하는 것을 목적으로 한다.

## 5. 권장 보강 발명 F-04

시각화만으로는 process flow diagram과 pipe simulation 선행기술이 강하다. 다음 실제 I/O 진단을 추가한다.

### 추가 입력

- pump run command와 run feedback
- valve open/close command와 position feedback
- flow meter, pressure sensor, level sensor
- 유체 종류, 혼합 금지 rule, line volume
- sensor timestamp와 quality

### 진단 처리

1. 화면 설계에서 node/port/edge graph를 생성한다.
2. command와 feedback 상태로 각 edge의 예상 통과 가능성을 계산한다.
3. source에서 sink까지 예상 활성 경로와 예상 flow direction을 전파한다.
4. sensor observation을 graph의 node/edge에 매핑한다.
5. 예상 상태와 실제 observation 사이의 constraint violation을 생성한다.
6. violation을 설명할 수 있는 최소 edge/node 집합을 계산한다.
7. 다음 이상 유형과 신뢰도를 출력한다.
   - closed valve leak
   - open path blockage
   - pump no-flow/dry run
   - external leak
   - backflow
   - wrong valve lineup
   - sensor fault
8. 복수 유체 source가 연결될 수 있는 경로를 추적해 최초 합류점과 오염 가능 sink를 출력한다.

### 예시 rule

- pump feedback = running, downstream valve = open, 예상 path 존재, downstream flow ≈ 0 → blockage 또는 pump fault
- valve feedback = closed, downstream flow > threshold → valve leak 또는 alternate path
- upstream flow − downstream flow > tolerance → edge 구간 leak 후보
- downstream pressure > upstream pressure이고 reverse-flow 가능 edge → backflow 후보
- 서로 다른 fluid labels가 공통 edge에 도달 → contamination risk

## 6. 선행기술 대비

| 문헌 | 알려진 부분 | Going 보강안의 초점 |
|---|---|---|
| US5596704A | process model에서 flow diagram 생성 | 화면 생성이 아니라 runtime constraint diagnosis |
| WO2017074885A1 | pipe network fluid simulation과 GUI | 정밀 물리 simulation 대신 HMI topology·device feedback 불일치의 원인 graph |
| EP3568681B1 | expected/detected pressure와 leak location lookup | 편집 화면 graph, pump/valve state, 복수 anomaly와 최소 모순 집합 |
| EP2472440A1 | engineering data + DCS data plant diagnosis | 화면에서 직접 구축된 port topology와 contamination path |
| US11360463B2 | dynamic process flow diagram | 단순 dynamic display를 넘어 원인과 위험 sink 출력 |

## 7. 예비 청구항 골격

### 독립 방법항 — 보강안

산업 설비의 이상을 진단하는 방법으로서,

1. HMI 화면 정의에 포함된 복수 설비 객체의 port와 connection으로부터 방향성 graph를 생성하는 단계;
2. pump 및 valve의 command 또는 feedback 상태에 따라 graph edge의 통과 가능 상태를 결정하는 단계;
3. 하나 이상의 fluid source에서 graph를 따라 예상 활성 경로와 유체 식별자를 전파하는 단계;
4. graph의 node 또는 edge에 대응하는 flow·pressure·level sensor observation을 수신하는 단계;
5. 예상 활성 경로에 따른 예상 상태와 sensor observation의 불일치 constraint를 생성하는 단계;
6. 복수 불일치 constraint를 설명하는 하나 이상의 edge 또는 node를 이상 원인 후보로 결정하는 단계;
7. 이상 원인 후보와 그에 연결된 영향 경로를 HMI에 표시하는 단계;

를 포함하는 방법.

### 다른 독립항 — 오염 경로

복수 fluid source의 식별자를 topology에 전파하고, 서로 다른 식별자가 합류 가능한 최초 node/edge와 해당 지점에서 도달 가능한 sink를 계산해 contamination risk를 출력하는 방법.

### 종속항 후보

1. input/output/bidirectional port
2. valve close/open 및 pump run feedback
3. closed-valve leak rule
4. open-path blockage rule
5. upstream/downstream mass-balance leak rule
6. reverse pressure/flow backflow rule
7. 최소 cardinality의 원인 edge/node 집합
8. sensor quality에 따른 후보 confidence
9. mixed fluid color와 최초 합류점 표시
10. line volume과 elapsed time을 이용한 contamination arrival estimate
11. cyclic graph의 visited state와 방향 제약
12. GUDX로 topology를 저장·배포

## 8. 현재 구현만 출원할 때의 독립항 핵심

보강 전에 공지예외 시계 때문에 현재 구현을 먼저 출원한다면 다음 결합을 사용한다.

- port type과 control `IsFlow`를 이용한 방향성 graph 전파
- Tee/Cross의 incident connection에 현재 기록된 색의 distinct 개수에 따른 단일/혼합 표시
- 계산된 방향과 connection geometry에 따른 animation particle 진행방향 결정
- 편집 가능한 anchor와 serialized topology의 runtime 재구성

다만 이 범위는 시각화 선행기술과 가까워 등록·해외 확장 전망을 낮게 봐야 한다.

## 9. 도면 초안

- 도 1: pump/valve/tee/cross와 port graph
- 도 2: graph dictionary 재구성
- 도 3: source에서의 재귀 flow propagation
- 도 4: 두 유체 합류와 mixed state
- 도 5: 예상 flow와 live sensor mapping
- 도 6: constraint violation 생성
- 도 7: 최소 이상 원인 edge/node 선택
- 도 8: contamination first-merge와 affected sink

## 10. 시험 계획

| Scenario | 예상 판정 |
|---|---|
| 직렬 path 모두 open | 정상 flow |
| 중간 valve closed | valve 이후 예상 flow 없음 |
| closed valve downstream 실제 flow | valve leak/alternate path |
| pump running, path open, flow 없음 | blockage/pump fault |
| upstream/downstream flow imbalance | 중간 edge leak |
| reverse sensor direction | backflow |
| two sources same fluid | 정상 단일 fluid |
| two sources different fluid | first merge와 contamination sink |
| sensor stale/bad | 낮은 confidence 또는 sensor fault 분리 |
| loop topology | **보완 solver 목표:** 무한재귀 없이 방향·visited/provenance constraint 유지 |

## 11. 출원 결정 gate

- [ ] 실제 공개일이 2026-04-12인지 확인
- [ ] F-04 prototype을 공개 없이 4~6주 내 만들 수 있는지 판단
- [ ] 가능하면 현재 P-03과 F-04를 한 명세서에 충분히 기재해 국내 출원
- [ ] 불가능하고 공지예외 기한이 촉박하면 현재 P-03 기본 출원 후 F-04 별도 출원 검토
- [ ] process plant diagnosis 선행특허의 claims와 실시예를 정식 조사
- [ ] 실제 또는 simulated sensor test로 오탐·미탐 자료 확보
