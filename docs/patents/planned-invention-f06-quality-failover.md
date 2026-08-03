# 장래 발명 F-06 — 다중 프로토콜 확인값의 품질 기반 선택과 failover

## 0. 지위

- 등급: A-
- 상태: 미구현; Modbus/MQTT/CNet/MC client 자산은 존재
- 핵심: 최신 timestamp 하나를 고르는 것이 아니라 확인응답·품질·sequence·경로 ownership으로 논리 tag 상태를 합성

## 1. mapping

```text
LogicalTag = {
  Id, DataType, Unit, Tolerance,
  ReadPaths: [PhysicalPath...],
  WritePaths: [PhysicalPath...],
  SelectionPolicy
}

PhysicalPath = {
  Protocol, Channel, Device, Address,
  Priority, ReadOnly,
  LastValue, Timestamp, Quality, Sequence,
  ConnectionState, ConfirmedByDevice
}
```

## 2. candidate normalization

각 path 값을 공통 type/unit로 변환하고 clock skew를 보정한다. bad/stale path는 탈락시키거나 penalty를 준다.

```text
score(path) =
  protocolPriority
  + confirmationBonus
  + freshnessScore
  + sequenceContinuity
  + connectionHealth
  - qualityPenalty
  - clockUncertainty
```

## 3. confirmed value 선택

허용 tolerance 안에서 복수 path가 일치하면 가장 높은 score 값을 confirmed로 채택하고 agreement confidence를 올린다. 값이 충돌하면 단순 overwrite하지 않고 `Conflict` 상태와 path별 값을 유지한다.

```text
LogicalValue = {
  Value?, State: Confirmed|Stale|Conflict|Bad|Unavailable,
  SelectedPath?, SupportingPaths,
  Timestamp, Confidence
}
```

## 4. write ownership

읽기는 다중 path로 허용해도 쓰기는 같은 시점에 하나의 lease owner path만 사용한다. failover 전 다음을 확인한다.

- 기존 write의 ACK/feedback/pending 상태
- 새 path가 같은 physical actuator를 가리키는지
- command sequence/idempotency 지원 여부
- logical-to-physical scale/endian 동등성

unknown command가 있으면 자동 failover write를 중지하고 F-03 reconciliation을 실행한다.

## 5. path 전환

```text
Healthy → Degraded → ReadFailoverCandidate → ReadFailover
WriteOwner → Quiescing → Reconciled → NewWriteOwner
```

hysteresis와 minimum dwell time으로 경로 flapping을 막는다. primary 회복 시 즉시 되돌아가지 않고 값 일치와 안정시간을 확인한다.

## 6. 시험

- primary timeout, secondary 정상
- 두 path timestamp는 최신이나 값 충돌
- clock skew/sequence rollback
- write ACK 전 primary 단절
- primary recovery flapping
- protocol별 scale/endian mapping 불일치
- 동일값 supporting path 수에 따른 confidence

## 7. 청구항 중심

하나의 logical industrial tag에 연결된 이종 protocol path들의 device confirmation, freshness, sequence, health를 이용해 confirmed/conflict 상태를 계산하고, write pending reconciliation 후에만 단일 write-owner path를 전환하는 결합.

