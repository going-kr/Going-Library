# 구체적 기술원리 01 — 확인응답형 HMI 제어·표시 동기화

## 0. 문서 목적과 구현 경계

이 문서는 발명신고서의 개요를 실제 명세서의 “발명을 실시하기 위한 구체적인 내용”으로 발전시키기 위한 기술 부속서다. 설명은 다음 세 수준을 명확히 구분한다.

| 구분 | 의미 |
|---|---|
| 현재 구현 | 2026-08-03 저장소 코드에서 직접 확인되는 동작 |
| 결합 보완 필요 | 현재 구성요소는 존재하지만 하나의 실행경로로 완전히 연결되지 않은 부분 |
| 확장 실시형태 | 출원 명세서에 기재할 수 있도록 구체화한 구현 가능한 변형이며 아직 코드에 없을 수 있는 부분 |

중요한 현재 구현 차이는 두 가지다.

1. 일반 control 변화 경로에서는 `PendingCommandValue`, `PendingCommandTick`, `HasPendingCommand`가 설정되지만, 조작 억제 종료 후 `PendingFlush` 경로에서는 현재 이 세 필드가 설정되지 않는다. 따라서 drag/release 최종값 전송과 command timeout 보호가 현재 코드상 자동 결합되어 있지 않다.
2. RTU는 CRC를 검사하지만 응답의 slave/function/address/value 전체를 원 요청과 대조하지 않고, TCP도 MBAP transaction identifier와 echoed payload의 완전한 상관검증이 구현돼 있지 않다. 현재 wrapper cache는 수신 response의 function에 따라 발생한 event에서 원 `Work`의 주소·값을 사용해 갱신한다.

따라서 아래의 “완성 실시형태”는 이 두 간극을 보완한다. 출원 시에는 현재 구현 사실과 예정 구현을 혼동하지 않고, 실시 가능한 구체 알고리즘으로 설명한다.

## 1. 기술적 문제의 발생 원리

### 1.1 polling과 command가 공유하는 값

HMI control 하나가 PLC의 동일 논리 tag에 연결되면 적어도 세 값이 동시에 존재한다.

- `C`: control에 표시된 값
- `D`: 운전자가 요구한 명령값(desired/commanded value)
- `A`: 장치 응답으로 확인된 값(acknowledged/confirmed value)

평상시에는 `C = A`다. 사용자가 값을 바꾸는 순간 `C = D`가 되지만 장치 응답 전에는 `A`가 이전값이다. 이 기간에 단순 binding이 `A`를 다시 `C`에 복사하면 control이 이전값으로 되돌아간다. 반대로 요청 직후 `A := D`로 바꾸면 장치가 실제로 적용하지 않았는데도 성공한 것처럼 보인다.

### 1.2 비동기 도착 순서

시간 `t0`에 자동 read 요청 R0가 전송되고, `t1`에 사용자가 write 요청 W1을 만든 경우 응답은 다음처럼 올 수 있다.

```text
t0  R0 전송: 장치값 10을 읽음
t1  사용자 조작: D=20
t2  W1 전송: 20 쓰기 요청
t3  R0 응답: A=10
t4  W1 응답: 20 쓰기 확인
t5  R1 응답: A=20
```

`t3~t4` 또는 `t3~t5` 구간에서 A는 10이지만 C는 20으로 유지돼야 한다. 단, W1이 실패하거나 제한시간이 지나면 C는 A로 돌아가야 한다.

## 2. 논리 구성요소

### 2.1 Control adapter

각 control property에 대해 다음 함수를 제공한다.

```text
ControlGet(control) -> object
ControlSet(control, value)
SourceGet() -> confirmedValue
SourceSet(value) -> command enqueue
IsManipulating(control) -> bool
```

현재 구현은 expression tree에서 control property를 추출하고 `Func<GoControl, object?>` 및 `Action<GoControl, object?>` delegate를 compile한다. 이로써 매 update cycle의 reflection 호출을 피한다.

### 2.2 Binding state record

현재 `GoBinding`의 논리 상태는 다음과 같다.

| 필드 | 역할 |
|---|---|
| `CtrlProperty` | binding 대상 property 식별 |
| `CtrlGet`, `CtrlSet` | compile된 control 접근자 |
| `SourceGet`, `SourceSet` | confirmed value 조회 및 command 생성 함수 |
| `LastSrcValue` | 마지막으로 처리한 source 값 |
| `LastCtrlValue` | 마지막으로 처리한 control 값 |
| `Initialized` | 최초 source-to-control 동기화 완료 여부 |
| `PendingFlush` | 조작 억제 종료 후 최종값 전송 필요 여부 |
| `IsCommand` | 일반 two-way binding과 command binding 구별 |
| `CommandTimeout` | pending을 유지할 최대시간(ms) |
| `PendingCommandValue` | 확인을 기다리는 명령값 |
| `PendingCommandTick` | monotonic clock 기준 명령 생성시각 |
| `HasPendingCommand` | pending command 존재 여부 |

완성 실시형태에서는 다음 식별정보를 추가할 수 있다.

```text
CommandKey = {
  protocolId,
  channelId,
  deviceId,
  areaOrFunction,
  startAddress,
  elementCount,
  sequenceId
}

PendingCommand = {
  key,
  desiredValues[],
  createdTick,
  deadlineTick,
  state,
  retryCount,
  confirmationMask[]
}
```

### 2.3 Work scheduler

작업은 적어도 다음 정보를 가진다.

```text
Work = {
  messageId,
  requestFrame,
  expectedResponseLength,
  timeout,
  retryLimit,
  class: Manual | Automatic
}
```

현재 RTU/TCP master는 `ManualWorkList`, `WorkQueue`, `AutoWorkList`를 사용한다. 선택 시점에 manual list가 비어 있지 않으면 첫 manual 작업을 꺼내고, 그렇지 않으면 queue의 자동 작업을 꺼낸다. queue가 비면 auto list를 다시 queue에 채워 순환 polling한다.

### 2.4 Confirmed-value cache

Modbus wrapper는 slave별로 영역 dictionary를 분리한다.

```text
DeviceMemory[slave].Coils[address]           -> bool
DeviceMemory[slave].Contacts[address]        -> bool
DeviceMemory[slave].HoldingRegister[address] -> int
DeviceMemory[slave].InputRegister[address]   -> int
```

읽기 응답은 `startAddress + index`에 순차 저장한다. 쓰기 API 호출 자체는 dictionary를 바꾸지 않는다. write response event가 발생하면 FC5/FC6은 한 주소를, FC15/FC16은 요청에 담긴 복수 값을 해당 주소 범위에 저장한다.

완성 실시형태에서는 값과 품질을 함께 저장한다.

```text
ConfirmedEntry<T> = {
  value,
  confirmedAt,
  source: ReadResponse | WriteResponse,
  quality: Good | Stale | Bad | Conflict,
  responseSequence,
  deviceSession
}
```

## 3. 현재 구현의 update 알고리즘

`FireUpdate()`는 먼저 binding을 pump하고 그 다음 control 자체의 update hook을 실행한다. `PumpBindings()`는 binding 목록을 배열 snapshot으로 복사한다. source setter가 pump 도중 binding 목록을 바꿔도 이번 cycle은 동일 snapshot을 처리하기 위한 것이다.

### 3.1 현재 알고리즘 의사코드

```text
function PumpBindings(control):
    if bindings is empty or control is disposed:
        return

    snapshot = copy(bindings)
    suppressed = control.IsBindingSuppressed

    for binding in snapshot:
        if suppressed:
            if binding is writable and binding.Initialized:
                binding.PendingFlush = true
            continue

        if binding.PendingFlush and binding is writable:
            current = ControlGet(control)
            SourceSet(current)
            binding.LastCtrlValue = current
            binding.LastSrcValue = current
            binding.PendingFlush = false
            continue

        newSource = SourceGet()

        if binding.IsCommand and binding.HasPendingCommand:
            if Equal(newSource, binding.PendingCommandValue):
                binding.LastSrcValue = newSource
                binding.LastCtrlValue = newSource
                clear pending command
            else if nowTick - PendingCommandTick < CommandTimeout:
                continue
            else:
                clear pending command
                ControlSet(control, newSource)
                binding.LastSrcValue = newSource
                binding.LastCtrlValue = newSource
                continue

        if not Initialized or newSource differs from LastSrcValue:
            ControlSet(control, newSource)
            LastSrcValue = newSource
            LastCtrlValue = newSource
            Initialized = true

        if binding is writable:
            current = ControlGet(control)
            if current differs from LastCtrlValue:
                SourceSet(current)
                LastCtrlValue = current
                if IsCommand:
                    PendingCommandValue = current
                    PendingCommandTick = monotonicNow
                    HasPendingCommand = true
                else:
                    LastSrcValue = current
```

### 3.2 최초 초기화 보호

`Initialized == false`인 binding이 조작 억제 상태라고 해서 `PendingFlush`를 세우면 control의 default 값이 실제 source를 덮어쓸 수 있다. 따라서 현재 구현은 초기화된 writable binding에만 `PendingFlush`를 설정한다. 조작이 풀리면 미초기화 binding은 정상 source-to-control 경로를 수행한다.

### 3.3 자기 트리거 방지

source-to-control 동기화 때 `LastSrcValue`와 `LastCtrlValue`를 같은 값으로 함께 갱신한다. 같은 cycle의 역방향 검사에서 control 변화로 오인해 setter를 호출하지 않게 하기 위함이다.

### 3.4 예외 격리

각 getter/setter 호출은 개별 `try/catch`로 둘러싸여 있다. 하나의 binding getter가 실패해도 다음 binding의 pump를 계속한다. 이는 한 tag의 통신 또는 conversion 오류가 control 전체 update loop를 중단시키는 것을 막는다.

## 4. 완성 실시형태의 통합 알고리즘

### 4.1 필요한 보완

`PendingFlush`와 일반 change path가 같은 `RegisterPendingCommand()`를 호출하도록 통합한다.

```text
function SendControlValue(binding, value):
    SourceSet(value)
    binding.LastCtrlValue = value

    if binding.IsCommand:
        binding.PendingCommandValue = Clone(value)
        binding.PendingCommandTick = MonotonicNow()
        binding.HasPendingCommand = true
        // LastSrcValue는 confirmed source가 아니므로 desired value로 덮지 않는다.
    else:
        binding.LastSrcValue = value
```

조작 종료 flush는 다음과 같이 된다.

```text
if binding.PendingFlush and binding.SourceSet exists:
    finalValue = ControlGet(control)
    SendControlValue(binding, finalValue)
    binding.PendingFlush = false
    continue
```

핵심은 command binding에서 `LastSrcValue`를 명령값으로 바꾸지 않는 것이다. `LastSrcValue`는 confirmed source의 마지막 관측값이고, desired value는 별도 pending field에 둔다.

### 4.2 응답 상관검증

RTU의 유효 write confirmation 조건 예시는 다음과 같다.

```text
rtuValid =
    response.length == expectedLength
    AND CRC(response) == response.crc
    AND response.slave == request.slave
    AND response.function == request.function
    AND response.startAddress == request.startAddress
    AND (
         function in {FC5, FC6}  -> response.echoedValue == request.value
         function in {FC15,FC16} -> response.echoedCount == request.count
        )
```

TCP의 유효 조건 예시는 다음과 같다.

```text
tcpValid =
    response.mbap.transactionId == request.mbap.transactionId
    AND response.mbap.protocolId == 0
    AND response.mbap.length is structurally valid
    AND response.unitId == request.unitId
    AND response.function == request.function
    AND address/count/value echo matches request
```

exception response(`function | 0x80`)는 confirmation으로 사용하지 않고 command를 `Rejected`로 전이시킨다. 다른 session에서 늦게 도착한 응답은 `deviceSession` 또는 connection generation이 다르면 폐기한다.

### 4.3 cache 갱신 원칙

```text
on WriteRequestCreated(request):
    enqueue request
    do not mutate ConfirmedCache

on CorrelatedValidWriteResponse(response, request):
    for each requested element i:
        ConfirmedCache[key(address+i)] = {
            value: request.values[i],
            source: WriteResponse,
            quality: Good,
            responseSequence: request.sequenceId
        }

on ValidReadResponse(response):
    for each returned element i:
        update cache with response.values[i]
```

write response가 값 자체를 모두 echo하지 않는 FC15/FC16에서는 response가 시작주소와 count를 확인한 뒤, correlation된 request가 보관한 values를 cache에 반영한다. 더 엄격한 실시형태에서는 즉시 후속 readback을 실행하고 readback 일치 뒤 `Applied`로 확정한다.

### 4.4 두 단계 확인 모델

장치와 protocol 성격에 따라 확인을 두 수준으로 구분할 수 있다.

| 상태 | 의미 |
|---|---|
| `Accepted` | protocol write response가 요청을 정상 수락했음을 확인 |
| `Applied` | 후속 read/feedback sensor가 실제 값 또는 물리 상태를 확인 |

스위치 coil은 `Accepted`만으로 화면을 확정할 수 있고, motor run command는 write response와 별개인 run feedback tag가 true가 되어야 `Applied`로 확정할 수 있다.

```text
PendingCommand {
  desiredCommandValue,
  confirmationPolicy: Echo | Readback | FeedbackTag | Composite,
  feedbackPredicate(confirmedCache) -> bool
}
```

### 4.5 값 일치 정책

단순 `object.Equals` 외에 type별 comparator를 둘 수 있다.

```text
bool IsConfirmed(desired, actual, policy):
    Exact: desired == actual
    NumericTolerance: abs(desired-actual) <= epsilon
    BitMask: (actual & mask) == (desired & mask)
    Range: min <= actual <= max
    Predicate: userDefined(desired, actual, cache)
```

float setpoint, 장치 clamp, bit field, 복합 feedback를 포괄하기 위한 실시형태다.

## 5. 상태 기계

### 5.1 상태

```text
Uninitialized
IdleConfirmed
Manipulating
Queued
Sent
Accepted
Applied
TimedOut
Rejected
Disconnected
Conflict
```

### 5.2 전이 규칙

| 상태 | 사건 | guard | 처리 | 다음 상태 |
|---|---|---|---|---|
| Uninitialized | confirmed value 수신 | quality good | C:=A, baseline 저장 | IdleConfirmed |
| Uninitialized | 조작 시작 | - | source overwrite 방지를 위해 flush 금지 | Manipulating |
| Manipulating | control 중간값 변경 | - | C만 변경, 송신 금지 | Manipulating |
| Manipulating | 조작 종료 | initialized | D:=C, work 생성 | Queued |
| IdleConfirmed | control 값 변경 | C≠lastC | D:=C, work 생성 | Queued |
| Queued | scheduler 선택 | manual priority | request 전송 | Sent |
| Sent | valid correlated write response | policy=Echo | A:=request value | Accepted/Applied |
| Sent | valid write response | policy=Readback | follow-up read 생성 | Accepted |
| Accepted | feedback predicate true | - | C:=A, pending 해제 | Applied/IdleConfirmed |
| Sent/Accepted | 이전 read response | A≠D, deadline 전 | cache만 갱신, C 유지 | 동일 |
| Sent/Accepted | deadline 초과 | - | C:=latest A, 오류 상태 | TimedOut |
| Sent | exception/NAK | - | C:=latest A, reason 기록 | Rejected |
| Pending | connection loss | retry policy 존재 | command 보존·session 종료 | Disconnected |
| Disconnected | reconnect | current A=D | 이미 적용으로 판정 | Applied |
| Disconnected | reconnect | current A≠D, precondition valid | 새 sequence로 재전송 | Queued |
| Disconnected | reconnect | current A≠D, precondition invalid | 재전송 금지 | Conflict |

## 6. Scheduler 상세 원리

### 6.1 우선순위와 starvation

현재 구현은 manual 작업이 존재하는 동안 manual을 먼저 꺼낸다. 명령 폭주가 지속되면 auto polling이 지연될 수 있다. 실시형태에는 다음 정책을 선택적으로 기재한다.

```text
priority classes:
  P0 safety/emergency
  P1 operator command
  P2 confirmation readback
  P3 alarm/high-rate polling
  P4 normal visible-page polling
  P5 hidden-page/background polling
```

각 priority에 deadline을 두고, `N`개의 manual 처리 뒤 deadline이 임박한 read를 하나 허용하는 weighted scheduling으로 confirmed cache의 장기 stale을 방지한다.

### 6.2 retry

retry는 같은 logical command에 새 physical attempt를 연결한다.

```text
logicalCommandId = constant
attemptNo = 0..retryLimit
transportSequence = new per attempt
```

응답은 logical command와 attempt에 상관된다. 이전 attempt 응답이 늦게 오면 최신 command를 잘못 확정하지 않도록 sequence와 device session을 비교한다.

## 7. Control별 조작 구간

| Control | 현재 suppression 조건 | 명령 생성 시점 |
|---|---|---|
| `GoSlider` | `isDragging` | drag release 후 최종 value |
| `GoRangeSlider` | lower 또는 upper handle dragging | 해당 handle release 후 최종 lower/upper value |
| `GoKnob` | pointer down 상태 `bDown` | pointer up 후 최종 회전값 |
| `GoOnOff` | `ptDown.HasValue` | pointer release로 toggle 확정 후 |

키보드 입력은 focus/edit buffer를 별도 manipulation 상태로 보고 Enter/commit 시 명령을 만들며 Escape/cancel 시 original confirmed value로 되돌릴 수 있다.

## 8. 대표 실시예

### 8.1 FC6 setpoint

```text
tag: Slave=1, HoldingRegister=100
display type: float slider 0..100
wire type: ushort scale 0..1000
timeout: 500 ms
confirmation: FC6 echo + optional FC3 readback
comparator: abs(actual-display - desired-display) <= 0.05
```

1. A=35.0을 표시한다.
2. 사용자가 slider를 35.0→62.7로 drag한다. 중간 41.2, 49.8, 58.3은 전송하지 않는다.
3. release 시 62.7을 wire value 627로 변환해 manual FC6 work를 만든다.
4. C=62.7을 유지하고 A=35.0은 별도 보존한다.
5. valid echo를 받으면 cache를 627로 갱신한다.
6. comparator가 일치를 판정하고 pending을 해제한다.

### 8.2 motor command와 feedback tag

```text
command tag: Coil 10
feedback tag: Contact 20
desired: true
confirmation policy: write accepted AND Contact20 == true
timeout: 3000 ms
```

FC5 echo는 command 수락만 의미한다. 실제 motor가 돌기 전에는 `Accepted` 상태로 표시하고, contact 20이 true가 되면 `Applied`로 바꾼다. timeout이면 contact 20의 실제값을 표시하고 start failure alarm을 만든다.

### 8.3 FC16 원자·부분 확인

요청 values `[100, 200, 300]`에 대해 confirmation mask를 `[false,false,false]`로 시작한다. FC16 echo의 address/count가 맞으면 `Accepted`로 전이한다. FC3 readback에서 각 word가 일치할 때 mask를 true로 한다. 전부 true이면 Applied, 일부만 true인 채 timeout이면 PartialMismatch로 표시한다.

## 9. 실패·경합 시나리오

### 9.1 장치 clamp

요청 D=120이지만 장치 허용범위가 0~100이라 readback A=100이면 exact comparator는 일치하지 않는다. 정책에 따라 다음 중 하나를 수행한다.

- timeout까지 기다린 뒤 C:=100으로 복귀
- 장치가 clamp status를 제공하면 즉시 `Adjusted`로 확정하고 C:=100
- 허용범위를 HMI에서 사전 검증해 명령 자체를 생성하지 않음

### 9.2 늦은 이전 명령 응답

D1=20을 보낸 뒤 D2=30을 보낸 경우 D1 응답으로 D2가 확정되면 안 된다. address만 같아도 `sequenceId`가 다르면 D2의 confirmation에는 사용하지 않는다. 단, readback A=30은 D2를 확정할 수 있다.

### 9.3 연결 단절

재접속 시 무조건 재전송하지 않는다. 먼저 actual value를 읽고 다음으로 분류한다.

```text
if A == D: AlreadyApplied
else if command expired: Expired
else if precondition still true and idempotent: Requeue
else: ConflictRequiresOperator
```

## 10. 동시성 및 메모리 일관성

통신 callback과 UI update thread가 다를 수 있으므로 confirmed cache entry는 lock, concurrent dictionary 또는 immutable snapshot으로 publish한다. 하나의 multi-register response는 중간 상태가 노출되지 않도록 address range를 하나의 transaction으로 갱신한다.

UI pump는 cycle 시작 때 binding list뿐 아니라 필요한 confirmed entries의 versioned snapshot을 사용할 수 있다. 한 cycle 안에서 tag A는 이전 version, tag B는 새 version을 읽는 혼합을 줄인다.

## 11. 기술효과의 인과관계

| 구성 | 직접 효과 |
|---|---|
| 조작 중 양방향 억제 | 중간명령 frame 감소, polling에 의한 handle 떨림 방지 |
| 최종값 단일 flush | 장치 write 횟수와 serial bus 점유 감소 |
| request 시 cache 미갱신 | 통신 실패를 성공으로 오표시하는 문제 방지 |
| manual priority | operator command의 queue wait 감소 |
| response correlation | 다른 장치·이전 요청 응답에 의한 오확정 방지 |
| pending display | 이전 polling값에 의한 순간 복귀 방지 |
| timeout rollback | 장치와 화면의 장기 불일치 방지 |
| feedback policy | protocol 수락과 실제 물리 동작을 구별 |

## 12. 도면에 표시할 구체 부호

| 부호 | 구성 |
|---:|---|
| 100 | HMI 장치 |
| 110 | 표시·입력 control |
| 120 | binding 처리부 |
| 130 | pending command 저장부 |
| 140 | work scheduler |
| 150 | protocol adapter |
| 160 | confirmed-value cache |
| 170 | response correlator |
| 180 | timeout/rollback 처리부 |
| 200 | industrial controller/PLC |
| 210 | controlled physical equipment |

## 13. 명세서에 포함할 의사코드 목록

- Algorithm 1: binding initialization and snapshot pump
- Algorithm 2: manipulation suppression and final-value flush
- Algorithm 3: pending command registration
- Algorithm 4: manual-priority work selection
- Algorithm 5: RTU/TCP response correlation
- Algorithm 6: confirmed cache transaction update
- Algorithm 7: pending match/timeout decision
- Algorithm 8: reconnect reconciliation
- Algorithm 9: multi-element confirmation mask

## 14. 현재 코드 보완 및 증거화 항목

- [ ] `PendingFlush`에서도 command pending을 등록하도록 공통 함수화
- [ ] command flush 뒤 `LastSrcValue`를 desired value로 덮지 않도록 수정
- [ ] RTU slave/function/address/value 또는 count echo 대조
- [ ] TCP transaction ID/unit/function/payload 대조
- [ ] exception response/NAK를 명시적 rejected state로 전달
- [ ] 통신 timeout event와 binding pending state 연결
- [ ] late response와 retry를 위한 sequence/session 식별자
- [ ] simulated slave end-to-end test
- [ ] 정상, stale read, timeout, reject, reconnect, multi-write 시험 log

이 보완은 발명을 새로 추상적으로 만드는 작업이 아니라, 현재 분리 구현된 UI 상태와 통신 확인 상태를 하나의 재현 가능한 실시형태로 완성하고 객관적 증거를 남기는 작업이다.

