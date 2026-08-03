# 장래 발명 F-08 — 통신 품질 전파형 HMI 표시·입력 제어

## 0. 지위

- 등급: B+
- 상태: 미구현 통합안
- 주의: OPC UA quality/status 자체가 아니라 GUDX binding graph, derived value, command enablement의 결합으로 한정

## 1. value envelope

```text
QualifiedValue<T> = {
  Value,
  State: Confirmed|Pending|Stale|Substituted|Conflict|Bad|Unavailable,
  Source, Timestamp, Age,
  QualityCode, Confidence,
  PendingCommandId?
}
```

## 2. binding 전파

GUDX binder가 raw value getter가 아니라 qualified getter를 연결한다. control adapter는 값과 상태를 각각 visual property로 변환한다.

```text
Confirmed   -> normal
Pending     -> command value + pending indicator
Stale       -> retained value + age/stale style
Substituted -> substitute badge
Conflict    -> no silent selection + conflict style
Bad         -> invalid style
```

## 3. derived quality 합성

여러 input으로 계산한 값의 상태는 worst-only가 아니라 transform policy에 따른다.

```text
DerivedQuality = Combine(input qualities, operation semantics)
DerivedTimestamp = min relevant source timestamp
```

예를 들어 `A+B`는 둘 다 필요하지만 `A ?? fallback`은 A bad일 때 fallback을 쓰고 `Substituted`를 출력한다. graph series는 point마다 quality를 보존하거나 bad interval을 gap으로 표시한다.

## 4. command enablement

write control은 다음 정책으로 입력 가능 여부를 계산한다.

```text
Enabled = Authorized
       AND ConnectionPolicyAllows
       AND RequiredFeedbackQuality acceptable
       AND not conflicting owner
       AND safety preconditions satisfied
```

stale 값에 기반한 증감 command처럼 위험한 입력은 disable하거나 absolute confirmation을 요구한다.

## 5. quality age update

새 packet이 없어도 monotonic time에 따라 Confirmed→Stale→Unavailable로 전이한다. page가 hidden이라 poll 주기가 길어진 경우 F-02 계획된 period를 stale threshold 산정에 포함해 정상 background tag를 오경보하지 않는다.

## 6. 시험

- packet 중단에 따른 age 상태 전이
- pending 후 confirmed/timeout
- failover conflict 표시
- derived value input quality 조합
- trend bad interval 표현
- stale feedback에서 command disable
- hidden-page adaptive polling과 stale threshold 일관성

## 7. 청구항 중심

통신 source의 confirmed/pending/stale/conflict 상태를 binding graph를 따라 derived/component value로 합성하고, 결과 quality에 따라 표시뿐 아니라 산업 command 입력 가능성과 confirmation 방식을 자동 결정하는 결합.

