# Going Library 실제 특허출원 프로젝트 기획서

- 작성일: 2026-08-03
- 목표: 현재 저장소의 발명을 실제 국내 출원 가능한 자료 묶음으로 만들고, 가치가 높은 발명은 국내 우선출원 후 해외 확장을 준비한다.
- 전제: 국내/해외 후보를 따로 가르지 않는다. 모든 발명은 국내에 먼저 출원할 수 있으며, 해외 확장 가치만 별도로 평가한다.

## 1. 즉시 실행 결론

### 출원 순서

| 순서 | 발명 | 결정 | 이유 |
|---:|---|---|---|
| 1 | 확인응답형 HMI 제어·표시 동기화 | 즉시 국내 명세서 착수 | 통신과 UI가 결합된 현재 최강 후보이며 2026-05 공개 가능성 때문에 시간이 중요 |
| 2 | GUDX 객체 그래프 직렬화·분할 배포 | 1번과 병행 착수 | 구현·시험이 풍부하고 2026-04 공개 가능성이 있어 오히려 공지예외 시계가 더 빠를 수 있음 |
| 3 | FlowSystem 흐름 전파 | 기본 구성 우선출원 여부를 조속히 결정 | 2026-04 공개 가능성. 다만 시각화만으로는 약해 실제 I/O 이상진단 보강이 바람직 |
| 4 | 화면 정의 기반 통신 scan-plan compiler | private prototype 후 신규 국내 출원 | 해외 확장 잠재력이 가장 큼. 출원 전 공개 금지 |

### 구현 완성도 gate

- P-01: 일반 control 변화에는 pending 등록이 있으나 manipulation 종료 `PendingFlush`에는 없다. RTU/TCP response와 request의 전 필드 상관검증도 보완 대상이다. 명세서에는 완성 실시형태를 구체적으로 쓰되 현재 코드와 혼동하지 않고, 출원 전 통합 prototype과 packet-level 시험을 만든다.
- P-02: 현재 split save는 기존 directory를 선삭제하므로 atomic deployment가 아니고 manifest/hash/schema migration도 없다. 이는 P-02 기본 구현과 F-05 확장을 분리한다.
- P-03: 현재 재귀는 cycle visited가 없고 branch state 공유 및 순차적 혼합 판정 한계가 있다. 현재 구현 출원과 fixed-point provenance solver/F-04 진단 확장을 구분한다.

### 가장 먼저 확정할 사실

local Git commit 날짜는 공개일이 아니다. 5영업일 안에 다음 실제 공개일을 기능별로 확정해야 한다.

- GitHub 최초 push와 public commit timestamp
- release/tag가 public으로 게시된 날
- NuGet package별 게시일과 해당 binary에 포함된 기능
- GitHub Pages, README, sample, 블로그, 영상, 전시, 고객 시연일
- 외부 제공자료와 NDA 체결 여부

## 2. 출원 목표와 완료 기준

### 목표 A — 국내 출원 준비 완료

다음 자료가 변리사에게 전달되면 완료다.

- 발명신고서
- 발명자·출원인·권리승계 정보
- 종래기술과 문제점
- 핵심 구성요소와 상호작용
- 최소 2개 이상의 실시예
- 방법·시스템·프로그램 청구항 초안
- 도면 목록과 원본 diagram
- 코드 근거와 시험 결과
- 선행기술 비교표
- 공지예외 주장 여부와 증명자료

### 목표 B — 해외 확장 판단 가능

국내 우선출원 뒤 9개월까지 다음을 확보한다.

- 국제조사 수준의 특허·논문 검색결과
- 핵심 시장과 경쟁사·침해 탐지 가능성
- 번역·현지대리인·심사비용을 포함한 국가별 예산
- PCT 또는 Paris 개별국 경로 결정
- 국내 최초 명세서에 해외용 실시예가 충분히 기재됐는지 점검

WIPO와 지식재산처 안내상 선출원의 우선권을 유지하려면 원칙적으로 최초 출원일부터 12개월 안에 PCT 또는 개별국 출원을 해야 한다.

## 3. 외부 자료조사 결과와 영향

### 3.1 법·심사 기준

| 자료 | 확인 내용 | 실제 조치 |
|---|---|---|
| [지식재산처 컴퓨터관련 발명](https://www.kipo.go.kr/ko/kpoContentView.do?menuCd=SCD0200238) | 소프트웨어 정보처리가 하드웨어를 통해 구체적으로 구현되는 발명을 인정 | UI 논리가 아니라 PLC, 통신장치, 메모리, display와의 구체적 처리로 서술 |
| [지식재산처 공지예외주장](https://www.kipo.go.kr/ko/kpoContentView.do?menuCd=SCD0200239) | 자기 공개 후 12개월, 출원서 취지 기재와 원칙적 30일 내 증명자료 제출 | GitHub/NuGet 공개 snapshot과 날짜를 출원 전에 준비 |
| [WIPO patent protection](https://www.wipo.int/en/web/patents/protection) | 출원 전 공개가 신규성을 파괴할 수 있고 특허는 국가별 권리 | 장래 발명은 private으로 관리 |
| [EPC Article 55](https://www.epo.org/en/legal/epc/2020/a55.html) | 유럽의 비불리 공개는 명백한 남용·공인 국제박람회 등 제한적 경우 | 자기 공개된 현재 발명의 유럽 확장은 보수적으로 판단 |
| [WIPO PCT filing](https://www.wipo.int/en/web/pct-system/filing/index) | 선출원 우선권 주장 PCT는 원칙적으로 12개월 내 | 출원 즉시 9·10·11개월 decision gate 등록 |

### 3.2 P-01 관련 선행기술

| 문헌 | 이미 알려진 부분 | Going의 잠정 차별점 |
|---|---|---|
| [US20040021679A1](https://patents.google.com/patent/US20040021679A1/en) | 산업 HMI의 XML binding 정의, 양방향 전달, optimistic/pessimistic binding, server 거부 시 이전값 복귀 | 컨트롤 조작 중 양방향 억제, 종료 시 최종값만 flush, 통신 작업 우선순위, 장치 응답 후 확인cache, 값 일치·timeout 상태 기계의 결합 |
| [EP2541354A3](https://patents.google.com/patent/EP2541354A3/en) | 그래픽 요소와 controller data binding | 일반 binding을 독립적 차별점으로 주장하지 않음 |
| [US20240248731A1](https://patents.google.com/patent/US20240248731A1/en) | UI와 비동기 command 상태를 binding하고 완료 notification으로 상태 변경 | 산업장치의 확인값 cache와 pending value 비교, 자동 poll과 manual command의 ordering 및 timeout rollback으로 한정 |
| [EP1774419B1](https://patents.google.com/patent/EP1774419B1/en) | embedded controller와 HMI의 주기적 data file synchronization | command별 상태·확인응답 cache·operator manipulation suppression의 결합 강조 |
| [Modbus V1.1b3](https://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf) | write response 형식 자체는 공개 표준 | ACK 자체가 아니라 ACK 이후 cache 갱신과 HMI state machine의 기술효과 청구 |

#### 조사 결론

“HMI 양방향 binding”, “명령 pending 표시”, “응답 후 UI 변경”을 각각 넓게 청구하면 위험하다. 독립항에는 최소한 다음 관계가 함께 들어가야 한다.

1. 주기적 확인값 수신
2. 운전자 조작 중 확인값 반영 억제
3. 조작 종료 시 최종값에 대한 쓰기 작업 생성
4. 자동 읽기 작업보다 쓰기 작업 우선 선택
5. 요청 시 cache를 변경하지 않고 유효한 응답 때 확인값 cache 변경
6. 보류 명령값과 확인값 비교
7. 일치 시 확정, 제한시간 초과 시 확인값으로 표시 복귀

위 2~7의 결합은 권장 독립항의 목표 실시형태다. 현재 코드는 조작 종료 flush의 pending 등록과 protocol response의 완전한 request correlation이 빠져 있으므로, 코드 완성·통합시험 없이 “전부 현재 구현 완료”라고 기재하지 않는다.

### 3.3 P-02 관련 선행기술

| 문헌 | 이미 알려진 부분 | Going의 잠정 차별점 |
|---|---|---|
| [US7325226B2](https://patents.google.com/patent/US7325226B2) | visual UI designer의 object graph와 serializer provider | HMI 전용 속성 family dispatch와 page/window/resource 분할·binding roundtrip 결합 |
| [US7814124B1](https://patents.google.com/patent/US7814124B1/en) | rule set 기반 object graph serialization | 범용 rule set이 아니라 다형 control collection별 구조와 충돌방지 그룹, 배포 복원 순서 강조 |
| [WO2013070561A1](https://patents.google.com/patent/WO2013070561A1/en) | metadata/reflection 기반 external object graph serialization | reflection 자체는 포기하고 P1~P5/B1/B2의 HMI 구조 보존과 split deployment를 결합 |
| [US7676740B2](https://patents.google.com/patent/US7676740B2/en) | annotated source와 XML schema 사이 mapping | annotation 자체가 아니라 이종 자식 collection collision 방지·다형 복원·외부 resource 결합 |
| [CN114509986A](https://patents.google.com/patent/CN114509986A/en) | XML 기반 cross-platform HMI configuration과 resource files | master/page/window/resource의 객체 그래프 무손실 왕복과 binding/component 사전등록 순서로 차별화 |

#### 조사 결론

P-02는 선행기술 밀도가 높다. “속성 기반 XML 직렬화”로 출원하면 약하다. 실제 오류를 해결한 **속성명 group에 의한 이종 collection 격리**와 **식별자·cell·key·binding·component 의미를 보존한 HMI split deployment**를 중심으로 해야 한다.

### 3.4 P-03/F-04 관련 선행기술

| 문헌 | 이미 알려진 부분 | Going의 잠정 차별점 |
|---|---|---|
| [US5596704A](https://patents.google.com/patent/US5596704A/en) | process model에서 process flow diagram 생성 | diagram 생성 자체를 차별점으로 삼지 않음 |
| [WO2017074885A1](https://patents.google.com/patent/WO2017074885A1/en) | GUI production model의 pipe network fluid flow simulation | 물리 시뮬레이션보다 HMI port topology와 live state 제약·혼합 root cause에 집중 |
| [EP3568681B1](https://patents.google.com/patent/EP3568681B1/en) | pipe network에서 예상·측정 pressure 비교로 leak 후보 추정 | 화면에서 편집한 topology, valve/pump feedback, 최소 모순 edge/node 및 contamination path 결합 |
| [EP2472440A1](https://patents.google.com/patent/EP2472440A1/en) | engineering data와 DCS data를 이용한 plant diagnosis | Going 화면 graph의 직접 실행과 예상 흐름/실제 I/O의 구조적 불일치 진단으로 한정 |
| [US11360463B2](https://patents.google.com/patent/US11360463B2/en) | dynamic online process flow diagram | 단순 동적 표시보다 이상 원인·혼합 경로 판정 필요 |

#### 조사 결론

현재 FlowSystem의 재귀 흐름·색 혼합 시각화만으로는 해외 확장성이 불확실하다. 국내 공지예외 시계를 지키기 위한 기본 출원은 검토할 수 있지만, 강한 권리는 live I/O 비교와 이상 원인 위치 추정을 추가한 F-04로 설계해야 한다.

### 3.5 F-01 관련 선행기술

| 문헌/표준 | 이미 알려진 부분 | 필요한 차별점 |
|---|---|---|
| [US20140316540A1](https://patents.google.com/patent/US20140316540A1/en) | PLC/HMI tag database 자동 생성 | 화면 binding graph를 실제 통신 block read plan으로 compile |
| [US8473854B2](https://patents.google.com/patent/US8473854B2/en) | tag scan rate와 graphic update rate를 포함한 visualization profile | page visibility·safety 예외·주소 병합·pending 명령을 동시에 반영 |
| [US10761514B2](https://patents.google.com/patent/US10761514B2) | automation object를 scan time·resource에 따라 controller에 binding | UI object graph에서 통신 master 작업을 생성하는 반대 방향의 변환과 protocol address coalescing |
| [OPC UA Part 4 §5.13](https://reference.opcfoundation.org/Core/Part4/v105/docs/5.13) | sampling interval, queue, triggering, monitored item | sampling 자체가 아니라 GUDX compile과 Modbus/CNet/MC address block 생성 |

### 3.6 KIPRIS 국내 직접검색 추가 결과 — 긴급 반영

2026-08-03 KIPRIS 국내 검색에서 F-01과 매우 가까운 공개출원과 국내 등록문헌이 확인되었다. 전체 검색식·건수·서지·청구항 요지와 의견은 [KIPRIS 국내 선행기술 1차 검색 기록](./2026-08-03-kipris-prior-art-search.md)에 보존한다.

| 문헌 | 확인된 핵심 | 출원전략 영향 |
|---|---|---|
| 10-2026-0094264, 공개 10-2026-0088432 | 시각 객체의 PLC address binding에서 read/write를 분리하고 polling table, 연속주소 read block, write queue를 생성해 수신값으로 객체상태 갱신 | F-01 적색경보. 전체 claim chart 전에는 넓은 독립항과 A+ 평가를 확정하지 않음 |
| 10-2019-0136591, 등록 10-2238383 | 인접 register address를 추출해 순차/개별 접근속도를 비교하고 접근방식을 주기적으로 최적화 | F-09 및 F-01의 단순 address merge·adaptive block 주장은 약함 |
| 10-2017-0100422, 등록 10-1872648 | 등록 tag의 참조 address로 receiver data block 자동 최적화 | tag-to-block이 아니라 GUDX scope expansion, decode map, confirmed command 결합 필요 |
| 10-2014-0097495, 등록 10-1584330 후 소멸 | HMI GUI object XML, PLC I/O tag map, 주기/변화 기반 화면갱신과 입력명령 | P-01/P-02의 XML·tag mapping·주기 갱신은 차별점이 아님 |
| 10-2022-0111201, 등록 10-2492443 | HMI 객체 정보에서 HTML/CSS/JSON과 SVG를 생성해 web SCADA 제공 | P-02 split document를 의미보존 graph·충돌방지·복원순서와 결합해야 함 |
| 10-2020-0169927, 등록 10-2430590 | pump·valve·배관과 단계별 유체 진행방향 시각화 | P-03 현재 시각화는 보수 평가, F-04 live I/O 원인진단 보강 필요 |

#### 즉시 gate

1. AN=[1020260094264] 전체 공보와 모든 청구항 확보
2. 출원인과 자사·관계인 관계 확인
3. F-01을 국내문헌 3건과 element-by-element charting
4. component/ItemList scope expansion, protocol decode map, P-01 confirmed state machine이 실제 차이를 만드는지 검증
5. 차이가 확인되기 전 F-01 구현 세부를 public repository, package, demo에 공개하지 않음

## 4. 추가 선행기술조사 계획

### 데이터베이스

- KIPRIS: 한국 공개·등록 및 한글 키워드, IPC/CPC
- WIPO PATENTSCOPE: PCT family와 국제조사보고서
- Espacenet: INPADOC family, citation, CPC
- Google Patents: full text와 family 빠른 탐색
- USPTO Patent Center, EPO Register: 법적 상태와 prosecution history 확인
- 비특허문헌: Microsoft WPF/XAML 문서, OPC UA, Modbus, SCADA/HMI vendor manuals, 학술논문

### 검색 절차

1. 발명별 핵심 개념을 3~5개 feature로 분해한다.
2. 한글·영문 동의어와 산업 용어를 만든다.
3. title/abstract → claims → description 순으로 검색한다.
4. 유력 문헌의 cited/citing와 patent family를 추적한다.
5. 최초 priority date와 출원 당시 공개된 청구항을 확인한다.
6. 각 독립항 element를 claim chart에 매핑한다.
7. 단일 문헌 신규성뿐 아니라 2개 문헌 결합의 진보성 공격도 작성한다.

### 권장 검색식

#### P-01

```text
(HMI OR SCADA OR operator-interface) AND
(command OR setpoint OR write) AND
(pending OR acknowledgement OR confirmed OR feedback) AND
(timeout OR rollback OR revert OR stale)

(PLC OR industrial-controller) AND
(polling OR cyclic-read) AND
(manual-command OR write-priority) AND
(cache OR readback OR echo-response)
```

#### P-02

```text
(HMI OR user-interface) AND (object-graph OR control-tree) AND
(serialize OR persist OR XML) AND
(resource OR page OR window OR split OR polymorphic)

(attribute OR annotation OR metadata) AND serialization AND
(heterogeneous-collection OR wrapper OR keyed-map OR cell-layout)
```

#### P-03/F-04

```text
(process-flow OR piping-network) AND (topology OR graph) AND
(valve OR pump) AND (expected-flow OR propagation) AND
(leak OR blockage OR backflow OR contamination)
```

#### F-01

```text
(HMI OR SCADA) AND (binding OR display-definition) AND
(tag-extraction OR scan-plan OR polling-schedule OR block-read) AND
(address-coalescing OR adjacent-register OR visibility)
```

### CPC/IPC 탐색 방향

정확한 분류는 조사 담당자가 유력 seed patent의 분류를 역추적해 확정한다. 우선 탐색 분야는 G05B 계열의 제어·감시, G06F 계열의 UI·데이터 처리·직렬화, G06Q가 아닌 산업 제어 구현, G01M/G01F 계열의 배관·유량 진단이다.

## 5. 청구항 작성 전략

### 공통 원칙

- “소프트웨어 기능”만 나열하지 않고 processor, memory, communication interface, display, industrial controller와 데이터 흐름을 구체적으로 연결한다.
- 결과를 “편리함”이 아니라 통신 frame 감소, stale display 방지, 중복 명령 방지, object graph corruption 방지 같은 기술효과로 적는다.
- 독립항 하나에 불필요한 UI class명이나 Modbus function code를 넣지 않고, 종속항에서 구체화한다.
- 구현 언어 C#, .NET, SkiaSharp는 실시예로 두되 권리범위를 특정 framework에 고정하지 않는다.
- 방법항, 시스템항, 컴퓨터프로그램/기록매체항을 병렬 준비한다.
- 독립항에는 선행기술과 구별되는 feature 관계를 모두 넣고, 종속항은 protocol·control·실패 시나리오별 fallback을 만든다.

### Claim chart 형식

| 청구항 구성 | 자사 코드/도면 | 문헌 A | 문헌 B | 차이·기술효과 | 유지/수정 |
|---|---|---|---|---|---|
| 구성 1 |  |  |  |  |  |
| 구성 2 |  |  |  |  |  |

변리사에게 문헌 목록만 넘기지 말고 이 표를 독립항마다 작성한다.

## 6. 도면 제작 계획

### P-01

1. HMI–binding engine–scheduler–communication master–PLC 구성도
2. `idle/manipulating/pending/confirmed/timed-out` 상태 전이도
3. 정상 응답 sequence diagram
4. 이전 polling값이 늦게 도착하는 sequence diagram
5. timeout·거부·재접속 sequence diagram
6. multi-register 확인 판정 흐름도

### P-02

1. HMI object graph와 attribute family 구조도
2. attribute dispatch decision tree
3. property-name group에 의한 collection 격리 전후 비교
4. master/page/window/resource 파일 분할도
5. serialize와 deserialize 순서도
6. component·binding·identifier 복원도

### P-03/F-04

1. port/node/edge topology
2. 현재 valve·pump state를 반영한 재귀 흐름 전파와 그 한계
3. 보완된 방향 arc·유체 provenance 집합·work queue 고정점 전파
4. 복수 유체의 최초 합류점과 영향 sink 판정
5. 예상 흐름과 sensor observation 비교
6. 최소 모순 edge/node를 이용한 원인 후보 추정
7. contamination path 표시

## 7. 검증·실험 계획

### P-01 필수 시험 matrix

| Case | 조건 | 기대 결과 |
|---|---|---|
| 1 | 명령 후 ACK 즉시 | pending 해제, 확인값 표시 |
| 2 | 명령 후 이전 poll값 도착, 그 뒤 ACK | 명령값 유지 후 확인 |
| 3 | ACK 없이 timeout | 최신 확인값으로 복귀 |
| 4 | 장치가 다른 값으로 clamp | timeout 또는 명시적 mismatch 후 장치값 표시 |
| 5 | drag 중 여러 중간값 발생 | source write 억제, release 최종값 1회 전송 |
| 6 | manual write와 auto read 경쟁 | manual 작업 우선 선택 |
| 7 | 재시도 후 중복 ACK | cache와 UI의 단일 확정 상태 유지 |
| 8 | 통신 단절·재접속 | false success 없이 pending/error 처리 |
| 9 | FC15/FC16 부분 불일치 | 정책에 따른 전체/부분 확정 판정 |
| 10 | RTU/TCP 동일 scenario | protocol에 무관한 동일 상태 전이 |

비교군은 (a) polling값을 즉시 UI에 반영하는 방식, (b) write 호출 즉시 cache를 바꾸는 optimistic 방식이다.

### P-02 필수 시험 matrix

- 같은 parent에 P2와 P4가 함께 존재하는 경우
- 복수 P5 map과 B2 resource가 함께 존재하는 경우
- 다형 wrapper 3단계 중첩
- cell/span/Id/key roundtrip
- binding expression과 component parameter roundtrip
- master 일부 page 변경 후 재배포
- 누락 resource, 알 수 없는 type, 잘못된 tag에 대한 방어
- 대형 design에서 기존 JSON/XML 대안과 size/load/diff 비교

### P-03/F-04 필수 시험 matrix

- 직렬, 분기, 순환, 양방향 port
- 서로 다른 두 유체의 합류와 분리
- closed valve 뒤 flow sensor 검출
- open path인데 downstream flow가 없는 blockage
- upstream/downstream 유량 불균형 leak
- 역방향 pressure/flow에 따른 backflow
- 복수 이상이 있을 때 최소 원인 후보 집합

## 8. 실제 일정

### 긴급 10영업일

| 기간 | 작업 | 산출물 |
|---|---|---|
| D0–D2 | 공개일 감사, 발명자 확정, 코드 snapshot | 공개 증거표, 발명자표, source archive |
| D0–D2 | F-01 국내 긴급 claim chart | 10-2026-0094264, 10-2019-0136591, 10-2017-0100422 전체 공보와 비교표 |
| D1–D4 | P-01/P-02 claim-level 검색 | search log, 유력문헌 PDF, claim charts |
| D2–D5 | P-01 통합시험과 sequence 자료 | test result, 상태 전이도 |
| D3–D6 | P-02 roundtrip·성능 자료 | test result, file structure 도면 |
| D5–D7 | 발명신고서 review | 확정 발명신고서 2건 |
| D6–D8 | 변리사 초안회의 | 쟁점·보정 방향 회의록 |
| D8–D10 | 국내 출원 명세서/청구항 검수 | 출원 승인본, 공지예외 서류 |

실제 출원일은 변리사 일정과 공개일 감사 결과로 정하되, “12개월째 되는 날”까지 기다리지 않는다.

### 국내 출원 후

| 시점 | 결정 |
|---:|---|
| M+1 | 누락 실시예·오류 검토, 후속 개선 발명 분리 |
| M+3 | 제품 roadmap와 침해 탐지 가능성 평가 |
| M+6 | 추가 선행기술조사, 해외 시장·예산 1차 결정 |
| M+9 | PCT 초안 시작, 국가 후보 shortlist |
| M+10 | 국내 최초 명세서 support와 번역 용어 확정 |
| M+11 | PCT/Paris 최종 승인 및 제출 준비 |
| M+12 전 | 우선권 주장 해외출원 완료 |

## 9. 역할과 책임

| 역할 | 책임 |
|---|---|
| 기술책임자 | 발명 범위, 동작 정확성, 대안 실시예 검토 |
| 발명자 | 착상·기여 내용 확인, 발명신고서 서명 |
| 개발자 | 재현 가능한 시험, source line, commit·release 증거 제공 |
| IP 담당/대표 | 출원인, 예산, 국가, 영업비밀과 공개 정책 결정 |
| 변리사 | 정식 검색 검토, 명세서·청구항 작성, 공지예외·우선권 절차 수행 |

## 10. 변리사 전달 폴더 구조

```text
01_invention_disclosure/
02_source_snapshot/
03_architecture_and_drawings/
04_test_results/
05_prior_art/
06_claim_charts/
07_public_disclosure_evidence/
08_inventor_and_ownership/
09_draft_and_review/
```

각 source snapshot에는 repository URL, commit hash, 파일 hash와 추출일을 적는다. 실제 고객명·장치주소·비밀번호는 제거한다.

## 11. 출원 직전 승인표

- [ ] 발명자 누락이 없는가
- [ ] 회사와 발명자 사이 권리승계 근거가 있는가
- [ ] 독립항의 모든 구성이 명세서와 도면에 기재됐는가
- [ ] protocol-neutral 실시예와 Modbus 구체 실시예가 모두 있는가
- [ ] 정상뿐 아니라 timeout·거부·재접속 실시예가 있는가
- [ ] 공개일과 공개된 구성이 정확히 매핑됐는가
- [ ] 공지예외 취지와 증명서류 제출 일정이 잡혔는가
- [ ] 한국 출원 전에 새 코드·문서를 public push하지 않았는가
- [ ] 영문 핵심 용어를 일관되게 정했는가
- [ ] 해외 확장 12개월 deadline이 캘린더에 등록됐는가
- [ ] FTO 조사와 특허성 조사를 혼동하지 않았는가

## 12. 명확한 한계

이 기획서는 실제 출원을 시작할 수 있는 기술 패키지다. 다만 다음은 등록 변리사 또는 특허법인의 최종 판단이 필요하다.

- 신규성·진보성에 관한 법률 의견
- 공지예외 적용 가능성과 증명 형식
- 발명자·출원인 및 직무발명 권리관계
- 국가별 자기 공개 grace period
- 최종 청구항 문언과 분할·우선권 전략
- 제3자 특허 침해 가능성에 관한 FTO 의견
