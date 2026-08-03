# 장래 발명 F-10 — 통신·화면 정의 공동 검증과 디지털 시운전

## 0. 지위

- 등급: B+
- 상태: 미구현 통합안; Modbus slave와 GUDX/바인딩 시험 자산 존재
- 핵심: 일반 test generation이 아니라 HMI command/feedback/address/state-machine의 공동 일관성 검증

## 1. 입력

```text
GudxDesignAndBindings
F01 TagAndPollPlan
CommandPolicies(P-01/F-03/F-07)
DeviceSchema(address, type, range, unit, access)
ScenarioRules and SafetyConstraints
```

## 2. 정적 검증

- 모든 binding path/tag resolve 여부
- read/write access와 control direction 일치
- command tag의 feedback mapping 존재
- type/width/endian/scale/unit 일치
- overlapping address의 호환성
- min/max와 device engineering range
- 권한/safety property 누락
- page/component/template 전개 후 중복·미사용 tag

## 3. simulator 생성

device schema에서 Modbus slave memory 또는 protocol adapter simulator를 만든다.

```text
SimTag = {
  AddressMapping,
  InitialValue,
  ReadBehavior,
  WriteAcceptanceRule,
  FeedbackDelay,
  FailureInjectionRules
}
```

write request를 받으면 accept/reject/delay/drop/partial-effect scenario에 따라 ACK와 feedback을 생성한다.

## 4. 상태경로 생성

각 command binding에 대해 다음 사건 순서를 조합한다.

- old poll response before/after write
- ACK success/exception/timeout
- feedback equal/different/delayed
- disconnect before/after device apply
- retry/reconnect
- simultaneous command conflict
- page hide/deployment during pending

state-space 폭발은 equivalence class와 pairwise 조합으로 줄이고 safety command는 exhaustive path를 적용한다.

## 5. oracle

```text
Never show Confirmed before valid confirmation
Never lose pending solely because page hidden
Timeout returns to latest confirmed value
Rejected command does not mutate confirmed cache
Duplicate reconnect does not repeat non-idempotent effect
UI and poll plan versions remain consistent
```

실제 control state, wrapper cache, sent frame journal을 oracle과 비교한다.

## 6. 산출물

- binding-to-address traceability matrix
- generated simulator configuration
- scenario별 event timeline
- pass/fail 및 first divergent state
- packet/frame evidence
- coverage: command, tag, control, state transition, fault type

## 7. 시험

- known misaddress/type/endian fixture 검출
- ACK 전 false success 검출
- stale read race 재현
- reconnect duplicate 재현
- component 100개 주소 전개 coverage
- simulator와 실제 PLC capture의 protocol parity

## 8. 청구항 중심

직렬화된 HMI binding에서 industrial address·command·feedback 관계를 추출해 protocol slave simulator와 비동기 응답 scenario를 자동 생성하고, confirmed/pending/rollback 상태 불변조건으로 화면과 통신 구현을 공동 검증하는 방법.

