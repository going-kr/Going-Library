# 장래 발명 F-01 — 화면 바인딩 기반 통신 스캔 계획 컴파일러

## 0. 지위와 비밀관리

- 등급: A+
- 상태: 전체 compiler 미구현, GUDX·binding·통신 wrapper 구성요소만 존재
- 출원: private prototype과 선행조사 후 국내 우선, PCT 적극 검토
- 주의: 출원 전 공개 repository, package, demo에 algorithm 공개 금지

### 2026-08-03 KIPRIS 긴급 경고

KIPRIS 국내 직접검색에서 공개출원 10-2026-0094264(공개 10-2026-0088432, 「PLC 주소 바인딩 기반 객체형 자동화 설비 시뮬레이션 시스템 및 방법」)가 확인되었다. 이 문헌은 시각 객체의 PLC address binding에서 읽기·쓰기 주소를 분리하고 polling table과 연속주소 read block을 구성하며, write queue를 만들고 수신값으로 객체상태를 갱신하는 구성을 청구한다.

따라서 위의 A+는 기술적 사업가치의 종전 평가일 뿐 등록 가능성 확정등급이 아니다. 전체 청구항을 charting하기 전에는 넓은 binding-to-polling-block 발상을 신규 핵심으로 취급하지 않는다. 10-2019-0136591의 인접주소 접근 최적화와 10-2017-0100422의 tag 기반 receiver block 생성도 함께 비교한다. 상세 기록은 [KIPRIS 국내 선행기술 1차 검색 기록](./2026-08-03-kipris-prior-art-search.md)을 따른다.

현 단계 차별화 가설은 component/ItemList scope expansion, protocol 제약 기반 block plan과 decode map의 동시 생성, command-feedback 관계에서 P-01 상태기계 자동 생성, source map 및 UI-plan deployment identity의 결합이다. 이 가설이 선행문헌 전체 명세서에서 부정되는지 먼저 확인한다.

## 1. 발명 입력과 출력

```text
Input = {
  GudxDesignGraph,
  ComponentDefinitions,
  TagSchema,
  ProtocolDeviceCapabilities,
  PageVisibilityGraph,
  SafetyPolicy
}

Output = {
  PollBlocks,
  WriteEndpoints,
  DecodeMap,
  BindingPlan,
  RuntimeTierRules,
  PlanVersionAndHash
}
```

`TagSchema`는 논리 tag를 `protocol/channel/device/area/address/dataType/endian/scale/access/feedbackTag`로 변환한다.

## 2. compiler pipeline

### 2.1 object graph 전개

master, page, window, component template, ItemList template를 읽는다. component instance는 parameter binding으로 치환하여 실제 논리 path 집합으로 전개한다. 동적 ItemList는 정적 item schema가 있으면 wildcard tag family로, 없으면 runtime plan extension point로 남긴다.

### 2.2 binding 분류

```text
getter only       -> read demand
setter only       -> write endpoint
getter + setter   -> read/write demand
command marker    -> write endpoint + confirmed feedback + P-01 state machine
format only       -> read demand + presentation transform
```

### 2.3 physical range 계산

각 tag data type의 bit/word width와 endian을 사용한다.

```text
Range(tag) = [startAddress, startAddress + encodedWidth - 1]
```

bit-in-word, 32/64-bit value, string length, array count를 반영한다. overflow, misalignment, device area boundary를 compile error로 만든다.

### 2.4 중복 제거와 block 병합

동일 physical tag를 여러 control이 읽으면 하나의 demand로 합친다. 같은 protocol/device/function/tier의 range를 주소순 정렬한 뒤 다음 제약을 만족하는 동안 병합한다.

```text
mergedLength <= device.MaxReadLength
gap <= AllowedGap
estimatedResponse <= PDU limit
all members share compatible period/deadline
no forbidden area boundary crossing
```

### 2.5 decode map

block response offset를 개별 typed tag에 역매핑한다.

```text
DecodeEntry = {
  BlockId, ByteOrBitOffset, Width,
  DataType, Endian, Scale, TargetLogicalTag
}
```

### 2.6 runtime binding plan

confirmed cache getter, write enqueue setter, quality source와 P-01 pending state를 control property에 연결한다. UI artifact hash와 scan plan hash를 하나의 deployment identity로 묶는다.

## 3. 오류 report

- unresolved binding path/tag
- overlapping tags with incompatible type/endian
- write-only tag에 confirmed feedback 없음
- device 최대 PDU 초과
- address range overflow
- component parameter로 주소가 확정되지 않음
- safety tag가 low/disabled tier로만 배정됨
- 동일 logical tag의 상충 physical mapping

compiler는 file/element/control/property/binding path까지 포함한 source location을 반환한다.

## 4. 예시

`Main` page의 세 control이 D100 `float`, D102 `short`, D110 `short`를 100ms 주기로 요구하고 device max가 16 words, allowed gap이 4라면 D100~D102는 하나의 block으로, D110은 gap 정책에 따라 별도 block으로 만든다. 두 control이 D100을 참조해도 wire read는 한 번이며 decode 결과를 두 binding에 fan-out한다.

## 5. 최소 prototype

1. GUDX에서 정적 `{Tags.*}` binding 수집
2. Modbus holding register만 대상으로 type width 계산
3. contiguous/allowed-gap block merge
4. simulated response decode
5. P-01 command binding 한 종류 자동 생성
6. plan JSON과 source map 출력

## 6. 시험·효과

- 수동 1-tag-per-request 대비 frame/wire byte 감소율
- binding 개수 대비 unique physical tag 감소율
- component N개 전개 정확도
- decode round-trip과 endian 조합
- 주소 중복·누락·범위 오류 검출률
- page load 첫 valid value 시간

## 7. 독립항 중심

직렬화된 HMI 객체 그래프에서 component/item scope를 전개해 통신 address demand를 만들고, protocol 제약에 따라 block plan 및 decode map을 생성하며, command binding에 확인응답 상태기계를 자동 결합하는 순서를 중심으로 한다. 단순 tag import나 일반 subscription 생성으로 넓히지 않는다.

## 8. 선행조사 검색

- HMI screen definition automatic PLC polling plan generation
- UI binding expression communication address block optimization
- SCADA tag grouping Modbus contiguous register compiler
- component template expansion communication subscription generation
