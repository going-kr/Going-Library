# 장래 발명 F-07 — 오프라인 산업 명령 queue와 상태 충돌 조정

## 0. 지위

- 등급: B+
- 상태: 미구현
- 권장 결합: F-03 command arbitration 및 P-01 confirmed state

## 1. offline command record

```text
OfflineCommand = {
  CommandId, IdempotencyKey,
  LogicalTag, RequestedValue,
  ExpectedPreviousValue,
  Preconditions,
  CreatedAt, ExpiresAt,
  CommandClass: SetState|Pulse|Increment|Recipe|Safety,
  SupersessionGroup,
  OperatorAndAuthorization,
  QueueState
}
```

모든 명령을 저장하지 않는다. pulse, emergency, 시간민감 command는 기본적으로 offline queue 금지 또는 매우 짧은 expiry를 갖는다.

## 2. enqueue validation

- operator 권한과 local/remote mode
- tag의 offline 허용 정책
- expiry와 precondition
- 동일 supersession group의 이전 command와 병합 가능성
- 저장소 암호화·무결성·감사 요구

예를 들어 온도 setpoint 20→21→22는 마지막 22로 합칠 수 있지만 start/stop sequence는 임의 병합하지 않는다.

## 3. reconnect classification

재접속 후 먼저 confirmed value를 읽고 각 command를 분류한다.

```text
if now > ExpiresAt: Expired
else if confirmed == RequestedValue: AlreadyApplied
else if preconditions fail: Conflict
else if confirmed == ExpectedPreviousValue and command allowed: Executable
else if command is commutative/idempotent under policy: ExecutableWithWarning
else: RequiresApproval
```

분류 완료 전 FIFO로 무조건 전송하지 않는다.

## 4. ordering

같은 tag는 causal order를 유지하고, 서로 다른 tag라도 precondition dependency graph가 있으면 topological order로 실행한다. cycle dependency는 operator approval 대상으로 둔다.

## 5. 실행과 확인

실행 가능한 command마다 P-01 pending state를 만들고 ACK/feedback 확인 후 다음 dependent command로 진행한다. 실패 시 dependent command를 보류하고 독립 command만 계속할 수 있다.

## 6. 시험

- offline 중 이미 다른 HMI가 목표값 적용
- expected previous value 변경
- expiry 통과
- pulse command queue 거부
- superseding setpoint merge
- dependency cycle
- reconnect 재단절과 중복방지

## 7. 청구항 중심

오프라인 명령에 예상 이전값·만료·멱등성·선행조건을 저장하고, 재접속 confirmed state와 비교해 already-applied/executable/conflict/expired로 분류한 뒤 안전 분류만 확인응답 상태기계로 실행하는 방법.

