# 장래 발명 F-02 — 화면 가시성·안전도 기반 적응형 폴링

## 0. 지위

- 등급: A-, F-01과 결합 권장
- 상태: 미구현 설계
- 핵심: 가시성만으로 주기를 바꾸는 것이 아니라 safety deadline, address block, 전환 prefetch, pending command를 함께 제약

## 1. demand model

```text
PollDemand = {
  TagId, Consumers,
  VisibilityState,
  SafetyClass,
  AlarmOrInterlock,
  PendingCommand,
  MinPeriod, MaxPeriod, Deadline,
  DeviceCost, LastQuality
}
```

같은 tag가 visible page와 hidden component에서 동시에 쓰이면 가장 엄격한 유효 demand를 적용한다.

## 2. tier 결정

```text
if SafetyClass >= Critical or AlarmOrInterlock:
    tier = SafetyFast
else if PendingCommand:
    tier = ConfirmationFast
else if Visible:
    tier = Foreground
else if PredictedNextPage:
    tier = Prefetch
else:
    tier = Background or Suspended
```

pending command의 feedback tag는 화면이 숨겨져도 timeout/확정이 끝날 때까지 `ConfirmationFast` 아래로 낮추지 않는다.

## 3. budget allocation

channel별 wire-time budget 안에서 earliest-deadline 또는 weighted fair scheduling을 사용한다. overload 시 background→prefetch→foreground noncritical 순으로 period를 늘리고 safety/alarm deadline은 유지한다.

```text
Utilization = Σ EstimatedTransactionTime_i / Period_i
require Utilization <= ChannelBudget
```

불가능하면 안전 tag를 희생하지 않고 compile/runtime overload alarm을 낸다.

## 4. page transition prefetch

navigation event 또는 next-page probability가 threshold를 넘으면 target page block을 prefetch tier로 올린다. 실제 전환 후 first valid sample을 받기 전에는 old cache를 정상값처럼 표시하지 않고 stale/loading quality를 제공한다.

## 5. block 재구성

tier가 다른 인접 tag를 한 block에 합치면 hidden tag도 불필요하게 빠르게 읽을 수 있다. compiler는 wire cost와 over-poll cost를 비교해 block split/merge를 결정한다. 상세 adaptive error split은 F-09로 분리한다.

## 6. 안전 조건

- safety deadline 초과 금지
- pending feedback 중 poll 제거 금지
- page close가 write command를 취소한다는 묵시적 정책 금지
- reconnect 직후 confirmation tag 우선
- visibility event storm에 debounce와 minimum residency 적용

## 7. 시험

- visible/hidden 전환별 실제 period
- safety tag worst-case latency
- pending 중 page close 후 confirmation 유지
- navigation prefetch의 first-value latency
- overload 시 degradation order
- static fast polling 대비 frame/byte/CPU 감소

## 8. 청구항 중심

HMI 객체의 runtime 가시성과 safety 속성 및 pending command 상태를 tag demand로 결합하고, address block 단위 poll tier를 재구성하되 safety deadline과 confirmation read를 보존하는 방법.

