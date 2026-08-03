# Going Library 특허 발명 포트폴리오

- 기준일: 2026-08-03
- 대상 저장소: Going Library
- 문서 성격: 기술·출원 전략 검토용 발명 목록
- 중요: 이 문서는 변리사의 신규성·진보성·권리범위 검토를 대체하지 않는다.

## 1. 기록 원칙

이 문서는 기술을 “국내용”과 “해외용”으로 배타적으로 나누지 않는다. 모든 후보는 국내 출원을 기본 경로로 검토할 수 있고, 그중 해외 사업성, 권리범위, 공개 이력 및 선행기술 상황이 좋은 후보를 PCT 또는 개별국 출원으로 확장한다.

표의 의미는 다음과 같다.

| 표시 | 의미 |
|---|---|
| 국내 | 대한민국 출원 후보로서의 상대적 가치 |
| 해외 확장 | 국내 우선출원 후 PCT·미국·유럽·일본·중국 등으로 확장할 상대적 가치 |
| 구현 | 현재 저장소에 구현된 정도 |
| 공개 위험 | GitHub, NuGet, 문서, 시연 등 선출원 전 공개로 신규성이 훼손되었을 가능성 |
| 등급 A | 독립 출원 또는 핵심 독립항으로 우선 검토 |
| 등급 B | 결합 발명, 종속항 또는 보강 후 별도 출원 검토 |
| 등급 C | 단독 출원은 약하고 다른 발명의 실시예·종속항으로 활용 |

“해외 확장 높음”은 해외 등록 가능성을 보장한다는 뜻이 아니다. 정식 선행기술조사와 청구항 대조 전의 기술적 우선순위다.

## 2. 결론과 우선순위

현재 코드에서 바로 발명신고서 작성에 들어갈 최우선 묶음은 세 가지다.

1. **산업 제어장치의 확인응답 캐시와 HMI 보류 명령을 결합한 제어·표시 동기화**
2. **속성 표지에 의해 다형 객체 그래프, 바인딩식, 리소스 및 화면을 분할 직렬화하는 GUDX 배포 방식**
3. **산업 설비 연결 토폴로지에서 흐름을 재귀 전파하고 복수 유체 혼합을 판정·표시하는 방식**

해외 확장까지 가장 강하게 만들 수 있는 장래 발명은 다음 두 묶음이다. 이 기능들은 출원 전 비공개로 개발해야 한다.

1. **GUDX 화면 바인딩을 분석해 통신 태그와 프로토콜 주소를 추출하고, 인접 주소 병합·주기·우선순위·쓰기 안전 상태를 포함한 통신 스캔 계획을 자동 컴파일하는 방식**
2. **확인응답형 HMI 명령 상태, 다중 운전자 중재, 재접속 중복 방지 및 다중 프로토콜 경로 선택을 결합한 산업 제어 통신 방식**

권장 포트폴리오 구조는 다음과 같다.

| 순번 | 발명군 | 국내 | 해외 확장 | 현재 조치 |
|---:|---|---|---|---|
| KR/PCT-1 | 확인응답형 HMI 제어·표시 동기화 | 매우 높음 | 높음, 단 공개 이력 검토 필수 | 즉시 발명신고서 및 선행기술조사 |
| KR/PCT-2 | GUDX 객체 그래프 직렬화·분할 배포 | 높음 | 중간~높음 | 즉시 발명신고서 및 청구항 차별화 |
| KR/PCT-3 | 흐름 토폴로지 전파·혼합 판정 | 중간~높음 | 조건부 높음 | 실제 센서 비교·이상진단을 결합해 보강 |
| KR/PCT-4 | 화면 정의 기반 통신 스캔 계획 컴파일 | 매우 높음 | 매우 높음 | 비공개 구현 후 출원, 공개 금지 |
| KR/PCT-5 | 다중 HMI 명령 중재·재접속 안전성 | 매우 높음 | 높음 | 비공개 구현 후 출원, 공개 금지 |
| KR/PCT-6 | UI와 통신 계획의 원자적 핫 배포 | 높음 | 높음 | 비공개 구현 후 출원 |

## 3. 현재 구현된 핵심 발명 후보

### P-01. 확인응답형 HMI 제어 및 표시 동기화

- 등급: **A**
- 국내: **매우 높음**
- 해외 확장: **높음, 공개 이력과 선행기술 때문에 신중한 청구항 설계 필요**
- 구현: 핵심 구성요소는 존재하나 아래 폐루프의 두 연결부는 보완 필요; 통신부터 UI까지 이어지는 단일 통합 예제·시험도 아직 부족
- 가칭: **산업 제어장치의 응답 지연을 고려한 확인응답형 HMI 제어 및 표시 동기화 방법 및 시스템**

#### 해결하려는 기술 문제

산업용 HMI가 PLC 값을 주기적으로 읽는 동시에 운전자의 쓰기 명령을 전송하면, 장치 응답 전의 이전 읽기값이 UI에 다시 반영되어 스위치나 슬라이더가 순간적으로 되돌아가는 현상이 생길 수 있다. 반대로 UI 값을 즉시 실제값으로 간주하면 장치가 명령을 거부하거나 통신이 끊겼는데도 명령이 성공한 것처럼 표시될 수 있다.

#### 현재 구현된 결합 동작

1. 운전자가 컨트롤을 조작하는 동안 폴링값의 컨트롤 반영과 역방향 쓰기를 억제한다.
2. 조작이 끝나면 최종값만 소스의 setter로 한 번 전달한다.
3. 수동 쓰기 작업은 자동 읽기 순환보다 먼저 처리된다.
4. 쓰기 API 호출만으로 통신 래퍼의 실제값 캐시를 낙관적으로 변경하지 않는다.
5. 장치의 유효한 쓰기 응답을 수신한 뒤에만 확인값 캐시를 갱신한다.
6. UI는 명령값을 보류값으로 유지하면서 확인값 캐시를 계속 비교한다.
7. 확인값이 명령값과 같아지면 보류 상태를 해제한다.
8. 지정 시간 안에 같아지지 않으면 UI를 장치의 확인값으로 복귀시킨다.

#### 구현상태 정정(2026-08-03 코드 재검증)

- 일반 control 변화 경로는 pending value와 생성시각을 등록하지만, 조작 종료 후 `PendingFlush` 경로는 현재 setter만 호출하고 pending command를 등록하지 않는다. 따라서 위 2번과 6~8번은 현재 하나의 실행경로로 완전히 결합되지 않았으며, 공통 `SendControlValue` 처리로 연결해야 한다.
- RTU parser는 CRC를 검사하지만 response의 slave/function/address/value 전체를 원 요청과 대조하지 않는다. TCP도 MBAP transaction identifier, unit/function 및 echoed payload를 완전히 상관검증하지 않는다. 따라서 위 5번의 “유효한 응답”은 출원 목표 실시형태이며, 현재 구현은 response function에 따라 발생한 event에서 원 `Work`의 주소·값을 사용해 cache를 갱신하는 수준이다.
- 이 후보의 A 등급은 완성된 단일 코드경로라는 뜻이 아니라, 이미 존재하는 UI pending·scheduler·response cache를 위 방식으로 결합했을 때의 권리화 잠재력 평가다. 상세 보완 algorithm은 `technical-principle-01-confirmed-hmi.md`에 기록한다.

#### 코드 근거

- 조작 중 억제, 최종값 flush, 명령값 보류, 일치 판정 및 timeout 복귀: `Going.UI/Controls/GoControl.cs` 344행 이후, 특히 363~470행
- 명령형 양방향 바인딩 API: `Going.UI/Bindings/GoControlBindingExtensions.cs` 61~72행
- 보류 중 복귀 방지와 timeout 시험: `Going.UI.Tests/Bindings/BindingTests.cs` 155~195행
- Modbus RTU의 수동 작업 우선 처리와 자동 순환: `Going.Basis/Communications/Modbus/RTU/ModbusRTUMaster.cs` 408~574행
- RTU 확인응답 기반 캐시와 API: `Going.Basis/Communications/Modbus/RTU/MasterRTU.cs` 78~179행
- TCP 확인응답 기반 캐시와 API: `Going.Basis/Communications/Modbus/TCP/MasterTCP.cs` 72~173행
- 쓰기 호출 때 캐시가 바뀌지 않고 응답 뒤에만 바뀌는 시험: `Going.UI.Tests/Communications/ModbusWrapperZeroBaseTests.cs` 63~119행

#### 청구항의 중심

독립항은 단순한 “Modbus 응답 수신”이나 “양방향 바인딩”이 아니라 다음 폐루프 조합을 중심으로 해야 한다.

> 조작 중 폴링 반영 억제 → 조작 종료 시 최종 명령 생성 → 자동 읽기보다 수동 명령 우선 처리 → 장치 확인응답에 의한 확인값 캐시 갱신 → UI 명령값과 확인값의 일치 여부 판단 → 일치 시 확정, 제한시간 초과 시 확인값으로 복귀

종속항 후보:

- 스위치, 슬라이더, 숫자 입력 등 컨트롤 종류별 조작 종료 판정
- 쓰기 요청의 주소·값·시퀀스 식별자와 응답의 상관관계 확인
- 읽기 응답과 쓰기 응답 중 어느 것이 먼저 도착하는지에 따른 상태 전이
- 재시도 중 동일 명령 중복 전송 방지
- 통신 품질, timeout, 거부 응답에 따른 시각 상태 표현
- 여러 레지스터를 한 명령으로 쓸 때 전부 또는 일부의 확인 상태 판정
- Modbus RTU/TCP 이외 CNet, MC, MQTT에 대한 프로토콜 독립 실시예

#### 선행기술 위험과 차별화

- 일반적인 산업 HMI 양방향 데이터 바인딩과 낙관적·비관적 바인딩은 오래된 기술이다. `US20040021679A1`은 데이터 소스와 표시 요소 사이의 양방향 전달 및 optimistic/pessimistic binding을 설명한다.
- `EP2541354A3`도 그래픽 요소와 컨트롤러 데이터의 바인딩을 다룬다.
- Modbus 쓰기 함수의 정상 응답이 요청 내용을 되돌려 주는 동작 자체는 프로토콜 표준이다.

따라서 차별점은 **산업용 장치의 확인값 캐시, HMI 조작 억제, 우선순위 통신 스케줄, 보류 표시 및 timeout 복귀가 하나의 상태 기계로 작동하는 전체 결합**이어야 한다.

#### 출원 전 보강

- 시뮬레이션 Modbus slave와 실제 GoOnOff/GoSlider를 연결한 end-to-end 시험 추가
- 요청, 이전 폴링값, 쓰기 응답, 후속 읽기값의 도착 순서를 바꾼 상태 전이 시험 추가
- 장치 거부, timeout, 재접속, 중복 응답, 늦게 도착한 응답 시험 추가
- 종래 방식과 비교한 깜박임 횟수, 오표시 시간, 중복 명령 횟수 측정

### P-02. GUDX 속성 기반 다형 객체 그래프 직렬화 및 분할 배포

- 등급: **A-**
- 국내: **높음**
- 해외 확장: **중간~높음, 범용 직렬화가 아니라 HMI 배포의 기술효과로 한정할 것**
- 구현: 구현 및 다수 회귀시험 존재
- 가칭: **속성 표지형 다형 객체 그래프의 화면·리소스 분할 직렬화 및 복원 방법**

#### 핵심 구성

- 리플렉션과 속성 표지로 스칼라(P1), 동종 컨트롤 목록(P2), 셀 위치 목록(P3), wrapper 목록(P4), keyed map(P5), 단일 자식(B1), 외부 리소스(B2)를 구분한다.
- 모든 컬렉션을 속성명 그룹 요소 아래에 배치해 서로 다른 컬렉션의 자식 태그 충돌과 잘못된 소비를 방지한다.
- 다형 wrapper와 일반 숫자형의 타입 별칭·타입 레지스트리를 자동 구성한다.
- 페이지, 윈도우, 이미지, 폰트를 master 문서와 개별 파일로 분할 저장하고 다시 하나의 설계 객체 그래프로 복원한다.
- 바인딩 식과 재사용 컴포넌트의 인스턴스 의미를 직렬화 왕복 후에도 보존한다.
- 셀 위치, span, 객체 식별자와 dictionary key를 보존한다.

#### 코드 근거

- 전체 알고리즘과 `WriteAny`: `Going.UI/Gudx/GoGudxConverter.cs` 19~283행
- 화면·윈도우·리소스 분할 저장: 같은 파일 327~514행
- P2~P5/B1/B2 쓰기 dispatch: 같은 파일 576~744행
- P2~P5/B1/B2 읽기 dispatch: 같은 파일 856~1105행
- 패턴 설계 명세: `Going.UI/Gudx/PATTERNS.md`

#### 청구항의 중심

- 복수 종류의 자식 속성을 속성 표지로 판별하는 단계
- 속성명 그룹을 생성해 동일 부모 아래 이종 컬렉션의 태그 충돌을 방지하는 단계
- 다형 객체의 런타임 타입과 셀·key·식별자 정보를 함께 보존하는 단계
- 객체 그래프 중 화면·윈도우·바이너리 리소스를 별도 파일로 추출하고 master 참조로 치환하는 단계
- 복원 전 컴포넌트·타입을 등록하고 참조와 바인딩 식을 재결합하는 단계

#### 선행기술 위험과 보강

XML serializer, XAML, 객체 그래프 직렬화, 외부 리소스 분할은 각각 알려진 기술이다. 독립항은 단순한 “리플렉션 직렬화”가 아니라 **산업 HMI의 다형 컨트롤 그래프와 선언형 바인딩을 무손실로 분할 배포하고, 컬렉션 충돌을 구조적으로 방지하는 조합**으로 좁혀야 한다. 해외 확장 전에는 XAML/BAML, WPF serialization, Qt UI, SCADA 화면 배포 특허와 청구항 단위 비교가 필요하다.

### P-03. 설비 연결 토폴로지의 흐름 전파 및 유체 혼합 판정

- 등급: **B+**, 실제 센서 이상진단 결합 시 **A- 가능**
- 국내: **중간~높음**
- 해외 확장: **조건부 높음**
- 구현: 시각 시뮬레이션과 편집 기능 구현
- 가칭: **산업 설비 연결 토폴로지에 따른 유체 흐름 전파, 혼합 판정 및 상태 시각화 방법**

#### 현재 구현

- 펌프·밸브·파이프 등 흐름 객체의 포트를 연결 그래프로 구성한다.
- 시작 포트에서 연결을 재귀 탐색하고, 경로상 객체의 `IsFlow` 상태에 따라 흐름을 전파 또는 차단한다.
- Tee/Cross 등 복수 경로에서 서로 다른 유체가 합류하면 혼합색을 판정한다.
- 흐름 방향에 맞추어 배관 위 버블 애니메이션을 진행한다.
- 편집기에서 연결점과 경로 anchor를 수정하고 GUDX로 보존할 수 있다.

#### 구현상태 정정(2026-08-03 코드 재검증)

- 현재 재귀에는 visited port/edge 집합이 없어 cyclic topology의 종료가 보장되지 않는다.
- sibling branch가 하나의 mutable `PropagationInfo`를 공유하므로 한 분기의 차단 결과가 뒤 분기에 전파될 수 있다.
- Tee/Cross 혼합은 현재까지 incident connection에 기록된 색을 순차 참조하므로 source 처리 순서와 무관한 provenance-set 고정점 계산이 아니다.
- 따라서 순환 안전성, 분기 독립성, 최초 합류점과 오염 sink 추적은 현재 구현이 아니라 `technical-principle-03-flow-system.md`의 보완 실시형태다.

#### 코드 근거

- 시스템 설명과 데이터 구조: `Going.UI/FlowSystem/FsFlowSystemPanel.cs` 20~177행
- 그래프 재구성: 같은 파일 478행 이후
- 흐름 재귀 전파 및 혼합 판정: 같은 파일 594~681행
- 연결 경로와 애니메이션: 같은 파일 696~920행

#### 권리화 방향

현재의 화면 애니메이션만으로는 그래프 탐색·공정 시각화 선행기술과 가까울 수 있다. 다음을 결합하면 기술효과가 강해진다.

- 화면 토폴로지에서 계산한 예상 흐름과 실제 유량·압력·밸브 피드백의 비교
- 막힘, 누출, 역류, 잘못 열린 밸브, 펌프 공회전의 원인 위치 추정
- 혼합 금지 유체의 오염 가능 경로와 최초 합류점 추적
- 센서 결측 때 토폴로지 제약으로 상태를 추론하고 신뢰도를 산출

### P-04. 컴포넌트 스코프를 갖는 선언형 HMI 바인딩

- 등급: **B**
- 국내: 중간
- 해외 확장: 낮음~중간, P-02 또는 F-01의 종속항 권장
- 구현: 완료

GUDX 속성에 `{path}` 또는 `{path:format}`을 보존하고, 컴포넌트마다 데이터 root를 바꿔 같은 템플릿을 다른 데이터에 재사용한다. 경로 접근자는 expression tree로 컴파일하고 `(root type, path)`별로 캐시한다. `Going.UI/Bindings/GudxBinder.cs`, `Going.UI/Bindings/GudxBindingExpression.cs`가 근거다.

일반적인 선언형 바인딩과 scoped data context는 오래된 기술이므로 단독 독립항보다 다음 결합이 낫다.

- P-02의 직렬화 왕복 후 스코프 복원
- F-01의 바인딩 경로에서 통신 태그·주소 추출
- P-01의 명령형 바인딩 상태 기계 적용

### P-05. 매개변수형 재사용 HMI 컴포넌트

- 등급: **B-**
- 국내: 중간
- 해외 확장: 낮음~중간
- 구현: 완료

컴포넌트 정의에 parameter와 복제 가능한 root 트리를 두고, 인스턴스 생성 때 식을 보존·치환하며 각 인스턴스에 독립된 바인딩 스코프를 부여한다. 일반 UI template/component 선행기술이 강하므로 P-02의 객체 그래프 배포 또는 F-01의 통신 스캔 컴파일과 결합한다.

### P-06. 컬렉션 데이터용 ItemList 템플릿 바인딩

- 등급: **B-**
- 국내: 중간
- 해외 확장: 낮음
- 구현: 완료

컬렉션 항목마다 HMI 템플릿 트리를 복제하고 항목 스코프를 연결하며, 컬렉션 변화 때 dirty 상태를 이용해 화면 트리를 재구축한다. WPF ItemsControl류 선행기술이 강하므로 단독 출원보다 P-02/P-04/F-01의 종속항이 적합하다. 근거는 `Going.UI/Controls/GoItemList.cs`와 2026-06-06 설계 문서다.

## 4. 통신 및 제어 기반 기술 전수 분류

통신 모듈에는 쓸 만한 자산이 많지만, 프로토콜 구현 그 자체보다 UI·캐시·스케줄·안전 상태를 결합할 때 특허성이 커진다.

| ID | 기술 | 구현 근거 | 단독 등급 | 활용 권고 |
|---|---|---|---|---|
| C-01 | 수동 명령 우선·자동 폴링 순환 작업 스케줄러 | Modbus RTU/TCP, CNet, MC의 WorkQueue/ManualWorkList/AutoWorkList | C+ | P-01 또는 F-01에 결합 |
| C-02 | 작업별 timeout·retry와 자동 재접속 | 각 master 통신 루프 | C | 명령 안전 상태의 종속항 |
| C-03 | 쓰기 응답 후에만 갱신되는 확인값 캐시 | `MasterRTU.cs`, `MasterTCP.cs` | B, 결합 시 A | P-01 핵심 구성 |
| C-04 | zero-based 주소와 장치별 bit/word cache wrapper | Modbus wrapper의 Devices/Mems | C+ | 프로토콜 추상화 실시예 |
| C-05 | Modbus RTU/TCP master/slave와 FC1/2/3/4/5/6/15/16/26 처리 | Modbus 폴더 전체 | C | 표준 구현이므로 독립항 부적합 |
| C-06 | Modbus slave 영역을 BitMemory/WordMemory에 직접 매핑 | `SlaveRTU.cs`, `SlaveTCP.cs` | C+ | 테스트 장치·edge gateway 결합 |
| C-07 | LS CNet 프레임/BCC/ACK·NAK 및 작업 스케줄 | `Communications/LS/CNet.cs` | C | 표준 프로토콜 동작 |
| C-08 | Mitsubishi MC 프레임/checksum/ACK·NAK 및 작업 스케줄 | `Communications/Mitsubishi/MC.cs` | C | 표준 프로토콜 동작 |
| C-09 | MQTT 자동 재접속·재구독·alive publish | `Communications/Mqtt/MQClient.cs` | C | F-05/F-06/F-07에 결합 |
| C-10 | TextComm STX/ETX/DLE stuffing과 checksum packet | `Communications/TextComm/TextCommPacket.cs` 및 RTU/TCP | C | 표준 framing, 독립항 부적합 |
| C-11 | 하나의 byte 배열에 word/dword/int/float/string/bit view 제공 | `Memories/WordMemory.cs`, `Memories/WordRef.cs` | C+ | protocol-neutral tag model로 보강 |
| C-12 | bit-packed memory 및 aligned access | `Memories/BitMemory.cs` | C | 일반 기술 |
| C-13 | ABCD/BADC/CDAB/DCBA endian 변환 | `Utils/EndianParser.cs` | C | F-01 주소·형식 추론의 일부 |
| C-14 | 입력 안정시간 판정 및 채터링 제거 | `Measure/Stable.cs`, `Measure/Chattering.cs` | C | 알람 품질·명령 안전 종속항 |
| C-15 | 마지막 수신시각과 연결 생존상태 관리 | 통신 master/client 계층 | C | F-06 신뢰도 계산에 결합 |

### 통신 기술에 대한 판단

- **단순 Modbus/MQTT/CNet/MC 구현은 큰 특허가 아니다.** 공개 표준과 상용 제품이 오래전부터 존재한다.
- **수동 우선 스케줄러만으로도 약하다.** 산업 통신에서 명령을 폴링보다 우선하는 것은 자연스러운 설계다.
- **확인값 캐시만으로도 약할 수 있다.** Modbus 쓰기 응답 형식 자체는 표준이다.
- 그러나 C-01, C-03과 P-01의 조작 억제·보류 표시·timeout 복귀가 결합되면 물리 장치의 오명령·오표시를 줄이는 하나의 제어 상태 기계가 된다. 현재 저장소의 통신 관련 최강 후보는 이 결합이다.

## 5. UI·편집기·렌더링 기반 기술 전수 분류

| ID | 기술 | 단독 등급 | 활용 권고 |
|---|---|---|---|
| U-01 | SkiaSharp 기반 플랫폼 독립 HMI control tree와 그리기 | C+ | 임베디드 성능 최적화 수치가 있으면 보강 |
| U-02 | WinForms/OpenTK 공통 UI 코어와 플랫폼 adapter | C | 일반 adapter 패턴 |
| U-03 | 40종 이상의 산업용 control family | C | 개별 외형보다 특정 제어 알고리즘과 결합 |
| U-04 | 비주얼 편집기의 drag/drop·property 편집·C# 생성 | C+ | 자동 통신 계획 생성과 결합 시 F-01 |
| U-05 | table/grid cell 위치와 span을 보존하는 control collection | B- | P-02의 P3 종속항 |
| U-06 | 다형 graph series/column/tree wrapper 복원 | B- | P-02의 P4 종속항 |
| U-07 | 이미지·폰트 외부화와 page/window 분할 | B | P-02 핵심 구성 |
| U-08 | 재사용 component 및 parameter 치환 | B- | P-05 또는 P-02 종속항 |
| U-09 | ItemList와 item-scope template 복제 | B- | P-06 또는 F-01 종속항 |
| U-10 | vector shape family와 path 렌더링 | C | 일반 그래픽 기술 |
| U-11 | chart, trend, sparkline, gauge, meter 시각화 | C | 특정 다운샘플링·이상표시가 추가되면 재평가 |
| U-12 | image canvas, 확대·이동·선택·편집 | C | 일반 편집기 기술 |
| U-13 | touch keyboard와 임베디드 입력 처리 | C | 독특한 오류방지·접근성 로직이 추가되면 재평가 |
| U-14 | theme, layout, container, dialog/window 관리 | C | 일반 UI framework 기술 |

## 6. 비공개 개발 후 출원할 장래 발명

이 절의 내용이 아직 GitHub·NuGet·공개 문서에 없다면, 구현 코드와 상세 설계는 출원 전 공개하지 않는다. 저장소가 공개 저장소라면 별도 private 저장소, 접근통제된 문서 및 NDA를 사용한다.

### F-01. 화면 바인딩 기반 통신 스캔 계획 컴파일

- 등급: **A+**
- 국내: **매우 높음**
- 해외 확장: **매우 높음**
- 상태: 제안; 현재 구성요소는 있으나 전체 compiler는 미구현

#### 구체안

1. GUDX 바인딩에 논리 태그 또는 `protocol/device/area/address/type/endian` 정보를 선언한다.
2. compiler가 모든 page, window, component 및 ItemList template의 바인딩을 분석한다.
3. getter와 setter의 존재로 read-only, write-only, command tag를 구분한다.
4. 데이터형과 endian 정보를 이용해 필요한 word/bit 범위를 계산한다.
5. 같은 장치·함수·주기·우선순위의 인접 주소를 하나의 block read로 병합한다.
6. 중첩·중복 태그를 제거하고 응답 payload에서 개별 typed value를 역매핑한다.
7. page 가시성, alarm/safety 예외 및 command pending 상태를 반영해 runtime poll plan을 만든다.
8. plan에서 통신 wrapper와 HMI binding getter/setter/state machine을 자동 생성한다.

#### 독립항의 차별 포인트

“HMI 태그를 자동 생성”이나 “구독 주기를 설정”처럼 넓게 청구하면 선행기술에 걸릴 가능성이 크다. 다음 결합을 중심으로 한다.

- 직렬화된 화면 객체 그래프의 바인딩 식을 입력으로 사용
- 다형 component·ItemList 스코프를 실제 태그 집합으로 전개
- 프로토콜 주소 범위를 병합해 block read plan 생성
- 화면 가시성과 안전 태그 예외로 poll tier를 변경
- 명령형 binding에는 P-01의 확인응답 상태를 자동 부여
- UI 파일과 통신 계획의 버전·hash를 함께 관리

#### 기술효과 측정

- 요청 frame 수와 wire byte 감소율
- page 전환 후 첫 유효값 표시 지연
- hidden page polling 감소율
- safety/alarm tag 최대 갱신 지연 보장
- 수작업 주소 중복·누락·형식 오류 감소

### F-02. 화면 가시성·안전도 기반 적응형 폴링

- 등급: **A-**, F-01과 합치면 더 강함
- 국내: 높음
- 해외 확장: 높음

현재 page의 태그는 고속, 숨은 page는 저속 또는 중지하되 alarm·interlock·safety 태그는 가시성과 무관하게 고속으로 유지한다. 화면 전환을 예측하거나 전환 이벤트를 받으면 새 page 태그를 prefetch하고, 닫힌 window의 작업은 제거한다. 네트워크 지연과 오류율에 따라 비안전 태그의 주기를 먼저 늘리고, 인접 주소 block을 재편성한다.

OPC UA에도 sampling interval, subscription, queue와 triggering model이 존재하므로 단순 가변 주기는 약하다. **객체 그래프 가시성, 안전 예외, 주소 병합, 전환 prefetch, pending 명령 유지의 결합**이 필요하다.

### F-03. 다중 HMI·운전자 명령 중재와 재접속 안전성

- 등급: **A**
- 국내: 매우 높음
- 해외 확장: 높음

#### 구성 후보

- 논리 tag별 lease/ownership과 만료시간
- 운전자 권한, 설비 안전등급, local/remote 모드에 따른 우선순위
- command sequence/idempotency key 및 장치 응답 상관관계
- 연결 끊김 뒤 재전송 전 장치 확인값과 명령 이력을 대조
- 이미 적용된 명령의 중복 실행 방지
- 충돌한 명령을 실행하지 않고 각 HMI에 확정·거부·대기 상태 통지
- 비상정지나 safety command의 별도 경로와 감사 로그

### F-04. FlowSystem과 실제 I/O를 결합한 공정 이상 진단

- 등급: **A-**
- 국내: 높음
- 해외 확장: 높음

P-03의 예상 흐름 그래프를 실제 pump/valve feedback, flow, pressure, level sensor와 비교해 막힘·누출·역류·오조작을 판정한다. 단순 임계치가 아니라, 토폴로지에서 가능한 경로와 불가능한 경로를 계산하고 모순을 일으킨 최소 edge/node 집합을 원인 후보로 출력한다. 서로 다른 유체의 예상·실제 합류점을 비교해 오염 위험 경로도 추정한다.

### F-05. UI와 통신 계획의 원자적 핫 배포

- 등급: **A-**
- 국내: 높음
- 해외 확장: 높음

#### 구성 후보

1. 기존 GUDX와 새 GUDX의 control/tag/resource diff를 계산한다.
2. 새 통신 scan plan과 binding graph를 실행 전 준비하고 검증한다.
3. 처리 중인 명령과 pending 확인 상태가 안전 지점에 도달할 때까지 기다린다.
4. UI graph, binding graph와 poll plan을 하나의 version으로 원자적으로 전환한다.
5. 실패하면 세 요소를 함께 이전 version으로 rollback한다.
6. 유지 가능한 확인값 cache, control state와 alarm state는 새 version으로 승계한다.

단순 UI hot reload가 아니라 **물리 장치에 연결된 통신 계획과 pending command의 일관성을 보장하는 배포 transaction**이 중심이다.

### F-06. 다중 프로토콜 확인값의 품질 기반 선택과 failover

- 등급: **A-**
- 국내: 높음
- 해외 확장: 높음

하나의 논리 tag를 Modbus, MQTT, CNet, MC의 하나 이상 물리 주소에 매핑하고, 값마다 timestamp, quality, source, sequence, confidence를 붙인다. 읽기 경로 장애 때 다른 경로로 전환하되, 쓰기는 lease를 가진 단일 경로로만 전송한다. 복수 경로 값이 다르면 단순 최신값 선택이 아니라 장치 응답 확인 여부, 시간 오차, 통신 상태와 source 우선순위로 확정값과 충돌 상태를 계산한다.

### F-07. 오프라인 산업 명령 queue와 충돌 조정

- 등급: **B+**
- 국내: 높음
- 해외 확장: 중간~높음

연결 단절 중 명령을 무조건 재전송하지 않고, 명령 종류별 만료시간, idempotency, 선행조건, 예상 이전값을 저장한다. 재접속 때 실제 확인값과 비교해 이미 적용됨, 아직 적용 가능, 충돌, 만료로 분류하고 안전한 명령만 실행한다. F-03과 함께 청구하는 것이 좋다.

### F-08. 통신 품질 전파형 HMI 표시

- 등급: **B+**
- 국내: 중간~높음
- 해외 확장: 중간

각 표시값에 단순 값뿐 아니라 confirmed/pending/stale/substituted/conflict/bad 상태와 age를 함께 전파한다. component 내부 파생값과 graph series에도 quality를 합성하고, stale 값이 정상값처럼 보이지 않도록 시각 표현과 운전자 입력 가능 여부를 자동 결정한다. OPC UA quality/status 선행기술이 강하므로 GUDX binding compiler 및 P-01과의 구체적 결합이 필요하다.

### F-09. 통신 부하·오류 기반 스캔 block 자동 재편성

- 등급: **B+**
- 국내: 중간~높음
- 해외 확장: 중간~높음

인접 주소를 병합한 block에서 특정 구간의 오류나 timeout이 반복되면 block을 분할해 장애 주소를 격리하고, 안정 구간은 다시 병합한다. 장치별 최대 PDU, 응답시간 분포, 오류율, 안전 태그의 deadline을 제약조건으로 poll plan을 온라인 재최적화한다. 일반 adaptive polling과 구분되도록 화면 수요 및 산업 프로토콜 주소 block을 함께 청구한다.

### F-10. 통신·화면 정의 공동 검증과 디지털 시운전

- 등급: **B+**
- 국내: 중간~높음
- 해외 확장: 중간

GUDX의 모든 write binding에서 장치 주소, 허용 범위, 단위, 권한과 feedback 주소를 추출하고, slave simulator를 자동 구성한다. 실제 장치 연결 전 명령→응답→화면 확정/rollback의 모든 상태 경로를 생성해 시험한다. 단순 test generation이 아니라 산업 통신 주소와 HMI 상태 기계의 상호 일관성 검증으로 한정한다.

## 7. 공개 이력과 출원 시계

### 저장소 Git 이력에서 확인된 구현 시점

아래 날짜는 **local commit 날짜**다. 실제 GitHub push, release, NuGet 배포, 문서 게시 또는 시연 날짜와 다를 수 있으므로 법적 공개일의 증거로 그대로 사용하면 안 된다.

| 기술 | 확인된 local commit | 보수적 국내 검토 기한의 기준점 |
|---|---:|---:|
| 초기 Modbus/CNet 계열 | 2025-03-31~2025-04-01 | 이미 12개월 경과 가능성 큼 |
| MQTT/TextComm | 2025-04-02 | 이미 12개월 경과 가능성 큼 |
| FlowSystem | 2026-04-12 | 실제 공개가 같다면 2027-04-12 전 |
| GUDX 핵심 패턴·분할 저장 | 2026-04-29 | 실제 공개가 같다면 2027-04-29 전 |
| 조작 억제형 양방향 binding | 2026-05-09 | 실제 공개가 같다면 2027-05-09 전 |
| command timeout binding | 2026-05-21 | 실제 공개가 같다면 2027-05-21 전 |
| Modbus 응답 기반 write cache | 2026-05-24 | P-01 전체 결합 기준 보수적으로 2027-05-24 전 |
| 선언형 binding/component/ItemList | 2026-06-06 | 실제 공개가 같다면 2027-06-06 전 |

### 국내 공지예외

지식재산처 안내상 권리자가 출원 전에 공개한 발명은 일정 요건 아래 공개일부터 12개월 이내 출원할 수 있다. 출원서에 공지예외 적용 취지를 적고 원칙적으로 출원일부터 30일 이내 증명서류를 제출해야 한다. 실제 사건에서는 공개 주체, 공개된 기술 내용, 날짜 및 증명 방법을 변리사와 확인해야 한다.

특히 다음 자료를 즉시 보존한다.

- GitHub commit·tag·release의 실제 공개 timestamp와 당시 파일 snapshot
- NuGet 각 package version의 게시일과 포함 binary/source
- README, GitHub Pages, 블로그, 영상, 전시 및 고객 시연일
- 외부인에게 전달한 설계서·sample·binary와 NDA 유무
- 각 기능별 최초 공개 내용과 이후 추가된 차별 구성

### 해외 출원

국가마다 자기 공개에 대한 grace period의 범위와 요건이 다르다. 유럽은 일반적인 자기 공개 grace period가 없고, 명백한 남용이나 공인 국제박람회 같은 매우 제한된 예외만 둔다. 이미 공개된 발명은 한국에서 공지예외로 살릴 수 있더라도 유럽 등에서는 신규성 문제가 생길 수 있다. PCT 출원은 공개로 잃은 신규성을 되살리는 제도가 아니다.

따라서 전략은 다음과 같다.

1. **이미 공개된 현재 기술**: 실제 공개일을 조사하고 한국 공지예외 출원을 우선 검토한다. 미국·일본 등은 국가별 요건을 별도 검토하고, 유럽·중국은 보수적으로 본다.
2. **아직 공개되지 않은 보강 기술**: 공개 전에 한국 우선출원한다.
3. 한국 출원 후 해외 가치가 있는 발명은 우선일로부터 12개월 안에 PCT 또는 개별국 출원을 결정한다.
4. 출원 전에는 public GitHub, NuGet, 발표, 영업자료 및 NDA 없는 고객 시연에 넣지 않는다.

## 8. 대표 선행기술과 표준

이 목록은 정식 선행기술조사의 결과가 아니라, 넓은 청구항이 위험하다는 것을 확인하기 위한 1차 자료다.

| 분야 | 자료 | 포트폴리오에 주는 의미 |
|---|---|---|
| 산업 HMI binding | [US20040021679A1 – Human machine interface](https://patents.google.com/patent/US20040021679A1/en) | 양방향 전달, optimistic/pessimistic binding, XML의 binding 정의가 오래된 기술임 |
| 그래픽-제어기 binding | [EP2541354A3 – Binding graphic elements to controller data](https://patents.google.com/patent/EP2541354A3/en) | 그래픽 요소와 controller data binding을 넓게 청구하기 어려움 |
| HMI 자동 생성 | [EP3929685A1 – HMI program file generation from CAD](https://patents.google.com/patent/EP3929685A1/en) | smart tag와 controller I/O를 이용한 HMI 생성이 존재 |
| HMI 자동 생성 | [US11422833B1 – Automatic generation of HMI in a vision system](https://patents.google.com/patent/US11422833B1/en) | HMI element와 tag 자동 생성 자체는 위험 |
| 산업 통신 topology | [EP4332705A1 – Industrial automation system topology](https://patents.google.com/patent/EP4332705A1/en) | 설정자료에서 장치와 통신 경로를 추론하는 넓은 개념이 존재 |
| OPC UA monitored item | [OPC UA Part 4, MonitoredItems](https://reference.opcfoundation.org/Core/Part4/v105/docs/5.13) | sampling interval, queue, trigger와 subscription은 표준 영역 |
| Modbus 쓰기 응답 | [Modbus Application Protocol V1.1b3](https://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf) | FC5/FC6 등의 요청·응답 자체는 표준 영역 |

정식 조사 때 사용할 키워드·분류 예:

- HMI command pending, confirmed value, write acknowledgement, feedback synchronization, rollback
- PLC polling scheduler, manual command priority, adaptive scan, address coalescing
- HMI binding compile, tag extraction, communication plan generation
- industrial UI object graph serialization, split deployment, hot reload transaction
- process topology flow inference, leak/blockage/backflow diagnosis, fluid contamination path
- multi-HMI command arbitration, idempotent industrial command, reconnect reconciliation

## 9. 출원 단위와 명세서 구성 제안

### 1차 국내 출원

#### 출원 1: P-01 확인응답형 HMI

- 독립항: 방법, 시스템, 컴퓨터 판독가능 기록매체/프로그램
- 실시예: Modbus RTU, Modbus TCP, 스위치, 슬라이더, multi-register write
- 필수 도면: 전체 구성도, 상태 전이도, 시간 순서도, 실패·복귀 흐름도

#### 출원 2: P-02 GUDX

- 독립항: 직렬화 방법, 복원 방법, 배포 시스템
- 실시예: P1~P5/B1/B2, page/window split, resource extraction, component/binding roundtrip
- 필수 도면: attribute dispatch tree, object graph-to-files 변환도, 복원 순서도

#### 출원 3: P-03 FlowSystem

- 현재 시각화만으로 바로 내기보다 F-04의 실제 I/O 이상진단을 보강한 후 출원하는 편이 유리
- 공개 시계 때문에 현재 구현을 먼저 국내 출원하고 후속 개선을 우선권 주장 또는 별도 출원으로 가져가는 방안도 변리사와 검토

### 2차 국내 우선출원 후 해외 확장

- F-01과 F-02는 하나의 명세서에 충분히 기재하되 독립항을 분리할 수 있다.
- F-03과 F-07은 명령 안전·중재 발명군으로 묶을 수 있다.
- F-05는 배포 transaction으로 독립성이 높아 별도 출원이 적합하다.
- F-04는 공정 이상진단으로 UI framework보다 산업 적용 분야에 맞춰 별도 출원한다.
- F-06/F-08/F-09는 통신 신뢰성 발명군으로 묶거나 시장성을 보고 분할한다.

## 10. 발명신고서에 반드시 남길 실험·증거

각 발명은 “코드가 다르다”보다 “컴퓨터 또는 산업 장치가 기술적으로 더 잘 작동한다”는 자료가 중요하다.

| 발명 | 권장 측정치 |
|---|---|
| P-01 | 오표시 지속시간, UI 반전/깜박임 횟수, 중복 명령 수, 장치 거부 시 잘못된 성공 표시 비율 |
| P-02 | 파일 크기, load 시간, 변경 page만 배포한 byte 수, 컬렉션 충돌·복원 오류율 |
| P-03/F-04 | 이상 탐지시간, 원인 후보 수, 오탐·미탐, 오염 경로 탐지 정확도 |
| F-01/F-02 | frame 수, wire byte, CPU, page 표시 지연, safety tag deadline 준수율 |
| F-03/F-07 | 중복 실행 방지율, 충돌 명령 차단율, 재접속 후 잘못 실행된 명령 수 |
| F-05 | 배포 중 불일치 시간, rollback 성공률, pending command 유실 수 |
| F-06/F-08 | stale/충돌 검출시간, failover 시간, 잘못된 source 선택률 |
| F-09 | 오류 주소 격리시간, 정상 주소 갱신 지연, 재병합 후 frame 감소율 |

발명자별 기여도, 최초 착상일, 설계 변경 이유, 시험 결과와 회의 기록도 함께 보존한다.

## 11. 실행 체크리스트

- [ ] GitHub·NuGet·문서·시연의 실제 최초 공개일을 기능별로 확정
- [ ] P-01의 통신-HMI end-to-end sample과 상태 전이 시험 작성
- [ ] P-01, P-02에 대한 KIPRIS·Google Patents·Espacenet 청구항 단위 조사
- [ ] P-03을 실제 I/O 이상진단과 결합할지 결정
- [ ] P-01과 P-02 발명신고서 초안 및 도면 작성
- [ ] 국내 공지예외 적용 여부와 증명자료를 변리사에게 전달
- [ ] F-01~F-10을 private 발명 backlog로 이전하고 접근권한 제한
- [ ] 공개 전에 F-01의 최소 prototype과 정량 비교시험 수행
- [ ] 각 국내 우선출원일부터 12개월짜리 해외출원 decision date 등록

## 12. 법·제도 참고자료

- [지식재산처 – 컴퓨터관련 발명](https://www.kipo.go.kr/ko/kpoContentView.do?menuCd=SCD0200238): 소프트웨어 정보처리가 하드웨어를 통해 구체적으로 구현되는 경우의 발명 성립 안내
- [지식재산처 – 공지예외주장 제도](https://www.kipo.go.kr/ko/kpoContentView.do?menuCd=SCD0200239): 자기 공개 후 12개월, 출원서 기재 및 증명서류 제출 요건 안내
- [WIPO – How to Protect Inventions through Patents](https://www.wipo.int/en/web/patents/protection): 선출원 전 공개가 신규성을 훼손할 수 있으며 특허는 속지권이라는 안내
- [EPO – EPC Article 55](https://www.epo.org/en/legal/epc/2020/a55.html): 명백한 남용 또는 공인 국제박람회에 관한 제한적 비불리 공개 규정

---

### 관리 메모

이 문서의 “장래 발명” 절은 공개 저장소에 commit하는 것 자체가 공개가 될 수 있다. 현재 작업 폴더가 public repository와 연결되어 있다면, 실제 commit/push 전에 해당 절을 private 발명관리 문서로 옮기거나 변리사와 공개 영향을 확인한다.
