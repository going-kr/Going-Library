# 장래 발명 F-09 — 통신 부하·오류 기반 scan block 자동 재편성

## 0. 지위

- 등급: B+
- 상태: 미구현
- 기반: F-01 initial block plan, F-02 demand tier

## 1. block telemetry

```text
BlockStats = {
  BlockId, AddressRange, Members,
  SampleCount, SuccessRate,
  TimeoutRate, ExceptionAddresses,
  LatencyDistribution,
  WireBytes, RetryCost,
  MemberDeadlines
}
```

## 2. 문제

인접 tag를 큰 block으로 읽으면 frame 수는 줄지만 한 invalid address 때문에 전체 block이 실패할 수 있다. 지나치게 작은 block은 overhead가 크다. 최적 경계는 장치 오류와 화면 demand 변화에 따라 달라진다.

## 3. split rule

반복 exception/timeout이 발생하면 binary split 또는 오류 주소 evidence를 이용해 후보 경계를 만든다.

```text
Benefit(split) = avoidedRetryCost + isolatedHealthyDeadlineBenefit
               - addedFrameOverhead
split if Benefit > threshold and dwell condition met
```

safety tag와 비안전 tag가 같은 failing block에 있으면 safety tag의 deadline을 회복하는 경계를 우선한다.

## 4. merge rule

인접 block이 안정시간 동안 성공하고 tier/protocol/PDU가 호환되면 병합 후보가 된다.

```text
mergedLength <= MaxPdu
predictedLatency <= strictestDeadline
gapReadCost < extraFrameCost
no known fault boundary crossed
```

split/merge flapping을 막기 위해 hysteresis, minimum sample count, cooldown을 둔다.

## 5. 탐색과 검증

새 block 경계는 shadow schedule에서 제한된 probe로 먼저 검증할 수 있다. probe가 safety deadline을 방해하지 않도록 budget을 제한한다. plan revision과 이전 통계를 journal에 기록한다.

## 6. 예시

D100~D119 block에서 D110 접근만 exception을 반복하면 D100~D109, D110, D111~D119로 격리한다. 이후 D100~D109와 D111~D119는 각각 정상 deadline을 유지하고 D110은 저속 health probe로 둔다.

## 7. 시험

- 단일 bad address 격리시간
- healthy tag latency 회복
- added wire overhead
- intermittent fault의 불필요 split 방지
- recovery 후 merge hysteresis
- PDU/deadline constraint 위반 없음

## 8. 청구항 중심

HMI demand deadline과 protocol address block telemetry를 함께 사용해 오류 구간을 분할하고 안정 구간을 재병합하며, safety tag deadline과 PDU 제약을 유지하는 online scan-plan 재편성.

