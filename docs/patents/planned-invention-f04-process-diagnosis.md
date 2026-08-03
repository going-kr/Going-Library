# 장래 발명 F-04 — 예상 흐름과 실제 I/O의 불일치 기반 공정 이상진단

## 0. 지위

- 등급: A-
- 상태: 미구현 설계
- 기반: P-03 topology solver
- 상세 algorithm: `technical-principle-03-flow-system.md` 8~10절

## 1. 입력

```text
TopologySnapshot
ActuatorCommandAndFeedback
Flow/Pressure/Level observations with timestamp and quality
Fluid source identities and forbidden-mix rules
Diagnostic thresholds and cause priors
```

## 2. 처리

1. port-pair gate와 source를 이용해 expected active edge/direction/fluid set을 고정점 계산한다.
2. sensor를 node/edge/port에 매핑하고 timestamp/quality를 정규화한다.
3. expected-active/observed-zero, expected-inactive/observed-flow, command/feedback mismatch, mass imbalance, reverse flow 등의 violation을 만든다.
4. 각 violation을 설명할 candidate cause set을 topology neighborhood에서 생성한다.
5. weighted hitting set으로 복수 violation을 설명하는 작은 cause set 상위 K개를 선택한다.
6. candidate cause, evidence, upstream/downstream 영향 path, confidence를 표시한다.

## 3. 원인 후보 예

- pump/drive non-response
- closed or mispositioned valve
- valve internal leak
- pipe blockage/leak
- alternate unmodeled path
- reverse installation/backflow
- sensor bias/stale/bad
- topology mapping error

## 4. 오염진단

서로 다른 `FluidId`가 처음 합쳐지는 element와 downstream reachable sink를 계산한다. 실제 conductivity/density/recipe sensor가 있으면 expected mix와 observation을 비교한다. line volume과 flow가 있을 때만 도달시간을 추정한다.

## 5. 출력

```text
Diagnosis = {
  RankedCauses,
  ExplainedViolations,
  UnexplainedEvidence,
  Confidence,
  AffectedPathsAndSinks,
  SnapshotVersions
}
```

## 6. prototype gate

- cycle-safe P-03 solver 구현
- pump/valve/flow/pressure simulator
- fault injection fixture 20종 이상
- top-1/top-3 cause recall과 false alarm 측정
- stale/bad sensor graceful degradation

## 7. 청구항 중심

HMI 화면 topology로부터 예상 경로를 만들고 live I/O를 같은 graph element에 매핑한 뒤, 복수 불일치 constraint를 함께 설명하는 원인 element 집합과 영향 경로를 표시하는 결합이다. 단순 threshold alarm으로 작성하지 않는다.

