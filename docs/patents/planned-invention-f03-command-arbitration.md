# 장래 발명 F-03 — 다중 HMI 명령 중재와 재접속 안전성

## 0. 지위

- 등급: A
- 상태: 미구현 설계
- 관계: P-01의 단일 client pending을 multi-client ownership·idempotency로 확장

## 1. command envelope

```text
Command = {
  CommandId, IdempotencyKey,
  LogicalTag, RequestedValue, ExpectedPreviousValue,
  ClientId, OperatorId, Role,
  Mode: Local|Remote|Auto,
  SafetyClass, CreatedAt, ExpiresAt,
  Preconditions, FeedbackTag
}
```

## 2. tag lease

```text
Lease = {LogicalTag, OwnerClient, OwnerOperator, Priority, IssuedAt, ExpiresAt, Epoch}
```

명령 전 tag lease를 획득한다. priority는 안전등급, local/remote mode, operator role, command type으로 계산한다. 단순 last-writer-wins를 사용하지 않는다.

## 3. 상태기계

```text
Received → Validated → WaitingLease → Dispatched → Acknowledged
                                      ↘ Rejected
Dispatched → UnknownOnDisconnect → ReconciledApplied|Requeued|Conflict|Expired
```

각 transition과 결정근거를 audit log에 append한다.

## 4. 충돌 중재

같은 logical tag에 양립 불가능한 command가 겹치면 다음 순서로 비교한다.

1. emergency/safety policy
2. physical local mode ownership
3. explicit lease epoch
4. operator role/authorization
5. command creation sequence

낮은 우선순위 command는 조용히 덮어쓰지 않고 Rejected 또는 Waiting 상태와 현재 owner 정보를 요청 HMI에 통지한다.

## 5. 재접속 reconciliation

전송 후 ACK 전에 끊기면 무조건 재전송하지 않는다.

```text
confirmed := Read(FeedbackTag)
if confirmed == RequestedValue:
    AppliedWithoutDuplicate
else if expired or precondition no longer holds:
    ExpiredOrConflict
else if command is idempotent and lease epoch still valid:
    Requeue with same IdempotencyKey
else:
    RequireOperatorDecision
```

장치가 sequence echo를 지원하면 `(DeviceId,CommandId)`를 대조한다. 지원하지 않으면 expected previous value와 confirmed value를 이용한 제한적 판단을 한다.

## 6. safety path

비상정지는 일반 lease 대기열과 분리할 수 있으나 인증·감사·중복방지 요건은 유지한다. 안전 command 우선이 곧 임의 client의 권한 우회를 뜻하지 않는다.

## 7. 시험

- 두 HMI의 동시 반대 command
- lease 만료 직전 command
- local/remote mode 변경 중 요청
- send 후 ACK 전 disconnect
- 이미 적용된 command 재접속
- non-idempotent command의 operator decision
- emergency path와 audit completeness

## 8. 청구항 중심

logical tag lease, command envelope의 expected state/idempotency, disconnect 후 confirmed feedback reconciliation, 각 HMI에 대한 확정·거부·충돌 상태 통지를 결합한다.

