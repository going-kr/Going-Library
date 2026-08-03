# KIPRIS 국내 선행기술 1차 검색 기록

- 조사일: 2026-08-03
- 대상: P-01 확인응답형 HMI, P-02 GUDX, P-03/F-04 FlowSystem, F-01/F-09 통신 스캔 계획
- 데이터베이스: KIPRIS 국내 특허·실용신안
- 성격: 일반 키워드 기반의 1차 후보 발굴 및 기술적 의견
- 한계: 변리사의 신규성·진보성 또는 FTO 법률의견을 대체하지 않는다.

## 1. 결론과 포트폴리오 의견

P-01의 목표 결합 전체와 동일한 문헌은 이번 일반검색 상위 결과에서 확인하지 못했다. 그러나 PLC-HMI 데이터 동기화, XML/tag map 기반 화면 갱신과 조작권 관리는 이미 알려져 있다. P-02는 HMI 설명 데이터, component property, XML/SVG/HTML/CSS/JSON 생성과 화면 복원 선행기술이 많다. P-03의 단순 유체 흐름·기동절차 시각화도 등록 사례가 있어 현재 구현만으로는 약하다.

가장 중요한 발견은 F-01과 매우 가까운 출원 10-2026-0094264이다. 시각 객체의 PLC 주소 바인딩에서 읽기 주소를 추출하고, 폴링 테이블과 연속 주소 읽기 블록을 생성하며, 쓰기 주소를 대기열에 등록하고, 수신값으로 시각 객체 상태를 갱신하는 구성을 청구한다. F-01의 넓은 핵심과 직접 충돌할 가능성이 있으므로 prototype이나 공개보다 먼저 전체 독립항 claim chart가 필요하다.

| 발명 | 조사 후 의견 |
|---|---|
| P-01 | A 유지. manipulation 억제, 최종값 flush, write 우선순위, request-response 상관검증, confirmed cache, pending 비교와 timeout 복귀의 전체 결합을 독립항에 넣는다. |
| P-02 | 넓은 범위는 위험. property-name group 격리, 다형 HMI 의미 보존, split deployment와 복원순서의 구체 결합으로 좁힌다. |
| P-03 현재 구현 | B- 또는 C+로 보수화. 공개시계용 기본출원과 F-04 보강을 분리한다. |
| F-04 | A- 유지 가능. 예상 topology와 실제 I/O 불일치, 최소 원인 edge/node와 contamination sink를 구체화한다. |
| F-01 | 적색 경보. 10-2026-0094264, 10-2017-0100422, 10-2019-0136591의 claim chart 전에는 A+나 해외확장성을 확정하지 않는다. |
| F-09 | 인접 주소와 순차/개별 접근 최적화 자체는 약하다. runtime evidence, hysteresis, page demand, safety tier가 필요하다. |

## 2. 방법, 비밀관리와 한계

KIPRIS 공개 검색 요청을 읽기 전용으로 호출했다. 검색식별 전체 건수와 관련도순 상위 5건 또는 10건을 확인하고, 유력 후보는 AN=[출원번호]로 재조회해 서지·상태·요약과 독립항 중심 청구범위를 확인했다.

번호만 입력하면 같은 숫자의 공개번호 문헌을 반환하는 사례가 있었다. 재현할 때는 반드시 AN=[1020260094264]처럼 field를 고정한다.

아직 공개되지 않았을 수 있는 발명의 고유한 전체 algorithm은 외부 서비스에 전송하지 않았다. 일반적인 2~4개 기술어 조합만 검색했다. KIPRIS 검색 스크립트에는 query log 저장 처리가 있으므로 비공개 발명의 독특한 전체 문장은 검색하지 않고, 짧은 용어·IPC/CPC·seed 문헌의 인용과 family로 확장한다.

이 조사는 상위 결과 중심이며 전체 결과를 모두 읽지 않았다. 번역·동의어·오기 때문에 누락될 수 있고, 일부 문헌은 독립항과 초반 종속항 중심으로 보았다. 복수 문헌 결합에 의한 진보성 공격은 별도 chart가 필요하다. 상태는 2026-08-03 KIPRIS 표시 기준이다.

## 3. 실제 검색 로그

건수는 검색어가 등장한 전체 결과 수이며 실제 유사특허 수가 아니다. HMI, 응답, 유체, 혼합, 스캔은 차량·의료·재료 잡음을 많이 포함한다.

### 공통 및 P-01

| 검색식 | 건수 | 의견 |
|---|---:|---|
| HMI*PLC | 1,687 | 연결 시험. 통합 hardware·보안 잡음 다수 |
| HMI*응답 | 3,895 | 차량 HMI와 일반 request/response 잡음 |
| HMI*쓰기*피드백 | 68 | 관련도 향상, simulation·차량 진단 포함 |
| HMI*폴링*명령 | 149 | F-01의 10-2026-0094264 발견 |
| HMI*쓰기*응답 | 170 | PLC-HMI 동기화 후보 발굴에 유효 |
| HMI*명령*확인 | 3,022 | 인증·승인·차량 명령 잡음 |
| HMI*설정값*피드백 | 1,493 | 일반 feedback이 많아 claims 검색 필요 |
| HMI*입력*갱신 | 801 | 화면 편집·갱신 문헌 다수 |
| SCADA*명령*응답*화면 | 150 | 원격제어·승인·device 연결 후보 |
| PLC*쓰기*응답*화면 | 103 | 프로그래머블 표시기 후보 |

### P-02

| 검색식 | 건수 | 의견 |
|---|---:|---|
| HMI*직렬화 | 71 | SCADA-HMI stream serialization 확인 |
| HMI*XML*화면 | 103 | XML, component, SVG, web 화면 문헌 다수 |
| 객체그래프*직렬화 | 1,438 | 범용 software serialization 잡음과 높은 선행밀도 |

### P-03/F-04

| 검색식 | 건수 | 의견 |
|---|---:|---|
| HMI*유체*흐름 | 1,325 | 물리 밸브·공구·굴착 HMI 잡음 |
| 배관*토폴로지*진단 | 98 | 검사로봇·통신 topology 중심 |
| 유체*혼합*경로 | 76,780 | 물리 혼합장치가 대부분 |
| 배관*흐름*시각화 | 974 | 물리 simulator와 SCV 후보 발굴 |
| 배관*유량*이상진단 | 2,149 | 유량계 자체 진단 중심 |
| 밸브*유로*표시 | 33,193 | 기계식 개폐표시 잡음 |
| 배관*그래프*누설 | 2,385 | sensor·음향 누설검출 중심 |
| 플랜트*오염*경로 | 3,887 | 막 오염·화학 정제 잡음 |

### F-01/F-09

| 검색식 | 건수 | 의견 |
|---|---:|---|
| HMI*태그*폴링 | 46 | 10-2026-0094264가 최상위로 나타남 |
| HMI*스캔*계획 | 359 | 의료 CT scan 잡음이 커 폴링·태그·주소가 더 적절 |

## 4. 핵심 후보 문헌과 세부 의견

### K-01. PLC 주소 바인딩 기반 객체형 자동화 설비 시뮬레이션 시스템 및 방법

- 출원/공개: 10-2026-0094264 / 10-2026-0088432
- 출원일/공개일: 2026-05-26 / 2026-06-12
- 출원인: 여정호
- 상태: 공개, 미등록
- IPC: G05B 19/05, G05B 17/02

확인 구성: 시각 객체별 PLC 주소·사용방식·역할, 읽기/쓰기 분리, 폴링 테이블, 장치·주소 범위·data type·연속 주소에 따른 읽기 블록, 객체 조작에 따른 쓰기 대기열, 주기/event 전송, 수신값에 따른 객체상태 갱신, 객체 간 위치·소유·감지·차단·이송 관계와 편집/runtime mode.

의견: 가장 위험한 문헌이다. F-01의 넓은 “binding 수집 -> read/write demand -> polling block/write queue -> 화면 갱신”은 어렵다. 남은 차별화 가설은 다음과 같다.

1. component parameter와 ItemList scope를 전개해 실제 tag family 결정
2. page/window/component visibility로 runtime demand tier 생성
3. protocol PDU·width·endian·area boundary를 적용해 block plan과 decode map 동시 생성
4. command-feedback 관계에서 P-01 상태기계 자동 생성
5. GUDX element/property까지 역추적하는 source map
6. UI와 communication plan의 hash/version을 하나의 deployment identity로 결합

전체 공보에서 이 차이까지 있는지 확인하기 전에는 등록 가능성을 높게 평가하지 않는다. 출원인과 자사·관계인 관계도 확인한다.

### K-02. 통신 최적화기능이 내장된 HMI

- 출원/등록: 10-2019-0136591 / 10-2238383
- 출원인: 주식회사 엠엑스온
- 상태: 등록

인접 register address를 추출하고 순차/개별 접근속도를 비교해 더 빠른 방식을 주기적으로 선택한다.

의견: F-09의 단순 주소병합·접근 최적화와 가깝다. runtime exception/timeout/quality, hysteresis, page visibility, safety tier, pending command와 protocol 제한이 필요하다.

### K-03. 고속통신을 위한 SCADA 시스템의 디바이스 연결 방법 및 그 장치

- 출원/등록: 10-2017-0100422 / 10-1872648
- 출원인: 주식회사 자이솜
- 상태: 등록

등록 tag의 참조 memory address로 receiver data block을 자동 최적화하고, 통신 이상 시 차순위 통신을 활성화한다.

의견: tag-to-block 자체는 F-01 차별점이 아니다. GUDX scope expansion, decode map, confirmed command와 UI-plan 일관성에 둔다. F-06 failover에도 관련된다.

### K-04. PLC기기와 HMI장비간의 공유메모리를 이용한 데이터 메모리 동기화 방법

- 출원/공개/등록: 10-2005-0060850 / 10-2007-0005825 / 10-0715915
- 출원인: 호서대학교 산학협력단
- 상태: 소멸(등록료 불납)

PLC 종류별 공유메모리, HMI 객체 지정주소 읽기, cache 저장과 UI 표시를 청구한다.

의견: read cache와 표시 동기화 자체는 오래됐다. P-01은 조작 중 readback 억제, 최종값 write, write 우선, response correlation, confirmed cache, pending 비교와 timeout rollback을 결합해야 한다.

### K-05. 제어기와 연동되는 HMI 단말기의 화면 처리 시스템 및 방법

- 출원/등록: 10-2014-0097495 / 10-1584330
- 출원인: 주식회사 솔바인
- 상태: 소멸(등록료 불납)
- IPC: G06F 3/048, G09G 5/00

GUI component XML, GUI ID·PLC I/O·갱신주기 tag map, 값 변화/주기별 표시명령, HMI 입력명령과 XML parsing 후 component 표시를 청구한다.

의견: P-01의 주기 갱신과 P-02의 XML component 저장은 차별점이 아니다. property group 격리, 다형 cell/key/id, resource 외부화, component pre-registration과 scoped binding의 차이를 charting한다.

### K-06. 그래픽 기반의 HMI 화면 구성이 가능한 HMI 장치 및 화면 구성 방법

- 출원/공개/등록: 10-2009-0127800 / 10-2011-0071281 / 10-1622402
- 출원인: 주식회사 디엔솔루션즈
- 상태: 등록

HMI 설명 데이터, component property 추출·편집, 설명 데이터 갱신과 parsing 후 component 생성·동일 화면 구동을 청구한다.

의견: “property 저장 후 화면 복원”은 가깝다. GUDX는 이종 collection의 silent corruption을 property group으로 방지하고 관계 의미와 split deployment를 보존하는 문제·해결관계를 전면에 둔다.

### K-07. 스카다 시스템을 위한 HMI 제작 방법 및 그 장치

- 출원/등록: 10-2022-0111201 / 10-2492443
- 출원인: 주식회사 비에스티그룹
- 상태: 등록

HMI 객체의 type/display property/user event로 HTML/CSS/JSON과 SVG symbol을 생성해 web SCADA를 제공한다.

의견: 복수 문서 배포 자체는 약하다. P-02는 master/page/window/resource 참조, 다형 graph의 cell/key/id/binding, 충돌방지 group, 등록 순서와 roundtrip 불변조건을 결합한다.

### K-08. SVG 파일 포맷을 이용한 컴포넌트 기반의 동적 이미지 표시 시스템 및 방법

- 출원/공개/등록: 10-2014-0191633 / 10-2016-0082732 / 10-1728786
- 출원인: 이엔유 주식회사, 한국수력원자력 주식회사
- 상태: 등록

SVG load, library/picture 분류, symbol 생성·복사, script/event 처리와 ID 기반 rendering을 청구한다.

의견: template/library 선구성과 instance 복제는 알려져 있다. pending binding expression, instance scope, parameter substitution과 전체 graph roundtrip을 결합한다.

### K-09. 스트림데이터를 이용한 SCADA-HMI 데이터 전송 시스템 및 통신방법

- 출원/등록: 10-2024-0127567 / 10-2898068
- 출원인: 한전KDN 주식회사
- 상태: 등록

주기별 point data에 속성 tag를 붙여 stream 직렬화하고 HMI가 역직렬화하며 중복 field를 제거·보완한다.

의견: runtime telemetry stream이라 P-02 design graph와 직접 충돌은 낮다. 명세서에서 두 종류를 명확히 구분한다.

### K-10. SCV 기동 절차의 시각화 장치 및 방법

- 출원/공개/등록: 10-2020-0169927 / 10-2022-0080624 / 10-2430590
- 출원인: 주식회사 한국가스기술공사
- 상태: 등록

pump·valve·배관을 표시하고 단계별로 air·water·fuel gas·LNG·NG 흐름방향과 장치상태를 표시한다.

의견: P-03의 넓은 흐름 시각화와 가깝다. editable topology와 복수 fluid provenance가 차이일 수 있지만 강한 권리는 F-04의 live observation, expected/actual violation, 최소 원인과 contamination sink에 둔다.

### K-11. 유체 흐름 모사 시스템 및 그 동작 방법

- 출원/공개: 10-2024-0127991 / 10-2026-0042684
- 출원인: 경상국립대학교산학협력단, 성균관대학교산학협력단
- 상태: 공개

저장조·연성배관·pump·flow meter의 물리 simulator다.

의견: 이름은 유사하나 HMI topology 실행과는 다르다. 배관*흐름*시각화 검색의 false positive다.

### K-12. 프로그래머블 표시기 및 제어 시스템

- 출원/공개/등록: 10-2018-7005559 / 10-2018-0026550 / 10-1886609
- 출원인: 미쓰비시전기 주식회사
- 상태: 등록

여러 표시기 사이에서 화면별 조작권을 배타적으로 관리한다.

의견: P-01 장치 ACK와는 다르지만 F-03 중재에 직접 관련된다. F-03은 UI lock이 아니라 command version, lease/sequence, reconnect reconciliation과 confirmed device state를 결합해야 한다.

### K-13. 2단계 승인 기반 원격 제어 방법 및 SCADA 시스템

- 출원/공개: 10-2024-0085597 / 10-2026-0001964
- 출원인: 한국전력공사
- 상태: 공개

제1 사용자의 제어요청을 제2 사용자가 승인·거절한 뒤 제어한다.

의견: P-01의 장치 ACK와 다른 human approval이다. 확인응답을 controller/protocol response와 readback confirmation으로 명확히 정의한다.

## 5. 이번 상위검색에서 한 문헌으로 확인하지 못한 결합

확인하지 못했다는 것은 부재가 확정됐다는 뜻이 아니다.

- P-01: manipulation readback 억제 -> 최종값 write -> write 우선 -> response 상관검증 -> confirmed cache -> pending 비교 -> 확정/timeout rollback. 현재 가장 유력한 권리 중심이다.
- P-02: annotation family dispatch -> property group 격리 -> 다형 cell/key/id -> page/window/resource split -> type pre-registration -> instance scoped binding compile. 실제 corruption 방지와 roundtrip 시험이 필요하다.
- P-03/F-04: editable port graph -> expected passability/path -> live sensor mapping -> violation -> 최소 원인 -> contamination first-merge와 affected sink. 현재 구현보다 이 보강의 가치가 높다.

## 6. 재현·후속 검색어

일반검색:

- HMI*쓰기*응답
- HMI*폴링*명령
- PLC*HMI*데이터동기화
- HMI*XML*화면
- HMI*직렬화
- 객체그래프*직렬화
- HMI*컴포넌트*속성
- 배관*흐름*시각화
- 배관*유량*이상진단
- PLC*주소*폴링테이블
- 태그*리시버블록
- 인접*메모리주소*순차접근

청구범위 검색:

- CL=[폴링*테이블]
- CL=[인접*주소*읽기*블록]
- CL=[쓰기*대기열*PLC]
- CL=[HMI*XML*컴포넌트]
- CL=[HMI*설명데이터*속성]
- CL=[배관*흐름*표시]

IPC 시작점: G05B19/05, G05B23/02, G05B17/02, G06F3/048.

## 7. 즉시 후속 조치

1. AN=[1020260094264] 전체 공보·모든 청구항으로 F-01 element chart 작성
2. 출원인과 자사·관계사·협업자 관계 확인
3. K-01 우선권, 조기공개 사유, family와 후속출원 확인
4. F-01을 넓은 안, scope expansion 안, confirmed-command 결합 안으로 나누어 K-01/K-02/K-03과 비교
5. P-02는 K-05/K-06/K-07/K-08과 pattern별 chart 작성
6. P-03 기본출원과 별개로 F-04 prototype·시험을 비공개로 우선
7. 등록문헌은 특허성 조사와 별도로 FTO 검토
8. 이 문서와 원문·claim chart를 변리사 전달폴더 05_prior_art와 06_claim_charts에 포함

## 8. 최종 의견

Going 후보가 모두 출원 불가능한 것은 아니다. 다만 HMI, binding, XML, 직렬화, PLC tag, polling, flow visualization 같은 상위개념은 포화돼 있다. 권리 가치는 특정 입력에서 특정 중간자료를 만들고 실패·충돌·지연을 처리해 기술효과를 내는 구성관계에 달려 있다.

- P-01은 전체 상태기계와 protocol correlation을 구현·시험해 먼저 출원한다.
- P-02는 silent corruption과 property group isolation을 중심으로 좁힌다.
- P-03은 공개시계용 기본출원과 F-04 보강의 가치를 분리한다.
- F-01은 K-01 chart 전에는 가장 강한 장래 발명으로 단정하지 않는다.
- 소멸·거절 문헌도 특허성 조사에서 무시하지 않으며, 등록문헌이 있다는 사실만으로 곧바로 침해가 되는 것도 아니다. 특허성 조사와 FTO를 분리한다.
