# 발명신고서 01 — 확인응답형 HMI 제어 및 표시 동기화

## 1. 서지 초안

- 발명의 명칭(국문): **산업 제어장치의 응답 지연을 고려한 확인응답형 HMI 제어 및 표시 동기화 방법 및 시스템**
- 영문 가칭: **Acknowledged HMI Control and Display Synchronization Method and System Considering Response Delay of an Industrial Controller**
- 발명자: 미확정 — Git 이력만으로 결정하지 말고 실제 착상과 기술적 기여를 확인할 것
- 출원인: 미확정
- 우선순위: 최우선
- 국내 출원: 권장
- 해외 확장: 국내 우선출원 후 선행기술·공개 이력 결과에 따라 PCT 검토

## 2. 기술분야

본 발명은 산업 자동화 시스템의 HMI/SCADA, 산업 통신 master, PLC 또는 원격 I/O 장치의 제어에 관한 것이다. 더 구체적으로는, HMI가 장치의 상태를 주기적으로 읽는 중에 운전자의 쓰기 명령이 발생했을 때, 장치가 확인한 값과 HMI에 표시되는 값을 일관되게 동기화하는 기술에 관한 것이다.

## 3. 배경기술과 문제점

산업용 HMI는 일반적으로 PLC의 coil/register를 반복해서 읽고 화면 요소에 반영한다. 운전자가 switch, slider 또는 numeric input을 조작하면 HMI는 같은 주소에 쓰기 요청을 전송한다.

이때 다음 문제가 발생한다.

1. 쓰기 요청 직후, 요청보다 먼저 또는 늦게 처리된 이전 read response가 들어오면 UI가 이전값으로 되돌아간다.
2. slider 조작 중 모든 중간값을 전송하면 통신량이 증가하고 설비에 불필요한 명령이 반복된다.
3. write API 호출 즉시 local cache를 변경하면 장치가 명령을 거부하거나 연결이 끊겨도 성공한 것처럼 보인다.
4. cache를 변경하지 않으면 주기적 read 값이 명령 처리 전 UI를 덮어써 flicker 또는 오판을 만든다.
5. 응답이 오지 않을 때 명령값을 무한 유지하면 실제 장치 상태와 화면이 달라진다.

## 4. 발명의 목적

- 운전자 조작 중 polling update가 입력을 방해하지 않도록 한다.
- 조작 종료 때 최종 명령만 전송한다.
- 장치가 확인하기 전에 HMI 값을 실제 장치값으로 오인하지 않도록 한다.
- 유효 응답 또는 후속 확인값으로 명령 성공을 판단한다.
- 정해진 시간 안에 확인되지 않으면 실제 확인값으로 안전하게 복귀한다.
- 자동 read와 수동 command가 경쟁할 때 명령 처리 지연을 줄인다.

## 5. 용어 정의

| 용어 | 정의 |
|---|---|
| 확인값(confirmed value) | 장치에서 수신한 유효 read response 또는 유효 write response에 의해 확인된 값 |
| 명령값(commanded value) | 운전자가 요청한 목표값 |
| 보류 명령(pending command) | 장치 확인 전의 명령값과 그 생성시각을 포함한 상태 |
| 표시값(displayed value) | HMI control에 현재 표시되는 값 |
| 조작 억제(manipulation suppression) | drag/press/edit 중 source-to-control 및 control-to-source 자동 처리를 일시 중단하는 상태 |
| 최종 flush | 조작 종료 후 control의 최종값을 setter에 한 번 전달하는 처리 |
| 확인 timeout | 명령값이 확인값으로 확인되기를 기다리는 최대시간 |

## 6. 시스템 구성

1. **표시 및 입력부**: switch, slider, numeric input 등을 표시하고 운전자 입력을 받는다.
2. **바인딩 처리부**: control property와 확인값 getter/명령 setter를 연결한다.
3. **조작 상태 판정부**: control이 조작 중인지 판정한다.
4. **명령 상태 저장부**: 명령값, 명령시각, timeout과 상태를 저장한다.
5. **작업 스케줄러**: 자동 read 작업과 수동 write 작업을 보관하고 실행순서를 선택한다.
6. **통신부**: Modbus RTU/TCP 등의 요청을 전송하고 응답을 검증한다.
7. **확인값 cache**: 장치별·영역별·주소별 confirmed value를 저장한다.
8. **일치 판정부**: pending command value와 confirmed value를 비교한다.
9. **복귀 처리부**: timeout 또는 실패 때 display를 confirmed value로 바꾼다.

### 6.1 현재 구현과 완성 실시형태의 경계

2026-08-03 코드 재검증 결과, 위 구성요소가 모두 존재하더라도 다음 두 결합은 현재 단일 실행경로에서 완성돼 있지 않다.

1. 일반 control 변화 경로는 pending command를 등록하지만, 조작 종료 후 최종값을 보내는 `PendingFlush` 경로는 setter 호출 후 `LastCtrlValue/LastSrcValue`를 갱신할 뿐 pending value·시각·flag를 설정하지 않는다. 7절 5~6단계의 완성 실시형태는 두 경로가 공통 command 생성 함수로 들어가도록 보완한 것이다.
2. RTU는 CRC를 확인하지만 slave/function/address/value 전부를 원 요청과 대조하지 않으며, TCP도 transaction identifier/unit/function/echoed payload를 완전히 검증하지 않는다. 현재 cache event는 응답 function을 통과한 뒤 원 `Work`의 주소·값을 사용한다. 7절의 “유효 response”는 이 상관검증을 추가한 완성 실시형태를 의미한다.

따라서 본 신고서는 구현된 부품의 단순 현황서가 아니라, 그 부품을 구체적으로 연결하는 출원 목표 실시형태를 포함한다. 세부 의사코드와 field-level 검증조건은 `technical-principle-01-confirmed-hmi.md`를 따른다.

## 7. 핵심 처리 순서

### 정상 시나리오

1. 통신부가 장치를 자동 polling하고 확인값 cache를 갱신한다.
2. 바인딩 처리부가 확인값을 control에 표시한다.
3. 운전자가 조작을 시작하면 조작 상태 판정부가 binding 처리를 억제한다.
4. 조작 중 control은 운전자 입력에 따라 바뀌지만 polling값은 이를 덮어쓰지 않는다.
5. 운전자가 조작을 끝내면 최종 표시값을 한 번 읽어 write setter를 호출한다.
6. 바인딩 처리부가 최종값과 시각을 pending command로 저장한다.
7. 스케줄러는 수동 write를 자동 polling보다 먼저 선택한다.
8. write 요청을 보냈다는 이유만으로 확인값 cache를 변경하지 않는다.
9. 유효 write response가 수신되면 통신 wrapper가 응답에 포함된 주소·값으로 확인값 cache를 갱신한다.
10. 다음 binding cycle에서 확인값이 pending value와 같음을 판정하고 pending을 해제한다.

### timeout 시나리오

1. 단계 1~8은 정상 시나리오와 같다.
2. 확인값이 pending value와 다르고 경과시간이 timeout 미만이면 명령 표시값을 유지한다.
3. timeout이 경과하면 pending을 해제한다.
4. control을 현재 확인값으로 설정해 실제 장치 상태를 표시한다.

### 이전 polling 응답 경합 시나리오

1. 운전자 명령 전에 생성된 read request의 response가 write request 직후 도착한다.
2. 확인값 cache에는 이전값이 기록될 수 있다.
3. pending 기간에는 이전 확인값이 control을 덮어쓰지 않는다.
4. write response 또는 후속 read response가 명령값을 확인하면 pending을 해제한다.

## 8. 상태 전이

| 현재 상태 | 사건 | 조건 | 동작 | 다음 상태 |
|---|---|---|---|---|
| Idle | control 조작 시작 | - | binding 양방향 억제 | Manipulating |
| Manipulating | polling값 도착 | - | cache는 갱신 가능, control 반영은 억제 | Manipulating |
| Manipulating | 조작 종료 | 초기화된 양방향 binding | 최종값 1회 setter 호출 | Pending |
| Pending | 확인값 갱신 | 확인값 = 명령값 | pending 해제 | Confirmed/Idle |
| Pending | 확인값 갱신 | 확인값 ≠ 명령값, timeout 전 | display 유지 | Pending |
| Pending | 시간 경과 | timeout 초과 | display를 확인값으로 복귀 | TimedOut/Idle |
| Pending | 명시적 거부 | 응답에 failure/exception | 정책에 따라 즉시 복귀·오류 표시 | Rejected/Idle |
| Pending | 재접속 | 장치값 확인 필요 | idempotency/sequence 대조 후 재전송 또는 복귀 | Pending/Idle |

현재 코드는 일반 control 변화 경로에서 Idle, Pending, Confirmed, TimedOut의 핵심 의미를 구현하고 manipulation suppression/final flush도 별도로 구현한다. 다만 final flush에서 Pending으로 이어지는 전이는 결합 보완이 필요하다. 명시적 Rejected와 재접속 idempotency는 후속 실시예다.

## 9. 현재 코드와 대응

| 발명 구성 | 코드 |
|---|---|
| binding snapshot과 재진입 안전 | `Going.UI/Controls/GoControl.cs` `PumpBindings()` |
| 조작 중 양방향 억제 | 같은 파일 356~365행 |
| 조작 종료 후 최종 flush | 같은 파일 367~396행 |
| pending value 일치 판정과 timeout 복귀 | 같은 파일 403~431행 |
| control 변화 감지와 pending 등록 | 같은 파일 448~477행 |
| timeout 지정 API | `Going.UI/Bindings/GoControlBindingExtensions.cs` 61~72행 |
| manual 우선 scheduler | `Going.Basis/Communications/Modbus/RTU/ModbusRTUMaster.cs` 408~574행 및 TCP 대응 코드 |
| write response 기반 cache | `MasterRTU.cs` 100~136행, `MasterTCP.cs` 94~130행 |
| write API가 cache를 미리 변경하지 않음 | 각 wrapper의 write method 148행 이후 |
| 회귀시험 | `BindingTests.cs`, `ModbusWrapperZeroBaseTests.cs` |

## 10. 실시예

### 실시예 1 — Modbus TCP coil switch

```csharp
onOff.Bind(
    c => c.OnOff,
    () => plc.GetCoil(1, 0) ?? false,
    value => plc.WriteCoil_FC5(1, 0, value),
    timeout: 500);
```

PLC slave 1의 coil address 0을 자동 monitor한다. 운전자 touch가 끝날 때 FC5 write를 queue에 넣고 write response에 의해 cache가 갱신되면 UI 명령을 확정한다.

### 실시예 2 — Modbus RTU setpoint slider

slider의 value를 holding register에 bind한다. drag 중에는 중간값을 전송하지 않고 release 때 최종값을 FC6으로 전송한다. 장치가 범위를 clamp해 다른 값을 반환하면 timeout 후 장치값으로 복귀하거나, protocol이 명시적 readback을 제공하면 mismatch 상태를 표시한다.

### 실시예 3 — multi-register recipe value

32-bit 값 또는 여러 setpoint를 FC16으로 쓴다. 확인값 cache의 각 word가 응답에 의해 갱신되며, 전부 일치할 때 transaction을 확정한다. 일부만 일치하면 partial mismatch로 분류하고 이전 recipe 또는 장치값으로 복귀한다.

### 실시예 4 — protocol-neutral adapter

`IConfirmedTag<T>`가 `ConfirmedValue`, `Quality`, `Timestamp`, `WriteAsync`를 제공한다. Modbus, CNet, MC, MQTT adapter가 이를 구현한다. HMI binding state machine은 protocol과 무관하게 동일하게 작동한다.

## 11. 선행기술 대비표

| 구성 | US20040021679A1 | US20240248731A1 | Modbus 표준 | 본 발명 |
|---|---:|---:|---:|---:|
| 산업 HMI 양방향 binding | 있음 | 일반 UI binding | 없음 | 있음 |
| optimistic/pessimistic 또는 async command state | 있음 | 있음 | 없음 | 있음 |
| 운전자 연속조작 중 양방향 억제 | 미확인 | 미확인 | 해당 없음 | 있음 |
| 조작 종료 최종값 1회 flush | 미확인 | 미확인 | 해당 없음 | 있음 |
| manual command의 cyclic read 대비 우선 선택 | 미확인 | 미확인 | 미규정 | 있음 |
| 요청 시 cache 미변경, 유효 응답 후 cache 변경 | 일반적 server sync 가능 | 완료 notification | 응답 형식만 규정 | 있음 |
| pending value와 confirmed cache의 값 일치 판정 | 구체 결합 미확인 | command 완료 state | 미규정 | 있음 |
| timeout 뒤 confirmed value로 display 복귀 | 이전값 복귀 개념 있음 | timeout은 핵심 아님 | 미규정 | 있음 |
| 위 구성 전체의 폐루프 결합 | 미확인 | 미확인 | 없음 | 있음 |

“미확인”은 부재가 확정됐다는 뜻이 아니다. 정식 청구항 조사가 필요하다.

## 12. 예비 청구항 골격

아래는 변리사 검토용 기술 골격이며 제출용 최종 문언이 아니다.

### 독립 방법항

산업 제어장치와 통신하는 HMI 장치가 수행하는 제어 및 표시 동기화 방법으로서,

1. 산업 제어장치의 대상 주소를 주기적으로 읽어 확인값 cache를 갱신하는 단계;
2. 표시 control에 대한 운전자 조작의 시작에 응답하여 확인값의 control 반영과 control값의 쓰기 전송을 억제하는 단계;
3. 조작 종료에 응답하여 control의 최종값을 명령값으로 정하고 쓰기 작업을 생성하는 단계;
4. 자동 읽기 작업과 상기 쓰기 작업이 함께 대기하는 경우 쓰기 작업을 우선 선택하는 단계;
5. 쓰기 요청의 생성 또는 전송만으로는 확인값 cache를 명령값으로 변경하지 않고, 산업 제어장치의 유효 응답을 수신한 경우 응답에 기초해 확인값 cache를 갱신하는 단계;
6. 명령값 및 명령 생성시각을 포함하는 보류 상태에서 확인값과 명령값을 비교하는 단계;
7. 값이 일치하면 보류 상태를 해제하고, 일치하지 않으면서 제한시간 전이면 control의 명령값 표시를 유지하며, 제한시간이 지나면 보류 상태를 해제하고 control을 확인값으로 변경하는 단계;

를 포함하는 방법.

### 독립 시스템항

- processor와 memory
- display/input interface
- industrial communication interface
- binding processor
- work scheduler
- confirmed-value cache
- pending-command state manager
- 위 방법을 수행하는 instruction

### 프로그램/기록매체항

컴퓨터에서 실행될 때 위 방법을 수행하게 하는 컴퓨터프로그램 또는 이를 저장한 컴퓨터 판독가능 기록매체.

### 종속항 후보

1. 조작이 drag이고 release 때 최종값만 전송
2. control이 switch이고 press/release를 조작 구간으로 판정
3. Modbus FC5/FC6/FC15/FC16 실시예
4. write response의 slave/address/value를 요청과 대조
5. 후속 read response도 확인 근거로 사용
6. 응답 sequence identifier로 늦은 응답 배제
7. 여러 주소가 전부 일치할 때만 확정
8. 일부 불일치의 partial state 표시
9. timeout·거부·stale quality에 따라 control의 색/아이콘/입력가능 여부 변경
10. 재접속 때 실제 확인값을 먼저 읽고 재전송 여부 결정
11. 중복 명령 방지를 위한 idempotency key
12. protocol-neutral logical tag와 복수 adapter

## 13. 청구범위 방어선

| 단계 | 유지할 구성 | 목적 |
|---|---|---|
| 넓은 안 | confirmed cache + pending compare + timeout rollback | protocol 독립 권리 |
| 1차 축소 | manipulation suppression + final flush 추가 | 일반 async command UI와 차별 |
| 2차 축소 | manual-over-auto scheduler 추가 | 산업 polling 환경의 처리순서 강조 |
| 3차 축소 | request 때 cache 미갱신, valid write response 때 갱신 | false success 방지의 구체 수단 |
| 4차 축소 | Modbus address/function/response correlation | 등록 가능성을 위한 구체 실시형태 |

선행기술상 넓은 안이 어렵다면 처음부터 핵심 결합을 독립항에 포함하고, protocol-neutral 표현은 별도 독립항 또는 후속 출원으로 검토한다.

## 14. 도면 설명 초안

- 도 1: 전체 시스템 블록도
- 도 2: HMI binding과 communication scheduler의 데이터 흐름도
- 도 3: Idle–Manipulating–Pending–Confirmed–TimedOut 상태 전이도
- 도 4: 정상 write acknowledgement의 시간 순서도
- 도 5: 이전 read response가 write response보다 먼저 도착하는 시간 순서도
- 도 6: timeout 후 confirmed value 복귀 흐름도
- 도 7: multi-address command의 전체·부분 확인 판정도
- 도 8: 재접속 때 명령 중복 방지 흐름도

## 15. 출원 전 필요한 추가자료

- [ ] 실제 장치 또는 simulated slave를 이용한 통합 sample
- [ ] 상태 전이별 자동시험과 실행 log
- [ ] 종래 polling 즉시반영 방식과 정량 비교
- [ ] 명령 생성부터 확인까지 timeline capture
- [ ] 최초 착상자와 각 구성의 기여자 확인
- [ ] 2026-05-09, 2026-05-21, 2026-05-24 기능의 실제 공개일
- [ ] 해당 공개 version의 source snapshot과 NuGet binary
- [ ] Google Patents 결과를 KIPRIS/PATENTSCOPE/Espacenet에서 family·claims 재확인

## 16. 공개 금지 후속 개선

다음 내용은 후속 국내출원 및 해외 확장 가치가 높으므로 구현 전에 private 발명기록으로 관리한다.

- 여러 HMI의 tag lease와 command arbitration
- sequence/idempotency를 이용한 재접속 중복방지
- protocol별 confirmed value quality와 failover
- GUDX binding에서 위 상태 기계를 자동 생성하는 scan-plan compiler
- UI와 communication plan의 원자적 hot deployment
